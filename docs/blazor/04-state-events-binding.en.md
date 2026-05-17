# B4. State, Events, and Data Binding

[한국어](04-state-events-binding.md) | English

## State and Rendering

In Blazor, **when state (private fields) changes, the UI re-renders automatically.**
A rendering cycle is triggered automatically after an event handler runs.

Core principles:
- State changes should only happen inside event handlers
- Design the UI as a "function" of state

## Event Handling

### Basic Event Handling

```razor
<button @onclick="Increment">Count: @count</button>

@code {
    private int count;

    private void Increment()
    {
        count++;
    }
}
```

### Async Event Handlers

```razor
<button @onclick="LoadDataAsync">Load</button>
<p>@message</p>

@code {
    private string message = string.Empty;

    private async Task LoadDataAsync()
    {
        message = "Loading...";
        await Task.Delay(500);   // replace with a real service call
        message = "Done";
    }
}
```

### Event Arguments (EventArgs)

```razor
<input @onkeydown="HandleKeyDown" />

@code {
    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            // handle Enter key
        }
    }
}
```

Common EventArgs types: `MouseEventArgs`, `KeyboardEventArgs`, `ChangeEventArgs`, `FocusEventArgs`

### Lambda Event Handlers

```razor
@foreach (var item in items)
{
    <button @onclick="() => Select(item)">@item.Name</button>
}

@code {
    private List<Item> items = [];

    private void Select(Item item) { }
}
```

> Note: Lambdas inside loops involve closure capture. `foreach` item variables are safe, but `for` loop index variables must be copied before use.

## Data Binding

### One-Way Binding

```razor
<p>@message</p>   ← read-only expression
```

### Two-Way Binding (@bind)

```razor
<input @bind="name" />
<p>Hello, @name</p>

@code {
    private string name = string.Empty;
}
```

`@bind` synchronizes on the `onchange` event (when focus leaves the element) by default.

### Real-Time Sync (oninput)

```razor
<input @bind="name" @bind:event="oninput" />
```

### Format Binding

```razor
<input @bind="price" @bind:format="F2" />   ← two decimal places

@code {
    private decimal price = 1.5m;
}
```

### Date Binding

```razor
<input type="date" @bind="selectedDate" />

@code {
    private DateTime selectedDate = DateTime.Today;
}
```

## EventCallback

Used to raise events from a child up to the parent.

```razor
<!-- Child component -->
<button @onclick="NotifyParent">Select</button>

@code {
    [Parameter]
    public EventCallback<string> OnSelected { get; set; }

    private async Task NotifyParent()
    {
        await OnSelected.InvokeAsync("selected value");
    }
}
```

```razor
<!-- Parent component -->
<ChildComponent OnSelected="HandleSelection" />

@code {
    private void HandleSelection(string value)
    {
        // process value
    }
}
```

`EventCallback` automatically calls `StateHasChanged()`.

## Manually Calling StateHasChanged

When state changes outside of a rendering cycle, manually request a re-render.

```razor
@code {
    private async Task UpdateFromTimer()
    {
        // when state changes outside Blazor's event loop (timer, external event, etc.)
        count++;
        await InvokeAsync(StateHasChanged);
    }
}
```

## CascadingValue / CascadingParameter

Used to propagate values down the component hierarchy. Suitable for shared values like the logged-in user or theme.

```razor
<!-- Ancestor component -->
<CascadingValue Value="currentUser">
    <ChildTree />
</CascadingValue>
```

```razor
<!-- Any descendant component -->
@code {
    [CascadingParameter]
    public UserInfo? CurrentUser { get; set; }
}
```

## When to Use What

| Situation | Recommended Approach |
| --- | --- |
| Sync input with state immediately | `@bind` |
| Need controlled timing, validation, or pre-processing | Event handler |
| Child-to-parent notification | `EventCallback` |
| Propagate a value through the whole hierarchy | `CascadingValue` |

## Avoiding Mistakes

- Scattering state change points across multiple places makes debugging harder.
- Be aware of closure capture issues with lambdas inside loops.
- When changing UI state from an external thread or timer, use `InvokeAsync(StateHasChanged)`.

## References

- https://learn.microsoft.com/aspnet/core/blazor/components/event-handling
- https://learn.microsoft.com/aspnet/core/blazor/components/data-binding
- https://learn.microsoft.com/aspnet/core/blazor/components/cascading-values-and-parameters
