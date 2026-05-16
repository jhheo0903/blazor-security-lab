# B2-WebApp

**문서**: `docs/blazor/02-hosting-render-modes.md`

Blazor **Web App** 통합 모델 예제 프로젝트입니다. 페이지마다 다른 렌더 모드를 사용합니다.

## 호스팅 모델 특징

- **모델**: Blazor Web App (.NET 8+)
- **혼합 모드**: Static SSR + Interactive Server + Interactive WebAssembly + Auto
- **선택 방식**: 페이지/컴포넌트 단위로 `@rendermode` 지정

## Program.cs 핵심 설정

```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()    // Server 모드 활성화
    .AddInteractiveWebAssemblyComponents(); // WASM 모드 활성화

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode();
```

## Interactive WebAssembly 동작 원리

`/interactive-wasm` 페이지는 C# 코드가 서버가 아니라 브라우저 안의 .NET WebAssembly 런타임에서 실행됩니다.

### 첫 방문(콜드 스타트)

1. 서버가 초기 HTML을 응답합니다.
2. 브라우저가 `_framework` 정적 리소스를 다운로드합니다.
   - 예: `dotnet.js`, 런타임 파일, 앱 DLL, `blazor.boot.json`
3. 브라우저에서 .NET 런타임이 시작되고 컴포넌트가 interactive 상태가 됩니다.
4. 이후 버튼 클릭 같은 이벤트는 브라우저에서 직접 처리됩니다.

### 재방문(웜 스타트)

- 이미 받은 `_framework` 번들이 브라우저 캐시에 있으면 네트워크 다운로드가 크게 줄어듭니다.
- 그래서 첫 방문보다 훨씬 빠르게 interactive 상태가 됩니다.

### 왜 `_framework` 요청을 확인하나요?

- Interactive WebAssembly가 실제로 동작하는지 확인하는 가장 쉬운 신호가 `_framework/*` 요청입니다.
- 만약 `_framework/dotnet.js`가 404라면 WASM 클라이언트 번들이 제대로 제공되지 않는 구성 문제일 가능성이 큽니다.

### Interactive Auto와 차이

- `InteractiveWebAssembly`: 처음부터 WASM 실행을 목표로 합니다.
- `InteractiveAuto`: 첫 방문은 Server로 빠르게 시작하고, WASM 번들이 준비되면 재방문 시 WASM으로 전환됩니다.

## B2-WebApp.Client의 역할

한 줄로 말하면, `B2-WebApp.Client`는 **브라우저에서 돌아갈 코드와 파일을 만들어 주는 프로젝트**입니다.

### 쉽게 비유하면

- `B2-WebApp`(서버): 가게
- `B2-WebApp.Client`: 손님(브라우저)에게 보낼 포장 세트

가게가 포장 세트를 준비해 보내야, 손님 쪽에서 바로 사용할 수 있습니다.

### 이 프로젝트가 하는 일

1. 브라우저 실행 파일 준비
   - `_framework` 아래 파일들(`dotnet.js`, `blazor.boot.json`, DLL 등)을 만듭니다.
2. 브라우저용 컴포넌트 보관
   - 브라우저에서 직접 동작할 UI/로직을 담습니다.
3. 서버가 배포하게 연결
   - 서버 프로젝트가 이 Client를 참조해서 브라우저에 파일을 전달합니다.

### 왜 중요한가요?

- `InteractiveWebAssembly`, `InteractiveAuto`는 이 파일들이 있어야 시작됩니다.
- `B2-WebApp.Client`가 없거나 연결이 깨지면 `_framework/dotnet.js` 404가 나고, WASM 모드가 동작하지 않습니다.

### "Client가 어떻게 _framework를 만드나요?"

아래 순서로 동작합니다.

1. **Client 프로젝트가 WASM SDK로 빌드됨**
   - `B2-WebApp.Client.csproj`는 `Microsoft.NET.Sdk.BlazorWebAssembly`를 사용합니다.
   - 이 SDK가 빌드 중 `_framework`에 들어갈 파일(`dotnet.js`, `blazor.boot.json`, DLL 등)을 생성합니다.

2. **Server 프로젝트가 Client를 참조함**
   - `B2-WebApp.csproj`의 `ProjectReference`로 Client를 연결합니다.
   - 그래서 서버 빌드 시 Client 정적 자산 정보도 함께 수집됩니다.

3. **런타임에서 서버가 정적 자산을 노출함**
   - `Program.cs`의 `app.MapStaticAssets();`가 정적 자산 엔드포인트를 매핑합니다.
   - 브라우저가 `/_framework/dotnet.js`를 요청하면 서버가 이 자산을 찾아 응답합니다.

4. **브라우저가 WASM 런타임 시작**
   - `_framework` 자산 다운로드 후 .NET 런타임이 브라우저에서 시작됩니다.
   - 이후 인터랙션은 브라우저에서 처리됩니다.

### 코드 기준으로 보면

- Client 생성 주체: `B2-WebApp.Client/B2-WebApp.Client.csproj`
  - `Sdk="Microsoft.NET.Sdk.BlazorWebAssembly"`
- Server-Client 연결: `B2-WebApp/B2-WebApp.csproj`
  - `<ProjectReference Include="B2-WebApp.Client\B2-WebApp.Client.csproj" />`
- 정적 자산 서빙 시작점: `B2-WebApp/Program.cs`
  - `app.MapStaticAssets();`

### 자주 헷갈리는 포인트

- `_framework` 파일을 "생성"하는 주체는 Client 빌드입니다.
- `_framework` 파일을 "HTTP로 제공"하는 주체는 Server 앱입니다.
- 둘 중 하나라도 빠지면 WASM 모드가 정상 동작하지 않습니다.

## 4개 렌더 모드 데모 페이지

| 페이지 | 경로 | rendermode | 특징 |
|--------|------|-----------|------|
| StaticSSRDemo | `/static` | 없음 | 서버 HTML, 버튼 작동 안 함 |
| InteractiveServerDemo | `/interactive-server` | InteractiveServer | SignalR, 서버 처리 |
| InteractiveWasmDemo | `/interactive-wasm` | InteractiveWebAssembly | 브라우저 처리 |
| InteractiveAutoDemo | `/interactive-auto` | InteractiveAuto | Server→WASM 자동 전환 |

## 실행 방법

```bash
dotnet run --project Examples/B2-WebApp/B2-WebApp.csproj
```

## 테스트 방법

1. **Static SSR** (`/static`)
   - 버튼 클릭 없음, `@DateTime.Now` 새로고침 시 갱신 확인

2. **Interactive Server** (`/interactive-server`)
   - 카운트 증가 클릭 → Network에 WebSocket 트래픽 확인
   - 브라우저 DevTools → Network → WS 탭

3. **Interactive WebAssembly** (`/interactive-wasm`)
   - 처음 방문 시 `_framework/` 번들 다운로드 확인
   - 재방문 시 캐시 사용, 즉시 인터랙션

4. **Interactive Auto** (`/interactive-auto`)
   - 첫 방문: Server 모드로 시작
   - WASM 번들 캐시 후 재방문: WASM으로 전환

## 다른 호스팅 모델과 비교

| 항목 | B2-Server | B2-WebAssembly | B2-WebApp |
|------|-----------|----------------|-----------|
| 실행 위치 | 서버 | 브라우저 | 선택 가능 |
| 서버 연결 | 항상 필요 | 불필요 | 모드별 |
| 초기 로딩 | 빠름 | 느림 (번들 다운로드) | 혼합 |
| 오프라인 | 안 됨 | 가능 | 모드별 |
