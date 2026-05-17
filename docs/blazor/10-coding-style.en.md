# B10. Coding Style Guide

[한국어](10-coding-style.md) | English

## Goal

Keep UI code readable and the structure maintainable.

## Recommended @code Block Order

```razor
@page "/policies"
@inject IPolicyService PolicyService
@inject ILogger<PolicyList> Logger

<h3>Policy List</h3>
<button @onclick="LoadPoliciesAsync">Refresh</button>

@foreach (var policy in policies)
{
    <PolicyCard @key="policy.Id" Policy="policy" />
}

@code {
    // 1. Parameters
    [Parameter] public string? Filter { get; set; }

    // 2. State fields
    private List<PolicyModel> policies = [];
    private bool isLoading;
    private string? errorMessage;

    // 3. Lifecycle methods
    protected override async Task OnInitializedAsync()
    {
        await LoadPoliciesAsync();
    }

    // 4. Event handlers
    private async Task LoadPoliciesAsync()
    {
        try
        {
            isLoading = true;
            policies = await PolicyService.GetPoliciesAsync(Filter);
        }
        catch (Exception ex)
        {
            errorMessage = "Failed to load policies.";
            Logger.LogError(ex, "Failed to load policies");
        }
        finally
        {
            isLoading = false;
        }
    }
}
```

## Code-Behind Pattern (Optional)

For complex components, separate code into a `.razor.cs` file.

```csharp
// PolicyList.razor.cs
public partial class PolicyList : ComponentBase
{
    [Inject] private IPolicyService PolicyService { get; set; } = default!;

    protected List<PolicyModel> Policies { get; private set; } = [];
    protected bool IsLoading { get; private set; }

    protected async Task LoadPoliciesAsync()
    {
        IsLoading = true;
        Policies = await PolicyService.GetPoliciesAsync();
        IsLoading = false;
    }
}
```

> This adds complexity to simple components. Use only when necessary.

## Naming Conventions

### Event Handlers — verb-based, intent-clear

```csharp
// Good
private async Task LoadPoliciesAsync() { }
private void ApplyFilter(string term) { }
private async Task AnalyzeConflictsAsync() { }
private void ToggleSelection(PolicyModel policy) { }

// Bad
private async Task Button1Clicked() { }
private void Handler() { }
```

### State Fields — describe the current state

```csharp
private bool isLoading;
private bool hasError;
private string? errorMessage;
private PolicyModel? selectedPolicy;
private List<PolicyModel> filteredPolicies = [];
```

## File/Folder Structure

```
Components/
  Pages/
    PolicyList.razor      ← screen-level
    PolicyDetail.razor
  Shared/
    PolicyCard.razor      ← reusable UI
    LoadingSpinner.razor
  Layout/
    MainLayout.razor
Services/
  IPolicyService.cs
  PolicyService.cs
Models/
  PolicyModel.cs
  PolicyConflict.cs
```

## When to Split a Component

Consider splitting when any of these apply:

- `@code` block exceeds 150 lines
- Two or more UI areas with distinct responsibilities
- The same markup pattern repeats two or more times

## Review Checkpoints

- Is the component size reasonable?
- Do event handler names reveal their intent?
- Is code with the same responsibility spread across multiple files?
- Does a service interface exist for testability?
- Are state change points clearly separated?
