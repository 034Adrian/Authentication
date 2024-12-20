using Authentication.Api.Data;

namespace Authentication.Api.Services.Identity.Interface;

public interface ITokenService
{
    string CreateToken(ApplicationUser user);
}
