# B5. 컴포넌트 생명주기

## 생명주기 흐름

```
컴포넌트 생성
    ↓
SetParametersAsync          ← 파라미터 설정 (커스터마이징 가능)
    ↓
OnInitialized(Async)        ← 최초 1회 초기화
    ↓
OnParametersSet(Async)      ← 파라미터 변경 시마다
    ↓
렌더링 (BuildRenderTree)
    ↓
OnAfterRender(Async)        ← 렌더 완료 후
    ↓
(파라미터 변경 시 OnParametersSet부터 반복)
    ↓
Dispose / DisposeAsync      ← 컴포넌트 제거 시
```

## 주요 메서드 상세

### OnInitialized / OnInitializedAsync

컴포넌트가 **최초 생성될 때 1회** 실행됩니다.

```razor
@code {
    private List<string> items = [];

    protected override async Task OnInitializedAsync()
    {
        // 초기 데이터 로드
        items = await DataService.GetItemsAsync();
    }
}
```

> prerender 환경에서는 **2번 실행**될 수 있습니다(서버 prerender 1회 + interactive 연결 후 1회).
> 이 점을 고려해 멱등성 있게 설계합니다.

### OnParametersSet / OnParametersSetAsync

부모로부터 파라미터가 변경될 때마다 실행됩니다.

```razor
@code {
    [Parameter]
    public string? Filter { get; set; }

    private List<string> filtered = [];

    protected override async Task OnParametersSetAsync()
    {
        // Filter 파라미터가 바뀔 때마다 재계산
        filtered = await DataService.GetFilteredAsync(Filter);
    }
}
```

### OnAfterRender / OnAfterRenderAsync

HTML이 DOM에 실제로 렌더링된 후 실행됩니다.

```razor
@inject IJSRuntime JS

@code {
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            // DOM이 준비된 후에만 JS 호출 가능
            await JS.InvokeVoidAsync("initChart", "chart-container");
        }
    }
}
```

- `firstRender: true` = 첫 번째 렌더 이후
- `firstRender: false` = 이후 모든 렌더 이후
- JS 연동, 포커스 설정, 스크롤 제어 등에 사용

### ShouldRender

불필요한 렌더링을 막기 위해 override합니다.

```razor
@code {
    private bool _shouldRender = true;

    protected override bool ShouldRender()
    {
        return _shouldRender;
    }
}
```

> 기본값은 `true`입니다. 성능이 중요한 컴포넌트에서만 사용합니다.

## IDisposable / IAsyncDisposable

컴포넌트가 DOM에서 제거될 때 정리 작업을 수행합니다.

```razor
@implements IAsyncDisposable

@code {
    private Timer? _timer;

    protected override void OnInitialized()
    {
        _timer = new Timer(OnTick, null, 0, 1000);
    }

    private void OnTick(object? state)
    {
        InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        if (_timer is not null)
        {
            await _timer.DisposeAsync();
        }
    }
}
```

이벤트 구독 해제, 타이머 정리, JS 객체 참조 해제 등에 필수입니다.

## SetParametersAsync (고급)

파라미터 설정 전체 흐름을 직접 제어할 때 override합니다.

```razor
@code {
    public override async Task SetParametersAsync(ParameterView parameters)
    {
        // 파라미터 처리 커스터마이징
        await base.SetParametersAsync(parameters);
    }
}
```

> 일반 케이스에서는 override할 필요가 없습니다.

## 자주 하는 실수

| 실수 | 원인 | 해결 |
| --- | --- | --- |
| JS 호출이 실패 | DOM 준비 전 호출 | `OnAfterRenderAsync(firstRender)` 안에서 호출 |
| 초기화가 2번 실행 | prerender 이중 실행 | 멱등성 보장, 또는 prerender 비활성화 |
| 타이머가 멈추지 않음 | Dispose 미구현 | `IAsyncDisposable` 구현 |
| 파라미터 변경 미반영 | `OnInitializedAsync`만 사용 | `OnParametersSetAsync` 병행 사용 |

## 참고

- https://learn.microsoft.com/aspnet/core/blazor/components/lifecycle
