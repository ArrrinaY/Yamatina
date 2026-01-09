using System.Data.Common;
using System.Text;
using InternshipService.Auth;
using InternshipService.Auth.models;
using InternshipService.Data;
using InternshipService.Mappings;
using InternshipService.Middleware;
using InternshipService.Repositories;
using InternshipService.Services;
using InternshipService.Validators;

using FluentValidation;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Npgsql;

namespace InternshipService;

public static partial class Init
{
    public static IServiceCollection AddSwaggerConfiguration(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        var swaggerSecurityDefinition = new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Enter your JWT token below. Example: Bearer {token}"
        };
        
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.AddSecurityDefinition("Bearer", swaggerSecurityDefinition);
            options.OperationFilter<SwaggerSecurityOperationFilter>();
        });
        return services;
    }
    
    public static IServiceCollection AddHybridAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var events = new JwtBearerEvents
        {
            OnAuthenticationFailed = context =>
            {
                throw new UnauthorizedAccessException("Authentication failed");
            },
            OnChallenge = context =>
            {
                throw new UnauthorizedAccessException("Authentication failed");
            },
            OnForbidden = context =>
            {
                throw new SecurityTokenException("Authorization failed");
            }
        };

        var tokenValidationParams = new TokenValidationParameters
        {
            RoleClaimType = "role",
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = configuration["Jwt:Issuer"],
            ValidAudience = configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Jwt:Key"]!)),
        };

        Func<HttpContext,string> authTypeSelector = context =>
        {
            if (context.Request.Headers.ContainsKey("Authorization"))
                return "Jwt";
            if (context.Request.Headers.ContainsKey("X-API-KEY"))
                return "ApiKey";
            return "Jwt";
        };
        
        services
            .AddAuthentication(options => options.DefaultScheme = "Hybrid")
            .AddJwtBearer("Jwt", options =>
            {
                options.Events = events;
                options.TokenValidationParameters = tokenValidationParams;
            })
            .AddScheme<AuthenticationSchemeOptions, ApiKeyAuthScheme>("ApiKey", null)
            .AddPolicyScheme("Hybrid", "Hybrid JWT or API Key", options =>
            {
                options.ForwardDefaultSelector = authTypeSelector;
            });
        return services;
    }

    public static IServiceCollection AddHybridAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            options.AddPolicy("CanRead", policy =>
                policy.RequireClaim("permissions", Permissions.Read));

            options.AddPolicy("CanCreate", policy =>
                policy.RequireClaim("permissions", Permissions.Create));

            options.AddPolicy("CanUpdate", policy =>
                policy.RequireClaim("permissions", Permissions.Update));

            options.AddPolicy("CanDelete", policy =>
                policy.RequireClaim("permissions", Permissions.Delete));
        });
        return services;
    }

    public static IServiceCollection AddAuthHelpers(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.Configure<JWTConfiguration>(configuration.GetSection("Jwt"));
        services.Configure<ApiKeySettings>(configuration.GetSection("ApiKeySettings"));
        services.AddScoped<ICurrentUser, HttpCurrentUser>();
        services.AddScoped<IJWTHelper, JWTHelper>();
        return services;
    }
    
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options => 
        {
            options.AddDefaultPolicy(corsBuilder => 
            {
                corsBuilder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
            });
        });
        return services;
    }
    
    public static IServiceCollection AddCustomHealthChecks(
        this IServiceCollection services, 
        IConfiguration configuration
    )
    {
        services.AddHealthChecks()
            .AddNpgSql(
                configuration.GetConnectionString("Postgres")!,
                name: "postgresql",
                failureStatus: HealthStatus.Unhealthy
            )
            .AddRedis(
                configuration.GetConnectionString("Redis")!,
                name: "redis",
                failureStatus: HealthStatus.Unhealthy
            );
        return services;
    }

    public static async Task WriteHealthChecksResponse(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";
        var result = new
        {
            status = report.Status.ToString(),
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description
            })
        };
        await context.Response.WriteAsJsonAsync(result);
    }
    
    public static IServiceCollection AddValidators(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<CompanyRequestModelValidator>();
        services.AddValidatorsFromAssemblyContaining<VacancyRequestModelValidator>();
        services.AddValidatorsFromAssemblyContaining<CandidateRequestModelValidator>();
        services.AddValidatorsFromAssemblyContaining<ApplicationRequestModelValidator>();
        services.AddValidatorsFromAssemblyContaining<TagRequestModelValidator>();
        services.AddValidatorsFromAssemblyContaining<LoginRequestModelValidator>();
        return services;
    }
    
    public static IServiceCollection AddMappings(this IServiceCollection services)
    {
        services.AddAutoMapper(cfg => cfg.AddProfile<CompanyMappingProfile>());
        services.AddAutoMapper(cfg => cfg.AddProfile<VacancyMappingProfile>());
        services.AddAutoMapper(cfg => cfg.AddProfile<CandidateMappingProfile>());
        services.AddAutoMapper(cfg => cfg.AddProfile<ApplicationMappingProfile>());
        services.AddAutoMapper(cfg => cfg.AddProfile<TagMappingProfile>());
        return services;
    }
    
    public static IServiceCollection AddRedisCache(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = configuration.GetConnectionString("Redis");
            options.InstanceName = "cachingRequests:";
        });
        return services;
    }
    
    public static IServiceCollection AddDbContextPoolWithPostgres(this IServiceCollection services, string? connectionString)
    {
        services.AddDbContextPool<AppDbContext>(options =>
            options.UseNpgsql(connectionString)
        );
        services.AddScoped<DbConnection>(_ =>
            new NpgsqlConnection(connectionString)
        );
        return services;
    }
    
    public static IServiceCollection RegisterControllers(this IServiceCollection services)
    {
        services.
            AddControllers(options => { options.Filters.Add<ValidationFilter>(); })
            .ConfigureApiBehaviorOptions(options => { options.SuppressModelStateInvalidFilter = true; });
        return services;
    }
    
    public static IServiceCollection RegisterServices(this IServiceCollection services)
    {
        services.AddScoped<ICompanyService, CompanyService>();
        services.AddScoped<IVacancyService, VacancyService>();
        services.AddScoped<ICandidateService, CandidateService>();
        services.AddScoped<IApplicationService, ApplicationService>();
        services.AddScoped<ITagService, TagService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<ICacheService, CacheService>();
        return services;
    }
    
    public static IServiceCollection RegisterRepositories(this IServiceCollection services)
    {
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<ICompanyRepository, CompanyRepository>();
        services.AddScoped<IVacancyRepository, VacancyRepository>();
        services.AddScoped<ICandidateRepository, CandidateRepository>();
        services.AddScoped<IApplicationRepository, ApplicationRepository>();
        services.AddScoped<ITagRepository, TagRepository>();
        return services;
    }
}
