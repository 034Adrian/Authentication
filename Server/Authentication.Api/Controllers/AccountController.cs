using Authentication.Api.Data;
using Authentication.Api.DataTransferObjects.Identity;
using Authentication.Api.Services.Identity.Interface;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Authentication.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ITokenService _tokenService;
    private readonly SignInManager<ApplicationUser> _signInManager;

    public AccountController(UserManager<ApplicationUser> userManager, ITokenService tokenService, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _tokenService = tokenService;
        _signInManager = signInManager;
    }

    [HttpPost]
    [Route("login")]
    public async Task<ActionResult<NewUserDto>> Login(LoginDto login)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.Users.FirstOrDefaultAsync(x => x.UserName == login.UserName.ToLower());
        if (user == null) return Unauthorized("Invalid Username!");

        var result = await _signInManager.CheckPasswordSignInAsync(user, login.Password, false);
        if (!result.Succeeded) return Unauthorized("Username not found / password is incorrect");

        var newUserDto = new NewUserDto
        {
            UserName = user.UserName,
            Email = user.Email,
            Token = _tokenService.CreateToken(user)
        };

        return Ok(newUserDto);
    }

    [HttpPost]
    [Route("register")]
    public async Task<ActionResult<NewUserDto>> Register([FromBody] RegisterDto model)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = new ApplicationUser
        {
            UserName = model.Username,
            Email = model.Email,
        };

        var createdUser = await _userManager.CreateAsync(user, model.Password);

        if (!createdUser.Succeeded)
            return StatusCode(500, createdUser.Errors);

        var roleResult = await _userManager.AddToRoleAsync(user, "User");
        if (!roleResult.Succeeded)
            return StatusCode(500, roleResult.Errors);

        var newUserDto = new NewUserDto
        {
            UserName = user.UserName,
            Email = user.Email,
            Token = _tokenService.CreateToken(user)
        };

        return Ok(newUserDto);
    }

    [HttpPost]
    [Route("logout")]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return Ok(new { message = "Logout successful" });
    }
}
