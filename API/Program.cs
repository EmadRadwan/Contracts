using System.Text.Json;
using System.Text.Json.Serialization;
using API.Controllers.Accounting.Transactions;
using Application.Accounting.Payments;
using Application.Shipments.Agreement;
using Application.Shipments.BillingAccounts;
using Application.Shipments.FinAccounts;
using Application.Shipments.FixedAssets;
using Application.Shipments.ForeignExchangeRates;
using Application.Shipments.GlobalGlSettings;
using Application.Shipments.InvoiceItemTypes;
using Application.Shipments.Invoices;
using Application.Shipments.OrganizationGlSettings;
using Application.Shipments.PaymentMethodTypes;
using Application.Shipments.Taxes;
using Application.Shipments.Transactions;
using Application.Catalog.Products;
using Application.Catalog.ProductStores;
using Application.Facilities.FacilityInventories;
using Application.Manufacturing;
using Application.Order.Orders;
using Application.Order.Orders.Returns;
using Application.Order.Quotes;
using Application.Parties.Parties;
using Application.Projects;
using Microsoft.AspNetCore.OData;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.OData.Edm;
using Microsoft.OData.ModelBuilder;
using Serilog;
using Serilog.Events;
using Serilog.Filters;
using Serilog.Sinks.SystemConsole.Themes;


Log.Logger = new LoggerConfiguration()
    .Enrich.WithThreadId()
    .Enrich.WithMachineName()
    .WriteTo.Console(
        outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3} T{ThreadId}] {Message:lj}{NewLine}{Exception}",
        theme: AnsiConsoleTheme.Literate,
        restrictedToMinimumLevel: LogEventLevel.Warning)
    .WriteTo.File("logs/logfile.log",
        LogEventLevel.Warning,
        "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
        rollingInterval: RollingInterval.Day)
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(Matching.WithProperty<string>("Transaction", tx => tx == "create sales order"))
        .WriteTo.File("logs/create-sales-order-logfile.log",
            LogEventLevel.Debug,
            "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
            rollingInterval: RollingInterval.Day))
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(Matching.WithProperty<string>("Transaction", tx => tx == "Get Routing Tasks"))
        .WriteTo.File($"logs/create-production-run-BOM-logfile-{DateTime.Now:yyyyMMdd_HHmmss}.log",
            LogEventLevel.Debug,
            "[{Timestamp:yyyy-MM-dd HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}",
            rollingInterval: RollingInterval.Infinite,
            buffered: true)) // Enable buffering to delay file creation until the first log
    .MinimumLevel.Information()
    .CreateLogger();


var builder = WebApplication.CreateBuilder(args);

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat(new[] { "text/css", "application/javascript" });
});

// ✅ Disable HTTPS configuration in Development mode
if (builder.Environment.IsProduction())
{
    Console.WriteLine("Running in PRODUCTION mode - HTTPS is required.");
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(5000); // HTTP
        options.ListenAnyIP(8444,
            listenOptions => { listenOptions.UseHttps("/root/.aspnet/https/aspnetapp.pfx", "Tmbtc202500"); });
    });
}
else
{
    Console.WriteLine("Running in DEVELOPMENT mode - HTTPS is DISABLED.");
    builder.WebHost.ConfigureKestrel(options =>
    {
        options.ListenAnyIP(5001); // HTTP only, no HTTPS
    });
}

builder.Host.UseSerilog();


builder.Services.AddControllers(opt =>
    {
        var policy = new AuthorizationPolicyBuilder().RequireAuthenticatedUser().Build();
        opt.Filters.Add(new AuthorizeFilter(policy));
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    })
    .AddOData(options =>
        options.Select().Filter().Count().OrderBy().Expand()
            .AddRouteComponents("odata", GetEdmModel()).SetMaxTop(100));

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddIdentityServices(builder.Configuration);


// Configure the HTTP request pipeline.

var app = builder.Build();

app.UseMiddleware<ExceptionMiddleware>();

// SPECIAL REMARK: Add Serilog request logging for diagnostics
app.UseSerilogRequestLogging(options =>
{
    options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
    {
        diagnosticContext.Set("RequestPath", httpContext.Request.Path);
        diagnosticContext.Set("Method", httpContext.Request.Method);
    };
});

app.UseResponseCompression();
app.UseStaticFiles(new StaticFileOptions
{
    OnPrepareResponse = ctx =>
    {
        if (ctx.File.Name.EndsWith(".css") || ctx.File.Name.EndsWith(".js"))
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "public,max-age=31536000");
        }
        else
        {
            ctx.Context.Response.Headers.Append("Cache-Control", "no-cache, no-store, must-revalidate");
            ctx.Context.Response.Headers.Append("Pragma", "no-cache");
            ctx.Context.Response.Headers.Append("Expires", "0");
        }
    }
});
app.UseCors("CorsPolicy");
app.UseAuthentication();
app.UseRouting();
app.UseAuthorization();


