# CAPolicyLab 실습 가이드

이 문서는 C# 개발자가 Blazor 사고방식으로 전환하도록 돕는 단계형 실습 가이드입니다.
실습은 기능 구현보다 "생각의 연결"에 초점을 둡니다.

## 루트 문서 연계

루트 기준 문서: [README.md](../README.md)

| 루트 코드 | 루트 주제 | 상세 문서 | CAPolicyLab 적용 | 근거 파일 |
| --- | --- | --- | --- | --- |
| B2 | 호스팅 모델과 렌더링 모드 | [02-hosting-render-modes.md](../docs/blazor/02-hosting-render-modes.md) | Interactive Server 모드 사용 | [Program.cs](Program.cs) |
| B3 | 컴포넌트 구조와 라우팅 | [03-components-routing.md](../docs/blazor/03-components-routing.md) | Pages/Shared 분리, App/Routes 라우팅 구성 | [Components/App.razor](Components/App.razor), [Components/Routes.razor](Components/Routes.razor) |
| B4 | 상태, 이벤트, 데이터 바인딩 | [04-state-events-binding.md](../docs/blazor/04-state-events-binding.md) | 페이지 컴포넌트에서 상태와 이벤트 처리 | [Components/Pages](Components/Pages) |
| B6 | DI, 서비스 분리, 상태관리 | [06-di-services-state.md](../docs/blazor/06-di-services-state.md) | Services 계층으로 로직 분리 및 DI 사용 | [Services](Services), [Program.cs](Program.cs) |
| B10 | 코딩 스타일 가이드 | [10-coding-style.md](../docs/blazor/10-coding-style.md) | 컴포넌트/서비스/모델 책임 분리 실습 | [Components](Components), [Models](Models), [Services](Services) |

## 이 프로젝트의 호스팅 방식

CAPolicyLab은 Blazor Interactive Server 방식으로 동작합니다.

- 서비스 등록: AddInteractiveServerComponents
- 렌더 모드 매핑: AddInteractiveServerRenderMode

확인한 코드 위치:

- [CAPolicyLab/Program.cs](Program.cs)

판별 예시:

```csharp
builder.Services.AddRazorComponents()
  .AddInteractiveServerComponents();

app.MapRazorComponents<App>()
  .AddInteractiveServerRenderMode();
```

의미:
- UI 이벤트 처리는 서버에서 실행됩니다.
- 브라우저와 서버는 실시간 연결을 통해 상호작용합니다.
- 이 프로젝트 실습에서는 서버 중심 Blazor 동작을 기준으로 상태/렌더링 흐름을 관찰하면 됩니다.

## 실습 목표

- 컴포넌트 중심 UI 사고 익히기
- 상태 변화와 렌더링 관계 이해하기
- 화면 코드와 서비스 코드를 역할별로 분리하기

## 실습 흐름도

```mermaid
flowchart TD
  A[1 구조 읽기] --> B[2 상태 읽기]
  B --> C[3 서비스 읽기]
  C --> D[4 변화 만들기]
  D --> E[5 설명하기]

  A1[Program.cs 진입점 확인] --> A
  A2[App.razor Routes.razor 흐름 확인] --> A
  A3[Pages 와 Shared 역할 구분] --> A

  B1[@code 상태 변수 찾기] --> B
  B2[@bind 입력 연결 확인] --> B
  B3[이벤트 핸들러 실행 경로 추적] --> B

  C1[@inject 위치 확인] --> C
  C2[Services 호출 흐름 확인] --> C
  C3[Models 와 UI 매핑 확인] --> C
```

## 단계별 실습

### 1단계: 구조 읽기

1. Program 진입점 확인: Program.cs
2. 라우팅 연결 확인: Components/App.razor, Components/Routes.razor
3. 화면 그룹 확인: Components/Pages, Components/Shared

체크 질문:
- 페이지 컴포넌트와 재사용 컴포넌트의 경계가 명확한가?
- 라우팅은 어디서 결정되는가?

### 2단계: 상태 읽기

1. Pages/Home.razor 또는 Pages/PolicyFilter.razor 열기
2. 상태 변수와 이벤트 핸들러를 먼저 표시해 보기
3. 입력값이 어떤 속성에 바인딩되는지 추적

체크 질문:
- 상태 변경 지점이 한눈에 보이는가?
- 이 상태가 바뀌면 어떤 UI가 바뀔지 예측 가능한가?

### 3단계: 서비스 읽기

1. Services/PolicyAnalyzerService.cs 열기
2. 페이지에서 서비스 호출 지점 찾기
3. Models/ConditionalAccessPolicyModel.cs와 결과 모델이 화면에 반영되는 경로 확인

체크 질문:
- UI 로직과 도메인 로직이 섞이지 않았는가?
- 서비스 메서드 입력/출력이 화면 요구사항과 맞는가?

### 4단계: 변화 만들기

1. 페이지에 작은 필터 UI 하나 추가
2. 기존 서비스 메서드 결과를 바탕으로 표시 조건 추가
3. 변경 전후 동작을 직접 확인

체크 질문:
- 변경 범위가 컴포넌트 내부로 제한되었는가?
- 서비스 수정이 필요하면 최소 변경으로 끝났는가?

### 5단계: 설명하기

아래를 문장으로 설명해 보세요.

- 이 페이지의 핵심 상태는 무엇인가?
- 어떤 이벤트가 어떤 상태를 바꾸는가?
- 서비스 분리가 유지보수에 어떤 이점을 주는가?

## 실행

```bash
dotnet restore
dotnet run --project CAPolicyLab.csproj
```

실행 후 각 페이지를 이동하며 "상태 -> 렌더링" 흐름을 관찰하세요.
