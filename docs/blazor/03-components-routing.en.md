# B3. Component Structure and Routing

[한국어](03-components-routing.md) | English

## Recommended Folder Strategy

| Folder | Role |
| --- | --- |
| Pages/ | Routable screen components |
| Shared/ | Reusable UI components |
| Layout/ | App-wide frames (header, sidebar, etc.) |
| Components/ | Feature-specific sub-components |

## Routing Basics

```razor
@page "/policies"
<h3>Policy List</h3>
```

- `@page` maps a URL to a component.
- Routing is handled by the `<Router>` component, located in `App.razor` or `Routes.razor` depending on project structure.

## Razor Rendering Flow

The actual screen is assembled and rendered in this order:

1. `Program.cs` registers `App` as the root component
2. `App.razor` calls `<Routes />`
3. `<Router>` in `Routes.razor` matches the URL
4. `<RouteView>` applies the `DefaultLayout` (e.g., `MainLayout`)
5. The matched `Pages/*.razor` is rendered at `@Body` in `MainLayout.razor`

```razor
<!-- Routes.razor example -->
<Router AppAssembly="typeof(Program).Assembly">
    <Found Context="routeData">
        <RouteView RouteData="routeData" DefaultLayout="typeof(Layout.MainLayout)" />
    </Found>
</Router>
```

### Route Parameters

```razor
@page "/policy/{id}"

<h3>Policy ID: @Id</h3>

@code {
    [Parameter]
    public string Id { get; set; } = string.Empty;
}
```

### Route Constraints

```razor
@page "/policy/{id:int}"   ← only int allowed; returns 404 for non-integers
```

Common constraints: `int`, `long`, `guid`, `bool`, `datetime`, `decimal`

### Optional Route Parameters

```razor
@page "/items/{page:int?}"

@code {
    [Parameter]
    public int? Page { get; set; }
}
```

## Multiple Route Declarations

A single component can be mapped to multiple URLs.

```razor
@page "/policies"
@page "/policies/all"
```

## Navigation

### NavLink

`<NavLink>` automatically adds an `active` CSS class when the current URL matches.

```razor
<NavLink href="/policies" Match="NavLinkMatch.All">Policy List</NavLink>
<NavLink href="/policies" Match="NavLinkMatch.Prefix">Policies</NavLink>
```

- `NavLinkMatch.All`: active only when the full URL matches
- `NavLinkMatch.Prefix`: active when the URL starts with the given path

### NavigationManager

Navigate programmatically from code.

```razor
@inject NavigationManager Nav

<button @onclick="GoToDetail">View Detail</button>

@code {
    private void GoToDetail()
    {
        Nav.NavigateTo("/policy/123");
    }
}
```

Navigation with query strings:

```razor
Nav.NavigateTo($"/policies?page=2&filter=active");
```

Checking the current URL:

```razor
@inject NavigationManager Nav

@code {
    protected override void OnInitialized()
    {
        var current = Nav.Uri;   // full URL
        var relative = Nav.ToBaseRelativePath(Nav.Uri);  // relative path
    }
}
```

## Layouts

The default layout is defined in `MainLayout.razor`.

```razor
<!-- MainLayout.razor -->
@inherits LayoutComponentBase

<div class="layout">
    <nav>@* navigation *@</nav>
    <main>@Body</main>   ← page component renders here
</div>
```

Applying a different layout to a specific page:

```razor
@page "/admin"
@layout AdminLayout
```

## Component Responsibility Separation

- **Page components**: Screen flow, user action orchestration
- **Shared components**: Repeated UI / presentation rules
- **Services**: Computation, external integration, data transformation

## Parameter Passing Pattern

```razor
<PolicyCard Policy="selected" OnApply="HandleApply" />
```

```razor
@code {
    [Parameter]
    public PolicyModel? Policy { get; set; }

    [Parameter]
    public EventCallback<PolicyModel> OnApply { get; set; }
}
```

## Slot Pattern with RenderFragment

Renders parent-defined content inside a child component.

```razor
<!-- Card.razor -->
<div class="card">
    <div class="card-body">
        @ChildContent   ← slot
    </div>
</div>

@code {
    [Parameter]
    public RenderFragment? ChildContent { get; set; }
}
```

```razor
<!-- Usage -->
<Card>
    <p>This content goes inside the card.</p>
</Card>
```

## Checklist

- Is reusable content separated into Shared?
- Does the page know too much about service implementation details?
- Do route names reflect domain meaning?
- Is `NotFound.razor` configured properly for 404 handling?

## References

- https://learn.microsoft.com/aspnet/core/blazor/fundamentals/routing
- https://learn.microsoft.com/aspnet/core/blazor/components/layouts
