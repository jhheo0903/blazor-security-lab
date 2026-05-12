using CAPolicyLab.Components;
using CAPolicyLab.Services;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.Kiota.Abstractions.Authentication;

var builder = WebApplication.CreateBuilder(args);

// ─── 인증 설정 ───────────────────────────────────────────────────────────────
// Microsoft.Identity.Web 을 사용해 Entra ID(Azure AD) OIDC 인증을 구성한다.
// appsettings.json 의 AzureAd 섹션 값을 읽어 OpenID Connect 미들웨어를 설정.
builder.Services.AddAuthentication(OpenIdConnectDefaults.AuthenticationScheme)
    .AddMicrosoftIdentityWebApp(builder.Configuration.GetSection("AzureAd"))
    // 다운스트림 API(Graph) 호출을 위한 토큰 획득 파이프라인 활성화
    // AddMicrosoftGraph() 를 사용하지 않는 이유: 해당 메서드는 Graph.Core 2.x 의
    // IAuthenticationProviderOption 을 내부에서 참조하며, Graph 5.x(Graph.Core 3.x)
    // 환경에서 TypeLoadException 이 발생한다. GraphServiceClient 는 아래에서 수동 등록한다.
    .EnableTokenAcquisitionToCallDownstreamApi(
        new[] { "Policy.Read.All", "Directory.Read.All" })
    // 액세스 토큰을 메모리에 캐싱 (프로덕션에서는 분산 캐시 권장)
    .AddInMemoryTokenCaches();

// ─── 인가 설정 ───────────────────────────────────────────────────────────────
// FallbackPolicy 를 설정하지 않으면 각 페이지의 [Authorize] 어트리뷰트만 적용된다.
// → 랜딩 페이지(/)는 누구나 접근 가능, 나머지 페이지는 [Authorize] 로 보호.
// (FallbackPolicy = DefaultPolicy 로 설정하면 앱 시작 시 OIDC 메타데이터를
//  즉시 요청해 TenantId 미설정 상태에서 IDX20807 오류가 발생함)
builder.Services.AddAuthorization();

// Blazor 컴포넌트 트리 전체에 인증 상태(AuthenticationState)를 cascade 로 전달
builder.Services.AddCascadingAuthenticationState();

// Microsoft Identity UI 컨트롤러 등록
// /MicrosoftIdentity/Account/SignIn, SignOut 엔드포인트를 자동 생성
builder.Services.AddControllersWithViews().AddMicrosoftIdentityUI();

// GraphService 에서 IHttpContextAccessor 를 사용할 경우를 위해 등록
builder.Services.AddHttpContextAccessor();

// ─── Razor / Blazor 서비스 ───────────────────────────────────────────────────
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// ─── GraphServiceClient 수동 등록 ────────────────────────────────────────────
// Microsoft.Identity.Web.MicrosoftGraph 의 AddMicrosoftGraph() 대신 사용.
// ITokenAcquisition → TokenAcquisitionTokenProvider(IAccessTokenProvider) →
// BaseBearerTokenAuthenticationProvider → GraphServiceClient 순으로 연결.
builder.Services.AddScoped<GraphServiceClient>(sp =>
{
    var tokenAcquisition = sp.GetRequiredService<ITokenAcquisition>();
    var tokenProvider = new TokenAcquisitionTokenProvider(tokenAcquisition);
    var authProvider = new BaseBearerTokenAuthenticationProvider(tokenProvider);
    return new GraphServiceClient(authProvider);
});

// ─── 애플리케이션 서비스 ─────────────────────────────────────────────────────
// Scoped: 사용자 요청(SignalR 세션)마다 새 인스턴스 생성
builder.Services.AddScoped<GraphService>();
builder.Services.AddScoped<PolicyAnalyzerService>();

// 정책 데이터를 세션 내 캐싱하기 위한 메모리 캐시
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

// 인증 → 인가 순서가 중요: 순서가 바뀌면 인증 없이 인가 체크가 먼저 실행됨
app.UseAuthentication();
app.UseAuthorization();

app.UseAntiforgery();

app.MapStaticAssets();

// Razor 컴포넌트 라우팅 + 인터랙티브 서버 렌더 모드
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

// Microsoft Identity UI 컨트롤러 라우팅 (로그인/로그아웃)
app.MapControllers();

app.Run();
