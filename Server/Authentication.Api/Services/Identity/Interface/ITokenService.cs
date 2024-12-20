using Authentication.Api.Data;

namespace Authentication.Api.Services.Identity.Interface;

public interface ITokenService
{
    /// <summary>
    /// Creates a JWT token for the specified user.
    /// </summary>
    /// <param name="user">The user for whom the token is being created.</param>
    /// <returns>A JWT token as a string.</returns>
    string CreateToken(ApplicationUser user);
}
