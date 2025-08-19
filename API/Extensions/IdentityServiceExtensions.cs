using System.Text;
using API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using InvalidOperationException = System.InvalidOperationException;

namespace API.Extensions;

public static class IdentityServiceExtensions
{
    public static IServiceCollection AddIdentityServices(this IServiceCollection services,
        IConfiguration config)
    {
        services.AddIdentityCore<AppUserLogin>(opt =>
        {
            opt.Password.RequireNonAlphanumeric = false;
            opt.SignIn.RequireConfirmedEmail = true;
        });


        var builder = services.AddIdentityCore<AppUserLogin>();
        var identityBuilder = new IdentityBuilder(builder.UserType, builder.Services);
        identityBuilder.AddRoles<ApplicationRole>();
        identityBuilder.AddEntityFrameworkStores<DataContext>();
        identityBuilder.AddSignInManager<SignInManager<AppUserLogin>>();
        identityBuilder.AddDefaultTokenProviders();

        var tokenKeyBase64 = config["TokenKey"];
        if (string.IsNullOrWhiteSpace(tokenKeyBase64))
        {
            throw new InvalidOperationException("TokenKey is missing in configuration.");
        }

        // Decode the Base64 string to bytes
        var keyBytes = Convert.FromBase64String(tokenKeyBase64);
        if (keyBytes.Length < 64)
        {
            throw new InvalidOperationException("TokenKey must be at least 64 bytes (512 bits) for HMAC-SHA512.");
        }

        var key = new SymmetricSecurityKey(keyBytes);


        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(opt =>
            {
                opt.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = key,
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ClockSkew = TimeSpan.Zero
                };
                opt.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var accessToken = context.Request.Query["access_token"];
                        var path = context.HttpContext.Request.Path;
                        if (!string.IsNullOrEmpty(accessToken) && path.StartsWithSegments("/chat"))
                            context.Token = accessToken;
                        return Task.CompletedTask;
                    }
                };
            });

        services.AddAuthorization(opt =>
        {
            /*opt.AddPolicy("IsActivityHost", policy =>
            {
                policy.Requirements.Add(new IsHostRequirement());
            });*/
        });
        //services.AddTransient<IAuthorizationHandler, IsHostRequirementHandler>();
        services.AddScoped<TokenService>();

        return services;
    }
}