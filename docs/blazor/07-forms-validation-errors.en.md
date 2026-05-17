# B7. Forms, Validation, and Error Handling

[한국어](07-forms-validation-errors.md) | English

## Core Building Blocks

Blazor form essentials:

- `EditForm`: Form container, connected to a model
- `DataAnnotationsValidator`: Validation based on model annotations
- `ValidationSummary`: Displays all error messages
- `ValidationMessage`: Displays errors for a specific field

## Basic Form Example

```razor
<EditForm Model="model" OnValidSubmit="SubmitAsync" OnInvalidSubmit="HandleInvalid">
    <DataAnnotationsValidator />
    <ValidationSummary />

    <div>
        <label>Name</label>
        <InputText @bind-Value="model.Name" />
        <ValidationMessage For="() => model.Name" />
    </div>

    <div>
        <label>Age</label>
        <InputNumber @bind-Value="model.Age" />
        <ValidationMessage For="() => model.Age" />
    </div>

    <button type="submit">Save</button>
</EditForm>

@code {
    private FormModel model = new();

    private async Task SubmitAsync()
    {
        await SaveService.SaveAsync(model);
    }

    private void HandleInvalid() { }

    public class FormModel
    {
        [Required(ErrorMessage = "Name is required.")]
        [MaxLength(50, ErrorMessage = "Maximum 50 characters allowed.")]
        public string Name { get; set; } = string.Empty;

        [Range(1, 120, ErrorMessage = "Enter a number between 1 and 120.")]
        public int Age { get; set; }
    }
}
```

## Input Component Types

| Component | Corresponding Type |
| --- | --- |
| `InputText` | `string` |
| `InputNumber<T>` | `int`, `decimal`, `double`, etc. |
| `InputDate<T>` | `DateTime`, `DateOnly` |
| `InputCheckbox` | `bool` |
| `InputSelect<T>` | `enum`, `string`, etc. |
| `InputTextArea` | `string` (multi-line) |
| `InputFile` | File upload |

### InputSelect Example

```razor
<InputSelect @bind-Value="model.Status">
    <option value="">Select...</option>
    @foreach (var status in Enum.GetValues<PolicyStatus>())
    {
        <option value="@status">@status</option>
    }
</InputSelect>
```

## Custom Validation

### IValidatableObject

```csharp
public class DateRangeModel : IValidatableObject
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (To <= From)
        {
            yield return new ValidationResult(
                "End date must be after start date.",
                [nameof(To)]);
        }
    }
}
```

### Manual Validation with EditContext

```razor
@code {
    private EditContext? editContext;
    private FormModel model = new();

    protected override void OnInitialized()
    {
        editContext = new EditContext(model);
    }

    private void ValidateManually()
    {
        var isValid = editContext!.Validate();
        // handle based on isValid
    }
}
```

## Error Handling

### Catching Exceptions at Action Boundaries

```razor
<button @onclick="SaveAsync">Save</button>
<p class="error">@errorMessage</p>

@code {
    private string? errorMessage;

    private async Task SaveAsync()
    {
        try
        {
            errorMessage = null;
            await MyService.SaveAsync(model);
        }
        catch (HttpRequestException ex)
        {
            errorMessage = "A server communication error occurred.";
            Logger.LogError(ex, "Save failed");
        }
        catch (Exception ex)
        {
            errorMessage = "An unexpected error occurred.";
            Logger.LogError(ex, "Unexpected error");
        }
    }
}
```

### ErrorBoundary (Isolating Component Errors)

Prevents exceptions from a child component from crashing the entire page.

```razor
<ErrorBoundary>
    <ChildContent>
        <PolicyVisualizer Policy="selected" />
    </ChildContent>
    <ErrorContent Context="ex">
        <p>An error occurred: @ex.Message</p>
    </ErrorContent>
</ErrorBoundary>
```

### Loading State Pattern

```razor
@if (isLoading)
{
    <p>Loading...</p>
}
else if (errorMessage is not null)
{
    <p class="error">@errorMessage</p>
}
else
{
    <PolicyList Items="items" />
}

@code {
    private bool isLoading;
    private string? errorMessage;
    private List<PolicyModel> items = [];

    protected override async Task OnInitializedAsync()
    {
        try
        {
            isLoading = true;
            items = await PolicyService.GetPoliciesAsync();
        }
        catch (Exception ex)
        {
            errorMessage = "Failed to load data.";
            Logger.LogError(ex, "Failed to load policies");
        }
        finally
        {
            isLoading = false;
        }
    }
}
```

## Error Handling Principles

- Catch exceptions at user action boundaries (save, delete, analyze)
- Show cause-focused messages in the UI; log detailed stacks
- Restore the screen to a retryable state after failure
- Never expose sensitive information (stack traces, etc.) in the UI

## References

- https://learn.microsoft.com/aspnet/core/blazor/forms/
- https://learn.microsoft.com/aspnet/core/blazor/forms/validation
- https://learn.microsoft.com/aspnet/core/blazor/fundamentals/handle-errors
