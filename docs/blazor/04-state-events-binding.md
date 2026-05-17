# B4. 상태, 이벤트, 데이터 바인딩

[한국어](04-state-events-binding.md) | [English](04-state-events-binding.en.md)

## 상태와 렌더링

Blazor에서 **상태(private field)가 바뀌면 UI가 자동으로 다시 렌더링**됩니다.
이벤트 핸들러가 실행된 후 렌더링 사이클이 자동으로 트리거됩니다.

핵심 원칙:
- 상태 변경은 이벤트 핸들러 내에서만 수행
- UI는 상태의 "함수"로 설계

## 이벤트 처리

### 기본 이벤트 처리

```razor
<button @onclick="Increment">Count: @count</button>

@code {
    private int count;

    private void Increment()
    {
        count++;
    }
}
```

### 비동기 이벤트 핸들러

```razor
<button @onclick="LoadDataAsync">불러오기</button>
<p>@message</p>

@code {
    private string message = string.Empty;

    private async Task LoadDataAsync()
    {
        message = "로딩 중...";
        await Task.Delay(500);   // 실제로는 서비스 호출
        message = "완료";
    }
}
```

### 이벤트 인자(EventArgs)

```razor
<input @onkeydown="HandleKeyDown" />

@code {
    private void HandleKeyDown(KeyboardEventArgs e)
    {
        if (e.Key == "Enter")
        {
            // 엔터 처리
        }
    }
}
```

주요 EventArgs: `MouseEventArgs`, `KeyboardEventArgs`, `ChangeEventArgs`, `FocusEventArgs`

### 람다 이벤트 핸들러

```razor
@foreach (var item in items)
{
    <button @onclick="() => Select(item)">@item.Name</button>
}

@code {
    private List<Item> items = [];

    private void Select(Item item) { }
}
```

> 주의: 루프 내 람다는 클로저 캡처가 일어납니다. `foreach`의 `item`은 안전하지만 `for`의 인덱스 변수는 값을 복사해 사용해야 합니다.

## 데이터 바인딩

### 단방향 바인딩

```razor
<p>@message</p>   ← 읽기 전용 표현
```

### 양방향 바인딩 (@bind)

```razor
<input @bind="name" />
<p>Hello, @name</p>

@code {
    private string name = string.Empty;
}
```

`@bind`는 기본적으로 `onchange` 이벤트(포커스 벗어날 때) 기준으로 동기화됩니다.

### 실시간 동기화 (oninput)

```razor
<input @bind="name" @bind:event="oninput" />
```

### 바인딩 형식 지정

```razor
<input @bind="price" @bind:format="F2" />   ← 소수점 2자리 형식

@code {
    private decimal price = 1.5m;
}
```

### 날짜 바인딩

```razor
<input type="date" @bind="selectedDate" />

@code {
    private DateTime selectedDate = DateTime.Today;
}
```

## EventCallback

자식에서 부모로 이벤트를 올릴 때 사용합니다.

```razor
<!-- 자식 컴포넌트 -->
<button @onclick="NotifyParent">선택</button>

@code {
    [Parameter]
    public EventCallback<string> OnSelected { get; set; }

    private async Task NotifyParent()
    {
        await OnSelected.InvokeAsync("선택된 값");
    }
}
```

```razor
<!-- 부모 컴포넌트 -->
<ChildComponent OnSelected="HandleSelection" />

@code {
    private void HandleSelection(string value)
    {
        // value 처리
    }
}
```

`EventCallback`은 자동으로 `StateHasChanged()`를 호출합니다.

## StateHasChanged 수동 호출

렌더링 사이클 외부에서 상태가 변경된 경우 수동으로 렌더링을 요청합니다.

```razor
@code {
    private async Task UpdateFromTimer()
    {
        // 타이머/외부 이벤트 등 Blazor 이벤트 루프 밖에서 상태 변경 시
        count++;
        await InvokeAsync(StateHasChanged);
    }
}
```

## CascadingValue / CascadingParameter

컴포넌트 계층을 따라 값을 전파할 때 사용합니다. 로그인 사용자, 테마 등 공통 값에 적합합니다.

```razor
<!-- 상위 컴포넌트 -->
<CascadingValue Value="currentUser">
    <ChildTree />
</CascadingValue>
```

```razor
<!-- 하위 컴포넌트 어디서든 -->
@code {
    [CascadingParameter]
    public UserInfo? CurrentUser { get; set; }
}
```

## 선택 기준

| 상황 | 권장 방식 |
| --- | --- |
| 입력과 상태를 즉시 동기화 | `@bind` |
| 처리 타이밍, 검증, 전처리 필요 | 이벤트 핸들러 |
| 자식 → 부모 알림 | `EventCallback` |
| 계층 전체에 값 전파 | `CascadingValue` |

## 실수 방지

- 상태 변경 지점이 여러 군데 분산되면 디버깅이 어려워집니다.
- 루프 내 람다 클로저 캡처 문제를 인지하고 사용하세요.
- 외부 스레드/타이머에서 UI 상태를 변경할 때는 `InvokeAsync(StateHasChanged)`를 사용합니다.

## 참고

- https://learn.microsoft.com/aspnet/core/blazor/components/event-handling
- https://learn.microsoft.com/aspnet/core/blazor/components/data-binding
- https://learn.microsoft.com/aspnet/core/blazor/components/cascading-values-and-parameters
