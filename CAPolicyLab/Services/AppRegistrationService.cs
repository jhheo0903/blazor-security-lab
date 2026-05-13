using System.Text;
using System.Text.Json;
using Azure.Core;
using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Applications.Item.AddPassword;
using Microsoft.Graph.Models;

namespace CAPolicyLab.Services;

public enum RegStatus
{
    Idle,
    GettingCode,          // Background task started, waiting for device code from Azure
    WaitingForUser,       // Device code shown, waiting for admin to authenticate in browser
    CreatingApp,
    GrantingPermissions,
    CreatingSecret,
    Saving,
    Done,
    Failed
}

public sealed record DeviceCodeDisplay(
    string UserCode,
    string VerificationUri,
    string Message,
    DateTimeOffset ExpiresOn);

/// <summary>
/// Singleton service that automates Azure AD app registration via device code flow.
/// The admin authenticates interactively; this service creates the app, grants
/// admin consent, generates a secret, and saves the result to appsettings.
/// </summary>
public sealed class AppRegistrationService(
    AppConfigService configService,
    ILogger<AppRegistrationService> logger)
{
    // Well-known Microsoft Azure PowerShell public client — supports device code
    // and is pre-authorized for high-privilege Graph delegated scopes.
    private const string BootstrapClientId = "1950a258-227b-4e31-a9cf-717495945fc2";

    // Microsoft Graph API identifiers
    private const string GraphAppId         = "00000003-0000-0000-c000-000000000000";
    private const string PolicyReadAllId    = "246dd0d5-5bd0-4def-940b-0421030a5b68";
    private const string DirectoryReadAllId = "7ab1d382-f21e-4acd-a863-ba3e13f7da61";

    private readonly object      _lock     = new();
    private readonly List<string> _progress = [];

    public RegStatus          Status     { get; private set; } = RegStatus.Idle;
    public DeviceCodeDisplay? DeviceCode { get; private set; }
    public string?            Error      { get; private set; }
    public AzureAdConfig?     Result     { get; private set; }

    public IReadOnlyList<string> Progress { get { lock (_lock) return [.. _progress]; } }

    // UI components subscribe to receive push notifications when state changes.
    public event Action? OnStateChanged;

    // ── Public API ────────────────────────────────────────────────────────────

    public void StartRegistration()
    {
        if (Status is RegStatus.GettingCode or RegStatus.WaitingForUser
                   or RegStatus.CreatingApp or RegStatus.GrantingPermissions
                   or RegStatus.CreatingSecret or RegStatus.Saving)
            return;

        Reset(notify: false);
        Status = RegStatus.GettingCode;
        Notify();
        _ = Task.Run(RunAsync);
    }

    public void Reset(bool notify = true)
    {
        Status    = RegStatus.Idle;
        DeviceCode = null;
        Error     = null;
        Result    = null;
        lock (_lock) _progress.Clear();
        if (notify) Notify();
    }

    // ── Registration flow ─────────────────────────────────────────────────────

    private async Task RunAsync()
    {
        try
        {
            var (token, tenantId) = await AuthenticateAsync();
            var config = await CreateAppAsync(token, tenantId);

            SetStatus(RegStatus.Saving);
            AddProgress("⏳ 설정 저장 중...");

            await configService.SaveConfigAsync(config);
            Result = config;

            AddProgress("✓ 설정 저장 완료");
            SetStatus(RegStatus.Done);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "앱 등록 자동화 실패");
            if (Status is not RegStatus.Failed)
            {
                Error = BuildErrorMessage(ex);
                SetStatus(RegStatus.Failed);
            }
        }
    }

    private async Task<(AccessToken Token, string TenantId)> AuthenticateAsync()
    {
        var credential = new DeviceCodeCredential(new DeviceCodeCredentialOptions
        {
            ClientId = BootstrapClientId,
            DeviceCodeCallback = (info, _) =>
            {
                DeviceCode = new DeviceCodeDisplay(
                    info.UserCode,
                    info.VerificationUri.ToString(),
                    info.Message,
                    info.ExpiresOn);
                SetStatus(RegStatus.WaitingForUser);
                return Task.CompletedTask;
            },
        });

        string[] scopes =
        [
            "https://graph.microsoft.com/Application.ReadWrite.All",
            "https://graph.microsoft.com/AppRoleAssignment.ReadWrite.All",
        ];

        var token = await credential.GetTokenAsync(
            new TokenRequestContext(scopes), CancellationToken.None);

        var tenantId = ExtractTidFromJwt(token.Token)
            ?? throw new InvalidOperationException("인증 토큰에서 테넌트 ID를 추출할 수 없습니다.");

        AddProgress("✓ 전역 관리자 인증 완료");
        Notify();

        return (token, tenantId);
    }

    private async Task<AzureAdConfig> CreateAppAsync(AccessToken token, string tenantId)
    {
        var client = new GraphServiceClient(
            new StaticTokenCredential(token),
            ["https://graph.microsoft.com/.default"]);

        // ── 1. Create app registration ────────────────────────────────────────
        SetStatus(RegStatus.CreatingApp);
        AddProgress("⏳ 앱 등록 생성 중...");

        var newApp = await client.Applications.PostAsync(new Application
        {
            DisplayName    = "CAPolicyLab",
            SignInAudience = "AzureADMyOrg",
            RequiredResourceAccess =
            [
                new RequiredResourceAccess
                {
                    ResourceAppId  = GraphAppId,
                    ResourceAccess =
                    [
                        new ResourceAccess { Id = Guid.Parse(PolicyReadAllId),    Type = "Role" },
                        new ResourceAccess { Id = Guid.Parse(DirectoryReadAllId), Type = "Role" },
                    ]
                }
            ]
        }) ?? throw new InvalidOperationException("앱 등록 생성 응답이 없습니다.");

        AddProgress($"✓ 앱 등록 완료 (ClientId: {newApp.AppId})");

        // ── 2. Create service principal ───────────────────────────────────────
        SetStatus(RegStatus.GrantingPermissions);
        AddProgress("⏳ 서비스 주체 생성 중...");

        var sp = await client.ServicePrincipals.PostAsync(new ServicePrincipal
        {
            AppId = newApp.AppId
        }) ?? throw new InvalidOperationException("서비스 주체 생성 응답이 없습니다.");

        AddProgress("✓ 서비스 주체 생성 완료");

        // ── 3. Grant admin consent ────────────────────────────────────────────
        AddProgress("⏳ 관리자 동의 부여 중...");

        var graphSpPage = await client.ServicePrincipals.GetAsync(cfg =>
            cfg.QueryParameters.Filter = $"appId eq '{GraphAppId}'");

        var graphSp = graphSpPage?.Value?.FirstOrDefault()
            ?? throw new InvalidOperationException("Microsoft Graph 서비스 주체를 찾을 수 없습니다.");

        await client.ServicePrincipals[sp.Id].AppRoleAssignments.PostAsync(new AppRoleAssignment
        {
            PrincipalId = Guid.Parse(sp.Id!),
            ResourceId  = Guid.Parse(graphSp.Id!),
            AppRoleId   = Guid.Parse(PolicyReadAllId),
        });

        await client.ServicePrincipals[sp.Id].AppRoleAssignments.PostAsync(new AppRoleAssignment
        {
            PrincipalId = Guid.Parse(sp.Id!),
            ResourceId  = Guid.Parse(graphSp.Id!),
            AppRoleId   = Guid.Parse(DirectoryReadAllId),
        });

        AddProgress("✓ 관리자 동의 부여 완료 (Policy.Read.All, Directory.Read.All)");

        // ── 4. Create client secret ───────────────────────────────────────────
        SetStatus(RegStatus.CreatingSecret);
        AddProgress("⏳ 클라이언트 시크릿 생성 중...");

        var secretResult = await client.Applications[newApp.Id].AddPassword.PostAsync(
            new AddPasswordPostRequestBody
            {
                PasswordCredential = new PasswordCredential
                {
                    DisplayName = "CAPolicyLab-Secret",
                    EndDateTime = DateTimeOffset.UtcNow.AddYears(2),
                }
            }) ?? throw new InvalidOperationException("시크릿 생성 응답이 없습니다.");

        AddProgress("✓ 클라이언트 시크릿 생성 완료");
        Notify();

        return new AzureAdConfig
        {
            TenantId     = tenantId,
            ClientId     = newApp.AppId!,
            ClientSecret = secretResult.SecretText!,
        };
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private void SetStatus(RegStatus status) { Status = status; Notify(); }

    private void AddProgress(string msg) { lock (_lock) _progress.Add(msg); }

    private void Notify() => OnStateChanged?.Invoke();

    private static string? ExtractTidFromJwt(string jwt)
    {
        try
        {
            var parts = jwt.Split('.');
            if (parts.Length < 2) return null;
            var padding = (4 - parts[1].Length % 4) % 4;
            var payload = parts[1] + new string('=', padding);
            var json    = Encoding.UTF8.GetString(Convert.FromBase64String(payload));
            using var doc = JsonDocument.Parse(json);
            return doc.RootElement.TryGetProperty("tid", out var tid)
                ? tid.GetString()
                : null;
        }
        catch { return null; }
    }

    private static string BuildErrorMessage(Exception ex)
    {
        var inner = ex.InnerException?.Message ?? ex.Message;
        return inner.Length > 400 ? inner[..400] + "…" : inner;
    }

    // Wraps a pre-obtained AccessToken so Graph SDK can use it directly.
    private sealed class StaticTokenCredential(AccessToken token) : TokenCredential
    {
        public override AccessToken GetToken(TokenRequestContext _, CancellationToken __) => token;
        public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext _, CancellationToken __) =>
            ValueTask.FromResult(token);
    }
}
