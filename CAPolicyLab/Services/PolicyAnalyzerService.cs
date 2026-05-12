using CAPolicyLab.Models;

namespace CAPolicyLab.Services;

/// <summary>
/// 조건부 액세스 정책을 분석하는 서비스.
///
/// 주요 기능:
///   1. 충돌 감지 : 두 정책이 같은 범위에서 다른 제어를 요구하는지 탐지
///   2. What-If   : 특정 사용자·앱 조합에 어떤 정책이 적용되는지 로컬 시뮬레이션
///   3. Mermaid   : 정책을 Mermaid.js 플로우차트 정의 문자열로 변환
/// </summary>
public class PolicyAnalyzerService
{
    // ── 1. 충돌 감지 ────────────────────────────────────────────────────────

    /// <summary>
    /// 정책 목록에서 잠재적 충돌을 감지한다.
    /// 현재 구현: 활성·보고 전용 정책만 대상으로, 2개씩 쌍을 비교.
    /// </summary>
    public List<PolicyConflict> DetectConflicts(List<ConditionalAccessPolicyModel> policies)
    {
        var conflicts = new List<PolicyConflict>();

        // 비활성 정책은 비교 대상에서 제외
        var activePolicies = policies
            .Where(p => p.State != "disabled")
            .ToList();

        // O(n²) 비교 – 정책 수가 보통 수십 개이므로 충분히 빠름
        for (int i = 0; i < activePolicies.Count; i++)
        {
            for (int j = i + 1; j < activePolicies.Count; j++)
            {
                var a = activePolicies[i];
                var b = activePolicies[j];

                // 두 정책의 애플리케이션 범위가 겹치지 않으면 충돌 없음
                var overlappingApps = GetOverlappingApps(a, b);
                if (overlappingApps.Count == 0 && !BothTargetAllApps(a, b))
                    continue;

                // 두 정책의 사용자·그룹 범위가 겹치는지 확인
                var overlappingUsers  = GetOverlappingUsers(a, b);
                var overlappingGroups = GetOverlappingGroups(a, b);

                bool userScopeOverlaps = BothTargetAllUsers(a, b)
                    || overlappingUsers.Count > 0
                    || overlappingGroups.Count > 0;

                if (!userScopeOverlaps)
                    continue;

                // 범위가 겹치므로 제어 충돌 여부 판단
                var conflict = AnalyzeControlConflict(a, b, overlappingUsers, overlappingGroups, overlappingApps);
                if (conflict is not null)
                    conflicts.Add(conflict);
            }
        }

        // 심각도 내림차순 정렬
        return [.. conflicts.OrderByDescending(c => c.Severity)];
    }

    // ── 2. What-If 시뮬레이션 ───────────────────────────────────────────────

    /// <summary>
    /// 특정 사용자·앱 조합에 어떤 정책이 적용되는지 로컬에서 시뮬레이션한다.
    /// 실제 Entra ID 의 What-If API 와 완전히 동일하지 않으며, 학습/개요 파악 목적이다.
    /// (정확한 결과는 Azure Portal > Entra ID > What-If 사용 권장)
    ///
    /// 평가 순서: Block 정책 → Grant Controls 정책 → 제어 없는 허용
    /// </summary>
    /// <param name="condition">시뮬레이션 입력 조건</param>
    /// <param name="allPolicies">전체 CA 정책 목록</param>
    /// <param name="userGroupIds">사용자가 속한 그룹 ID 목록</param>
    public WhatIfResult Simulate(
        WhatIfCondition condition,
        List<ConditionalAccessPolicyModel> allPolicies,
        List<string> userGroupIds)
    {
        var result = new WhatIfResult { Condition = condition };

        foreach (var policy in allPolicies.Where(p => p.State != "disabled"))
        {
            var (applies, reason) = EvaluatePolicy(policy, condition, userGroupIds);

            if (applies)
            {
                var detail = new AppliedPolicyDetail
                {
                    PolicyId       = policy.Id,
                    PolicyName     = policy.DisplayName,
                    PolicyState    = policy.State,
                    AppliedReason  = reason,
                    GrantControls  = policy.GrantControls
                };
                result.AppliedPolicies.Add(detail);

                // Block 정책이 있으면 즉시 차단 (Report-Only 정책은 실제 차단 안 함)
                if (policy.IsBlockPolicy && policy.IsEnabled)
                {
                    result.BlockingPolicy = detail;
                }
            }
            else
            {
                result.NotAppliedPolicies.Add(new NotAppliedPolicyDetail
                {
                    PolicyId = policy.Id,
                    PolicyName = policy.DisplayName,
                    Reason = reason
                });
            }
        }

        // 최종 접근 결정
        if (result.BlockingPolicy is not null)
        {
            result.AccessResult = WhatIfAccessResult.BlockedByPolicy;
        }
        else
        {
            // 적용된 정책들의 Grant Controls 를 집계
            result.AggregatedControls = result.AppliedPolicies
                .Where(p => p.PolicyState == "enabled")
                .SelectMany(p => p.GrantControls)
                .Distinct()
                .ToList();

            result.AccessResult = result.AggregatedControls.Count > 0
                ? WhatIfAccessResult.RequiresControls
                : WhatIfAccessResult.Allowed;
        }

        return result;
    }

