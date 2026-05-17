# B5. Component Lifecycle

[한국어](05-lifecycle.md) | English

## Lifecycle Flow

```
Component created
    ↓
SetParametersAsync          ← parameter setup (customizable)
    ↓
OnInitialized(Async)        ← runs once on first initialization
    ↓
OnParametersSet(Async)      ← runs each time parameters change
    ↓
Render (BuildRenderTree)
    ↓
OnAfterRender(Async)        ← runs after render completes
    ↓
(repeats from OnParametersSet when parameters change)
    ↓
Dispose / DisposeAsync      ← runs when component is removed
```

## Key Methods in Detail

### OnInitialized / OnInitializedAsync

Runs **once when the component is first created**.

```razor
@code {
    private List<string> items = [];

    protected override async Task OnInitializedAsync()
    {
        // load initial data
        items = await DataService.GetItemsAsync();
    }
}
```

> In a prerender environment, this may run **twice** (once during server prerender, once after the interactive connection is established).
> Design with idempotency in mind.

### OnParametersSet / OnParametersSetAsync

Runs every time parameters change from the parent.

```razor
@code {
    [Parameter]
    public string? Filter { get; set; }

    private List<string> filtered = [];

    protected override async Task OnParametersSetAsync()
    {
        // recalculate whenever the Filter parameter changes
        filtered = await DataService.GetFilteredAsync(Filter);
    }
}
```

### OnAfterRender / OnAfterRenderAsync

Runs after the HTML is actually rendered to the DOM.

```razor
@inject IJSRuntime JS

@code {
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // JS can only be called after the DOM is ready
            await JS.InvokeVoidAsync("initChart", "chart-container");
        }
    }
}
```

- `firstRender: true` = after the first render
- `firstRender: false` = after every subsequent render
- Used for JS interop, setting focus, controlling scroll, etc.

### ShouldRender

Override to prevent unnecessary re-renders.

```razor
@code {
    private bool _shouldRender = true;

    protected override bool ShouldRender()
    {
        return _shouldRender;
    }
}
```

> Default is `true`. Use only in performance-critical components.

## IDisposable / IAsyncDisposable

Perform cleanup when the component is removed from the DOM.

```razor
@implements IAsyncDisposable

@code {
    private Timer? _timer;

    protected override void OnInitialized()
    {
        _timer = new Timer(OnTick, null, 0, 1000);
    }

    private void OnTick(object? state)
    {
        InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        if (_timer is not null)
        {
            await _timer.DisposeAsync();
        }
    }
}
```

Essential for unsubscribing from events, cleaning up timers, and releasing JS object references.

## SetParametersAsync (Advanced)

Override to take full control of the parameter-setting flow.

```razor
@code {
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        // customize parameter handling
        await base.SetParametersAsync(parameters);
    }
}
```

> No need to override in typical cases.

## Common Mistakes

| Mistake | Cause | Fix |
| --- | --- | --- |
| JS call fails | Called before DOM is ready | Call inside `OnAfterRenderAsync(firstRender)` |
| Initialization runs twice | Prerender double execution | Ensure idempotency, or disable prerender |
| Timer doesn't stop | Dispose not implemented | Implement `IAsyncDisposable` |
| Parameter change not reflected | Only `OnInitializedAsync` used | Also use `OnParametersSetAsync` |

## References

- https://learn.microsoft.com/aspnet/core/blazor/components/lifecycle
