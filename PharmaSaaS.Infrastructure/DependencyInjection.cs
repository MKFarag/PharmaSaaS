using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PharmaSaaS.Application.Interfaces.Services;
using PharmaSaaS.Application.Services;
using PharmaSaaS.Infrastructure.Factories;
using PharmaSaaS.Infrastructure.Messaging;
using PharmaSaaS.Infrastructure.Services;

namespace PharmaSaaS.Infrastructure;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddInfrastructureDependencies(IConfiguration configuration)
        {
            services.AddHttpContextAccessor();

            services.AddTenantDbConfig();

            services.AddSharedDbConfig(configuration);
            services.AddScoped<ICustomerLoyaltyService, CustomerLoyaltyService>();
            services.AddScoped<IReturnService, ReturnService>();
            services.AddScoped<IInventoryFilterService, InventoryFilterService>();
            services.AddScoped<IAIService, AIService>();

            services.AddOptions<OpenRouterSettings>().BindConfiguration(nameof(OpenRouterSettings));
            services.AddHttpClient("OpenRouter", client => client.BaseAddress = new Uri("https://openrouter.ai/"));

            services.AddHybridCache();

            services.AddStackExchangeRedisCache(options => options.Configuration = configuration.GetConnectionString("Redis"));

            services.AddCQRSConfig(typeof(INotificationService).Assembly);

            return services;
        }

        private IServiceCollection AddSharedDbConfig(IConfiguration configuration)
        {
            var connectionString = configuration.GetConnectionString("SharedDb") ??
                throw new InvalidOperationException("SharedDb connection string is not found in appsettings.json");

            services.AddDbContext<SharedDbContext>(options => options.UseSqlServer(connectionString));

            return services;
        }

        private IServiceCollection AddTenantDbConfig()
        {
            services.AddOptions<TenantSettings>().BindConfiguration(nameof(TenantSettings));

            services.AddScoped<TenantProvisioningService>();
            services.AddScoped<ICurrentTenantGetter, TenantService>();
            services.AddScoped<ITenantService, TenantService>();


            services.AddDbContext<ApplicationDbContext>();

            return services;
        }

        private IServiceCollection AddCQRSConfig(params Assembly[] assemblies)
        {
            services.AddScoped<ISender, Sender>();

            if (assemblies.Length == 0)
                assemblies = [Assembly.GetCallingAssembly()];

            foreach (var assembly in assemblies)
            {
                var handlers = assembly.GetTypes()
                    .Where(t => t is { IsClass: true, IsAbstract: false, IsGenericType: false })
                    .SelectMany(t => t.GetInterfaces()
                        .Where(i => i.IsGenericType &&
                            (i.GetGenericTypeDefinition() == typeof(IRequestHandler<,>) ||
                                i.GetGenericTypeDefinition() == typeof(IRequestHandler<>)))
                        .Select(i => (Interface: i, Implementation: t)));

                foreach (var (iface, impl) in handlers)
                    services.AddScoped(iface, impl);
            }

            return services;
        }
    }
}
