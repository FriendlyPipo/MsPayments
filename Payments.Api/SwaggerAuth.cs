using Microsoft.OpenApi.Models;
using System.Diagnostics.CodeAnalysis;
using System.Collections.Generic;
using System;

namespace Payments.Api
{
    [ExcludeFromCodeCoverage]
    internal static class SwaggerAuth
    {
        internal static IServiceCollection AddSwaggerGenWithAuth(
            this IServiceCollection services,
            IConfiguration configuration)
        {
            services.AddSwaggerGen(o =>
            {
                o.CustomSchemaIds(id => id.FullName!.Replace("+", "-"));

                o.AddSecurityDefinition("Keycloak", new OpenApiSecurityScheme
                {
                    Type = SecuritySchemeType.OAuth2,
                    Flows = new OpenApiOAuthFlows
                    {
                        Implicit = new OpenApiOAuthFlow
                        {
                            AuthorizationUrl = new Uri($"{configuration["Keycloak:BaseUrl"]}/realms/{configuration["Keycloak:Realm"]}/protocol/openid-connect/auth"),
                            Scopes = new Dictionary<string, string>{
                                {"openid", "openid"},
                                {"profile", "profile"}
                            }
                        }
                    }
                });

                var securityRequirement = new OpenApiSecurityRequirement{
                    {
                        new OpenApiSecurityScheme{
                            Reference = new OpenApiReference{
                                Type = ReferenceType.SecurityScheme,
                                Id = "Keycloak"
                            },
                            In = ParameterLocation.Header,
                            Name = "Bearer",
                            Scheme = "Bearer"
                        },
                        new string[] {}
                    }
                };

                o.AddSecurityRequirement(securityRequirement);
            });

            return services;
        }
    }
}
