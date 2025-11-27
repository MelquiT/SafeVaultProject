using BlazorClient;
using BlazorClient.Services;
//using Blazored.LocalStorage;
using Blazored.SessionStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

//builder.Services.AddBlazoredLocalStorage();
builder.Services.AddBlazoredSessionStorage();

// 2. Registrar el Provider de Autenticación
// Usamos el CustomAuthStateProvider que es el estándar para JWT.
builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthenticationStateProvider, CustomAuthStateProvider>();

// 3. Registrar el handler del Token (Esto es CRUCIAL)
builder.Services.AddScoped<TokenHandler>();

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// 4. Configurar HttpClient para usar el TokenHandler
builder.Services.AddHttpClient("ServerApi", client => client.BaseAddress = new Uri("http://localhost:5035/"))
    .AddHttpMessageHandler<TokenHandler>(); // <--- Usa el handler aquí

// 5. Registrar el HttpClient configurado
builder.Services.AddScoped(sp => sp.GetRequiredService<IHttpClientFactory>()
    .CreateClient("ServerApi"));

await builder.Build().RunAsync();
