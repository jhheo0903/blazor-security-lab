# B2. Hosting Models and Rendering Modes

[한국어](02-hosting-render-modes.md) | English

## Official Concept Summary (per Microsoft docs)

In Blazor Web App, render modes control the "execution location" and "interactivity."

| Mode | Execution | Interactivity | Notes |
| --- | --- | --- | --- |
| Static SSR | Server | No | Initial HTML only, no event handling |
| Interactive Server | Server | Yes | Real-time SignalR circuit connection |
| Interactive WebAssembly | Client | Yes | Browser .NET runtime |
| Interactive Auto | Server → Client | Yes | Server initially, switches to WASM after bundle download |

## The Three Hosting Models

### 1. Blazor Server
Renders on the server and maintains a real-time SignalR connection with the client.

- Pros: Fast initial load, lower client resource usage
- Cons: Requires persistent server connection, latency-sensitive

### 2. Blazor WebAssembly
The .NET runtime and app code are downloaded to the browser and run client-side.

- Pros: No server connection needed, offline capable
- Cons: Slow initial bundle download, uses browser resources

### 3. Blazor Web App (.NET 8+)
A unified model that can mix Static SSR with Interactive modes.
Render mode is selected per page or component.

## Program.cs Configuration Examples

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

### Mixed Mode (Server + WASM)

```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents()
    .AddInteractiveWebAssemblyComponents();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode()
    .AddInteractiveWebAssemblyRenderMode();
```

## Applying @rendermode

### On a component instance

```razor
<Dialog @rendermode="InteractiveServer" />
```

### On a page component declaration

```razor
@page "/example"
@rendermode InteractiveServer
```

### App-wide (App.razor)

```razor
<Routes @rendermode="InteractiveServer" />
<HeadOutlet @rendermode="InteractiveServer" />
```

## Rendering Path and Where @rendermode Applies

When understanding hosting/rendering modes, it helps to look at the component rendering path together.

1. `Program.cs`: Register `MapRazorComponents<App>()` + `AddInteractive*RenderMode()`
2. `App.razor`: Renders `<Routes />` and `<HeadOutlet />`
3. `Routes.razor` (or Router inside App): URL matching and page selection
4. `MainLayout.razor`: Renders the common frame
5. `@Body`: Renders the matched `Pages/*.razor`

So `@rendermode` is declared at the page/component level, but the actual rendering flow follows the component tree above.

## Prerender in Detail

Interactive modes have **prerendering enabled by default**.

Rendering flow:
1. Server immediately returns static HTML (= prerender)
2. Client establishes an interactive connection
3. Events can be handled from this point

### Disabling Prerender

```razor
<Routes @rendermode="new InteractiveServerRenderMode(prerender: false)" />
<HeadOutlet @rendermode="new InteractiveServerRenderMode(prerender: false)" />
```

### Checking Interactive Status at Runtime

```razor
@if (!RendererInfo.IsInteractive)
{
    <p>Connecting...</p>
}
else
{
    <button @onclick="Send">Send</button>
}

@code {
    private void Send() { }
}
```

## Render Mode Propagation Rules

- If a parent is Interactive Server, children also run as Interactive Server.
- A child cannot be set to a more restrictive mode than its parent.
- Using an Interactive child under Static SSR causes an error.

## Common Mistakes

| Mistake | Cause | Fix |
| --- | --- | --- |
| JS call fails during prerender | JS called before interactive connection | Call inside `OnAfterRenderAsync(firstRender)` |
| Error injecting client-only service | No browser available during prerender | Use optional injection or provide a server-side abstraction |
| Parent-child render mode mismatch | Unaware of propagation rules | Design render modes at the component hierarchy level |

## References

- https://learn.microsoft.com/aspnet/core/blazor/components/render-modes
- https://learn.microsoft.com/aspnet/core/blazor/components/prerender
