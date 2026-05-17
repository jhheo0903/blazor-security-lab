# B5.LifecycleLab

[한국어](README.md) | English

**Docs**: `docs/blazor/05-lifecycle.md`

Example project for Blazor component lifecycle methods.

## Docs-to-Code Mapping

| Doc Section | Implementation File | Description |
|-------------|---------------------|-------------|
| OnInitialized / OnInitializedAsync | `Components/Pages/LifecycleInitDemo.razor` | Async initialization, loading state handling |
| OnParametersSet | `Components/Pages/LifecycleParamsDemo.razor` + `Components/Shared/ParamChild.razor` | Detecting parameter changes, child component reactions |
| OnAfterRender | `Components/Pages/LifecycleAfterRenderDemo.razor` | JS interop after DOM render, render count tracking |
| IDisposable | `Components/Pages/LifecycleDisposeDemo.razor` | Timer resource cleanup, Dispose pattern |

## How to Run

```bash
dotnet run --project Examples/B5.LifecycleLab/B5.LifecycleLab.csproj
```

## Build Only

```bash
dotnet build Examples/B5.LifecycleLab/B5.LifecycleLab.csproj
```

## Verification Steps

1. **OnInitialized** (`/lifecycle-init`): Confirm that a 0.8-second loading delay occurs on page entry, then a list appears
2. **OnParametersSet** (`/lifecycle-params`): Confirm that typing in the input increases the child component's call count
3. **OnAfterRender** (`/lifecycle-afterrender`): Confirm auto-focus on page entry and render count display
4. **IDisposable** (`/lifecycle-dispose`): Confirm elapsed time counter ticks per second → navigate away and back to see Dispose called

## OnAfterRender Behavior (Step-by-Step)

Target file: `Components/Pages/LifecycleAfterRenderDemo.razor`

1. When the page first renders, `OnAfterRenderAsync(bool firstRender)` is called.
2. On entry, `renderCount++` runs to accumulate the render count.
3. JS is called only on the first render (`firstRender == true`).
4. The JS call goes through `IJSRuntime` to focus the `focusTarget` input.
5. The status message (`jsMessage`) is then set to "JS executed after first render" and reflected in the UI.

Key points:

- `IJSRuntime` is the channel for calling browser JavaScript from Blazor (C#).
- `OnAfterRenderAsync` runs after the DOM is actually painted, making it suitable for focus, size measurements, and external JS library initialization.
- Without a `firstRender` check, JS would be called repeatedly on every render.
