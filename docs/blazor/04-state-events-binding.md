# B4. 상태, 이벤트, 데이터 바인딩

## 상태와 렌더링

Blazor에서 상태가 바뀌면 UI가 다시 계산됩니다.
핵심은 "어디서 상태를 바꾸는지"를 명확히 두는 것입니다.

## 이벤트 처리 예시

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

## @bind 예시

```razor
<input @bind="name" />
<p>Hello, @name</p>

@code {
    private string name = string.Empty;
}
```

## 선택 기준

- @bind: 사용자 입력과 상태를 즉시 동기화할 때
- 이벤트 핸들러: 처리 타이밍/검증/전처리를 제어할 때

## 실수 방지

- 상태 변경 지점이 여러 군데 분산되면 디버깅이 어려워집니다.
- 상태와 파생 계산을 분리하지 않으면 렌더 결과 예측이 어려워집니다.
