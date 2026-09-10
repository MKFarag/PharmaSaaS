using Hangfire;
using HangfireBasicAuthenticationFilter;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using PharmaSaaS.Domain.Entities;
using PharmaSaaS.Infrastructure.Factories;
using PharmaSaaS.Presentation;
using PharmaSaaS.Presentation.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDependencies(builder.Configuration);

builder.Host.UseSerilog((context, configuration) => configuration.ReadFrom.Configuration(context.Configuration));

var app = builder.Build();

using var scope = app.Services.CreateScope();
var provisioningService = scope.ServiceProvider.GetRequiredService<TenantProvisioningService>();
await provisioningService.ProvisionAllTenantsAsync();

app.UseExceptionHandler();

if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();

    app.UseSwaggerUi(options =>
    {
        options.Path = "/swagger";
    });

    app.UseReDoc(options =>
    {
        options.Path = "/redoc";
        options.DocumentPath = "/swagger/v1/swagger.json";
    });
}

app.UseSerilogRequestLogging();

app.UseHttpsRedirection();

app.MapHangfireDashboard("/jobs", new DashboardOptions
{
    Authorization =
    [
        new HangfireCustomBasicAuthenticationFilter
        {
            User = app.Configuration.GetValue<string>("Hangfire:Username"),
            Pass = app.Configuration.GetValue<string>("Hangfire:Password")
        }
    ],
    DashboardTitle = "Hangfire Dashboard",
    IsReadOnlyFunc = context => true
});

RecurringJob.AddOrUpdate<IRecurringJobService>(
    "RemoveExpiredRefreshTokens",
    x => x.RemoveExpiredRefreshTokenAsync(),
    Cron.DayInterval(RefreshToken.AutoRemoveAfterDays));

app.UseCors();

app.UseAuthentication();

app.UseAuthorization();

app.UseMiddleware<TenantSubscriptionMiddleware>();

app.UseRateLimiter();

app.MapControllers();

app.MapHealthChecks("health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
