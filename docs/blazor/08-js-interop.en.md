# B8. JS Interop Principles

[한국어](08-js-interop.md) | English

## When Is It Needed?

- When browser-only APIs are required (clipboard, localStorage, geolocation, etc.)
- When integrating UI widgets that are hard to build with Blazor alone (charts, editors, etc.)
- When reusing existing JS assets

## Calling JS from C#

### InvokeVoidAsync (no return value)

```razor
@inject IJSRuntime JS

<button @onclick="ShowAlert">Alert</button>

@code {
    private async Task ShowAlert()
    {
        await JS.InvokeVoidAsync("alert", "Hello from Blazor");
    }
}
```

### InvokeAsync (with return value)

```razor
@inject IJSRuntime JS

@code {
    private async Task<string> GetLocalStorage(string key)
    {
        return await JS.InvokeAsync<string>("localStorage.getItem", key) ?? string.Empty;
    }
}
```

## Using JS Modules (Recommended)

Manage JS as ES modules instead of global functions. Place files in `wwwroot/js/`.

```javascript
// wwwroot/js/myModule.js
export function showMessage(message) {
    alert(message);
}

export function focusElement(element) {
    element.focus();
}
```

```razor
@inject IJSRuntime JS
@implements IAsyncDisposable

@code {
    private IJSObjectReference? _module;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await JS.InvokeAsync<IJSObjectReference>(
                "import", "./js/myModule.js");
        }
    }

    private async Task ShowMessage()
    {
        if (_module is not null)
        {
            await _module.InvokeVoidAsync("showMessage", "Called from module");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }
}
```

## Passing DOM Element References

Pass `ElementReference` to JS to delegate direct DOM manipulation.

```razor
@inject IJSRuntime JS

<input @ref="inputRef" type="text" />
<button @onclick="FocusInput">Focus</button>

@code {
    private ElementReference inputRef;

    private async Task FocusInput()
    {
        await JS.InvokeVoidAsync("focusElement", inputRef);
    }
}
```

## Calling C# from JS

Use `DotNetObjectReference` to let JS call back into C# methods.

```razor
@inject IJSRuntime JS
@implements IAsyncDisposable

@code {
    private DotNetObjectReference<MyComponent>? _dotNetRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("registerCallback", _dotNetRef);
        }
    }

    [JSInvokable]
    public void OnExternalEvent(string data)
    {
        InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        _dotNetRef?.Dispose();
    }
}
```

## Service Abstraction Example

```csharp
public interface IChartService
{
    ValueTask InitAsync(string elementId);
    ValueTask UpdateDataAsync(IEnumerable<double> values);
}

public sealed class ChartService : IChartService, IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private IJSObjectReference? _module;

    public ChartService(IJSRuntime js) => _js = js;

    public async ValueTask InitAsync(string elementId)
    {
        _module ??= await _js.InvokeAsync<IJSObjectReference>("import", "./js/chart.js");
        await _module.InvokeVoidAsync("init", elementId);
    }

    public async ValueTask UpdateDataAsync(IEnumerable<double> values)
    {
        if (_module is not null)
            await _module.InvokeVoidAsync("update", values);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
            await _module.DisposeAsync();
    }
}
```

## Design Principles

- Wrap interop calls in a service to reduce component dependencies
- Use ES modules instead of global functions
- JS cannot be called during prerender — use `OnAfterRenderAsync(firstRender)`
- Always Dispose `IJSObjectReference` and `DotNetObjectReference`

## Checklist

- Is there a fallback UX if a call fails?
- Does the JS module load timing conflict with the rendering timing?
- Are `IJSObjectReference` and `DotNetObjectReference` disposed?
- Is the prerender phase properly guarded?

## References

- https://learn.microsoft.com/aspnet/core/blazor/javascript-interoperability/
- https://learn.microsoft.com/aspnet/core/blazor/javascript-interoperability/call-javascript-from-dotnet
- https://learn.microsoft.com/aspnet/core/blazor/javascript-interoperability/call-dotnet-from-javascript
