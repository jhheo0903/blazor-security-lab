# B6.DIServiceStateLab

**문서**: `docs/blazor/06-di-services-state.md`

Blazor의 의존성 주입(DI), 서비스 설계, 상태 관리를 다루는 예제 프로젝트입니다.

## 문서-코드 매핑

| 문서 섹션 | 구현 파일 | 설명 |
|-----------|-----------|------|
| @inject 지시문 | `Components/Pages/ServiceInjectionDemo.razor` | 서비스 주입 및 호출 |
| Scoped/Singleton 수명 | `Components/Pages/ScopedVsSingletonDemo.razor` | 서비스 수명 차이 비교 |
| 상태 서비스 | `Components/Pages/StateServiceDemo.razor` | 전역 상태 공유 (Singleton) |
| 서비스 등록 | `Program.cs` | AddScoped, AddSingleton 등록 |

## 서비스 목록

- **ICounterService** (Scoped): 페이지별 독립적인 카운터
- **IGlobalStateService** (Singleton): 앱 전역 상태 공유
- **IPolicyService** (Scoped): 데이터 조회 서비스

## 실행 방법

\`\`\`bash
dotnet run --project Examples/B6.DIServiceStateLab/B6.DIServiceStateLab.csproj
\`\`\`

## 빌드 방법

\`\`\`bash
dotnet build Examples/B6.DIServiceStateLab/B6.DIServiceStateLab.csproj
\`\`\`

## 확인 단계

1. **서비스 주입** (`/service-injection`): @inject로 PolicyService 호출, 목록 표시 확인
2. **Scoped vs Singleton** (`/scoped-vs-singleton`): 
   - 좌측 Scoped 카운트 증가 후 F5 새로고침 → 0으로 리셋
   - 우측 Singleton 카운트 증가 후 F5 새로고침 → 유지됨
3. **상태 서비스** (`/state-service`): 메시지 입력 후 다른 페이지 이동 → 돌아오면 유지됨

## IGlobalStateService 작동 방식 (상세 분석)

### 서비스 정의

```csharp
public class GlobalStateService : IGlobalStateService
{
    public string Message { get; set; } = "앱 전역 상태 (Singleton)";
    private int _globalCounter = 0;

    public int GlobalCounter => _globalCounter;

    public void IncrementGlobal() => _globalCounter++;
}
```

### Program.cs 등록
```csharp
builder.Services.AddSingleton<IGlobalStateService, GlobalStateService>();
```
→ **AddSingleton**: 앱 시작 시 **한 번만** 인스턴스 생성

### 상태 유지 흐름도

```
┌─ 앱 시작
│  ↓
│  GlobalStateService 인스턴스 1개 생성
│  ├ Message = "앱 전역 상태 (Singleton)"
│  └ _globalCounter = 0
│
├─ 사용자 1 접속 (Circuit 1)
│  ↓ @inject IGlobalStateService GlobalState
│  ↓ 위 **인스턴스를 주입받음**
│  ├ Message 읽기 (가능)
│  ├ Message = "새 메시지 입력" (수정!)
│  ├ IncrementGlobal() 호출 (_globalCounter = 1)
│  └ StateServiceDemo 페이지에서 UI 갱신
│
├─ 사용자 1이 다른 페이지 이동 (Circuit 1 유지)
│  ↓ ScopedVsSingletonDemo 페이지로 이동
│  ↓ 같은 Circuit이므로 **같은 GlobalStateService 인스턴스**
│  └ GlobalCounter = 1 (유지됨!)
│
├─ 사용자 1이 StateServiceDemo로 돌아옴
│  ↓ **같은 인스턴스 접근**
│  └ Message = "새 메시지 입력" (유지됨!)
│
├─ 사용자 1이 F5 새로고침
│  ↓ 새로운 Circuit 생성
│  ↓ 하지만 **GlobalStateService는 새로 생성하지 않음**
│  ├ GlobalCounter = 1 (!!유지됨!! Singleton이므로)
│  └ Message = "새 메시지 입력" (!!유지됨!!)
│
└─ 다른 사용자 2 접속 (Circuit 2)
   ↓ @inject IGlobalStateService GlobalState
   ↓ **위와 같은 인스턴스!**
   └ GlobalCounter = 1 (사용자 1이 증가시킨 값!)
   └ Message = "새 메시지 입력" (사용자 1이 입력한 값!)
```

### 핵심 포인트

1. **AddSingleton의 의미**
   - 앱 시작 시: `new GlobalStateService()` 1회만 실행
   - 모든 inject/DI 요청: 같은 인스턴스 반환

2. **Circuit 유지 중**
   - 페이지 이동 = 같은 Circuit = **같은 인스턴스**
   - StateServiceDemo → ScopedVsSingletonDemo → StateServiceDemo
   - 모두 같은 GlobalStateService 접근

3. **F5 새로고침 후**
   - 새 Circuit 생성 (SignalR 재연결)
   - 하지만 GlobalStateService는 여전히 메모리에 같은 인스턴스
   - **상태 값 유지됨!**

4. **여러 사용자일 때**
   - 사용자 A가 Message 수정 → "사용자A 메시지"
   - 사용자 B 접속 → **같은 GlobalStateService 인스턴스**
   - 사용자 B도 "사용자A 메시지" 시각
   - ⚠️ **Race condition 주의!**

### StateServiceDemo 코드 흐름

```razor
@inject IGlobalStateService GlobalState

<input @bind="GlobalState.Message" />
← 사용자 입력
  ↓
GlobalState.Message = 입력값
  ↓ (메모리에 저장됨!)
  ↓
다른 페이지 이동 후 돌아와도
또는 F5 새로고침 후 돌아와도
GlobalState.Message는 **여전히** 입력값
```
