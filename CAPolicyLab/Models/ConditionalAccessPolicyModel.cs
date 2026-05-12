namespace CAPolicyLab.Models;

/// <summary>
/// Microsoft Graph API 로 조회한 조건부 액세스(CA) 정책을
/// 앱 내부에서 사용하기 편리한 형태로 변환한 모델.
/// Graph SDK 의 ConditionalAccessPolicy 타입을 직접 쓰지 않는 이유:
/// - Nullable 처리가 복잡하고, 뷰 바인딩 시 불필요한 의존성이 생기기 때문.
/// </summary>
public class ConditionalAccessPolicyModel
{
    public string Id { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;

    // 정책 상태
    // "enabled"                         → 실제 적용
    // "disabled"                        → 비활성
    // "enabledForReportingButNotEnforced" → 보고 전용(Report-Only)
    public string State { get; set; } = string.Empty;

    // ── 사용자/그룹/역할 조건 ──────────────────────────────────────────────
    // "All" 이면 전체 사용자, 특정 ID 이면 해당 사용자만
    public List<string> IncludeUsers { get; set; } = [];
    public List<string> ExcludeUsers { get; set; } = [];
    public List<string> IncludeGroups { get; set; } = [];
    public List<string> ExcludeGroups { get; set; } = [];

    // 디렉터리 역할 (전역 관리자, 사용자 관리자 등)
    public List<string> IncludeRoles { get; set; } = [];
    public List<string> ExcludeRoles { get; set; } = [];

    // ── 애플리케이션 조건 ───────────────────────────────────────────────────
    // "All" 이면 모든 앱, "Office365" 같은 특수값 또는 앱 ID
    public List<string> IncludeApplications { get; set; } = [];
    public List<string> ExcludeApplications { get; set; } = [];

    // ── 플랫폼 조건 ─────────────────────────────────────────────────────────
    // android, iOS, windows, macOS, linux 등
    public List<string> IncludePlatforms { get; set; } = [];
    public List<string> ExcludePlatforms { get; set; } = [];

    // ── 위치 조건 ───────────────────────────────────────────────────────────
    // Named Location ID 또는 "AllTrusted"
    public List<string> IncludeLocations { get; set; } = [];
    public List<string> ExcludeLocations { get; set; } = [];

    // ── 클라이언트 앱 유형 ───────────────────────────────────────────────────
    // browser, mobileAppsAndDesktopClients, exchangeActiveSync, other
    public List<string> ClientAppTypes { get; set; } = [];

    // ── 부여 제어(Grant Controls) ────────────────────────────────────────────
    // mfa, compliantDevice, domainJoinedDevice, approvedApplication,
    // compliantApplication, passwordChange, block
    public List<string> GrantControls { get; set; } = [];

    // 여러 Grant Control 의 결합 방식: "AND" → 모두 필요, "OR" → 하나만 필요
    public string GrantOperator { get; set; } = "OR";

    // ── 세션 제어 ────────────────────────────────────────────────────────────
    public bool SignInFrequencyEnabled { get; set; }
    public int? SignInFrequencyValue { get; set; }

    // hours 또는 days
    public string SignInFrequencyType { get; set; } = string.Empty;

    // ── 타임스탬프 ───────────────────────────────────────────────────────────
    public DateTimeOffset? CreatedDateTime { get; set; }
    public DateTimeOffset? ModifiedDateTime { get; set; }

    // ── 헬퍼 프로퍼티 ────────────────────────────────────────────────────────

    /// <summary>정책이 현재 적용 중인지 여부</summary>
    public bool IsEnabled => State == "enabled";

    /// <summary>보고 전용 모드(Report-Only)인지 여부</summary>
    public bool IsReportOnly => State == "enabledForReportingButNotEnforced";

    /// <summary>정책이 사용자 전체를 대상으로 하는지</summary>
    public bool TargetsAllUsers => IncludeUsers.Contains("All");

    /// <summary>정책이 모든 앱을 대상으로 하는지</summary>
    public bool TargetsAllApps => IncludeApplications.Contains("All");

    /// <summary>이 정책이 차단(Block) 정책인지</summary>
    public bool IsBlockPolicy => GrantControls.Contains("block");

    /// <summary>MFA 를 요구하는 정책인지</summary>
    public bool RequiresMfa => GrantControls.Contains("mfa");

    /// <summary>Bootstrap 배지 색상 (State 에 따라 결정)</summary>
    public string StateBadgeClass => State switch
    {
        "enabled" => "bg-success",
        "disabled" => "bg-secondary",
        "enabledForReportingButNotEnforced" => "bg-warning text-dark",
        _ => "bg-secondary"
    };

    /// <summary>State 의 사람이 읽기 쉬운 한글 표시명</summary>
    public string StateDisplayName => State switch
    {
        "enabled" => "활성",
        "disabled" => "비활성",
        "enabledForReportingButNotEnforced" => "보고 전용",
        _ => State
    };
}
