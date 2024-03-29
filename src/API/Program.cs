using Asp.Versioning;
using Beseler.API;
using Beseler.API.Accounts;
using Beseler.API.Budgeting;
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

app.UseStatusCodeMiddleware();
app.UseStaticFiles();
app.UseExceptionHandler(app =>
    app.Run(async context => await TypedResults.Problem().ExecuteAsync(context)));

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
    .MapBudgetingEndpoints(versions)
    .MapWeatherEndpoints(versions)
    .MapSwaggerUI();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Beseler.Web._Imports).Assembly);

app.Run();
