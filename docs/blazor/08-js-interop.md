# B8. JS Interop 사용 원칙

## 언제 필요한가

- 브라우저 전용 API가 필요한 경우
- Blazor 기본 컴포넌트만으로 구현이 어려운 UI 위젯 연동
- 기존 JS 자산을 재활용해야 하는 경우

## 최소 예시

```razor
@inject IJSRuntime JS

<button @onclick="ShowAlert">Alert</button>

@code {
    private async Task ShowAlert()
    {
        await JS.InvokeVoidAsync("alert", "Hello from Blazor");
    }
}
```

## 설계 원칙

- 인터롭 호출을 서비스로 감싸 컴포넌트 의존도 축소
- 문자열 기반 함수명 남발을 줄이고 래퍼 API 제공
- static SSR/prerender 구간 동작 차이를 고려

## 체크리스트

- 호출 실패 시 fallback UX가 있는가?
- JS 모듈 로드 시점이 렌더링 시점과 충돌하지 않는가?
- 테스트 가능한 경계(서비스 인터페이스)가 있는가?
