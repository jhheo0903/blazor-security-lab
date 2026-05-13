using Azure.Identity;
using CAPolicyLab.Components;
using CAPolicyLab.Services;
using Microsoft.Graph;

var builder = WebApplication.CreateBuilder(args);

// ─── Razor / Blazor 서비스 ───────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ─── GraphServiceClient (앱 권한 / 클라이언트 자격 증명 흐름) ─────────────────
// Azure.Identity 의 ClientSecretCredential 을 사용해 사용자 로그인 없이
// 앱 자체 권한으로 Graph API 를 호출한다 (Policy.Read.All, Directory.Read.All).
//
// 주의: ClientSecretCredential 은 생성자에서 tenantId 형식을 검증하므로
//       "YOUR_TENANT_ID" 같은 플레이스홀더를 그대로 넘기면 ArgumentException 이 발생한다.
//       → AppConfigService.GetStatus() 로 미설정 여부를 먼저 확인하고,
//         미설정이면 형식상 유효한 더미 GUID 를 사용한다.
//         페이지의 config guard 가 실제 Graph 호출을 차단하므로 더미 클라이언트는
//         실제로 API 를 호출하지 않는다.
builder.Services.AddScoped<GraphServiceClient>(sp =>
{
    var appCfg = sp.GetRequiredService<AppConfigService>();

    string tenantId, clientId, secret;

    if (appCfg.GetStatus() == ConfigStatus.Configured)
    {
        var azCfg = appCfg.GetCurrentConfig();
        tenantId  = azCfg.TenantId;
        clientId  = azCfg.ClientId;
        secret    = azCfg.ClientSecret;
    }
    else
    {
        // 형식상 유효한 더미 GUID — 생성자 검증을 통과하지만 실제 인증은 실패한다.
        // 페이지의 _notConfigured 가드가 Graph 호출 자체를 막으므로
        // 이 더미 클라이언트로 실제 API 를 호출하는 일은 없다.
        tenantId = "00000000-0000-0000-0000-000000000000";
        clientId = "00000000-0000-0000-0000-000000000000";
        secret   = "placeholder";
    }

    try
    {
        var credential = new ClientSecretCredential(tenantId, clientId, secret);
        return new GraphServiceClient(credential, ["https://graph.microsoft.com/.default"]);
    }
    catch
    {
        // 저장된 값이 잘못된 형식이면 더미로 폴백 — DI 팩토리가 절대 예외를 던지지 않도록 보장
        var dummy = new ClientSecretCredential(
            "00000000-0000-0000-0000-000000000000",
            "00000000-0000-0000-0000-000000000000",
            "placeholder");
        return new GraphServiceClient(dummy, ["https://graph.microsoft.com/.default"]);
    }
});

// ─── 애플리케이션 서비스 ─────────────────────────────────────────────────────
builder.Services.AddScoped<GraphService>();
builder.Services.AddScoped<PolicyAnalyzerService>();
builder.Services.AddSingleton<AppConfigService>();
builder.Services.AddSingleton<AppRegistrationService>();
builder.Services.AddMemoryCache();

// ─── 미들웨어 파이프라인 ──────────────────────────────────────────────────────
var app = builder.Build();

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

app.Run();
