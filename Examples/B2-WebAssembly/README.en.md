# B2-WebAssembly

[한국어](README.md) | English

**Docs**: `docs/blazor/02-hosting-render-modes.md`

Example project for the Blazor **WebAssembly** standalone hosting model.

## Hosting Model Characteristics

- **Execution location**: Browser (WebAssembly)
- **Server connection**: Not needed after initial file download
- **Initial load**: Slow (.NET runtime + app bundle download)
- **Offline**: Supported (after download)

## Program.cs Key Setup

```csharp
using B2_WebAssembly.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
await builder.Build().RunAsync();
```

> No server! Uses `WebAssemblyHostBuilder` instead of `WebApplication.CreateBuilder`.

## Project Structure Differences

| Item | Blazor Server | Blazor WebAssembly |
|------|--------------|-------------------|
| SDK | `Microsoft.NET.Sdk.Web` | `Microsoft.NET.Sdk.BlazorWebAssembly` |
| Entry point | `App.razor` (server render) | `wwwroot/index.html` + `App.razor` |
| Router | `Routes.razor` / `<Routes>` | `<Router>` directly in `App.razor` |
| rendermode | `@rendermode InteractiveServer` | Not needed (entire app is client-side) |
| ReconnectModal | Present | Absent |

## How to Run

```bash
dotnet run --project Examples/B2-WebAssembly/B2-WebAssembly.csproj
```

## Testing

1. Open `https://localhost:<port>` in a browser after running
2. DevTools → Network → confirm `_framework/` files (WASM bundle)
3. Notice that initial load is slower than Server (8~30 MB bundle download)
4. Counter page → click button → confirm no server requests in Network (processed in browser)
5. Stop the server → already-loaded pages continue to work

## Comparison with Other Hosting Models

| Item | B2-Server | B2-WebAssembly | B2-WebApp |
|------|-----------|----------------|-----------|
| Execution | Server | Browser | Selectable |
| Server connection | Always required | Not needed | Per mode |
| Initial load | Fast | Slow (bundle download) | Mixed |
| Offline | No | Yes | Per mode |