    // ── 3. Mermaid 다이어그램 생성 ───────────────────────────────────────────

    /// <summary>
    /// 단일 CA 정책을 Mermaid.js flowchart TD 정의 문자열로 변환한다.
    /// MermaidDiagram 컴포넌트에서 렌더링에 사용한다.
    /// </summary>
    public string GenerateMermaidDiagram(ConditionalAccessPolicyModel policy)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("flowchart TD");

        // 시작 노드
        sb.AppendLine("    START([\"🔐 로그인 요청\"])");

        // 상태 노드
        if (!policy.IsEnabled && !policy.IsReportOnly)
        {
            sb.AppendLine("    START --> DISABLED[\"⏸️ 정책 비활성\"]");
            sb.AppendLine("    style DISABLED fill:#adb5bd,color:#fff");
            return sb.ToString();
        }

        // 사용자 조건 노드
        var userDesc = FormatUserCondition(policy);
        sb.AppendLine($"    START --> USER{{\"👤 사용자 조건\\n{EscapeMermaid(userDesc)}\"}}");;

        // 포함 사용자 기준 → 앱 조건으로
        var appDesc = FormatAppCondition(policy);
        sb.AppendLine($"    USER -->|\"포함 대상\"| APP{{\"📱 앱 조건\\n{EscapeMermaid(appDesc)}\"}}");;
        sb.AppendLine("    USER -->|\"제외 대상\"| SKIP_U([\"⏭ 정책 미적용\"])");

        // 앱 조건 → 플랫폼/위치 or 부여 제어
        if (policy.IncludePlatforms.Count > 0 || policy.IncludeLocations.Count > 0)
        {
            var condDesc = FormatAdditionalConditions(policy);
            sb.AppendLine($"    APP -->|\"앱 일치\"| COND{{\"⚙️ 추가 조건\\n{EscapeMermaid(condDesc)}\"}}");;
            sb.AppendLine("    APP -->|\"앱 불일치\"| SKIP_A([\"⏭ 정책 미적용\"])");
            sb.AppendLine("    COND -->|\"조건 충족\"| GRANT");
            sb.AppendLine("    COND -->|\"조건 불충족\"| SKIP_C([\"⏭ 정책 미적용\"])");
        }
        else
        {
            sb.AppendLine("    APP -->|\"앱 일치\"| GRANT");
            sb.AppendLine("    APP -->|\"앱 불일치\"| SKIP_A([\"⏭ 정책 미적용\"])");
        }

        // Grant Controls 노드
        if (policy.IsBlockPolicy)
        {
            sb.AppendLine("    GRANT[\"제어 판정\"] --> BLOCK([\"🚫 액세스 차단\"])");
            sb.AppendLine("    style BLOCK fill:#dc3545,color:#fff");
        }
        else if (policy.GrantControls.Count > 0)
        {
            var controlsStr = string.Join(", ", policy.GrantControls.Select(FormatGrantControl));
            sb.AppendLine($"    GRANT[\"제어 판정\"] --> REQUIRE([\"✅ 필요: {EscapeMermaid(controlsStr)}\"])");
            if (policy.IsReportOnly)
                sb.AppendLine("    style REQUIRE fill:#fd7e14,color:#fff");
            else
                sb.AppendLine("    style REQUIRE fill:#198754,color:#fff");
        }
        else
        {
            sb.AppendLine("    GRANT[\"제어 판정\"] --> ALLOW([\"✅ 허용 (제어 없음)\"])");
            sb.AppendLine("    style ALLOW fill:#198754,color:#fff");
        }

        // 보고 전용 표시
        if (policy.IsReportOnly)
            sb.AppendLine("    style START fill:#fd7e14,color:#fff");

