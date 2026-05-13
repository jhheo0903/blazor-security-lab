using System.Text.Json;
using Azure.Identity;
using Microsoft.Graph;

namespace CAPolicyLab.Services;

public enum ConfigStatus { NotConfigured, PartiallyConfigured, Configured }

/// <summary>
/// appsettings.json 의 AzureAd 섹션을 읽고 검증·저장하는 서비스.
/// 앱 권한(클라이언트 자격 증명) 설정 상태를 관리한다.
/// </summary>
public class AppConfigService(
    IConfiguration configuration,
    IWebHostEnvironment env,
    ILogger<AppConfigService> logger)
{
    private const string PlaceholderPrefix = "YOUR_";

    public ConfigStatus GetStatus()
    {
        bool t = IsSet("AzureAd:TenantId");
        bool c = IsSet("AzureAd:ClientId");
        bool s = IsSet("AzureAd:ClientSecret");

        if (t && c && s) return ConfigStatus.Configured;
        if (t || c || s) return ConfigStatus.PartiallyConfigured;
        return ConfigStatus.NotConfigured;
    }

    public AzureAdConfig GetCurrentConfig() => new()
    {
        TenantId     = SafeGet("AzureAd:TenantId"),
        ClientId     = SafeGet("AzureAd:ClientId"),
        ClientSecret = SafeGet("AzureAd:ClientSecret"),
        Domain       = SafeGet("AzureAd:Domain"),
    };

    /// <summary>실제 Graph API 호출로 자격 증명을 검증한다.</summary>
    public async Task<(bool Success, string Message)> TestConnectionAsync(
        string tenantId, string clientId, string clientSecret)
    {
        try
        {
            var credential = new ClientSecretCredential(tenantId, clientId, clientSecret);
            var client = new GraphServiceClient(
                credential, ["https://graph.microsoft.com/.default"]);

            await client.Identity.ConditionalAccess.Policies.GetAsync(cfg =>
            {
                cfg.QueryParameters.Top    = 1;
                cfg.QueryParameters.Select = ["id"];
            });

            return (true, "Graph API 연결 성공: Policy.Read.All 권한이 정상적으로 동작합니다.");
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Graph API 연결 테스트 실패");
            return (false, BuildErrorMessage(ex));
        }
    }

    /// <summary>자격 증명을 appsettings.Development.json 에 저장한다.</summary>
    public async Task<bool> SaveConfigAsync(AzureAdConfig config)
    {
        try
        {
            var path = Path.Combine(
                env.ContentRootPath,
                env.IsDevelopment() ? "appsettings.Development.json" : "appsettings.local.json");

            Dictionary<string, object> existing = new();
            if (File.Exists(path))
            {
                var text = await File.ReadAllTextAsync(path);
                existing = JsonSerializer.Deserialize<Dictionary<string, object>>(text) ?? new();
            }

            existing["AzureAd"] = new
            {
                Instance     = "https://login.microsoftonline.com/",
                Domain       = config.Domain,
                TenantId     = config.TenantId,
                ClientId     = config.ClientId,
                ClientSecret = config.ClientSecret,
            };

            var opts = new JsonSerializerOptions { WriteIndented = true };
            await File.WriteAllTextAsync(path, JsonSerializer.Serialize(existing, opts));
            return true;
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "설정 파일 저장 실패");
            return false;
        }
    }

    /// <summary>관리자 동의 URL을 생성한다 (앱 권한 grant 확인용).</summary>
    public string GetAdminConsentUrl(string tenantId, string clientId) =>
        $"https://login.microsoftonline.com/{tenantId}/adminconsent?client_id={clientId}";

    // ── helpers ──────────────────────────────────────────────────────────────

    private bool IsSet(string key)
    {
        var v = configuration[key];
        return !string.IsNullOrWhiteSpace(v)
            && !v.StartsWith(PlaceholderPrefix, StringComparison.OrdinalIgnoreCase);
    }

    private string SafeGet(string key)
    {
        var v = configuration[key] ?? "";
        return v.StartsWith(PlaceholderPrefix, StringComparison.OrdinalIgnoreCase) ? "" : v;
    }

    private static string BuildErrorMessage(Exception ex)
    {
        var msg = ex.InnerException?.Message ?? ex.Message;
        var idx = msg.IndexOf("AADSTS", StringComparison.Ordinal);
        if (idx >= 0)
        {
            var end = msg.IndexOf('\n', idx);
            return end >= 0 ? msg[idx..end].Trim() : msg[idx..].Trim();
        }
        return msg.Length > 200 ? msg[..200] + "…" : msg;
    }
}

public class AzureAdConfig
{
    public string TenantId     { get; set; } = "";
    public string ClientId     { get; set; } = "";
    public string ClientSecret { get; set; } = "";
    public string Domain       { get; set; } = "";
}
