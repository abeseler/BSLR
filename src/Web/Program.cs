using Beseler.Web.Accounts.Services;
using Beseler.Web.Common;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Serilog.Core;
using Serilog.Debugging;

var builder = WebAssemblyHostBuilder.CreateDefault(args);

SelfLog.Enable(m => Console.Error.WriteLine(m));
var levelSwitch = new LoggingLevelSwitch();
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.ControlledBy(levelSwitch)
    .WriteTo.BrowserConsole()
    .CreateLogger();

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddAuthorizationCore();
builder.Services.AddScoped<AuthStateProvider>();
builder.Services.AddScoped<AuthenticationStateProvider>(p => p.GetRequiredService<AuthStateProvider>());
builder.Services.AddScoped<ApiClient>();

builder.Services.AddBlazoredLocalStorage();
builder.Services.AddCascadingAuthenticationState();

await builder.Build().RunAsync();
