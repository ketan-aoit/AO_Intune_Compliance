using AOIntuneAlerts.Application.Common.Interfaces;
using AOIntuneAlerts.Infrastructure.Persistence;
using AOIntuneAlerts.Infrastructure.Services;
using AOIntuneAlerts.Infrastructure.Services.Compliance;
using AOIntuneAlerts.Infrastructure.Services.Email;
using AOIntuneAlerts.Infrastructure.Services.Graph;
using Azure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Graph;

namespace AOIntuneAlerts.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
            options.UseSqlServer(connectionString, builder =>
                builder.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName)));

        services.AddScoped<IApplicationDbContext>(provider =>
            provider.GetRequiredService<ApplicationDbContext>());

        // Graph API - supports both "GraphApi" and "AzureAd:GraphApi" configuration sections
        services.AddSingleton(sp =>
        {
            // Try GraphApi section first, then fall back to AzureAd:GraphApi
            var graphConfig = configuration.GetSection("GraphApi");
            if (!graphConfig.Exists())
                graphConfig = configuration.GetSection("AzureAd:GraphApi");

            var tenantId = graphConfig["TenantId"] ?? configuration["AzureAd:TenantId"];
            var clientId = graphConfig["ClientId"];
            var clientSecret = graphConfig["ClientSecret"];

            if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
            {
                throw new InvalidOperationException(
                    "Graph API configuration is missing. Please configure GraphApi:ClientId and GraphApi:ClientSecret.");
            }

            var options = new ClientSecretCredentialOptions
            {
                AuthorityHost = AzureAuthorityHosts.AzurePublicCloud
            };

            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret, options);
            var scopes = new[] { "https://graph.microsoft.com/.default" };

            return new GraphServiceClient(credential, scopes);
        });

        // Services
        services.AddScoped<IIntuneGraphService, IntuneGraphService>();
        services.AddScoped<IEmailService, EmailGraphService>();
        services.AddScoped<IComplianceEvaluator, ComplianceEvaluator>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Email options
        services.Configure<EmailOptions>(configuration.GetSection(EmailOptions.SectionName));

        return services;
    }
}
