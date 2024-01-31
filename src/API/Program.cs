using Asp.Versioning;
using Beseler.API;
using Beseler.API.Accounts;
using Beseler.API.Application;
using Beseler.API.Swagger;
using Beseler.Domain;
using Beseler.Infrastructure;

var app = WebApplication.CreateBuilder(args)
    .AddAPIServices()
    .AddDomainServices()
    .AddInfrastructure()
    .Build();

if (app.Environment.IsDevelopment())
    app.UseWebAssemblyDebugging();
else
    app.UseHsts();

app.UseStaticFiles();
app.UseExceptionHandler(app =>
    app.Run(async context => await TypedResults.Problem().ExecuteAsync(context)));

app.UseInfrastructure();
app.UseSerilogRequestLogging();
app.UseAntiforgery();

app.UseAuthentication();
app.UseAuthorization();

var versions = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .ReportApiVersions()
    .Build();

app.MapApplicationEndpoints(versions)
    .MapAccountEndpoints(versions)
    .MapWeatherEndpoints(versions)
    .MapSwaggerUI();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Beseler.Web._Imports).Assembly);

app.Run();
