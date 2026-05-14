# B6. DI, 서비스 분리, 상태관리

## 기본 원칙

- 컴포넌트는 UI 상태와 사용자 액션 중심
- 서비스는 도메인 로직과 외부 통신 중심
- 상태 공유는 최소 범위로 제한

## 서비스 주입 방법

### @inject 지시문

```razor
@inject IMyService MyService

<button @onclick="Run">실행</button>

@code {
    private async Task Run()
    {
        await MyService.DoWorkAsync();
    }
}
```

### .NET 10 생성자 주입

```razor
@code {
    private readonly IMyService _myService;

    public MyComponent(IMyService myService)
    {
        _myService = myService;
    }
}
```

## 서비스 수명 주기

### 등록 방법 (Program.cs)

```csharp
builder.Services.AddScoped<IPolicyService, PolicyService>();
builder.Services.AddSingleton<IAppConfig, AppConfig>();
builder.Services.AddTransient<IAnalyzer, Analyzer>();
```

### 수명 선택 기준

| 수명 | 용도 | 주의 |
| --- | --- | --- |
| `Scoped` | 요청/사용자 단위 상태, 대부분의 서비스 | Blazor Server에서 circuit 단위 |
| `Singleton` | 앱 전역 캐시/설정 | 동시성 안전성 필요, mutable 상태 주의 |
| `Transient` | 상태 없는 짧은 작업 | 매 주입마다 새 인스턴스 생성 |

> Blazor Server에서 `Scoped`는 SignalR circuit 수명과 동일합니다. 사용자별 상태 유지에 적합합니다.

## 서비스 설계 패턴

### 인터페이스로 추상화

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

인터페이스 추상화의 장점:
- 테스트 시 mock으로 교체 가능
- 컴포넌트가 구현 상세를 알 필요 없음

## 상태관리 패턴

### 로컬 상태 (컴포넌트 필드)

```razor
@code {
    private bool isLoading;
    private string? errorMessage;
    private List<PolicyModel> policies = [];
}
```

컴포넌트 내부에서만 사용하는 상태에 적합합니다.

### 페이지 공유 상태 (Scoped 서비스)

여러 컴포넌트가 같은 사용자 세션에서 상태를 공유할 때 사용합니다.

```csharp
// 상태 서비스
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
<!-- 컴포넌트 A: 필터 입력 -->
@inject FilterState State

<input @oninput="e => State.SetSearch(e.Value?.ToString())" />
```

```razor
<!-- 컴포넌트 B: 목록 표시 -->
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

### PersistentComponentState (SSR → Interactive 전환 시 상태 유지)

Static SSR에서 Interactive로 전환할 때 초기 데이터를 전달합니다.

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

## 설계 팁

- 컴포넌트에서 외부 API 호출 코드를 직접 늘리지 않기
- 서비스 인터페이스를 통해 테스트 가능성 확보
- 전역 상태는 변경 이벤트와 일관성 정책을 명확히 정의

## 참고

- https://learn.microsoft.com/aspnet/core/blazor/fundamentals/dependency-injection
- https://learn.microsoft.com/aspnet/core/blazor/components/prerender#persist-prerendered-state
