# B8. JS Interop 사용 원칙

## 언제 필요한가

- 브라우저 전용 API가 필요한 경우 (clipboard, localStorage, geolocation 등)
- Blazor 기본 컴포넌트만으로 구현이 어려운 UI 위젯 연동 (차트, 에디터 등)
- 기존 JS 자산을 재활용해야 하는 경우

## C#에서 JS 호출

### InvokeVoidAsync (반환값 없음)

```razor
@inject IJSRuntime JS

<button @onclick="ShowAlert">알림</button>

@code {
    private async Task ShowAlert()
    {
        await JS.InvokeVoidAsync("alert", "Hello from Blazor");
    }
}
```

### InvokeAsync (반환값 있음)

```razor
@inject IJSRuntime JS

@code {
    private async Task<string> GetLocalStorage(string key)
    {
        return await JS.InvokeAsync<string>("localStorage.getItem", key) ?? string.Empty;
    }
}
```

## JS 모듈 사용 (권장 방식)

전역 함수 대신 ES 모듈로 JS를 관리합니다. `wwwroot/js/` 에 파일을 배치합니다.

```javascript
// wwwroot/js/myModule.js
export function showMessage(message) {
    alert(message);
}

export function focusElement(element) {
    element.focus();
}
```

```razor
@inject IJSRuntime JS
@implements IAsyncDisposable

@code {
    private IJSObjectReference? _module;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _module = await JS.InvokeAsync<IJSObjectReference>(
                "import", "./js/myModule.js");
        }
    }

    private async Task ShowMessage()
    {
        if (_module is not null)
        {
            await _module.InvokeVoidAsync("showMessage", "모듈에서 호출");
        }
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
        {
            await _module.DisposeAsync();
        }
    }
}
```

## DOM 요소 참조 전달

`ElementReference`를 JS로 전달해 직접 DOM 조작을 위임합니다.

```razor
@inject IJSRuntime JS

<input @ref="inputRef" type="text" />
<button @onclick="FocusInput">포커스</button>

@code {
    private ElementReference inputRef;

    private async Task FocusInput()
    {
        await JS.InvokeVoidAsync("focusElement", inputRef);
    }
}
```

## JS에서 C# 호출

`DotNetObjectReference`를 사용해 JS에서 C# 메서드를 역호출합니다.

```razor
@inject IJSRuntime JS
@implements IAsyncDisposable

@code {
    private DotNetObjectReference<MyComponent>? _dotNetRef;

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _dotNetRef = DotNetObjectReference.Create(this);
            await JS.InvokeVoidAsync("registerCallback", _dotNetRef);
        }
    }

    [JSInvokable]
    public void OnExternalEvent(string data)
    {
        InvokeAsync(StateHasChanged);
    }

    public async ValueTask DisposeAsync()
    {
        _dotNetRef?.Dispose();
    }
}
```

## 서비스 추상화 예시

```csharp
public interface IChartService
{
    ValueTask InitAsync(string elementId);
    ValueTask UpdateDataAsync(IEnumerable<double> values);
}

public sealed class ChartService : IChartService, IAsyncDisposable
{
    private readonly IJSRuntime _js;
    private IJSObjectReference? _module;

    public ChartService(IJSRuntime js) => _js = js;

    public async ValueTask InitAsync(string elementId)
    {
        _module ??= await _js.InvokeAsync<IJSObjectReference>("import", "./js/chart.js");
        await _module.InvokeVoidAsync("init", elementId);
    }

    public async ValueTask UpdateDataAsync(IEnumerable<double> values)
    {
        if (_module is not null)
            await _module.InvokeVoidAsync("update", values);
    }

    public async ValueTask DisposeAsync()
    {
        if (_module is not null)
            await _module.DisposeAsync();
    }
}
```

## 설계 원칙

- 인터롭 호출을 서비스로 감싸 컴포넌트 의존도 축소
- 전역 함수 대신 ES 모듈 방식 사용
- prerender 구간에서는 JS 호출 불가 → `OnAfterRenderAsync(firstRender)` 사용
- `IJSObjectReference`, `DotNetObjectReference`는 반드시 Dispose

## 체크리스트

- 호출 실패 시 fallback UX가 있는가?
- JS 모듈 로드 시점이 렌더링 시점과 충돌하지 않는가?
- `IJSObjectReference`, `DotNetObjectReference`를 Dispose하는가?
- prerender 구간 보호가 되어 있는가?

## 참고

- https://learn.microsoft.com/aspnet/core/blazor/javascript-interoperability/
- https://learn.microsoft.com/aspnet/core/blazor/javascript-interoperability/call-javascript-from-dotnet
- https://learn.microsoft.com/aspnet/core/blazor/javascript-interoperability/call-dotnet-from-javascript
