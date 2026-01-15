using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Payments.Api
{
    [ExcludeFromCodeCoverage]
    public static class AuthConfiguration
    {
        public static IServiceCollection KeycloakConfiguration(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.Audience = configuration["Authentication:Audience"];
                    options.MetadataAddress = configuration.GetValue<string>("Authentication:MetadataAddress");
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidIssuer = configuration["Authentication:ValidIssuer"],
                        ValidateAudience = true,
                        ValidAudiences = new[] { "account", "realm-management" },
                        ValidateLifetime = true,
                        ClockSkew = TimeSpan.Zero
                    };

                    options.Events = new JwtBearerEvents
                    {
                        OnMessageReceived = context =>
                        {
                            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Replace("Bearer ", "").Trim();
                            return Task.CompletedTask;
                        },
                        OnAuthenticationFailed = context =>
                        {
                            return Task.CompletedTask;
                        }
                    };
                });

            services.AddAuthorization(o =>
            {
                o.AddPolicy("AdministradorPolicy", policy =>
                    policy.RequireAuthenticatedUser()
                        .RequireAssertion(context =>
                        {
                            var resourceAccess = context.User.FindFirst("resource_access")?.Value;
                            if (string.IsNullOrEmpty(resourceAccess)) return false;

                            var resourceAccessJson = JsonDocument.Parse(resourceAccess);
                            return resourceAccessJson.RootElement.TryGetProperty("admin-client", out var clientAccess) &&
                                clientAccess.GetProperty("roles").EnumerateArray()
                                    .Any(role => role.GetString() == "Administrator");
                        }));
            });

            services.AddHttpContextAccessor();

            return services;
        }
    }
}
