# B1. Blazor Basics

[한국어](01-overview.md) | English

## What Makes It Different

Blazor is a component-based framework for building web UI with C#.
For developers coming from ASP.NET MVC/API, three key mindset shifts are essential.

| Old Mindset | Blazor Mindset |
| --- | --- |
| Request/response driven | State-driven UI |
| Page-level screens | Component composition |
| Direct DOM manipulation via JS | State change → automatic re-render |

## .razor File Structure

A Blazor component contains HTML, C#, and optional CSS in a single `.razor` file.

```razor
@page "/counter"            ← route declaration (page component)
@inject ILogger<Counter> Logger  ← service injection

<h3>Counter: @count</h3>
<button @onclick="Increment">+1</button>

@code {
    private int count;

    private void Increment()
    {
        count++;
        Logger.LogInformation("Count: {Count}", count);
    }
}
```

- `@page`: Declares the file as a routable page.
- `@inject`: Injects a service from the DI container.
- `@code { }`: C# code block.
- `@onclick`: Connects an event handler.

## Component Basics

**When state (fields) change, the UI re-renders automatically.**

```razor
<p>Current state: @message</p>
<button @onclick="ChangeMessage">Change</button>

@code {
    private string message = "initial";

    private void ChangeMessage()
    {
        message = "changed";   // this change triggers a re-render
    }
}
```

## Passing Parameters

Pass values from a parent component to a child component.

```razor
<!-- Parent -->
<ChildComponent Title="Hello" />
```

```razor
<!-- ChildComponent.razor -->
<h4>@Title</h4>

@code {
    [Parameter]
    public string Title { get; set; } = string.Empty;
}
```

- Only properties declared with `[Parameter]` can receive values from outside.
- Parameter names are case-sensitive.

## Event Callbacks

Use `EventCallback` to pass events from a child up to the parent.

```razor
<!-- Parent -->
<ChildComponent OnSelected="HandleSelected" />

@code {
    private void HandleSelected(string value)
    {
        // handle selection from child
    }
}
```

```razor
<!-- Child -->
<button @onclick="Select">Select</button>

@code {
    [Parameter]
    public EventCallback<string> OnSelected { get; set; }

    private async Task Select()
    {
        await OnSelected.InvokeAsync("selected value");
    }
}
```

## Constructor Injection (.NET 10)

Starting with .NET 10, constructor injection is supported in components.

```razor
@* Constructor injection can be used instead of @inject *@

@code {
    private readonly IMyService _service;

    public MyComponent(IMyService service)
    {
        _service = service;
    }
}
```

## Key Takeaways for C# Developers

- "State-driven UI" thinking matters more than controller-centric thinking.
- DTO/service separation maps directly to existing C# backend habits.
- Stabilizing service-level tests before UI-level tests is the most effective strategy.

## Checklist

- Can you explain where component boundaries are?
- Can you trace a state change to its rendered result?
- Can you articulate the criteria for separating services?
- Do you understand the direction of data/event flow between parent and child?

## References

- https://learn.microsoft.com/aspnet/core/blazor/components/
- https://learn.microsoft.com/aspnet/core/blazor/components/event-handling
