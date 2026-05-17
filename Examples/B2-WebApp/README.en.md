# B2-WebApp

[한국어](README.md) | English

**Docs**: `docs/blazor/02-hosting-render-modes.md`

Example project for the Blazor **Web App** unified model. Uses a different render mode per page.

## Hosting Model Characteristics

- **Model**: Blazor Web App (.NET 8+)
- **Mixed modes**: Static SSR + Interactive Server + Interactive WebAssembly + Auto
- **Selection**: `@rendermode` specified per page/component

## Program.cs Key Setup

```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode();
```

## How Interactive WebAssembly Works

On the `/interactive-wasm` page, C# code runs inside the browser's .NET WebAssembly runtime, not on the server.

### First visit (cold start)

1. Server responds with initial HTML.
2. Browser downloads `_framework` static assets (`dotnet.js`, runtime files, app DLL, `blazor.boot.json`).
3. .NET runtime starts in the browser and the component becomes interactive.
4. Button clicks and other events are handled directly in the browser from this point.

### Revisit (warm start)

- Already-downloaded `_framework` bundles are cached, so network downloads are greatly reduced.
- The component becomes interactive much faster than on a first visit.

### Why check `_framework` requests?

- The presence of `_framework/*` requests is the easiest signal confirming Interactive WebAssembly is actually working.
- A 404 for `_framework/dotnet.js` most likely indicates a configuration problem where the WASM client bundle is not being served correctly.

### Difference from Interactive Auto

- `InteractiveWebAssembly`: targets WASM execution from the start.
- `InteractiveAuto`: starts with Server mode for fast first-visit, then switches to WASM on revisit once the bundle is cached.

## Role of B2-WebApp.Client

In one sentence: `B2-WebApp.Client` is the project that **builds and packages the code and files that run in the browser**.

### Simple analogy

- `B2-WebApp` (server): the store
- `B2-WebApp.Client`: the package sent to the customer (browser)

The store must prepare and send the package before the customer can use it.

### What this project does

1. Prepares browser executables — generates `_framework` files (`dotnet.js`, `blazor.boot.json`, DLLs, etc.)
2. Hosts browser components — holds UI/logic that runs directly in the browser
3. Links to the server for deployment — the server project references this Client to deliver files to the browser

### Why it matters

- `InteractiveWebAssembly` and `InteractiveAuto` both require these files to start.
- If `B2-WebApp.Client` is missing or the reference is broken, `_framework/dotnet.js` returns 404 and WASM mode does not work.

### How Client generates `_framework`

1. **Client project builds with the WASM SDK** — `B2-WebApp.Client.csproj` uses `Microsoft.NET.Sdk.BlazorWebAssembly`, which generates the `_framework` files during build.
2. **Server project references Client** — `B2-WebApp.csproj` includes a `ProjectReference` to Client, so static asset info is collected during the server build.
3. **Server exposes static assets at runtime** — `app.MapStaticAssets()` in `Program.cs` maps the static asset endpoints.
4. **Browser starts WASM runtime** — after downloading `_framework` assets, the .NET runtime starts in the browser.

### Key source files

- Client builder: `B2-WebApp.Client/B2-WebApp.Client.csproj` (`Sdk="Microsoft.NET.Sdk.BlazorWebAssembly"`)
- Server-Client link: `B2-WebApp/B2-WebApp.csproj` (`<ProjectReference Include="B2-WebApp.Client\B2-WebApp.Client.csproj" />`)
- Static asset entry point: `B2-WebApp/Program.cs` (`app.MapStaticAssets();`)

### Common point of confusion

- The entity that **generates** `_framework` files is the Client build.
- The entity that **serves** `_framework` files over HTTP is the Server app.
- If either is missing, WASM mode will not work.

## Four Render Mode Demo Pages

| Page | Path | rendermode | Notes |
|------|------|-----------|-------|
| StaticSSRDemo | `/static` | None | Server HTML, buttons don't work |
| InteractiveServerDemo | `/interactive-server` | InteractiveServer | SignalR, server-side processing |
| InteractiveWasmDemo | `/interactive-wasm` | InteractiveWebAssembly | Browser-side processing |
| InteractiveAutoDemo | `/interactive-auto` | InteractiveAuto | Auto Server→WASM transition |

## How to Run

```bash
dotnet run --project Examples/B2-WebApp/B2-WebApp.csproj
```

## Testing

1. **Static SSR** (`/static`) — no button clicks; verify `@DateTime.Now` updates on refresh
2. **Interactive Server** (`/interactive-server`) — click count increment → check WebSocket traffic in Network → WS tab
3. **Interactive WebAssembly** (`/interactive-wasm`) — first visit: confirm `_framework/` bundle download; revisit: instant interaction from cache
4. **Interactive Auto** (`/interactive-auto`) — first visit: Server mode; after WASM bundle is cached, revisit switches to WASM

## Comparison with Other Hosting Models

| Item | B2-Server | B2-WebAssembly | B2-WebApp |
|------|-----------|----------------|-----------|
| Execution | Server | Browser | Selectable |
| Server connection | Always required | Not needed | Per mode |
| Initial load | Fast | Slow (bundle download) | Mixed |
| Offline | No | Yes | Per mode |