// SPECIAL REMARK: Handle API routes and unmatched API routes
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    // Return JSON 404 for unmatched API routes
    endpoints.Map("/api/{**path}", async context =>
    {
        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
        logger.LogWarning("Unmatched API endpoint requested: {Method} {Path}", context.Request.Method,
            context.Request.Path);
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.4",
            title = "Not Found",
            status = 404,
            detail = $"The requested API endpoint '{context.Request.Path}' was not found.",
            traceId = context.TraceIdentifier
        });
    });
});

// SPECIAL REMARK: Fallback to index.html for non-API routes (if needed)
app.UseDefaultFiles();
app.MapFallbackToFile("index.html");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try

    {
        var context = services.GetRequiredService<DataContext>();
        var userManager = services.GetRequiredService<UserManager<AppUserLogin>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
        await context.Database.MigrateAsync();
        await SeedContracts.SeedData(context, userManager, roleManager);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occured during migration");
    }
}

await app.RunAsync();

static IEdmModel GetEdmModel()
{
    var modelBuilder = new ODataConventionModelBuilder();
    modelBuilder.EntitySet<OrderRecord>("OrderRecords");
    modelBuilder.EntitySet<PartyRecord>("PartyRecords");
    modelBuilder.EntitySet<QuoteRecord>("QuoteRecords");
    modelBuilder.EntitySet<ReturnRecord>("ReturnRecords");
    modelBuilder.EntitySet<ProductRecord>("ProductRecords");
    modelBuilder.EntitySet<ProductStoreRecord>("ProductStoreRecords");
    modelBuilder.EntitySet<FacilityInventoryRecord>("FacilityInventoryRecord");
    modelBuilder.EntitySet<FacilityInventoryItemRecord>("FacilityInventoryItemRecord");
    modelBuilder.EntitySet<FacilityInventoryItemDetailRecord>("FacilityInventoryItemDetailRecord");
    modelBuilder.EntitySet<GlAccountRecord>("GlAccountRecord");
    modelBuilder.EntitySet<InvoiceRecord>("InvoiceRecord");
    modelBuilder.EntitySet<PaymentRecord>("PaymentRecord");
    modelBuilder.EntitySet<PaymentMethodTypeRecord>("PaymentMethodTypeRecord");
    modelBuilder.EntitySet<InvoiceItemTypeRecord>("InvoiceItemTypeRecord");
    modelBuilder.EntitySet<UomConversionDatedRecord>("UomConversionDatedRecord");
    modelBuilder.EntitySet<BillingAccountRecord>("BillingAccountRecord");
    modelBuilder.EntitySet<FinAccountRecord>("FinAccountRecord");
    modelBuilder.EntitySet<TaxAuthorityRecord>("TaxAuthorityRecord");
    modelBuilder.EntitySet<FixedAssetRecord>("FixedAssetRecord");
    modelBuilder.EntitySet<BillOfMaterialRecord>("BillOfMaterialRecord");
    modelBuilder.EntitySet<BOMProductComponentRecord>("BOMProductComponentRecord");
    modelBuilder.EntitySet<AgreementRecord>("AgreementRecord");
    modelBuilder.EntitySet<InternalAccountingOrganizationRecord>("InternalAccountingOrganizationRecord");
    modelBuilder.EntitySet<OrganizationGlAccountRecord>("OrganizationGlAccountRecord");
    modelBuilder.EntitySet<GlAccountTypeDefaultRecord>("GlAccountTypeDefaultRecord");
    modelBuilder.EntitySet<AccountingTransactionRecord>("AccountingTransactionRecord");
    modelBuilder.EntitySet<AccountingTransactionEntryRecord>("AccountingTransactionEntryRecord");
    modelBuilder.EntitySet<WorkEffortReservationSummaryRecord>("WorkEffortReservationSummaryRecord");
    modelBuilder.EntitySet<ProjectCertificateRecord>("ProjectCertificateRecord");
    modelBuilder.EntitySet<WorkEffortRecord>("WorkEffortRecord");
    modelBuilder.EntitySet<CostComponentCalcRecord>("CostComponentCalcRecord");
    modelBuilder.EntitySet<CostComponentRecord>("CostComponentRecord");

    var model = modelBuilder.GetEdmModel();


    return model;
}

public class DateTimeConverter : JsonConverter<DateTime>
{
    public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        //return DateTime.ParseExact(reader.GetString(), "yyyy-MM-ddTHH:mm:sss.K", null).ToUniversalTime();
        return DateTime.Parse(reader.GetString()).ToUniversalTime();
    }

    public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(DateTime.SpecifyKind(value, DateTimeKind.Utc));
    }
}