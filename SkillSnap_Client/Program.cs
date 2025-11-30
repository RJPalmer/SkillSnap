using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
// using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Json;
using SkillSnap_Client;
using SkillSnap_Client.Services;
using Microsoft.AspNetCore.Components.Authorization;
using SkillSnap_Client.Services.Authentication;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure typed HttpClient with auth handler
builder.Services.AddHttpClient<ISkillSnapApiClient, SkillSnapApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["API:BaseAddress"] ?? "https://localhost:7271/");
})
.AddHttpMessageHandler<AuthTokenHandler>();



// // Register authorization handler
// builder.Services.AddScoped<BaseAddressAuthorizationMessageHandler>();

// // Configure MSAL authentication
// builder.Services.AddMsalAuthentication(options =>
// {
//     var authentication = builder.Configuration.GetSection("AzureAd");
//     builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
//     options.ProviderOptions.DefaultAccessTokenScopes.Add($"{authentication["ClientId"]}/.default");
// });

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<ApiAuthenticationStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(sp => 
    sp.GetRequiredService<ApiAuthenticationStateProvider>());

builder.Services.AddTransient<AuthTokenHandler>();
builder.Services.AddHttpClient("SkillSnapAPI", client =>
{
    client.BaseAddress = new Uri("https://localhost:7271");
})
.AddHttpMessageHandler<AuthTokenHandler>();

builder.Services.AddScoped(sp =>
    sp.GetRequiredService<IHttpClientFactory>().CreateClient("SkillSnapAPI"));

//
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<UserContext>();

await builder.Build().RunAsync();
