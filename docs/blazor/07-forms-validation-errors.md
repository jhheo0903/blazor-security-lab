# B7. 폼, 유효성 검사, 예외 처리

## 기본 조합


Blazor 폼의 핵심 구성요소:

- `EditForm`: 폼 컨테이너, 모델과 연결
- `DataAnnotationsValidator`: 모델 어노테이션 기반 검증
- `ValidationSummary`: 전체 오류 목록 표시
- `ValidationMessage`: 특정 필드 오류 표시

## 기본 폼 예시

```razor
<EditForm Model="model" OnValidSubmit="SubmitAsync" OnInvalidSubmit="HandleInvalid">
    <DataAnnotationsValidator />
    <ValidationSummary />

    <div>
        <label>이름</label>
        <InputText @bind-Value="model.Name" />
        <ValidationMessage For="() => model.Name" />
    </div>

    <div>
        <label>나이</label>
        <InputNumber @bind-Value="model.Age" />
        <ValidationMessage For="() => model.Age" />
    </div>

    <button type="submit">저장</button>
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
        [Required(ErrorMessage = "이름은 필수입니다.")]
        [MaxLength(50, ErrorMessage = "최대 50자까지 입력 가능합니다.")]
        public string Name { get; set; } = string.Empty;

        [Range(1, 120, ErrorMessage = "1~120 사이의 숫자를 입력하세요.")]
        public int Age { get; set; }
    }
}
```

## 입력 컴포넌트 종류

| 컴포넌트 | 대응 타입 |
| --- | --- |
| `InputText` | `string` |
| `InputNumber<T>` | `int`, `decimal`, `double` 등 |
| `InputDate<T>` | `DateTime`, `DateOnly` |
| `InputCheckbox` | `bool` |
| `InputSelect<T>` | `enum`, `string` 등 |
| `InputTextArea` | `string` (여러 줄) |
| `InputFile` | 파일 업로드 |

### InputSelect 예시

```razor
<InputSelect @bind-Value="model.Status">
    <option value="">선택하세요</option>
    @foreach (var status in Enum.GetValues<PolicyStatus>())
    {
        <option value="@status">@status</option>
    }
</InputSelect>
```

## 커스텀 유효성 검사

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
                "종료일은 시작일 이후여야 합니다.",
                [nameof(To)]);
        }
    }
}
```

### EditContext로 수동 유효성 검사

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
        // isValid 기반 처리
    }
}
```

## 예외 처리

### 사용자 액션 경계에서 예외 포착

```razor
<button @onclick="SaveAsync">저장</button>
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
            errorMessage = "서버 통신 오류가 발생했습니다.";
            Logger.LogError(ex, "Save failed");
        }
        catch (Exception ex)
        {
            errorMessage = "예기치 않은 오류가 발생했습니다.";
            Logger.LogError(ex, "Unexpected error");
        }
    }
}
```

### ErrorBoundary (컴포넌트 오류 격리)

자식 컴포넌트에서 발생한 예외가 전체 페이지를 중단시키지 않도록 격리합니다.

```razor
<ErrorBoundary>
    <ChildContent>
        <PolicyVisualizer Policy="selected" />
    </ChildContent>
    <ErrorContent Context="ex">
        <p>오류가 발생했습니다: @ex.Message</p>
    </ErrorContent>
</ErrorBoundary>
```

### 로딩 상태 패턴

```razor
@if (isLoading)
{
    <p>로딩 중...</p>
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
            errorMessage = "데이터를 불러오지 못했습니다.";
            Logger.LogError(ex, "Failed to load policies");
        }
        finally
        {
            isLoading = false;
        }
    }
}
```

## 예외 처리 권장 원칙

- 사용자 액션 경계(저장/삭제/분석 실행)에서 예외 포착
- UI에는 원인 중심 메시지 제공, 로그에는 상세 스택 기록
- 실패 후 재시도 가능 상태로 화면 복구
- 민감한 정보(스택 트레이스 등)는 UI에 노출하지 않음

## 참고

- https://learn.microsoft.com/aspnet/core/blazor/forms/
- https://learn.microsoft.com/aspnet/core/blazor/forms/validation
- https://learn.microsoft.com/aspnet/core/blazor/fundamentals/handle-errors
