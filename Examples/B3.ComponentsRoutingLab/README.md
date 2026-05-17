# B3 Components Routing Lab

[한국어](README.md) | [English](README.en.md)

이 프로젝트는 루트 기준서의 B3(컴포넌트 구조와 라우팅)를 빠르게 체험하기 위한 최소 예제입니다.

## 루트 문서 연계

루트 기준 문서: [README.md](../../README.md)

| 루트 코드 | 루트 주제 | 상세 문서 | B3 예제 적용 | 근거 파일 |
| --- | --- | --- | --- | --- |
| B3 | 컴포넌트 구조와 라우팅 | [03-components-routing.md](../../docs/blazor/03-components-routing.md) | `@page`, 라우트 파라미터, 다중 라우트, NavLink 구성 | [Components/Routes.razor](Components/Routes.razor), [Components/Layout/NavMenu.razor](Components/Layout/NavMenu.razor), [Components/Pages/RouteParameterDemo.razor](Components/Pages/RouteParameterDemo.razor), [Components/Pages/MultiRouteDemo.razor](Components/Pages/MultiRouteDemo.razor) |
| B4 | 상태, 이벤트, 데이터 바인딩 | [04-state-events-binding.md](../../docs/blazor/04-state-events-binding.md) | Shared 컴포넌트 파라미터와 EventCallback 상호작용 | [Components/Pages/ComponentCompositionDemo.razor](Components/Pages/ComponentCompositionDemo.razor), [Components/Shared/PolicyCard.razor](Components/Shared/PolicyCard.razor) |

## 예제 구성

- Home: 예제 목적과 페이지 안내
- Route Parameter: `/policy/{id:int}` 라우트 파라미터 바인딩
- Multi Route: `/policies`, `/policies/all`, `/items/{page:int?}` 다중/선택 라우트
- Composition: Shared 컴포넌트 파라미터와 EventCallback 처리

## 테스트 방법

1. 아래 명령으로 프로젝트를 실행합니다.
2. Home에서 안내를 확인한 뒤 Route Parameter, Multi Route, Composition 순서로 이동합니다.
3. Route Parameter에서 URL `policy/101`, `policy/999`로 바꿨을 때 바인딩된 ID가 표시되는지 확인합니다.
4. Multi Route에서 `/policies`, `/policies/all`, `/items`, `/items/3`이 모두 정상 렌더링되는지 확인합니다.
5. Composition에서 카드의 Apply 버튼을 누르면 선택 정책 메시지가 갱신되는지 확인합니다.

```bash
dotnet restore
dotnet run --project Examples/B3.ComponentsRoutingLab/B3.ComponentsRoutingLab.csproj
```

빌드만 확인하려면 아래 명령을 사용합니다.

```bash
dotnet build Examples/B3.ComponentsRoutingLab/B3.ComponentsRoutingLab.csproj
```
