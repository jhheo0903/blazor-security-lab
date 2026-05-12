namespace CAPolicyLab.Models;

/// <summary>
/// 두 조건부 액세스 정책 간의 잠재적 충돌 정보.
/// 충돌 = 동일한 사용자·앱 범위에 적용되면서 상반되는 제어를 요구하는 상황.
/// </summary>
public class PolicyConflict
{
    /// <summary>충돌 당사자 정책 A</summary>
    public ConditionalAccessPolicyModel PolicyA { get; set; } = null!;

    /// <summary>충돌 당사자 정책 B</summary>
    public ConditionalAccessPolicyModel PolicyB { get; set; } = null!;

    /// <summary>충돌 심각도</summary>
    public ConflictSeverity Severity { get; set; }

    /// <summary>충돌 유형</summary>
    public ConflictType Type { get; set; }

    /// <summary>관리자가 이해할 수 있는 충돌 설명 (한글)</summary>
    public string Description { get; set; } = string.Empty;

    /// <summary>두 정책이 겹치는 사용자 ID 목록</summary>
    public List<string> OverlappingUsers { get; set; } = [];

    /// <summary>두 정책이 겹치는 그룹 ID 목록</summary>
    public List<string> OverlappingGroups { get; set; } = [];

    /// <summary>두 정책이 겹치는 앱 ID 목록</summary>
    public List<string> OverlappingApplications { get; set; } = [];

    /// <summary>Bootstrap 경고 색상 (Severity 에 따라 결정)</summary>
    public string SeverityBadgeClass => Severity switch
    {
        ConflictSeverity.High   => "bg-danger",
        ConflictSeverity.Medium => "bg-warning text-dark",
        ConflictSeverity.Low    => "bg-info text-dark",
        _                       => "bg-secondary"
    };

    /// <summary>충돌 유형의 한글 표시명</summary>
    public string TypeDisplayName => Type switch
    {
        ConflictType.MfaVsNoMfa        => "MFA 요구 vs 미요구",
        ConflictType.BlockVsAllow      => "차단 vs 허용 충돌",
        ConflictType.ExclusionOverlap  => "제외 조건 중복",
        ConflictType.DuplicatePolicy   => "동일 범위 중복 정책",
        ConflictType.ShadowedPolicy    => "상위 정책에 가려진 정책",
        _                              => Type.ToString()
    };
}

/// <summary>충돌의 심각도 수준</summary>
public enum ConflictSeverity
{
    /// <summary>즉각 검토 필요 – 접근 차단/허용이 예측 불가</summary>
    High,

    /// <summary>MFA 요구 불일치 등 보안 수준 차이 발생</summary>
    Medium,

    /// <summary>정책이 중복되거나 하나가 불필요할 가능성</summary>
    Low
}

/// <summary>충돌의 구체적 유형</summary>
public enum ConflictType
{
    /// <summary>한 정책은 MFA 요구, 다른 정책은 동일 범위에 MFA 없이 허용</summary>
    MfaVsNoMfa,

    /// <summary>한 정책은 차단, 다른 정책은 허용 – 실제 동작이 불명확</summary>
    BlockVsAllow,

    /// <summary>A 의 제외 대상이 B 의 포함 대상과 겹쳐 의도치 않은 적용 발생</summary>
    ExclusionOverlap,

    /// <summary>두 정책이 동일 범위에 동일 제어를 중복 적용</summary>
    DuplicatePolicy,

    /// <summary>더 넓은 범위 정책 때문에 좁은 범위 정책이 사실상 무의미</summary>
    ShadowedPolicy
}
