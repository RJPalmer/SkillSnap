using Microsoft.JSInterop;

public class TokenService : ITokenService
{
    private readonly IJSRuntime _js;

    public TokenService(IJSRuntime js)
    {
        _js = js;
    }

    public async Task SaveTokenAsync(string token) =>
        await _js.InvokeVoidAsync("localStorage.setItem", "authToken", token);

    public async Task<string?> GetTokenAsync() =>
        await _js.InvokeAsync<string>("localStorage.getItem", "authToken");

    public async Task RemoveTokenAsync() =>
        await _js.InvokeVoidAsync("localStorage.removeItem", "authToken");
}