# B4 State Events Binding Lab

[한국어](README.md) | English

A minimal example project for quickly experiencing B4 (State, Events, and Data Binding) from the root study guide.

## Root Document Mapping

Root reference: [README.md](../../README.md)

| Code | Topic | Detail Doc | B4 Application | Source File |
| --- | --- | --- | --- | --- |
| B4 | State, Events, Data Binding | [04-state-events-binding.md](../../docs/blazor/04-state-events-binding.md) | Event handler-based state changes and async state transitions | [Components/Pages/StateCounterDemo.razor](Components/Pages/StateCounterDemo.razor) |
| B4 | State, Events, Data Binding | [04-state-events-binding.md](../../docs/blazor/04-state-events-binding.md) | `@bind`, `@bind:event="oninput"`, format binding, date binding | [Components/Pages/BindingLiveDemo.razor](Components/Pages/BindingLiveDemo.razor) |
| B4 | State, Events, Data Binding | [04-state-events-binding.md](../../docs/blazor/04-state-events-binding.md) | Passing `EventCallback<T>` events from child to parent | [Components/Pages/EventCallbackDemo.razor](Components/Pages/EventCallbackDemo.razor), [Components/Shared/RuleToggle.razor](Components/Shared/RuleToggle.razor) |

## Demo Pages

- **Home**: Purpose and page overview
- **State Counter**: State changes and rendering updates after click/async events
- **Binding Live**: One-way/two-way/real-time binding and format binding
- **Event Callback**: Reflecting child component events in parent state

## Testing

1. Run the project with the command below.
2. Review the overview on Home, then navigate to State Counter, Binding Live, and Event Callback in order.
3. On State Counter, click Increment and Load Async — confirm state text and count update.
4. On Binding Live, confirm name input reflects instantly (oninput), and price/date binding results display correctly.
5. On Event Callback, click the Apply button — confirm the parent's "last selected rule" message changes.

```bash
dotnet restore
dotnet run --project Examples/B4.StateEventsBindingLab/B4.StateEventsBindingLab.csproj
```

Build only:

```bash
dotnet build Examples/B4.StateEventsBindingLab/B4.StateEventsBindingLab.csproj
```
