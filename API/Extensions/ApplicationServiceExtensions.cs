using Application.Accounting.Services;
using Application.Catalog.Products;
using Application.Catalog.Products.Services.Cost;
using Application.Catalog.Products.Services.Inventory;
using Application.Catalog.ProductStores;
using Application.Common;
using Application.Core;
using Application.Facilities;
using Application.Interfaces;
using Application.Manufacturing;
using Application.order.Orders;
using Application.Order.Orders;
using Application.Order.Orders.Returns;
using Application.order.Quotes;
using Application.Parties.ContactMechTypes;
using Application.Parties.Parties;
using Application.Services;
using Application.Shipments;
using Application.WorkEfforts;
using FluentValidation;
using Infrastructure.Contents;
using Infrastructure.Security;
using MediatR;

namespace API.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration config)
    {
        services.AddDbContext<DataContext>(options =>
        {
            var env = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            string connStr;
            var serverVersion = new MySqlServerVersion(new Version(8, 3, 0));

            if (env == "Development")

                options.UseMySql(config.GetConnectionString("DefaultConnection"), serverVersion)
                    .LogTo(Console.WriteLine, LogLevel.Information)
                    .EnableSensitiveDataLogging(false)
                    .EnableDetailedErrors(false);
            /*options.UseMySql(config.GetConnectionString("DefaultConnection"), serverVersion)
                .LogTo(Console.WriteLine, LogLevel.Warning);*/
            else
                options.UseMySql(config.GetConnectionString("DefaultConnection"), serverVersion)
                    .LogTo(Console.WriteLine, LogLevel.Information);
        });
        services.AddCors(opt =>
        {
            opt.AddPolicy("CorsPolicy", policy =>
            {
                policy
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials()
                    .WithOrigins("http://localhost:3000");
            });
        });

        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<CreateProduct>();

        services.AddMediatR(typeof(List.Handler).Assembly);
        services.AddAutoMapper(typeof(MappingProfiles).Assembly);

        services.AddMvc().AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new DateTimeConverter());
        });


        services.AddScoped<IUserAccessor, UserAccessor>();
        services.AddScoped<ICommonService, CommonService>();
        services.AddScoped<IGeneralLedgerService, GeneralLedgerService>();
        services.AddScoped<IAcctgTransService, AcctgTransService>();
        services.AddScoped<IAcctgReportsService, AcctgReportsService>();
        services.AddScoped<IAcctgMiscService, AcctgMiscService>();
        services.AddScoped<ITaxService, TaxService>();
        services.AddScoped<IInvoiceService, InvoiceService>();
        services.AddScoped<IInvoiceUtilityService, InvoiceUtilityService>();
        services.AddScoped<IInvoiceHelperService, InvoiceHelperService>();
        services.AddScoped<IPaymentWorkerService, PaymentWorkerService>();
        services.AddScoped<IBillingAccountService, BillingAccountService>();
        services.AddScoped<IPaymentService, PaymentService>();
        //services.AddScoped<IPaymentGatewayService, PaymentGatewayService>();
        services.AddScoped<IPaymentHelperService, PaymentHelperService>();
        services.AddScoped<IPaymentApplicationService, PaymentApplicationService>();
        services.AddScoped<IProductService, ProductService>();
        services.AddScoped<IProductHelperService, ProductHelperService>();
        services.AddScoped<IProductStoreService, ProductStoreService>();
        services.AddScoped<IProductionRunService, ProductionRunService>();
        services.AddScoped<IProductionRunHelperService, ProductionRunHelperService>();
        services.AddScoped<IBOMTreeService, BOMTreeService>();
        services.AddScoped<IBOMNodeService, BOMNodeService>();
        services.AddScoped<IWorkEffortService, WorkEffortService>();
        services.AddScoped<IRepositoryService, RepositoryService>();
        services.AddScoped<IOrderService, OrderService>();
        services.AddScoped<IReturnService, ReturnService>();
        services.AddScoped<IOrderReadService, OrderReadService>();
        services.AddScoped<IOrderHelperService, OrderHelperService>();
        services.AddScoped<IQuoteService, QuoteService>();
        services.AddScoped<IUtilityService, UtilityService>();
        services.AddScoped<IShipmentService, ShipmentService>();
        services.AddScoped<IShipmentHelperService, ShipmentHelperService>();
        services.AddScoped<IIssuanceService, IssuanceService>();
        services.AddScoped<IInventoryService, InventoryService>();
        services.AddScoped<IInventoryHelperService, InventoryHelperService>();
        services.AddScoped<ICostService, CostService>();
        services.AddScoped<IRoutingService, RoutingService>();
        services.AddScoped<ITechDataService, TechDataService>();
        services.AddScoped<IFacilityService, FacilityService>();
        services.AddScoped<IPickListService, PickListService>();
        services.AddScoped<IPackService, PackService>();
        services.AddScoped<IFinAccountService, FinAccountService>();
        services.AddScoped<IVehicleService, VehicleService>();
        services.AddScoped<IStatusService, StatusService>();
        services.AddScoped<IPartyService, PartyService>();
        services.AddScoped<IContentAccessor, ContentAccessor>();
        services.AddScoped<IPriceService, PriceService>(); // Register the service
        services.AddScoped<CustomMethodsService>();
        services.Configure<DigitalOceanSettings>(config.GetSection("DigitalOceanSpaces"));
        services.AddSignalR();
        services.AddScoped<Lazy<IGeneralLedgerService>>(sp =>
            new Lazy<IGeneralLedgerService>(() => sp.GetRequiredService<IGeneralLedgerService>()));
        services.AddScoped<Lazy<IPaymentApplicationService>>(sp =>
            new Lazy<IPaymentApplicationService>(() => sp.GetRequiredService<IPaymentApplicationService>()));
        services.AddScoped<Lazy<IInvoiceService>>(sp =>
            new Lazy<IInvoiceService>(() => sp.GetRequiredService<IInvoiceService>()));
        services.AddScoped<Lazy<IAcctgReportsService>>(sp =>
            new Lazy<IAcctgReportsService>(() => sp.GetRequiredService<IAcctgReportsService>()));
        services.AddScoped<Lazy<IShipmentService>>(sp =>
            new Lazy<IShipmentService>(() => sp.GetRequiredService<IShipmentService>()));
        services.AddScoped<Lazy<IShipmentHelperService>>(sp =>
            new Lazy<IShipmentHelperService>(() => sp.GetRequiredService<IShipmentHelperService>()));
        services.AddScoped<Lazy<IPaymentHelperService>>(sp =>
            new Lazy<IPaymentHelperService>(() => sp.GetRequiredService<IPaymentHelperService>()));
        services.AddScoped<Lazy<IFinAccountService>>(sp =>
            new Lazy<IFinAccountService>(() => sp.GetRequiredService<IFinAccountService>()));
        services.AddScoped<Lazy<IOrderHelperService>>(sp =>
            new Lazy<IOrderHelperService>(() => sp.GetRequiredService<IOrderHelperService>()));

        return services;
    }
}