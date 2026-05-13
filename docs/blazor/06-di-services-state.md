# B6. DI, 서비스 분리, 상태관리

## 기본 원칙

- 컴포넌트는 UI 상태와 사용자 액션 중심
- 서비스는 도메인 로직과 외부 통신 중심
- 상태 공유는 최소 범위로 제한

## 주입 예시

```razor
@inject PolicyAnalyzerService Analyzer

<button @onclick="Analyze">Analyze</button>

@code {
    private async Task Analyze()
    {
        await Analyzer.AnalyzeAsync();
    }
}
```

## 수명주기 선택 가이드

- scoped: 페이지/요청 단위 상태 및 서비스
- singleton: 앱 전역 설정/캐시(동시성 주의)
- transient: 상태를 보관하지 않는 짧은 작업

## 설계 팁

- 컴포넌트에서 외부 API 호출 코드를 직접 늘리지 않기
- 서비스 인터페이스를 통해 테스트 가능성 확보
- 전역 상태는 변경 이벤트와 일관성 정책을 명확히 정의
