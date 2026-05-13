# B7. 폼, 유효성 검사, 예외 처리

## 기본 조합

- EditForm
- DataAnnotationsValidator
- ValidationSummary / ValidationMessage

## 예시

```razor
<EditForm Model="model" OnValidSubmit="SubmitAsync">
    <DataAnnotationsValidator />
    <ValidationSummary />

    <InputText @bind-Value="model.Name" />
    <ValidationMessage For="() => model.Name" />

    <button type="submit">Save</button>
</EditForm>

@code {
    private InputModel model = new();

    private Task SubmitAsync() => Task.CompletedTask;

    public class InputModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string Name { get; set; } = string.Empty;
    }
}
```

## 예외 처리 권장

- 사용자 액션 경계(저장/삭제/분석 실행)에서 예외 포착
- UI에는 원인 중심 메시지 제공, 로그에는 상세 스택 기록
- 실패 후 재시도 가능 상태로 화면 복구
