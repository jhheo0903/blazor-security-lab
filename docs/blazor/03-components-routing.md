# B3. 컴포넌트 구조와 라우팅

## 권장 폴더 전략

| 폴더 | 역할 |
| --- | --- |
| Pages/ | 라우팅되는 화면 컴포넌트 |
| Shared/ | 재사용 UI 컴포넌트 |
| Layout/ | 앱 공통 프레임(헤더, 사이드바 등) |
| Components/ | 기능 단위 하위 컴포넌트 |

## 라우팅 기본

```razor
@page "/policies"
<h3>정책 목록</h3>
```

- `@page`는 URL과 컴포넌트를 매핑합니다.
- 라우팅은 `<Router>` 컴포넌트가 처리하며, 프로젝트 구조에 따라 `App.razor` 또는 `Routes.razor`에 위치합니다.

## Razor 화면 렌더링 이동 흐름

실제 화면은 아래 순서로 조합되어 렌더링됩니다.

1. `Program.cs`에서 `App`를 루트 컴포넌트로 등록
2. `App.razor`에서 `<Routes />` 호출
3. `Routes.razor`의 `<Router>`가 URL 매칭
4. `<RouteView>`가 `DefaultLayout`(예: `MainLayout`) 적용
5. `MainLayout.razor`의 `@Body`에 매칭된 `Pages/*.razor` 렌더링

```razor
<!-- Routes.razor 예시 -->
<Router AppAssembly="typeof(Program).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)" />
    </Found>
</Router>
```

### 라우트 파라미터

```razor
@page "/policy/{id}"

<h3>정책 ID: @Id</h3>

@code {
    [Parameter]
    public string Id { get; set; } = string.Empty;
}
```

### 라우트 제약 조건

```razor
@page "/policy/{id:int}"   ← int만 허용, 문자열이면 404 처리
```

주요 제약: `int`, `long`, `guid`, `bool`, `datetime`, `decimal`

### 선택적 라우트 파라미터

```razor
@page "/items/{page:int?}"

@code {
    [Parameter]
    public int? Page { get; set; }
}
```

## 복수 라우트 선언

하나의 컴포넌트에 여러 URL을 매핑할 수 있습니다.

```razor
@page "/policies"
@page "/policies/all"
```

## 네비게이션

### NavLink

`<NavLink>`는 현재 URL과 일치하면 `active` 클래스를 자동으로 추가합니다.

```razor
<NavLink href="/policies" Match="NavLinkMatch.All">정책 목록</NavLink>
<NavLink href="/policies" Match="NavLinkMatch.Prefix">정책</NavLink>
```

- `NavLinkMatch.All`: 전체 URL이 일치할 때만 active
- `NavLinkMatch.Prefix`: URL이 해당 경로로 시작하면 active

### NavigationManager

코드에서 프로그래밍 방식으로 이동합니다.

```razor
@inject NavigationManager Nav

<button @onclick="GoToDetail">상세 보기</button>

@code {
    private void GoToDetail()
    {
        Nav.NavigateTo("/policy/123");
    }
}
```

쿼리 문자열 포함 이동:

```razor
Nav.NavigateTo($"/policies?page=2&filter=active");
```

현재 URL 확인:

```razor
@inject NavigationManager Nav

@code {
    protected override void OnInitialized()
    {
        var current = Nav.Uri;   // 전체 URL
        var relative = Nav.ToBaseRelativePath(Nav.Uri);  // 상대 경로
    }
}
```

## 레이아웃

기본 레이아웃은 `MainLayout.razor`에서 정의합니다.

```razor
<!-- MainLayout.razor -->
@inherits LayoutComponentBase

<div class="layout">
    <nav>@* 내비게이션 *@</nav>
    <main>@Body</main>   ← 페이지 컴포넌트가 여기에 렌더링
</div>
```

특정 페이지에서 다른 레이아웃 적용:

```razor
@page "/admin"
@layout AdminLayout
```

## 컴포넌트 책임 분리

- **페이지 컴포넌트**: 화면 흐름, 사용자 액션 오케스트레이션
- **공유 컴포넌트**: 반복 UI/표현 규칙
- **서비스**: 계산/외부 연동/데이터 변환

## 파라미터 전달 패턴

```razor
<PolicyCard Policy="selected" OnApply="HandleApply" />
```

```razor
@code {
    [Parameter]
    public PolicyModel? Policy { get; set; }

    [Parameter]
    public EventCallback<PolicyModel> OnApply { get; set; }
}
```

## RenderFragment로 슬롯 패턴

자식 컴포넌트 내에 부모가 정의한 내용을 렌더링할 수 있습니다.

```razor
<!-- Card.razor -->
<div class="card">
    <div class="card-body">
        @ChildContent   ← 슬롯
    </div>
</div>

@code {
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

```razor
<!-- 사용 -->
<Card>
    <p>이 내용이 카드 안에 들어갑니다.</p>
</Card>
```

## 체크리스트

- 재사용 가능한 부분이 Shared로 분리되어 있는가?
- 페이지가 서비스 구현 상세를 과도하게 알고 있지 않은가?
- 라우트 이름이 도메인 의미를 잘 드러내는가?
- 404 처리를 위해 `NotFound.razor`가 적절히 구성되어 있는가?

## 참고

- https://learn.microsoft.com/aspnet/core/blazor/fundamentals/routing
- https://learn.microsoft.com/aspnet/core/blazor/components/layouts
