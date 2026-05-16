using B6.DIServiceStateLab.Components;
using B6.DIServiceStateLab.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// B6 DI 서비스 등록
builder.Services.AddScoped<ICounterService, CounterService>();
builder.Services.AddSingleton<IGlobalStateService, GlobalStateService>();
builder.Services.AddScoped<IPolicyService, PolicyService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}
app.UseStatusCodePagesWithReExecute("/not-found", createScopeForStatusCodePages: true);
app.UseHttpsRedirection();

app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

await app.RunAsync();
