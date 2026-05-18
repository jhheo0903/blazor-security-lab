# CAPolicyLab

[한국어](README.md) | [English](README.en.md)

Microsoft Entra ID 조건부 액세스(Conditional Access) 정책을 Blazor Interactive Server로 시각화·분석하는 실습 프로젝트입니다.
Graph API를 직접 호출해 정책을 가져오고, 충돌 감지·What-If 시뮬레이션·Mermaid 플로우차트 생성을 로컬에서 수행합니다.

---

## 사전 지식

이 프로젝트를 이해하려면 아래 개념을 먼저 파악해 두면 좋습니다.

### Microsoft Entra ID / Azure AD

| 개념 | 설명 |
|---|---|
| **테넌트(Tenant)** | 조직 단위의 Azure AD 디렉터리. TenantId(GUID)로 식별 |
| **앱 등록(App Registration)** | API를 호출할 클라이언트 앱을 Entra ID에 등록하는 것. ClientId 발급 |
| **클라이언트 자격 증명 흐름** | 사용자 로그인 없이 앱 자체 권한으로 API를 호출하는 OAuth 2.0 흐름. ClientSecret 사용 |
| **API 권한(앱 권한)** | 관리자가 사전 동의(Admin Consent)해야 유효해지는 서비스 단위 권한 |

이 프로젝트에 필요한 앱 권한:

- `Policy.Read.All` — 조건부 액세스 정책 읽기
- `Directory.Read.All` — 사용자·그룹·앱 정보 읽기

### 조건부 액세스(Conditional Access)

CA 정책은 **누가(Users/Groups)** · **어떤 앱에(Applications)** · **어떤 조건에서(Platforms, Locations)** 접근할 때 **어떤 제어(Grant Controls)** 를 적용할지를 정의합니다.

| 상태 | 의미 |
|---|---|
| `enabled` | 실제 적용. 제어 조건 미충족 시 접근 차단 |
| `disabled` | 비활성. 평가 대상에서 제외 |
| `enabledForReportingButNotEnforced` | Report-Only. 로그만 기록, 실제 차단 없음 |

Grant Controls 예시: `mfa`(MFA 필요), `block`(접근 차단), `compliantDevice`(준수 디바이스 필요)

### Microsoft Graph API

REST 기반 API로 Microsoft 365 / Azure AD 리소스를 조회·관리합니다.

- CA 정책 엔드포인트: `GET /identity/conditionalAccess/policies`
- 사용자: `GET /users`
- 서비스 주체(앱): `GET /servicePrincipals`
- 중첩 그룹 멤버십: `GET /users/{id}/transitiveMemberOf`

이 프로젝트는 Graph SDK v5(Kiota 기반)를 사용하며, 페이지 기반 응답은 `PageIterator`로 전체 수집합니다.

### Blazor Interactive Server

UI 이벤트를 서버에서 처리하고, 브라우저와 SignalR로 실시간 통신합니다.
컴포넌트 생명주기(`OnInitializedAsync`), `@bind` 양방향 바인딩, `@inject` DI, `@rendermode InteractiveServer`가 핵심 개념입니다.

---

## 프로젝트 구조

```
CAPolicyLab/
├── Program.cs                   # 앱 진입점, DI 등록, 미들웨어 파이프라인
├── Components/
│   ├── App.razor / Routes.razor # 라우팅 루트
│   ├── Layout/                  # MainLayout, NavMenu
│   ├── Pages/
│   │   ├── Home.razor           # 대시보드 (KPI 카드, 정책 목록)
│   │   ├── PolicyFilter.razor   # 사용자·앱 기준 필터링
│   │   ├── PolicyVisualizer.razor  # 정책 Mermaid 플로우차트
│   │   ├── ConflictDetector.razor  # 정책 간 충돌 감지
│   │   ├── WhatIfSimulator.razor   # 접근 정책 시뮬레이션
│   │   └── Setup.razor          # Azure AD 앱 자격 증명 설정 UI
│   └── Shared/
│       ├── PolicyCard.razor     # 정책 카드 재사용 컴포넌트
│       ├── ConfigGuard.razor    # 설정 미완료 시 접근 차단 가드
│       └── MermaidDiagram.razor # Mermaid.js 렌더링 래퍼
├── Services/
│   ├── GraphService.cs          # Graph API 호출 + 5분 메모리 캐시
│   ├── PolicyAnalyzerService.cs # 충돌 감지, What-If, Mermaid 생성 (로컬 분석)
│   ├── AppConfigService.cs      # appsettings 읽기/쓰기, 연결 테스트
│   └── AppRegistrationService.cs
└── Models/
    ├── ConditionalAccessPolicyModel.cs  # Graph SDK 타입 → 내부 뷰 모델
    ├── PolicyConflict.cs
    └── WhatIfResult.cs
```

