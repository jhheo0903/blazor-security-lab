# B2. 호스팅 모델과 렌더링 모드

## 공식 개념 요약(Microsoft docs 기준)

Blazor Web App은 렌더 모드로 "실행 위치"와 "인터랙션 가능 여부"를 제어합니다.

| 모드 | 실행 위치 | 인터랙션 | 특성 |
| --- | --- | --- | --- |
| Static SSR | 서버 | 불가 | 초기 HTML 중심 |
| Interactive Server | 서버 | 가능 | SignalR circuit 기반 |
| Interactive WebAssembly | 클라이언트 | 가능 | 브라우저 런타임 기반 |
| Interactive Auto | 서버 -> 클라이언트 | 가능 | 초기 서버, 이후 클라이언트 선택 가능 |

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

## @rendermode 적용

```razor
<Dialog @rendermode="InteractiveServer" />
```

```razor
@page "/example"
@rendermode InteractiveWebAssembly
```

## Prerender 핵심

- Interactive 모드는 기본적으로 prerender가 활성화됩니다.
- 초기에는 정적 HTML, 이후 interactive 연결이 붙습니다.
- 필요 시 비활성화할 수 있습니다.

```razor
<Routes @rendermode="new InteractiveServerRenderMode(prerender: false)" />
```

## 자주 발생하는 실수

- prerender 중 클라이언트 전용 서비스를 바로 주입해 런타임 예외 발생
- 부모/자식 컴포넌트 간 호환되지 않는 render mode 혼합
- mode 전파 규칙을 모른 채 child에서 강제 전환 시도

## 참고

- https://learn.microsoft.com/aspnet/core/blazor/components/render-modes
- https://learn.microsoft.com/aspnet/core/blazor/components/prerender
