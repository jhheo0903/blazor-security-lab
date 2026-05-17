# B6.DIServiceStateLab

[한국어](README.md) | English

**Docs**: `docs/blazor/06-di-services-state.md`

Example project covering Blazor dependency injection (DI), service design, and state management.

## Docs-to-Code Mapping

| Doc Section | Implementation File | Description |
|-------------|---------------------|-------------|
| @inject directive | `Components/Pages/ServiceInjectionDemo.razor` | Service injection and invocation |
| Scoped/Singleton lifetime | `Components/Pages/ScopedVsSingletonDemo.razor` | Comparing service lifetime behavior |
| State service | `Components/Pages/StateServiceDemo.razor` | Global state sharing (Singleton) |
| Service registration | `Program.cs` | AddScoped, AddSingleton registration |

## Services

- **ICounterService** (Scoped): Independent counter per page
- **IGlobalStateService** (Singleton): App-wide shared state
- **IPolicyService** (Scoped): Data query service

## How to Run

```bash
dotnet run --project Examples/B6.DIServiceStateLab/B6.DIServiceStateLab.csproj
```

## Build Only

```bash
dotnet build Examples/B6.DIServiceStateLab/B6.DIServiceStateLab.csproj
```

## Verification Steps

1. **Service injection** (`/service-injection`): Confirm PolicyService is called via @inject and a list is displayed
2. **Scoped vs Singleton** (`/scoped-vs-singleton`):
   - Increment the left-side Scoped counter, then press F5 → resets to 0
   - Increment the right-side Singleton counter, then press F5 → persists
3. **State service** (`/state-service`): Enter a message, navigate away, and come back — confirm it persists

## How IGlobalStateService Works (In Depth)

### Service Definition

```csharp
public class GlobalStateService : IGlobalStateService
{
    public string Message { get; set; } = "App-wide state (Singleton)";
    private int _globalCounter = 0;

    public int GlobalCounter => _globalCounter;

    public void IncrementGlobal() => _globalCounter++;
}
```

### Registration in Program.cs

```csharp
builder.Services.AddSingleton<IGlobalStateService, GlobalStateService>();
```
→ **AddSingleton**: creates the instance **once** at app start

### State Persistence Flow

```
┌─ App starts
│  ↓
│  GlobalStateService instance created (1 instance)
│  ├ Message = "App-wide state (Singleton)"
│  └ _globalCounter = 0
│
├─ User 1 connects (Circuit 1)
│  ↓ @inject IGlobalStateService GlobalState
│  ↓ Receives the above instance
│  ├ Reads Message
│  ├ Message = "new input" (modified!)
│  ├ IncrementGlobal() called (_globalCounter = 1)
│  └ UI updates in StateServiceDemo
│
├─ User 1 navigates to another page (Circuit 1 maintained)
│  ↓ Navigates to ScopedVsSingletonDemo
│  ↓ Same Circuit → same GlobalStateService instance
│  └ GlobalCounter = 1 (persisted!)
│
├─ User 1 returns to StateServiceDemo
│  ↓ Accessing the same instance
│  └ Message = "new input" (persisted!)
│
├─ User 1 presses F5 (page refresh)
│  ↓ New Circuit created
│  ↓ But GlobalStateService is NOT re-created
│  ├ GlobalCounter = 1 (!!still persisted!! — it's a Singleton)
│  └ Message = "new input" (!!still persisted!!)
│
└─ User 2 connects (Circuit 2)
   ↓ @inject IGlobalStateService GlobalState
   ↓ Same instance as above!
   └ GlobalCounter = 1 (value incremented by User 1!)
   └ Message = "new input" (value set by User 1!)
```

### Key Points

1. **What AddSingleton means**: `new GlobalStateService()` runs exactly once at app start; every inject/DI request returns the same instance.
2. **During Circuit**: Page navigation = same Circuit = same instance.
3. **After F5 refresh**: New Circuit is created, but GlobalStateService remains the same in-memory instance — state persists.
4. **Multiple users**: If User A modifies Message, User B will see the same modified value. ⚠️ **Watch out for race conditions!**

### StateServiceDemo Code Flow

```razor
@inject IGlobalStateService GlobalState

<input @bind="GlobalState.Message" />
← User input
  ↓
GlobalState.Message = input value
  ↓ (stored in memory!)
  ↓
Returns to this page after navigation
or after F5 refresh →
GlobalState.Message is still the input value
```
