# B1. Blazor 기본 개념

## 무엇이 다른가

Blazor는 C#으로 웹 UI를 작성하는 컴포넌트 기반 프레임워크입니다.
기존 ASP.NET MVC/API 개발자에게는 아래 세 가지 사고 전환이 핵심입니다.

| 기존 사고 | Blazor 사고 |
| --- | --- |
| 요청/응답 중심 | 상태 중심 UI |
| 페이지 단위 화면 | 컴포넌트 조합 |
| JS로 DOM 직접 조작 | 상태 변경 → 렌더링 자동 반영 |

## .razor 파일 구조

Blazor 컴포넌트는 `.razor` 파일 하나에 HTML, C#, CSS(선택)를 포함합니다.

```razor
@page "/counter"           ← 라우트 선언 (페이지 컴포넌트)
@inject ILogger<Counter> Logger  ← 서비스 주입

<h3>카운터: @count</h3>
<button @onclick="Increment">+1</button>

@code {
    private int count;

    private void Increment()
    {
        count++;
        Logger.LogInformation("Count: {Count}", count);
    }
}
```

- `@page`: 라우팅 가능한 페이지로 선언합니다.
- `@inject`: DI 컨테이너에서 서비스를 주입합니다.
- `@code { }`: C# 코드 블록입니다.
- `@onclick`: 이벤트 핸들러를 연결합니다.

## 컴포넌트 기본 규칙

**상태(field)가 바뀌면 렌더링이 자동으로 다시 수행됩니다.**

```razor
<p>현재 상태: @message</p>
<button @onclick="ChangeMessage">변경</button>

@code {
    private string message = "초기값";

    private void ChangeMessage()
    {
        message = "변경됨";   // 이 변경이 렌더링을 유도합니다.
    }
}
```

## 파라미터 전달

부모 컴포넌트에서 자식 컴포넌트로 값을 전달합니다.

```razor
<!-- 부모 -->
<ChildComponent Title="안녕하세요" />
```

```razor
<!-- ChildComponent.razor -->
<h4>@Title</h4>

@code {
    [Parameter]
    public string Title { get; set; } = string.Empty;
}
```

- `[Parameter]`로 선언된 프로퍼티만 외부에서 전달받을 수 있습니다.
- 파라미터 이름은 대소문자를 구분합니다.

## 이벤트 콜백

자식에서 부모로 이벤트를 전달할 때 `EventCallback`을 사용합니다.

```razor
<!-- 부모 -->
<ChildComponent OnSelected="HandleSelected" />

@code {
    private void HandleSelected(string value)
    {
        // 자식이 선택했을 때 처리
    }
}
```

```razor
<!-- 자식 -->
<button @onclick="Select">선택</button>

@code {
    [Parameter]
    public EventCallback<string> OnSelected { get; set; }

    private async Task Select()
    {
        await OnSelected.InvokeAsync("선택된 값");
    }
}
```

## .NET 10 생성자 주입

.NET 10부터 컴포넌트에서 생성자 주입을 지원합니다.

```razor
@* @inject 대신 생성자 주입을 사용할 수 있습니다 *@

@code {
    private readonly IMyService _service;

    public MyComponent(IMyService service)
    {
        _service = service;
    }
}
```

## C# 개발자 관점 핵심

- 컨트롤러 중심 사고보다 "상태 중심 UI" 사고가 중요합니다.
- DTO/서비스 분리는 기존 C# 백엔드 습관을 그대로 살릴 수 있습니다.
- 테스트는 UI 단위보다 서비스 단위를 먼저 안정화하는 전략이 효과적입니다.

## 체크리스트

- 컴포넌트 경계를 설명할 수 있는가?
- 상태 변경 지점과 렌더 결과를 연결해 설명할 수 있는가?
- 서비스 분리 기준을 말할 수 있는가?
- 부모-자식 간 데이터/이벤트 흐름 방향을 이해하는가?

## 참고

- https://learn.microsoft.com/aspnet/core/blazor/components/
- https://learn.microsoft.com/aspnet/core/blazor/components/event-handling
