using Authentication.Web.DataTransferObjects.Identity;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.JSInterop;

namespace Authentication.Web.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly AuthenticationStateProvider _authenticationStateProvider;
    private readonly IJSRuntime _jsRuntime;

    public AuthService(HttpClient httpClient, AuthenticationStateProvider authenticationStateProvider, IJSRuntime jsRuntime)
    {
        _httpClient = httpClient;
        _authenticationStateProvider = authenticationStateProvider;
        _jsRuntime = jsRuntime;
    }

    public async Task<NewUserDto> Register(RegisterDto registerDto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/account/register", registerDto);
        response.EnsureSuccessStatusCode();

        var newUser = await response.Content.ReadFromJsonAsync<NewUserDto>();
        await ((CustomAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(newUser.Token);
        return newUser;
    }

    public async Task<NewUserDto> Login(LoginDto loginDto)
    {
        var response = await _httpClient.PostAsJsonAsync("api/account/login", loginDto);
        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            throw new HttpRequestException($"Error al iniciar sesión: {response.StatusCode}, Detalles: {errorContent}");
        }

        var newUser = await response.Content.ReadFromJsonAsync<NewUserDto>();
        await ((CustomAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsAuthenticated(newUser.Token);
        return newUser;
    }

    public async Task Logout()
    {
        await _httpClient.PostAsync("api/account/logout", null);
        await ((CustomAuthenticationStateProvider)_authenticationStateProvider).MarkUserAsLoggedOut();
    }
}
