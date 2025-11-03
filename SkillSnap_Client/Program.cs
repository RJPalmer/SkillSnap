using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Authentication;
using System.Net.Http.Json;
using SkillSnap_Client;
using SkillSnap_Client.Services;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Configure typed HttpClient with auth handler
builder.Services.AddHttpClient<ISkillSnapApiClient, SkillSnapApiClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["API:BaseAddress"] ?? builder.HostEnvironment.BaseAddress);
}).AddHttpMessageHandler<BaseAddressAuthorizationMessageHandler>();

// Register authorization handler
builder.Services.AddScoped<BaseAddressAuthorizationMessageHandler>();

// Configure MSAL authentication
builder.Services.AddMsalAuthentication(options =>
{
    var authentication = builder.Configuration.GetSection("AzureAd");
    builder.Configuration.Bind("AzureAd", options.ProviderOptions.Authentication);
    options.ProviderOptions.DefaultAccessTokenScopes.Add($"{authentication["ClientId"]}/.default");
});

await builder.Build().RunAsync();
