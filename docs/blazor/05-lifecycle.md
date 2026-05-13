# B5. 컴포넌트 생명주기

## 주요 메서드

- OnInitialized / OnInitializedAsync
- OnParametersSet / OnParametersSetAsync
- OnAfterRender / OnAfterRenderAsync

## 언제 무엇을 쓰나

- 초기 데이터 로드: OnInitializedAsync
- 부모 파라미터 변화 반영: OnParametersSetAsync
- DOM 또는 JS 호출: OnAfterRenderAsync(firstRender)

## 예시

```razor
@code {
    protected override async Task OnInitializedAsync()
    {
        await LoadInitialDataAsync();
    }

    protected override async Task OnParametersSetAsync()
    {
        await RecalculateByParametersAsync();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            await InitializeJsAsync();
        }
    }

    private Task LoadInitialDataAsync() => Task.CompletedTask;
    private Task RecalculateByParametersAsync() => Task.CompletedTask;
    private Task InitializeJsAsync() => Task.CompletedTask;
}
```

## 주의

- prerender 구간과 interactive 구간의 동작 시점을 구분해서 설계해야 합니다.
- 무거운 작업을 매 렌더마다 반복하면 체감 성능이 급격히 떨어집니다.
