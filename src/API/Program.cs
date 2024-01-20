using Asp.Versioning;
using Beseler.API;
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
app.UseRouting();
app.UseAntiforgery();

var versionSet = app.NewApiVersionSet()
    .HasApiVersion(new ApiVersion(1))
    .HasApiVersion(new ApiVersion(2))
    .ReportApiVersions()
    .Build();

app.MapApplicationEndpoints(versionSet);
app.MapWeatherEndpoints();

app.MapSwaggerUI();
app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode()
    .AddAdditionalAssemblies(typeof(Beseler.Web._Imports).Assembly);

app.Run();
