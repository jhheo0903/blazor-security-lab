# B5.LifecycleLab

[한국어](README.md) | [English](README.en.md)

**문서**: `docs/blazor/05-lifecycle.md`

Blazor 컴포넌트 생명주기 메서드 예제 프로젝트입니다.

## 문서-코드 매핑

| 문서 섹션 | 구현 파일 | 설명 |
|-----------|-----------|------|
| OnInitialized / OnInitializedAsync | `Components/Pages/LifecycleInitDemo.razor` | 비동기 초기화, 로딩 상태 처리 |
| OnParametersSet | `Components/Pages/LifecycleParamsDemo.razor` + `Components/Shared/ParamChild.razor` | 파라미터 변경 감지, 자식 컴포넌트 반응 |
| OnAfterRender | `Components/Pages/LifecycleAfterRenderDemo.razor` | DOM 렌더 후 JS 연동, 렌더 횟수 추적 |
| IDisposable | `Components/Pages/LifecycleDisposeDemo.razor` | 타이머 리소스 정리, Dispose 패턴 |

## 실행 방법

```bash
dotnet run --project Examples/B5.LifecycleLab/B5.LifecycleLab.csproj
```

## 빌드 방법

```bash
dotnet build Examples/B5.LifecycleLab/B5.LifecycleLab.csproj
```

## 확인 단계

1. **OnInitialized** (`/lifecycle-init`): 페이지 진입 시 0.8초 로딩 후 목록 표시 확인
2. **OnParametersSet** (`/lifecycle-params`): 입력창 타이핑 시 자식 컴포넌트의 호출 횟수 증가 확인
3. **OnAfterRender** (`/lifecycle-afterrender`): 페이지 진입 시 포커스 자동 이동, 렌더 횟수 표시 확인
4. **IDisposable** (`/lifecycle-dispose`): 초당 경과 시간 카운트 → 다른 페이지 이동 후 돌아오면 Dispose 호출 확인

## OnAfterRender 동작 방식 (쉽게 보기)

대상 파일: `Components/Pages/LifecycleAfterRenderDemo.razor`

1. 페이지가 처음 렌더링되면 `OnAfterRenderAsync(bool firstRender)`가 호출됩니다.
2. 메서드 진입 시 `renderCount++`가 실행되어 렌더 횟수를 누적합니다.
3. 첫 렌더일 때만(`firstRender == true`) JS를 호출합니다.
4. JS 호출은 `IJSRuntime`을 통해 수행되며, `focusTarget` input에 포커스를 줍니다.
5. 이후 상태 메시지(`jsMessage`)를 "첫 렌더 후 JS 실행 완료"로 바꿔 화면에 반영합니다.

핵심 포인트:

- `IJSRuntime`은 Blazor(C#)에서 브라우저 JavaScript를 실행하는 통로입니다.
- `OnAfterRenderAsync`는 DOM이 실제로 그려진 뒤에 실행되므로, 포커스 이동/크기 측정/외부 JS 라이브러리 초기화에 적합합니다.
- `firstRender` 체크를 하지 않으면 렌더링이 발생할 때마다 JS가 반복 호출될 수 있습니다.

