using AOIntuneAlerts.Application;
using AOIntuneAlerts.BackgroundJobs;
using AOIntuneAlerts.BackgroundJobs.Jobs;
using AOIntuneAlerts.Infrastructure;
using AOIntuneAlerts.Infrastructure.Persistence;
using AOIntuneAlerts.WebApi.Auth;
using Hangfire;
using Microsoft.EntityFrameworkCore;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Identity.Web;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddBackgroundJobs();

// Check if dev auth is enabled (Development environment + DevAuth:Secret configured)
var devAuthEnabled = builder.Environment.IsDevelopment() &&
                     !string.IsNullOrEmpty(builder.Configuration["DevAuth:Secret"]);

// Authentication
var authBuilder = builder.Services.AddAuthentication(options =>
{
    // Use JWT as default, but allow dev auth to work alongside
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
});

// Add dev authentication handler in development mode
if (devAuthEnabled)
{
    Console.WriteLine("WARNING: Development authentication bypass is ENABLED");
    authBuilder.AddScheme<AuthenticationSchemeOptions, DevAuthenticationHandler>(
        DevAuthenticationHandler.SchemeName, _ => { });
}

// Add Azure AD authentication
authBuilder.AddMicrosoftIdentityWebApi(options =>
    {
        builder.Configuration.Bind("AzureAd", options);
        options.TokenValidationParameters.RoleClaimType = "roles";
        options.TokenValidationParameters.NameClaimType = "name";

        // Add logging for auth events
        options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtBearer");
                logger.LogError(context.Exception, "Authentication failed: {Message}", context.Exception.Message);
                return Task.CompletedTask;
            },
            OnTokenValidated = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtBearer");
                logger.LogInformation("Token validated for: {Name}", context.Principal?.Identity?.Name);
                return Task.CompletedTask;
            },
            OnChallenge = context =>
            {
                var logger = context.HttpContext.RequestServices.GetRequiredService<ILoggerFactory>()
                    .CreateLogger("JwtBearer");
                logger.LogWarning("Auth challenge: {Error} - {ErrorDescription}", context.Error, context.ErrorDescription);
                return Task.CompletedTask;
            }
        };
    }, options => builder.Configuration.Bind("AzureAd", options));

// Authorization policies - temporarily allow any authenticated user for debugging
// TODO: Re-enable role-based authorization after debugging
var authPolicyBuilder = builder.Services.AddAuthorizationBuilder()
    .AddPolicy("Admin", policy => policy.RequireAuthenticatedUser())
    .AddPolicy("Manager", policy => policy.RequireAuthenticatedUser())
    .AddPolicy("Viewer", policy => policy.RequireAuthenticatedUser());

// Configure default policy to accept both JWT and DevAuth schemes
if (devAuthEnabled)
{
    authPolicyBuilder.SetDefaultPolicy(new Microsoft.AspNetCore.Authorization.AuthorizationPolicyBuilder(
        JwtBearerDefaults.AuthenticationScheme,
        DevAuthenticationHandler.SchemeName)
        .RequireAuthenticatedUser()
        .Build());
}

// Controllers
builder.Services.AddControllers();
builder.Services.AddHttpContextAccessor();

// Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Intune Compliance Portal API",
        Version = "v1",
        Description = "API for monitoring device compliance against Cyber Essentials requirements"
    });

    options.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.OAuth2,
        Flows = new OpenApiOAuthFlows
        {
            Implicit = new OpenApiOAuthFlow
            {
                AuthorizationUrl = new Uri($"https://login.microsoftonline.com/{builder.Configuration["AzureAd:TenantId"]}/oauth2/v2.0/authorize"),
                TokenUrl = new Uri($"https://login.microsoftonline.com/{builder.Configuration["AzureAd:TenantId"]}/oauth2/v2.0/token"),
                Scopes = new Dictionary<string, string>
                {
                    { $"api://{builder.Configuration["AzureAd:ClientId"]}/access_as_user", "Access API" }
                }
            }
        }
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "oauth2"
                }
            },
            new[] { $"api://{builder.Configuration["AzureAd:ClientId"]}/access_as_user" }
        }
    });
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowSpa", policy =>
    {
        policy.WithOrigins(builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>())
            .AllowAnyMethod()
            .AllowAnyHeader()
            .AllowCredentials();
    });
});