        return sb.ToString();
    }

    /// <summary>여러 정책의 전체 맵을 하나의 Mermaid 다이어그램으로 생성</summary>
    public string GenerateOverviewDiagram(List<ConditionalAccessPolicyModel> policies)
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("flowchart LR");
        sb.AppendLine("    USER([\"👤 사용자 로그인\"])");

        foreach (var p in policies.Take(15)) // 가독성을 위해 최대 15개
        {
            var safeId = $"P{p.Id.Replace("-", "")}"; // Mermaid 노드 ID 는 특수문자 불가
            var safeName = EscapeMermaid(p.DisplayName.Length > 25
                ? p.DisplayName[..22] + "..."
                : p.DisplayName);

            var fillColor = p.State switch
            {
                "enabled" when p.IsBlockPolicy   => "#dc3545",
                "enabled" when p.RequiresMfa      => "#0d6efd",
                "enabled"                         => "#198754",
                "enabledForReportingButNotEnforced" => "#fd7e14",
                _                                 => "#adb5bd"
            };

            sb.AppendLine($"    USER --> {safeId}[\"{safeName}\"]");
            sb.AppendLine($"    style {safeId} fill:{fillColor},color:#fff");
        }

        return sb.ToString();
    }

    // ── 내부 헬퍼 ───────────────────────────────────────────────────────────

    private static (bool Applies, string Reason) EvaluatePolicy(
        ConditionalAccessPolicyModel policy,
        WhatIfCondition condition,
        List<string> userGroupIds)
    {
        // 1. 사용자 포함 여부 확인
        bool userIncluded = policy.IncludeUsers.Contains("All")
            || policy.IncludeUsers.Contains(condition.UserId)
            || policy.IncludeGroups.Intersect(userGroupIds).Any();

        if (!userIncluded)
            return (false, "사용자가 포함 조건에 해당하지 않음");

        // 2. 사용자 제외 여부 확인
        bool userExcluded = policy.ExcludeUsers.Contains(condition.UserId)
            || policy.ExcludeGroups.Intersect(userGroupIds).Any();

        if (userExcluded)
            return (false, "사용자가 제외 조건에 해당함");

        // 3. 앱 포함 여부 확인
        bool appIncluded = policy.IncludeApplications.Contains("All")
            || policy.IncludeApplications.Contains(condition.ApplicationId)
            || policy.IncludeApplications.Contains("Office365"); // 특수 그룹

        if (!appIncluded)
            return (false, "앱이 포함 조건에 해당하지 않음");

        // 4. 앱 제외 여부 확인
        if (policy.ExcludeApplications.Contains(condition.ApplicationId))
            return (false, "앱이 제외 조건에 해당함");

        // 5. 플랫폼 조건 (지정된 경우만 평가)
        if (condition.Platform is not null && policy.IncludePlatforms.Count > 0)
        {
            bool platformMatch = policy.IncludePlatforms.Contains(condition.Platform, StringComparer.OrdinalIgnoreCase);
            if (!platformMatch)
                return (false, $"플랫폼({condition.Platform})이 포함 조건에 해당하지 않음");
        }

        var reason = BuildApplyReason(policy, condition, userGroupIds);
        return (true, reason);
    }

    private static string BuildApplyReason(
        ConditionalAccessPolicyModel policy,
        WhatIfCondition condition,
        List<string> userGroupIds)
    {
        if (policy.IncludeUsers.Contains("All"))
            return "전체 사용자 대상 정책";

        if (policy.IncludeUsers.Contains(condition.UserId))
            return "사용자가 직접 포함됨";

        var matchedGroup = policy.IncludeGroups.Intersect(userGroupIds).FirstOrDefault();
        if (matchedGroup is not null)
            return $"그룹 멤버십으로 포함됨 (그룹 ID: {matchedGroup[..Math.Min(8, matchedGroup.Length)]}…)";

        return "조건 일치";
    }

    private static PolicyConflict? AnalyzeControlConflict(
        ConditionalAccessPolicyModel a,
        ConditionalAccessPolicyModel b,
        List<string> overlappingUsers,
        List<string> overlappingGroups,
        List<string> overlappingApps)
    {
        // Block vs Allow
        if (a.IsBlockPolicy != b.IsBlockPolicy)
        {
            return new PolicyConflict
            {
                PolicyA = a, PolicyB = b,
                Severity = ConflictSeverity.High,
                Type = ConflictType.BlockVsAllow,
                Description = $"'{a.DisplayName}' 은(는) 차단 정책이고 '{b.DisplayName}' 은(는) 허용 정책입니다. " +
                              "같은 범위의 사용자·앱에 상반된 제어가 적용됩니다.",
                OverlappingUsers = overlappingUsers,
                OverlappingGroups = overlappingGroups,
                OverlappingApplications = overlappingApps
            };
        }

        // MFA vs No-MFA
        if (a.RequiresMfa != b.RequiresMfa && !a.IsBlockPolicy && !b.IsBlockPolicy)
        {
            return new PolicyConflict
            {
                PolicyA = a, PolicyB = b,
                Severity = ConflictSeverity.Medium,
                Type = ConflictType.MfaVsNoMfa,
                Description = $"'{a.DisplayName}' 은(는) MFA 를 요구하고 '{b.DisplayName}' 은(는) 요구하지 않습니다. " +
                              "같은 범위 사용자·앱에 보안 수준 불일치가 발생할 수 있습니다.",
                OverlappingUsers = overlappingUsers,
                OverlappingGroups = overlappingGroups,
                OverlappingApplications = overlappingApps
            };
        }

        // 동일 Grant Controls (중복 정책)
        if (a.GrantControls.Count > 0
            && a.GrantControls.OrderBy(x => x).SequenceEqual(b.GrantControls.OrderBy(x => x)))
        {
            return new PolicyConflict
            {
                PolicyA = a, PolicyB = b,
                Severity = ConflictSeverity.Low,
                Type = ConflictType.DuplicatePolicy,
                Description = $"'{a.DisplayName}' 과 '{b.DisplayName}' 은 동일한 범위에 동일한 제어({string.Join(", ", a.GrantControls)})를 적용합니다. " +
                              "하나의 정책으로 통합을 고려하세요.",
                OverlappingUsers = overlappingUsers,
                OverlappingGroups = overlappingGroups,
                OverlappingApplications = overlappingApps
            };
        }

        return null;
    }

    // ── 범위 겹침 계산 헬퍼 ──────────────────────────────────────────────────

    private static bool BothTargetAllUsers(ConditionalAccessPolicyModel a, ConditionalAccessPolicyModel b)
        => a.TargetsAllUsers && b.TargetsAllUsers;

    private static bool BothTargetAllApps(ConditionalAccessPolicyModel a, ConditionalAccessPolicyModel b)
        => a.TargetsAllApps && b.TargetsAllApps;

    private static List<string> GetOverlappingUsers(ConditionalAccessPolicyModel a, ConditionalAccessPolicyModel b)
    {
        if (a.TargetsAllUsers || b.TargetsAllUsers) return [];
        return a.IncludeUsers.Intersect(b.IncludeUsers).ToList();
    }

    private static List<string> GetOverlappingGroups(ConditionalAccessPolicyModel a, ConditionalAccessPolicyModel b)
    {
        if (a.TargetsAllUsers || b.TargetsAllUsers) return [];
        return a.IncludeGroups.Intersect(b.IncludeGroups).ToList();
    }

    private static List<string> GetOverlappingApps(ConditionalAccessPolicyModel a, ConditionalAccessPolicyModel b)
    {
        if (a.TargetsAllApps || b.TargetsAllApps) return [];
        return a.IncludeApplications.Intersect(b.IncludeApplications).ToList();
    }

    // ── Mermaid 텍스트 포맷 헬퍼 ─────────────────────────────────────────────

    private static string FormatUserCondition(ConditionalAccessPolicyModel p)
    {
        if (p.TargetsAllUsers)
            return $"포함: 전체 사용자\n제외: {(p.ExcludeUsers.Count + p.ExcludeGroups.Count > 0 ? "있음" : "없음")}";
        var include = p.IncludeUsers.Count + p.IncludeGroups.Count;
        return $"포함: {include}개 사용자/그룹";
    }

    private static string FormatAppCondition(ConditionalAccessPolicyModel p)
    {
        if (p.TargetsAllApps)
            return "포함: 모든 앱";
        return $"포함: {p.IncludeApplications.Count}개 앱";
    }

    private static string FormatAdditionalConditions(ConditionalAccessPolicyModel p)
    {
        var parts = new List<string>();
        if (p.IncludePlatforms.Count > 0)
            parts.Add($"플랫폼: {string.Join(", ", p.IncludePlatforms)}");
        if (p.IncludeLocations.Count > 0)
            parts.Add($"위치: {p.IncludeLocations.Count}개");
        return string.Join(" / ", parts);
    }

    private static string FormatGrantControl(string control) => control switch
    {
        "mfa"                   => "MFA",
        "block"                 => "차단",
        "compliantDevice"       => "준수 디바이스",
        "domainJoinedDevice"    => "도메인 가입 디바이스",
        "approvedApplication"   => "승인된 앱",
        "compliantApplication"  => "앱 보호 정책",
        "passwordChange"        => "비밀번호 변경",
        _                       => control
    };

    /// <summary>Mermaid 노드 레이블에서 큰따옴표·줄바꿈을 이스케이프</summary>
    private static string EscapeMermaid(string text)
        => text.Replace("\"", "'").Replace("\n", "\\n");
}
