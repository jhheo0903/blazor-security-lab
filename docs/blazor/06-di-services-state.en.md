# B6. DI, Service Separation, and State Management

[한국어](06-di-services-state.md) | English

## Core Principles

- Components focus on UI state and user actions
- Services focus on domain logic and external communication
- State sharing should be limited to the minimum necessary scope

## Service Injection

### @inject Directive

```razor
@inject IMyService MyService

<button @onclick="Run">Run</button>

@code {
    private async Task Run()
    {
        await MyService.DoWorkAsync();
    }
}
```

### Constructor Injection (.NET 10)

```razor
@code {
    private readonly IMyService _myService;

    public MyComponent(IMyService myService)
    {
        _myService = myService;
    }
}
```

## Service Lifetimes

### Registration in Program.cs

```csharp
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddSingleton<IAppConfig, AppConfig>();
builder.Services.AddTransient<IAnalyzer, Analyzer>();
```

### Choosing a Lifetime

| Lifetime | Use Case | Notes |
| --- | --- | --- |
| `Scoped` | Per-request or per-user state; most services | In Blazor Server, tied to the SignalR circuit |
| `Singleton` | App-wide cache or configuration | Must be thread-safe; avoid mutable state |
| `Transient` | Stateless, short-lived operations | Creates a new instance on every injection |

> In Blazor Server, `Scoped` lifetime matches the SignalR circuit lifetime. Suitable for maintaining per-user state.

## Service Design Patterns

### Interface Abstraction

```csharp
public interface IPolicyService
{
    Task<IReadOnlyList<PolicyModel>> GetPoliciesAsync();
}

public class PolicyService : IPolicyService
{
    private readonly HttpClient _http;

    public PolicyService(HttpClient http)
    {
        _http = http;
    }

    public async Task<IReadOnlyList<PolicyModel>> GetPoliciesAsync()
    {
        return await _http.GetFromJsonAsync<List<PolicyModel>>("/api/policies")
               ?? [];
    }
}
```

Benefits of interface abstraction:
- Can be replaced with a mock for testing
- Components don't need to know implementation details

## State Management Patterns

### Local State (Component Fields)

```razor
@code {
    private bool isLoading;
    private string? errorMessage;
    private List<PolicyModel> policies = [];
}
```

Suitable for state used only within the component.

### Shared State Across a Page (Scoped Service)

Used when multiple components share state within the same user session.

```csharp
// State service
public class FilterState
{
    public string? SearchTerm { get; private set; }
    public event Action? OnChange;

    public void SetSearch(string? term)
    {
        SearchTerm = term;
        OnChange?.Invoke();
    }
}
```

```csharp
// Program.cs
builder.Services.AddScoped<FilterState>();
```

```razor
<!-- Component A: filter input -->
@inject FilterState State

<input @oninput="e => State.SetSearch(e.Value?.ToString())" />
```

```razor
<!-- Component B: list display -->
@inject FilterState State
@implements IDisposable

@code {
    protected override void OnInitialized()
    {
        State.OnChange += OnStateChanged;
    }

    private void OnStateChanged()
    {
        InvokeAsync(StateHasChanged);
    }

    public void Dispose()
    {
        State.OnChange -= OnStateChanged;
    }
}
```

### PersistentComponentState (Preserving State Across SSR → Interactive Transition)

Passes initial data when transitioning from Static SSR to Interactive mode.

```razor
@inject PersistentComponentState ApplicationState

@code {
    private List<PolicyModel> policies = [];
    private PersistingComponentStateSubscription _subscription;

    protected override async Task OnInitializedAsync()
    {
        _subscription = ApplicationState.RegisterOnPersisting(PersistData);

        if (!ApplicationState.TryTakeFromJson<List<PolicyModel>>("policies", out var restored))
        {
            policies = await PolicyService.GetPoliciesAsync();
        }
        else
        {
            policies = restored!;
        }
    }

    private Task PersistData()
    {
        ApplicationState.PersistAsJson("policies", policies);
        return Task.CompletedTask;
    }

    public void Dispose() => _subscription.Dispose();
}
```

## Design Tips

- Don't let components grow their own external API call code
- Ensure testability through service interfaces
- Define clear change events and consistency policies for global state

## References

- https://learn.microsoft.com/aspnet/core/blazor/fundamentals/dependency-injection
- https://learn.microsoft.com/aspnet/core/blazor/components/prerender#persist-prerendered-state