// Hangfire
builder.Services.AddHangfire(configuration => configuration
    .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
    .UseSimpleAssemblyNameTypeSerializer()
    .UseRecommendedSerializerSettings()
    .UseSqlServerStorage(builder.Configuration.GetConnectionString("HangfireConnection") ??
                         builder.Configuration.GetConnectionString("DefaultConnection"),
        new SqlServerStorageOptions
        {
            CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            QueuePollInterval = TimeSpan.Zero,
            UseRecommendedIsolationLevel = true,
            DisableGlobalLocks = true
        }));

builder.Services.AddHangfireServer();

// Health checks
builder.Services.AddHealthChecks()
    .AddDbContextCheck<ApplicationDbContext>();

var app = builder.Build();

// Apply pending migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    try
    {
        logger.LogInformation("Applying database migrations...");
        db.Database.Migrate();
        logger.LogInformation("Database migrations applied successfully.");
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "Error applying database migrations");
    }
}

// Configure the HTTP request pipeline.
// Enable swagger and developer exception page for debugging
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.OAuthClientId(builder.Configuration["AzureAd:ClientId"]);
});

// Add exception handling for debugging - shows detailed errors in response
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionHandler = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        if (exceptionHandler?.Error != null)
        {
            context.Response.StatusCode = 500;
            context.Response.ContentType = "application/json";
            var error = new {
                message = exceptionHandler.Error.Message,
                stackTrace = exceptionHandler.Error.StackTrace,
                innerException = exceptionHandler.Error.InnerException?.Message
            };
            await context.Response.WriteAsJsonAsync(error);
        }
    });
});

app.UseHttpsRedirection();

app.UseCors("AllowSpa");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health");

// Debug endpoint to check auth headers
app.MapGet("/debug/headers", (HttpContext context) =>
{
    var headers = context.Request.Headers
        .Select(h => new { h.Key, Value = h.Key.ToLower().Contains("auth") ? h.Value.ToString().Substring(0, Math.Min(50, h.Value.ToString().Length)) + "..." : h.Value.ToString() })
        .ToList();
    return Results.Ok(new { headers, hasAuth = context.Request.Headers.ContainsKey("Authorization") });
}).AllowAnonymous();

// Debug endpoint to check token claims
app.MapGet("/debug/claims", (HttpContext context) =>
{
    var user = context.User;
    var isAuth = user.Identity?.IsAuthenticated ?? false;
    var claims = user.Claims.Select(c => new { c.Type, c.Value }).ToList();
    var authHeader = context.Request.Headers.Authorization.ToString();
    var tokenPreview = authHeader.Length > 50 ? authHeader.Substring(0, 50) + "..." : authHeader;

    return Results.Ok(new
    {
        isAuthenticated = isAuth,
        identityName = user.Identity?.Name,
        authenticationType = user.Identity?.AuthenticationType,
        claimsCount = claims.Count,
        claims = claims.Take(10),
        tokenPreview
    });
});

// Hangfire Dashboard (Admin only in production)
app.MapHangfireDashboard("/hangfire", new DashboardOptions
{
    Authorization = app.Environment.IsDevelopment()
        ? Array.Empty<IDashboardAuthorizationFilter>()
        : new[] { new HangfireAuthorizationFilter() }
});

// Configure recurring jobs
RecurringJob.AddOrUpdate<DeviceSyncJob>(
    "device-sync",
    job => job.ExecuteAsync(),
    "0 */4 * * *"); // Every 4 hours

RecurringJob.AddOrUpdate<ComplianceEvaluationJob>(
    "compliance-evaluation",
    job => job.ExecuteAsync(),
    "0 2 * * *"); // Daily at 2 AM

RecurringJob.AddOrUpdate<AlertProcessingJob>(
    "alert-processing",
    job => job.ExecuteAsync(),
    "0 8 * * *"); // Daily at 8 AM

app.Run();

// Hangfire authorization filter
public class HangfireAuthorizationFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true &&
               httpContext.User.HasClaim("roles", "Admin");
    }
}
