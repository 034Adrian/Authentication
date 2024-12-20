using Authentication.Api.Data;
using Authentication.Api.Services.Identity.Interface;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Authentication.Api.Services.Identity.Implement;

public class TokenService : ITokenService
{
    private readonly IConfiguration _config;
    private readonly SymmetricSecurityKey _key;

    private const string SigninKeyConfig = "JWT:SigninKey";
    private const string IssuerConfig = "JWT:Issuer";
    private const string AudienceConfig = "JWT:Audience";

    public TokenService(IConfiguration config)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config));

        var signinKey = _config[SigninKeyConfig];
        if (string.IsNullOrEmpty(signinKey))
        {
            throw new ArgumentException($"Configuration '{SigninKeyConfig}' is missing or empty.");
        }

        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(signinKey));
    }

    /// <summary>
    /// Creates a JWT token for the specified user.
    /// </summary>
    /// <param name="user">The user for whom the token is being created.</param>
    /// <returns>A JWT token as a string.</returns>
    public string CreateToken(ApplicationUser user)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));

        var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.GivenName, user.UserName),
            };

        var creds = new SigningCredentials(_key, SecurityAlgorithms.HmacSha512Signature);
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.Now.AddDays(1),
            SigningCredentials = creds,
            Issuer = _config[IssuerConfig],
            Audience = _config[AudienceConfig]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        try
        {
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }
        catch (Exception ex)
        {
            // Log the exception (logging mechanism not shown here)
            throw new InvalidOperationException("Error creating JWT token.", ex);
        }
    }
}

