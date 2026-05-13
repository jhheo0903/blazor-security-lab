using System.Reflection;
using System.Runtime.Serialization;
using CAPolicyLab.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Graph;
using Microsoft.Graph.Models;

namespace CAPolicyLab.Services;

/// <summary>
/// Microsoft Graph API 를 통해 Entra ID 데이터를 조회하는 서비스.
///
/// 의존성:
///   - GraphServiceClient : Program.cs 에서 ITokenAcquisition + Kiota 어댑터로 수동 등록
///   - IMemoryCache       : 반복 API 호출 최소화 (Graph API 는 호출 횟수 제한 있음)
///
/// 필요 권한 (Azure Portal > 앱 등록 > API 권한):
///   - Policy.Read.All    : 조건부 액세스 정책 조회
///   - Directory.Read.All : 사용자, 그룹, 앱 정보 조회
/// </summary>
public class GraphService(GraphServiceClient graphClient, IMemoryCache cache, ILogger<GraphService> logger)
{
    // 캐시 만료 시간: 5분
    // CA 정책은 자주 바뀌지 않으므로 짧은 캐시로 API 부하 감소
    private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(5);

    // ── 조건부 액세스 정책 ──────────────────────────────────────────────────

    /// <summary>
    /// 테넌트의 모든 조건부 액세스 정책을 조회하여 내부 모델로 변환한다.
    /// Graph API: GET /identity/conditionalAccess/policies
    /// </summary>
    public async Task<List<ConditionalAccessPolicyModel>> GetPoliciesAsync()
    {
        const string cacheKey = "ca_policies";

        if (cache.TryGetValue(cacheKey, out List<ConditionalAccessPolicyModel>? cached) && cached is not null)
            return cached;

        try
        {
            // Graph SDK v5: 페이지 기반 응답, PageIterator 로 전체 수집
            var response = await graphClient.Identity.ConditionalAccess.Policies.GetAsync(config =>
            {
                // 필요한 필드만 요청해 응답 크기 최소화
                config.QueryParameters.Select =
                [
                    "id", "displayName", "state", "conditions",
                    "grantControls", "sessionControls",
                    "createdDateTime", "modifiedDateTime"
                ];
            });

            var policies = new List<ConditionalAccessPolicy>();
            var pageIterator = PageIterator<ConditionalAccessPolicy, ConditionalAccessPolicyCollectionResponse>
                .CreatePageIterator(graphClient, response!, p =>
                {
                    policies.Add(p);
                    return true; // true 를 반환하면 다음 페이지 계속 조회
                });

            await pageIterator.IterateAsync();

            var result = policies.Select(MapToModel).ToList();
            cache.Set(cacheKey, result, CacheDuration);
            return result;
        }
        catch (ServiceException ex)
        {
            logger.LogError(ex, "Graph API 에서 CA 정책 조회 실패. 상태코드: {StatusCode}", ex.ResponseStatusCode);
            throw new GraphServiceException("조건부 액세스 정책을 불러올 수 없습니다. 권한(Policy.Read.All)을 확인하세요.", ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Graph API 인증 또는 연결 실패");
            throw new GraphServiceException(
                "Graph API 에 연결할 수 없습니다. /setup 에서 앱 인증 정보를 설정하세요.", ex);
        }
    }

    // ── 사용자 ─────────────────────────────────────────────────────────────

    /// <summary>
    /// 테넌트 사용자 목록을 조회한다 (최대 999명).
    /// Graph API: GET /users
    /// </summary>
    public async Task<List<(string Id, string DisplayName, string Upn)>> GetUsersAsync()
    {
        const string cacheKey = "users";

        if (cache.TryGetValue(cacheKey, out List<(string, string, string)>? cached) && cached is not null)
            return cached;

        try
        {
            var response = await graphClient.Users.GetAsync(config =>
            {
                config.QueryParameters.Select = ["id", "displayName", "userPrincipalName"];
                config.QueryParameters.Top = 999;
                config.QueryParameters.Orderby = ["displayName"];
            });

            var result = response?.Value?
                .Select(u => (u.Id ?? "", u.DisplayName ?? "", u.UserPrincipalName ?? ""))
                .ToList() ?? [];

            cache.Set(cacheKey, result, CacheDuration);
            return result;
        }
        catch (ServiceException ex)
        {
            logger.LogError(ex, "Graph API 에서 사용자 목록 조회 실패");
            throw new GraphServiceException("사용자 목록을 불러올 수 없습니다. 권한(Directory.Read.All)을 확인하세요.", ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Graph API 인증 또는 연결 실패 (사용자 목록)");
            throw new GraphServiceException(
                "Graph API 에 연결할 수 없습니다. /setup 에서 앱 인증 정보를 설정하세요.", ex);
        }
    }

    /// <summary>특정 사용자의 그룹 멤버십을 조회한다.</summary>
    /// <param name="userId">사용자 objectId</param>
    public async Task<List<string>> GetUserGroupIdsAsync(string userId)
    {
        string cacheKey = $"user_groups_{userId}";

        if (cache.TryGetValue(cacheKey, out List<string>? cached) && cached is not null)
            return cached;

        try
        {
            // transitiveMemberOf: 중첩 그룹까지 포함한 모든 그룹 반환
            var response = await graphClient.Users[userId].TransitiveMemberOf.GetAsync(config =>
            {
                config.QueryParameters.Select = ["id"];
            });

            var groupIds = response?.Value?
                .Where(m => m is Group)
                .Select(m => m.Id ?? "")
                .Where(id => !string.IsNullOrEmpty(id))
                .ToList() ?? [];

            cache.Set(cacheKey, groupIds, CacheDuration);
            return groupIds;
        }
        catch (ServiceException ex)
        {
            logger.LogError(ex, "사용자 {UserId} 의 그룹 멤버십 조회 실패", userId);
            return [];
        }
    }

    // ── 앱 ─────────────────────────────────────────────────────────────────

    /// <summary>
    /// 테넌트에 등록된 서비스 주체(앱) 목록을 조회한다.
    /// Graph API: GET /servicePrincipals
    /// </summary>
    public async Task<List<(string Id, string DisplayName)>> GetApplicationsAsync()
    {
        const string cacheKey = "applications";

        if (cache.TryGetValue(cacheKey, out List<(string, string)>? cached) && cached is not null)
            return cached;

        try
        {
            var response = await graphClient.ServicePrincipals.GetAsync(config =>
            {
                config.QueryParameters.Select = ["appId", "displayName"];
                config.QueryParameters.Top = 999;
            });

            var result = response?.Value?
                .Select(sp => (sp.AppId ?? "", sp.DisplayName ?? ""))
                .Where(t => !string.IsNullOrEmpty(t.Item1))
                .OrderBy(t => t.Item2, StringComparer.OrdinalIgnoreCase)
                .ToList() ?? [];

            cache.Set(cacheKey, result, CacheDuration);
            return result;
        }
        catch (ServiceException ex)
        {
            logger.LogError(ex, "Graph API 에서 앱 목록 조회 실패");
            throw new GraphServiceException("애플리케이션 목록을 불러올 수 없습니다. 권한(Directory.Read.All)을 확인하세요.", ex);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Graph API 인증 또는 연결 실패 (앱 목록)");
            throw new GraphServiceException(
                "Graph API 에 연결할 수 없습니다. /setup 에서 앱 인증 정보를 설정하세요.", ex);
        }
    }

    /// <summary>캐시를 강제로 초기화한다 (정책 새로 고침 버튼 등에서 호출).</summary>
    public void ClearCache()
    {
        cache.Remove("ca_policies");
        cache.Remove("users");
        cache.Remove("applications");
    }

    // ── Graph 모델 → 내부 모델 변환 ────────────────────────────────────────

    /// <summary>
    /// Graph SDK 의 ConditionalAccessPolicy 를 내부 모델로 변환.
    /// Graph SDK 타입은 Nullable 이 많아 직접 바인딩하면 번거로우므로 여기서 정규화한다.
    /// </summary>
    private static ConditionalAccessPolicyModel MapToModel(ConditionalAccessPolicy p)
    {
        var model = new ConditionalAccessPolicyModel
        {
            Id = p.Id ?? string.Empty,
            DisplayName = p.DisplayName ?? "(이름 없음)",
            CreatedDateTime = p.CreatedDateTime,
            ModifiedDateTime = p.ModifiedDateTime
        };

        // State 열거형 → 문자열 변환
        model.State = p.State switch
        {
            ConditionalAccessPolicyState.Enabled => "enabled",
            ConditionalAccessPolicyState.Disabled => "disabled",
            ConditionalAccessPolicyState.EnabledForReportingButNotEnforced => "enabledForReportingButNotEnforced",
            _ => "unknown"
        };

        // 사용자·그룹 조건
        var users = p.Conditions?.Users;
        if (users is not null)
        {
            model.IncludeUsers  = users.IncludeUsers  ?? [];
            model.ExcludeUsers  = users.ExcludeUsers  ?? [];
            model.IncludeGroups = users.IncludeGroups ?? [];
            model.ExcludeGroups = users.ExcludeGroups ?? [];
            model.IncludeRoles  = users.IncludeRoles  ?? [];
            model.ExcludeRoles  = users.ExcludeRoles  ?? [];
        }

        // 애플리케이션 조건
        var apps = p.Conditions?.Applications;
        if (apps is not null)
        {
            model.IncludeApplications = apps.IncludeApplications ?? [];
            model.ExcludeApplications = apps.ExcludeApplications ?? [];
        }

        // 플랫폼 조건
        var platforms = p.Conditions?.Platforms;
        if (platforms is not null)
        {
            model.IncludePlatforms = platforms.IncludePlatforms?
                .Select(pl => pl.ToString() ?? "")
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList() ?? [];

            model.ExcludePlatforms = platforms.ExcludePlatforms?
                .Select(pl => pl.ToString() ?? "")
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList() ?? [];
        }

        // 위치 조건
        var locations = p.Conditions?.Locations;
        if (locations is not null)
        {
            model.IncludeLocations = locations.IncludeLocations ?? [];
            model.ExcludeLocations = locations.ExcludeLocations ?? [];
        }

        // 클라이언트 앱 유형
        model.ClientAppTypes = p.Conditions?.ClientAppTypes?
            .Select(t => t.ToString() ?? "")
            .Where(s => !string.IsNullOrEmpty(s))
            .ToList() ?? [];

        // Grant Controls
        var grant = p.GrantControls;
        if (grant is not null)
        {
            model.GrantOperator = grant.Operator ?? "OR";
            model.GrantControls = grant.BuiltInControls?
                .Where(c => c.HasValue)
                .Select(c => GetEnumMemberValue(c!.Value))
                .Where(s => !string.IsNullOrEmpty(s))
                .ToList() ?? [];
        }

        // 세션 제어 (로그인 빈도)
        var sessionFreq = p.SessionControls?.SignInFrequency;
        if (sessionFreq?.IsEnabled == true)
        {
            model.SignInFrequencyEnabled = true;
            model.SignInFrequencyValue   = sessionFreq.Value;
            model.SignInFrequencyType    = sessionFreq.Type?.ToString() ?? "";
        }

        return model;
    }

    // Kiota 생성 enum은 ToString()이 PascalCase를 반환하므로
    // [EnumMember(Value = "...")] 속성에서 Graph API 실제 값(camelCase)을 추출한다.
    private static string GetEnumMemberValue<T>(T value) where T : struct, Enum
    {
        var field = typeof(T).GetField(value.ToString()!);
        return field?.GetCustomAttribute<EnumMemberAttribute>()?.Value
               ?? value.ToString()!;
    }
}

/// <summary>Graph API 호출 실패 시 던지는 커스텀 예외</summary>
public class GraphServiceException(string message, Exception? inner = null)
    : Exception(message, inner);
