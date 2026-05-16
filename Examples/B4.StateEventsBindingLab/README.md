# B4 State Events Binding Lab

이 프로젝트는 루트 기준서의 B4(상태, 이벤트, 데이터 바인딩)를 빠르게 체험하기 위한 최소 예제입니다.

## 루트 문서 연계

루트 기준 문서: [README.md](../../README.md)

| 루트 코드 | 루트 주제 | 상세 문서 | B4 예제 적용 | 근거 파일 |
| --- | --- | --- | --- | --- |
| B4 | 상태, 이벤트, 데이터 바인딩 | [04-state-events-binding.md](../../docs/blazor/04-state-events-binding.md) | 이벤트 핸들러 기반 상태 변경과 async 상태 전환 | [Components/Pages/StateCounterDemo.razor](Components/Pages/StateCounterDemo.razor) |
| B4 | 상태, 이벤트, 데이터 바인딩 | [04-state-events-binding.md](../../docs/blazor/04-state-events-binding.md) | `@bind`, `@bind:event="oninput"`, 형식 바인딩, 날짜 바인딩 | [Components/Pages/BindingLiveDemo.razor](Components/Pages/BindingLiveDemo.razor) |
| B4 | 상태, 이벤트, 데이터 바인딩 | [04-state-events-binding.md](../../docs/blazor/04-state-events-binding.md) | 자식에서 부모로 `EventCallback<T>` 이벤트 전달 | [Components/Pages/EventCallbackDemo.razor](Components/Pages/EventCallbackDemo.razor), [Components/Shared/RuleToggle.razor](Components/Shared/RuleToggle.razor) |

## 예제 구성

- Home: 예제 목적과 페이지 안내
- State Counter: 클릭/비동기 이벤트 후 상태 변경과 렌더링 갱신
- Binding Live: 단방향/양방향/실시간 바인딩과 형식 바인딩
- Event Callback: 자식 컴포넌트 이벤트를 부모 상태에 반영

## 테스트 방법

1. 아래 명령으로 프로젝트를 실행합니다.
2. Home에서 안내를 확인한 뒤 State Counter, Binding Live, Event Callback 순서로 이동합니다.
3. State Counter에서 Increment, Load Async 실행 후 상태 텍스트와 카운트가 갱신되는지 확인합니다.
4. Binding Live에서 이름 입력 시 즉시 반영(oninput)되는지, 가격/날짜 바인딩 결과가 표시되는지 확인합니다.
5. Event Callback에서 Apply 버튼을 누르면 부모의 마지막 선택 규칙 메시지가 변경되는지 확인합니다.

```bash
dotnet restore
dotnet run --project Examples/B4.StateEventsBindingLab/B4.StateEventsBindingLab.csproj
```

빌드만 확인하려면 아래 명령을 사용합니다.

```bash
dotnet build Examples/B4.StateEventsBindingLab/B4.StateEventsBindingLab.csproj
```
