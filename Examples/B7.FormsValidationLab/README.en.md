# B7.FormsValidationLab

[한국어](README.md) | English

**Docs**: `docs/blazor/07-forms-validation-errors.md`

Example project covering Blazor form input, data binding, and validation.

## Docs-to-Code Mapping

| Doc Section | Implementation File | Description |
|-------------|---------------------|-------------|
| Basic form input | `Components/Pages/BasicFormDemo.razor` | State updates via @bind |
| Validation | `Components/Pages/ValidationSimpleDemo.razor` | DataAnnotations validation |
| EditForm submission | `Components/Pages/FormSubmitDemo.razor` | EditForm + DataAnnotationsValidator |
| Form model | `Models/UserForm.cs` | Validation rule definitions |

## How to Run

```bash
dotnet run --project Examples/B7.FormsValidationLab/B7.FormsValidationLab.csproj
```

## Build Only

```bash
dotnet build Examples/B7.FormsValidationLab/B7.FormsValidationLab.csproj
```

## Three Demo Pages

1. **Basic Form** (`/basic-form`)
   - Uses `@bind="variable"`
   - State updates immediately on input
   - Simple UI display

2. **Validation** (`/validation-simple`)
   - UserForm model validation
   - Manual `Validate()` call
   - Error message display

3. **EditForm Submit** (`/form-submit`)
   - EditForm wrapper
   - DataAnnotationsValidator
   - ValidationSummary / ValidationMessage
   - OnValidSubmit callback
