using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Payments.Core.Repositories;
using Payments.Core.Services;
using Payments.Domain.Entities;
using Payments.Infrastructure.Database;
using Payments.Infrastructure.Repositories;
using Payments.Infrastructure.Services;
using Payments.Core.RabbitMQ;
using Payments.Infrastructure.RabbitMQ.Connection;
using Payments.Infrastructure.RabbitMQ.Producer;
using Payments.Infrastructure.RabbitMQ.Consumer;
using RabbitMQ.Client;

namespace Payments.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHttpClient();
            var connectionString = configuration.GetConnectionString("PostgresSQLConnection");
            
            services.AddDbContextFactory<PaymentDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.AddScoped<PaymentDbContext>(provider =>
                provider.GetRequiredService<IDbContextFactory<PaymentDbContext>>().CreateDbContext());

            services.AddScoped<IPaymentRepository, PaymentRepository>();
            services.AddScoped<IInvoiceRepository, InvoiceRepository>();
            services.AddScoped<IPdfService<Invoice>, InvoicePdfService>();
            services.AddScoped<IStripeService, StripeService>();
            services.AddTransient<ITokenService, TokenService>();
            services.AddTransient<IUserAuditService, UserAuditService>();
            services.AddTransient<IUserLogService, UserLogService>();

            // RabbitMQ
            services.AddSingleton<IConnectionFactory>(_ =>
            {
                var rabbitMqSection = configuration.GetSection("RabbitMQ");
                return new ConnectionFactory
                {
                    HostName = rabbitMqSection["HostName"],
                    Port = int.TryParse(rabbitMqSection["Port"], out var p) ? p : 5672,
                    UserName = rabbitMqSection["UserName"],
                    Password = rabbitMqSection["Password"]
                };
            });

            services.AddSingleton<IConnectionRabbitMQ>(provider =>
            {
                var connectionFactory = provider.GetRequiredService<IConnectionFactory>();
                var rabbitMQConnection = new RabbitMQConnection(connectionFactory);
                rabbitMQConnection.InitializeAsync().GetAwaiter().GetResult();
                return rabbitMQConnection;
            });

            services.AddScoped(typeof(IEventBus<>), typeof(RabbitMQProducer<>));

            // Consumers
            services.AddSingleton<CreateBookingConsumer>();
            services.AddSingleton<UpdateBookingStatusConsumer>();

            services.AddSingleton<IRabbitMQConsumer>(sp => new CompositeRabbitMQConsumer(new IRabbitMQConsumer[]
            {
                sp.GetRequiredService<CreateBookingConsumer>(),
                sp.GetRequiredService<UpdateBookingStatusConsumer>()
            }));

            services.AddHostedService<RabbitMQBackgroundService>();

            // Jobs
            services.AddScoped<Jobs.PaymentReconciliationJob>();

            return services;
        }
    }
}
