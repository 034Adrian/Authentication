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
    /// <summary>
    /// Agrega los servicios a la api.
    /// </summary>
    /// <param name="services">Los servicios de la aplicacion.</param>
    /// <returns>A scoped services.</returns>
    public static IServiceCollection AddApiServices(this IServiceCollection services)
    {
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }

    /// <summary>
    /// Selecciona el servicio de base de datos.
    /// </summary>
    /// <param name="services">Los servicios de la aplicacion.</param>
    /// <param name="configuration">Las configuraciones de la aplicacion.</param>
    /// <returns>Conexion a la base de datos.</returns>
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

    /// <summary>
    /// Configuracion de identity.
    /// </summary>
    /// <param name="services">Los servicios de la aplicacion.</param>
    public static void ConfigureIdentity(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
        }).AddEntityFrameworkStores<ApplicationDbContext>();
    }

    /// <summary>
    /// Configuracion de Authentication.
    /// </summary>
    /// <param name="services">Los servicios de la aplicacion.</param>
    /// <param name="configuration">Las configuraciones de la aplicacion.</param>
    /// <returns>Define la configuracion de Authentication.</returns>
    public static void ConfigureAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var issuer = configuration["JWT:Issuer"];
        var audience = configuration["JWT:Audience"];
        var signinKey = configuration["JWT:SigninKey"];

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
                ValidIssuer = issuer,
                ValidateAudience = true,
                ValidAudience = audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(signinKey)
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
