# B2-Server

[한국어](README.md) | English

**Docs**: `docs/blazor/02-hosting-render-modes.md`

Example project for the Blazor **Interactive Server** hosting model.

## Hosting Model Characteristics

- **Execution location**: Server
- **Server connection**: SignalR Circuit must be maintained
- **Initial load**: Fast (rendered on server)
- **Offline**: Not supported

## Program.cs Key Setup

```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();  // Server only

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
```

## How to Run

```bash
dotnet run --project Examples/B2-Server/B2-Server.csproj
```

## Testing

1. Open `https://localhost:<port>` in a browser after running
2. Click the button on the Counter page → verify count changes (processed via SignalR on server)
3. Check DevTools Network tab → confirm WebSocket connection (SignalR)
4. Stop the server → a "Reconnecting..." message should appear

## Comparison with Other Hosting Models

| Item | B2-Server | B2-WebAssembly | B2-WebApp |
|------|-----------|----------------|-----------|
| Execution | Server | Browser | Selectable |
| Server connection | Always required | Not needed | Per mode |
| Initial load | Fast | Slow (bundle download) | Mixed |
| Offline | No | Yes | Per mode |
