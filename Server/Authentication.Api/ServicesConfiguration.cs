using Authentication.Api.Data;
using Authentication.Api.Services.Identity.Implement;
using Authentication.Api.Services.Identity.Interface;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Authentication.Api;

public static class ServicesConfiguration
{
    // Global services
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }

    public static void ConfigureDatabase(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        if (environment.IsDevelopment())
        {
            ServicesConfiguration.ConfigureDbServices(configuration, services);
        }
        else
        {
            ServicesConfiguration.ConfigureDockerDbService(configuration, services);
        }
    }

    public static void ConfigureIdentity(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
        }).AddEntityFrameworkStores<ApplicationDbContext>();
    }

    public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(option =>
        {
            option.DefaultAuthenticateScheme =
            option.DefaultChallengeScheme =
            option.DefaultSignOutScheme =
            option.DefaultForbidScheme =
            option.DefaultSignInScheme =
            option.DefaultScheme =
            JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(option =>
        {
            option.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidIssuer = configuration["JWT:Issuer"],
                ValidateAudience = true,
                ValidAudience = configuration["JWT:Audience"],
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(configuration["JWT:SigninKey"])
                )
            };
        });
    }

    // Db services
    public static void ConfigureDbServices(IConfiguration configuration, IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
                        options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"))
                        .ConfigureWarnings(warnings => warnings
                        .Ignore(RelationalEventId.PendingModelChangesWarning)));
    }
    public static void ConfigureDockerDbService(IConfiguration configuration, IServiceCollection services)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DockerConnection");
            var password = Environment.GetEnvironmentVariable("MSSQL_SA_PASSWORD");
            options.UseSqlServer(connectionString)
                .ConfigureWarnings(warnings => warnings
                .Ignore(RelationalEventId.PendingModelChangesWarning));
        });
    }
}
