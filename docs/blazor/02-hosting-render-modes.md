# B2. 호스팅 모델과 렌더링 모드

[한국어](02-hosting-render-modes.md) | [English](02-hosting-render-modes.en.md)

## 공식 개념 요약(Microsoft docs 기준)

Blazor Web App은 렌더 모드로 "실행 위치"와 "인터랙션 가능 여부"를 제어합니다.

| 모드 | 실행 위치 | 인터랙션 | 특성 |
| --- | --- | --- | --- |
| Static SSR | 서버 | 불가 | 초기 HTML 중심, 이벤트 처리 없음 |
| Interactive Server | 서버 | 가능 | SignalR circuit 기반 실시간 연결 |
| Interactive WebAssembly | 클라이언트 | 가능 | 브라우저 .NET 런타임 기반 |
| Interactive Auto | 서버 → 클라이언트 | 가능 | 초기 서버, WASM 다운로드 후 클라이언트 전환 |

## 호스팅 모델 3종

### 1. Blazor Server
서버에서 렌더링하고, SignalR을 통해 클라이언트와 실시간 연결을 유지합니다.

- 장점: 초기 로딩 빠름, 클라이언트 리소스 절감
- 단점: 서버 연결 상태 유지 필요, 지연(latency) 민감

### 2. Blazor WebAssembly
.NET 런타임과 앱 코드가 브라우저에 다운로드되어 클라이언트에서 실행됩니다.

- 장점: 서버 연결 불필요, 오프라인 가능
- 단점: 초기 번들 다운로드 시간, 브라우저 리소스 사용

### 3. Blazor Web App (.NET 8+)
Static SSR + Interactive 모드를 혼합할 수 있는 통합 모델입니다.
페이지/컴포넌트 단위로 렌더 모드를 선택합니다.

## Program.cs 설정 예시

### Interactive Server

```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
```

### Interactive WebAssembly

```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveWebAssemblyComponents();

app.MapRazorComponents<App>()
    .AddInteractiveWebAssemblyRenderMode();
```

### 혼합 모드 기반(Server + WASM)

```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode();
```

## @rendermode 적용 위치

### 컴포넌트 인스턴스에 적용

```razor
<Dialog @rendermode="InteractiveServer" />
```

### 페이지 컴포넌트 선언에 적용

```razor
@page "/example"
@rendermode InteractiveServer
```

### 앱 전역 적용 (App.razor)

```razor
<Routes @rendermode="InteractiveServer" />
<HeadOutlet @rendermode="InteractiveServer" />
```

## Razor 화면 렌더링 경로와 RenderMode 적용 지점

호스팅/렌더링 모드를 이해할 때는 컴포넌트 렌더링 경로를 함께 보는 것이 중요합니다.

1. `Program.cs`: `MapRazorComponents<App>()` + `AddInteractive*RenderMode()` 등록
2. `App.razor`: `<Routes />`와 `<HeadOutlet />` 렌더링
3. `Routes.razor`(또는 App 내부 Router): URL 매칭과 페이지 선택
4. `MainLayout.razor`: 공통 프레임 렌더링
5. `@Body`: 실제 페이지(`Pages/*.razor`) 렌더링

즉, `@rendermode`는 페이지 단위/컴포넌트 단위에서 선언되지만, 실제 화면 표시 흐름은 위 컴포넌트 트리를 따라 진행됩니다.

## Prerender 상세

Interactive 모드는 기본적으로 **prerender가 활성화**됩니다.

렌더링 흐름:
1. 서버에서 정적 HTML 즉시 반환 (= prerender)
2. 클라이언트에서 interactive 연결 수립
3. 이후 이벤트 처리 가능

### Prerender 비활성화

```razor
<Routes @rendermode="new InteractiveServerRenderMode(prerender: false)" />
<HeadOutlet @rendermode="new InteractiveServerRenderMode(prerender: false)" />
```

### 런타임에서 interactive 상태 판별

```razor
@if (!RendererInfo.IsInteractive)
{
    <p>연결 중...</p>
}
else
{
    <button @onclick="Send">전송</button>
}

@code {
    private void Send() { }
}
```

## 렌더 모드 전파 규칙

- 부모가 Interactive Server면 자식도 Interactive Server로 동작합니다.
- 자식은 부모보다 더 제한적인 모드는 지정할 수 없습니다.
- Static SSR 하위에서 Interactive 자식을 사용하면 오류가 발생합니다.

## 자주 발생하는 실수

| 실수 | 원인 | 해결 |
| --- | --- | --- |
| prerender 중 JS 호출 실패 | interactive 연결 전 JS 호출 | `OnAfterRenderAsync(firstRender)` 내에서 호출 |
| 클라이언트 전용 서비스 주입 오류 | prerender 시점에 브라우저 없음 | optional 주입 또는 서버 측 추상화 제공 |
| 부모-자식 렌더 모드 불일치 | 모드 전파 규칙 미인지 | 컴포넌트 계층 단위로 모드 설계 |

## 참고

- https://learn.microsoft.com/aspnet/core/blazor/components/render-modes
- https://learn.microsoft.com/aspnet/core/blazor/components/prerender
