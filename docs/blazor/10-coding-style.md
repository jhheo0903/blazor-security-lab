# B10. 코딩 스타일 가이드

## 목표

읽기 쉬운 UI 코드와 유지보수 가능한 구조를 유지합니다.

## 컴포넌트 내부 순서 권장

```razor
@page "/policies"
@inject IPolicyService PolicyService
@inject ILogger<PolicyList> Logger

<h3>정책 목록</h3>
<button @onclick="LoadPoliciesAsync">새로고침</button>

@foreach (var policy in policies)
{
    <PolicyCard @key="policy.Id" Policy="policy" />
}

@code {
    // 1. 파라미터
    [Parameter] public string? Filter { get; set; }

    // 2. 상태 필드
    private List<PolicyModel> policies = [];
    private bool isLoading;
    private string? errorMessage;

    // 3. 생명주기 메서드
    protected override async Task OnInitializedAsync()
    {
        await LoadPoliciesAsync();
    }

    // 4. 이벤트 핸들러
    private async Task LoadPoliciesAsync()
    {
        try
        {
            isLoading = true;
            policies = await PolicyService.GetPoliciesAsync(Filter);
        }
        catch (Exception ex)
        {
            errorMessage = "정책을 불러오지 못했습니다.";
            Logger.LogError(ex, "Failed to load policies");
        }
        finally
        {
            isLoading = false;
        }
    }
}
```

## 코드 비하인드 패턴 (선택적)

복잡한 컴포넌트는 `.razor.cs` 파일로 코드를 분리합니다.

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

> 간단한 컴포넌트에는 오히려 복잡도를 높입니다. 필요한 경우에만 사용합니다.

## 네이밍 규칙

### 이벤트 핸들러 - 동사 기반, 의도 명확하게

```csharp
// 좋음
private async Task LoadPoliciesAsync() { }
private void ApplyFilter(string term) { }
private async Task AnalyzeConflictsAsync() { }
private void ToggleSelection(PolicyModel policy) { }

// 나쁨
private async Task Button1Clicked() { }
private void Handler() { }
```

### 상태 필드 - 현재 상태를 서술

```csharp
private bool isLoading;
private bool hasError;
private string? errorMessage;
private PolicyModel? selectedPolicy;
private List<PolicyModel> filteredPolicies = [];
```

## 파일/폴더 구조

```
Components/
  Pages/
    PolicyList.razor      <- 화면 단위
    PolicyDetail.razor
  Shared/
    PolicyCard.razor      <- 재사용 UI
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

## 컴포넌트 분리 기준

아래 조건을 넘으면 분리를 검토합니다.

- `@code` 블록 150줄 초과
- 서로 다른 책임의 UI 영역이 2개 이상
- 동일한 마크업 패턴이 2번 이상 반복

## 리뷰 체크포인트

- 컴포넌트 길이가 과도하지 않은가?
- 이벤트 핸들러 이름이 의도를 드러내는가?
- 동일 책임의 코드가 여러 파일에 분산되지 않았는가?
- 서비스 인터페이스가 존재해 테스트 가능한가?
- 상태 변경 지점이 명확하게 분리되어 있는가?
