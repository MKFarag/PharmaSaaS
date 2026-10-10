using Asp.Versioning;
using Hangfire;
using Mapster;
using MapsterMapper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.IdentityModel.Tokens;
using PharmaSaaS.Infrastructure;
using PharmaSaaS.Infrastructure.Authentication;
using PharmaSaaS.Infrastructure.Health;
using PharmaSaaS.Infrastructure.Persistence;
using PharmaSaaS.Infrastructure.Persistence.Identities;
using PharmaSaaS.Infrastructure.Services;
using PharmaSaaS.Presentation.Abstraction;
using PharmaSaaS.Presentation.SwaggerProcessors;
using SharpGrip.FluentValidation.AutoValidation.Mvc.Extensions;
using System.Reflection;
using System.Text;
using System.Threading.RateLimiting;

namespace PharmaSaaS.Presentation;

public static class DependencyInjection
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddDependencies(IConfiguration configuration)
        {
            services.AddControllers();
            services.AddInfrastructureDependencies(configuration);
            services.AddCORSConfig(configuration);
            services.AddAuthConfig(configuration);
            services.AddFluentValidationConfig();
            services.AddMapsterConfig();
            services.AddMailSettings(configuration);
            services.AddHangfireConfig(configuration);
            services.AddVersioningConfig();
            services.AddRateLimiterConfig(configuration);

            services.AddScoped<ICacheService, CacheService>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IJobManager, JobManager>();
            services.AddScoped<INotificationService, NotificationService>();
            services.AddScoped<IRecurringJobService, RecurringJobService>();
            services.AddScoped<ISignInService, SignInService>();
            services.AddScoped<IUrlEncoder, UrlEncoder>();
            services.AddScoped<IUnitOfWork, UnitOfWork>();

            services.AddScoped<PharmaSaaS.Application.Interfaces.Services.IInvoiceService, PharmaSaaS.Application.Services.InvoiceService>();
            services.AddScoped<PharmaSaaS.Application.Interfaces.Services.IPurchaseOrderService, PharmaSaaS.Application.Services.PurchaseOrderService>();
            services.AddScoped<PharmaSaaS.Application.Interfaces.Services.ISupplierPaymentService, PharmaSaaS.Application.Services.SupplierPaymentService>();
            services.AddScoped<PharmaSaaS.Application.Interfaces.Services.IReportService, PharmaSaaS.Application.Services.ReportService>();

            services.AddHealthChecks()
                .AddSqlServer(connectionString: configuration.GetConnectionString("SharedDb")!, name: "Database")
                .AddHangfire(options => { options.MinimumAvailableServers = 1; }, name: "Hangfire")
                .AddCheck<MailProviderHealthCheck>(name: "Mail service");

            services
                .AddEndpointsApiExplorer()
                .AddNSwagConfig();

            services
                .AddExceptionHandler<GlobalExceptionHandler>()
                .AddProblemDetails();

            return services;
        }

        private IServiceCollection AddCORSConfig(IConfiguration configuration)
        {
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder =>
                {
                    builder
                        .AllowAnyMethod()
                        .AllowAnyHeader()
                        .WithOrigins(configuration.GetSection("AllowedOrigins").Get<string[]>()!);
                });
            });

            return services;
        }

        private IServiceCollection AddAuthConfig(IConfiguration configuration)
        {
            #region Jwt

            services.AddSingleton<IJwtProvider, JwtProvider>();

            services.AddOptions<JwtOptions>()
                .BindConfiguration(JwtOptions.SectionName)
                .ValidateDataAnnotations()
                .ValidateOnStart();

            #endregion

            #region Add Identity

            services
                .AddIdentity<ApplicationUser, ApplicationRole>(options =>
                {
                    options.Lockout.AllowedForNewUsers = true;
                    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
                    options.Lockout.MaxFailedAccessAttempts = 5;
                })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            #region Configurations

            services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 8;
                options.SignIn.RequireConfirmedEmail = true;
                options.User.RequireUniqueEmail = true;
            });

            services.Configure<DataProtectionTokenProviderOptions>(options =>
            {
                options.TokenLifespan = TimeSpan.FromHours(24);
            });

            #endregion

            #endregion

            #region JWT Authentication

            var settings = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();

            services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                })
                .AddJwtBearer(o =>
                {
                    o.SaveToken = true;
                    o.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuerSigningKey = true,
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings?.Key!)),
                        ValidIssuer = settings?.Issuer,
                        ValidAudience = settings?.Audience
                    };
                });

            #endregion

            #region Permission based authentication

            services.AddTransient<IAuthorizationHandler, PermissionAuthorizationHandler>();
            services.AddTransient<IAuthorizationPolicyProvider, PermissionAuthorizationPolicyProvider>();

            #endregion

            return services;
        }

        private IServiceCollection AddFluentValidationConfig()
        {
            services
                .AddFluentValidationAutoValidation()
                .AddValidatorsFromAssembly(Assembly.GetExecutingAssembly())
                .AddValidatorsFromAssembly(typeof(INotificationService).Assembly);

            return services;
        }

        private IServiceCollection AddMapsterConfig()
        {
            var mappingConfig = TypeAdapterConfig.GlobalSettings;
            mappingConfig.Scan(
                Assembly.GetExecutingAssembly(),
                typeof(INotificationService).Assembly);

            services.AddSingleton<IMapper>(new Mapper(mappingConfig));

            return services;
        }

        private IServiceCollection AddVersioningConfig()
        {
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1.0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;

                options.ApiVersionReader = new HeaderApiVersionReader("x-api-version");
            }
            ).AddApiExplorer(options =>
            {
                options.GroupNameFormat = "'v'V";
                options.SubstituteApiVersionInUrl = true;
            });

            return services;
        }

        private IServiceCollection AddMailSettings(IConfiguration configuration)
        {
            services.AddOptions<EmailTemplateOptions>()
                .Bind(configuration.GetSection(nameof(EmailTemplateOptions)));

            services.AddOptions<AppUrlSettings>()
                .Bind(configuration.GetSection(AppUrlSettings.SectionName));

            services.AddOptions<MailSettings>()
                .Bind(configuration.GetSection(nameof(MailSettings)))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            return services;
        }

        private IServiceCollection AddHangfireConfig(IConfiguration configuration)
        {
            services.AddHangfire(config => config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(configuration.GetConnectionString("HangfireConnection"))
            );

            services.AddHangfireServer();

            return services;
        }

        private IServiceCollection AddRateLimiterConfig(IConfiguration configuration)
        {

            services.AddOptions<RateLimitingOptions>()
                .BindConfiguration(nameof(RateLimitingOptions))
                .ValidateDataAnnotations()
                .ValidateOnStart();

            var settings = configuration.GetSection(nameof(RateLimitingOptions)).Get<RateLimitingOptions>()
                ?? throw new InvalidOperationException("Failed to bind RateLimitingOptions from configuration.");

            services.AddRateLimiter(rateLimiterOptions =>
            {
                rateLimiterOptions.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                #region IpLimit

                rateLimiterOptions.AddPolicy(RateLimitingOptions.PolicyNames.IpLimit, httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.Connection.RemoteIpAddress?.ToString(),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = settings.IpPolicy.PermitLimit,
                            Window = TimeSpan.FromSeconds(settings.IpPolicy.WindowInSeconds)
                        }
                    )
                );

                #endregion

                #region UserLimit

                rateLimiterOptions.AddPolicy(RateLimitingOptions.PolicyNames.UserLimit, httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        partitionKey: httpContext.User.GetId(),
                        factory: _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = settings.UserPolicy.PermitLimit,
                            Window = TimeSpan.FromSeconds(settings.UserPolicy.WindowInSeconds)
                        }
                    )
                );

                #endregion

                #region Concurrency

                rateLimiterOptions.AddConcurrencyLimiter(RateLimitingOptions.PolicyNames.Concurrency, options =>
                {
                    options.QueueLimit = settings.Concurrency.QueueLimit;
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;

                    options.PermitLimit = settings.Concurrency.PermitLimit;
                });

                #endregion

                #region SlidingWindow

                rateLimiterOptions.AddSlidingWindowLimiter(RateLimitingOptions.PolicyNames.Sliding, options =>
                {
                    options.QueueLimit = settings.SlidingWindow.QueueLimit;
                    options.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;

                    options.PermitLimit = settings.SlidingWindow.PermitLimit;
                    options.Window = TimeSpan.FromSeconds(settings.SlidingWindow.WindowInSeconds);

                    options.SegmentsPerWindow = settings.SlidingWindow.SegmentsPerWindow;
                });

                #endregion

            });

            return services;
        }

        private IServiceCollection AddNSwagConfig()
        {
            var serviceProvider = services.BuildServiceProvider();
            var apiVersionDescriptionProvider = serviceProvider.GetRequiredService<IApiVersionDescriptionProvider>();

            foreach (var description in apiVersionDescriptionProvider.ApiVersionDescriptions)
            {
                services.AddOpenApiDocument(options =>
                {
                    options.ConfigureDocument(description);
                    options.AddBearerSecurity();
                    options.OperationProcessors.Add(new ApiVersionHeaderOperationProcessor(description));
                    options.OperationProcessors.Add(new TenantHeaderOperationProcessor());
                    options.OperationProcessors.Add(new IdempotencyHeaderOperationProcessor());
                });
            }

            return services;
        }
    }
}
