namespace CAPolicyLab.Models;

/// <summary>
/// What-If 시뮬레이션의 입력 조건.
/// 사용자가 특정 앱에 특정 환경에서 접속했을 때를 가정한다.
/// </summary>
public class WhatIfCondition
{
    /// <summary>시뮬레이션 대상 사용자 ID (Graph 사용자 objectId)</summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>화면 표시용 사용자 이름</summary>
    public string UserDisplayName { get; set; } = string.Empty;

    /// <summary>접속하려는 앱 ID</summary>
    public string ApplicationId { get; set; } = string.Empty;

    /// <summary>화면 표시용 앱 이름</summary>
    public string ApplicationDisplayName { get; set; } = string.Empty;

    // 선택적 조건 – 지정하지 않으면 시뮬레이션에서 무시
    public string? Platform { get; set; }    // windows, iOS, android 등
    public string? IpAddress { get; set; }   // IP 주소 (위치 조건 평가용)
    public bool IsCompliantDevice { get; set; }
    public bool IsDomainJoined { get; set; }
}

/// <summary>
/// What-If 시뮬레이션 결과.
/// 입력 조건에 대해 각 CA 정책의 적용 여부와 최종 접근 결정을 담는다.
/// </summary>
public class WhatIfResult
{
    /// <summary>시뮬레이션에 사용된 입력 조건</summary>
    public WhatIfCondition Condition { get; set; } = new();

    /// <summary>최종 접근 결과</summary>
    public WhatIfAccessResult AccessResult { get; set; }

    /// <summary>
    /// 이 사용자+앱 조합에 실제로 적용되는 정책 목록.
    /// Report-Only 정책도 여기 포함되지만 AccessResult 에는 영향 없음.
    /// </summary>
    public List<AppliedPolicyDetail> AppliedPolicies { get; set; } = [];

    /// <summary>
    /// 적용되지 않는 정책 목록과 그 이유.
    /// 디버깅/학습 목적으로 제공.
    /// </summary>
    public List<NotAppliedPolicyDetail> NotAppliedPolicies { get; set; } = [];

    /// <summary>차단을 유발한 정책 (BlockVsAllow 일 때 null 이 아님)</summary>
    public AppliedPolicyDetail? BlockingPolicy { get; set; }

    /// <summary>모든 적용 정책의 Grant Control 을 집계한 목록</summary>
    public List<string> AggregatedControls { get; set; } = [];

    public DateTime SimulatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Bootstrap 결과 색상</summary>
    public string ResultBadgeClass => AccessResult switch
    {
        WhatIfAccessResult.Allowed          => "bg-success",
        WhatIfAccessResult.BlockedByPolicy  => "bg-danger",
        WhatIfAccessResult.RequiresControls => "bg-warning text-dark",
        _                                   => "bg-secondary"
    };

    /// <summary>결과 한글 표시명</summary>
    public string ResultDisplayName => AccessResult switch
    {
        WhatIfAccessResult.Allowed          => "접근 허용",
        WhatIfAccessResult.BlockedByPolicy  => "정책에 의해 차단",
        WhatIfAccessResult.RequiresControls => "추가 제어 필요",
        _                                   => AccessResult.ToString()
    };
}

/// <summary>최종 접근 판정</summary>
public enum WhatIfAccessResult
{
    /// <summary>제어 없이 바로 허용</summary>
    Allowed,

    /// <summary>Block 정책에 의해 거부</summary>
    BlockedByPolicy,

    /// <summary>MFA 등 추가 인증/장치 조건 충족 시 허용</summary>
    RequiresControls
}

/// <summary>적용된 정책의 상세 정보</summary>
public class AppliedPolicyDetail
{
    public string PolicyId { get; set; } = string.Empty;
    public string PolicyName { get; set; } = string.Empty;
    public string PolicyState { get; set; } = string.Empty;

    /// <summary>이 정책이 적용된 이유 (사용자가 포함된 그룹, 앱 매칭 등)</summary>
    public string AppliedReason { get; set; } = string.Empty;

    /// <summary>이 정책이 요구하는 Grant Controls</summary>
    public List<string> GrantControls { get; set; } = [];
}

/// <summary>적용되지 않은 정책과 그 이유</summary>
public class NotAppliedPolicyDetail
{
    public string PolicyId { get; set; } = string.Empty;
    public string PolicyName { get; set; } = string.Empty;

    /// <summary>적용되지 않은 이유 (조건 불일치 등)</summary>
    public string Reason { get; set; } = string.Empty;
}