---

## 주요 기능

| 기능 | 설명 |
|---|---|
| **대시보드** | 전체·활성·보고전용 정책 수, Grant Control 분포, 감지된 충돌 수 KPI |
| **정책 필터** | 사용자 ID · 앱 ID · 상태로 정책 목록 필터링 |
| **정책 시각화** | 단일 정책을 Mermaid 플로우차트로 렌더링 (조건 → 제어 흐름) |
| **충돌 감지** | Block vs Allow, MFA vs No-MFA, 중복 정책을 심각도별로 탐지 |
| **What-If 시뮬레이터** | 사용자·앱 조합에 적용되는 정책과 최종 접근 결과 예측 |
| **설정 마법사** | 브라우저 UI에서 TenantId·ClientId·ClientSecret 입력 및 연결 테스트 |

---

## 기술 스택

| 항목 | 내용 |
|---|---|
| 런타임 | .NET 10, Blazor Interactive Server |
| Graph SDK | `Microsoft.Graph` 5.105.0 (Kiota 기반) |
| 인증 | `Azure.Identity` 1.13.2 — `ClientSecretCredential` |
| 캐시 | `IMemoryCache` (5분 TTL) |
| UI | Bootstrap 5, Bootstrap Icons, Mermaid.js |

---

## 설정 (Azure 앱 등록)

### 1. 앱 등록

1. [Azure Portal](https://portal.azure.com) → **Entra ID** → **앱 등록** → **새 등록**
2. 이름 지정 후 등록. **애플리케이션(클라이언트) ID**와 **디렉터리(테넌트) ID** 복사
3. **인증서 및 암호** → 새 클라이언트 암호 생성. 값 즉시 복사 (이후 재조회 불가)

### 2. API 권한 부여

**API 권한** → **권한 추가** → **Microsoft Graph** → **애플리케이션 권한**에서 아래 항목 추가 후 **관리자 동의** 클릭:

- `Policy.Read.All`
- `Directory.Read.All`

### 3. 자격 증명 입력

앱 실행 후 `/setup` 페이지에서 위 값을 입력하거나, `appsettings.Development.json`에 직접 작성:

```json
{
  "AzureAd": {
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret"
  }
}
```

> 설정 전에도 앱은 정상 실행됩니다. 미설정 상태에서는 대시보드에 안내 배너가 표시되고 Graph API 호출은 차단됩니다.

---

## 실행

```bash
dotnet restore
dotnet run --project CAPolicyLab/CAPolicyLab.csproj
```

---

## 루트 문서 연계

루트: [README.md](../README.md)

| 루트 코드 | 주제 | 상세 문서 | CAPolicyLab 적용 |
|---|---|---|---|
| B2 | 호스팅 모델과 렌더링 모드 | [02-hosting-render-modes.md](../docs/blazor/02-hosting-render-modes.md) | Interactive Server 모드 |
| B3 | 컴포넌트 구조와 라우팅 | [03-components-routing.md](../docs/blazor/03-components-routing.md) | Pages/Shared 분리, App/Routes |
| B4 | 상태, 이벤트, 데이터 바인딩 | [04-state-events-binding.md](../docs/blazor/04-state-events-binding.md) | 페이지 컴포넌트 상태 관리 |
| B6 | DI, 서비스 분리, 상태관리 | [06-di-services-state.md](../docs/blazor/06-di-services-state.md) | Services 계층 DI 분리 |
| B10 | 코딩 스타일 가이드 | [10-coding-style.md](../docs/blazor/10-coding-style.md) | 컴포넌트·서비스·모델 책임 분리 |


