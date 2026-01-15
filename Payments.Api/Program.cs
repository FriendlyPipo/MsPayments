using Payments.Api;
using Hangfire;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddPresentation(builder.Configuration);

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.OAuthClientId(builder.Configuration["Keycloak:ClientId"]);
        c.OAuthClientSecret(builder.Configuration["Keycloak:ClientSecret"]);
        c.OAuthRealm(builder.Configuration["Keycloak:Realm"]);
        c.OAuthAppName("Payments API");
    });
}

app.UseCors("AllowAll");

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

// Hangfire
app.UseHangfireDashboard();

RecurringJob.AddOrUpdate<Payments.Infrastructure.Jobs.PaymentReconciliationJob>(
    "payment-reconciliation",
    job => job.ReconcileAsync(),
    "*/5 * * * *");

app.MapControllers();

app.Run();
