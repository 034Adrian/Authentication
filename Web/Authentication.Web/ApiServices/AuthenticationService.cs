using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace Authentication.Web.ApiServices;

public class AuthenticationService
{
    private readonly HttpClient _httpClient;

    public AuthenticationService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri("https://localhost:7258");
    }

    public async Task<bool> Login(string username, string password)
    {
        var loginModel = new { userName = username, password = password };
        var response = await _httpClient.PostAsJsonAsync("api/account/login", loginModel);

        return response.IsSuccessStatusCode;
    }

    public async Task Logout()
    {
        await _httpClient.PostAsync("api/account/logout", null);
    }
}
