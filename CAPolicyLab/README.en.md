# CAPolicyLab

[한국어](README.md) | English

A hands-on project log exploring Microsoft Entra ID Conditional Access (CA) policy visualization in Blazor.
The focus is on connecting ideas rather than feature delivery — experiencing component-based thinking firsthand.

## Root Document Mapping

Root reference: [README.md](../README.md)

| Code | Topic | Detail Doc | CAPolicyLab Application | Source File |
| --- | --- | --- | --- | --- |
| B2 | Hosting Models and Rendering Modes | [02-hosting-render-modes.md](../docs/blazor/02-hosting-render-modes.md) | Uses Interactive Server mode | [Program.cs](Program.cs) |
| B3 | Component Structure and Routing | [03-components-routing.md](../docs/blazor/03-components-routing.md) | Pages/Shared separation, App/Routes routing setup | [Components/App.razor](Components/App.razor), [Components/Routes.razor](Components/Routes.razor) |
| B4 | State, Events, Data Binding | [04-state-events-binding.md](../docs/blazor/04-state-events-binding.md) | State and event handling in page components | [Components/Pages](Components/Pages) |
| B6 | DI, Services, State Management | [06-di-services-state.md](../docs/blazor/06-di-services-state.md) | Logic separated into a Services layer with DI | [Services](Services), [Program.cs](Program.cs) |
| B10 | Coding Style Guide | [10-coding-style.md](../docs/blazor/10-coding-style.md) | Separation of component/service/model responsibilities | [Components](Components), [Models](Models), [Services](Services) |

## Hosting Model

CAPolicyLab runs as a Blazor Interactive Server app.

- Service registration: `AddInteractiveServerComponents`
- Render mode mapping: `AddInteractiveServerRenderMode`

Source location: [CAPolicyLab/Program.cs](Program.cs)

```csharp
builder.Services.AddRazorComponents()
  .AddInteractiveServerComponents();

app.MapRazorComponents<App>()
  .AddInteractiveServerRenderMode();
```

What this means:
- UI event handling runs on the server.
- The browser and server communicate via a persistent real-time connection.
- In this project, observe state/rendering flow from a server-centric Blazor perspective.

## Learning Goals

- Build UI thinking around components
- Understand the relationship between state changes and rendering
- Separate UI code from service code by responsibility

## Practice Flow

```mermaid
flowchart TD
  A[1 Read Structure] --> B[2 Read State]
  B --> C[3 Read Services]
  C --> D[4 Make Changes]
  D --> E[5 Explain It]

  A1[Check entry point in Program.cs] --> A
  A2[Trace App.razor and Routes.razor] --> A
  A3[Distinguish roles of Pages vs Shared] --> A

  B1[Find "@code" state variables] --> B
  B2[Confirm "@bind" input connections] --> B
  B3[Trace event handler execution paths] --> B

  C1[Locate "@inject" usage] --> C
  C2[Trace service call flow] --> C
  C3[Map Models to UI] --> C
```

## Step-by-Step Practice

### Step 1: Read the Structure

1. Check entry point: `Program.cs`
2. Check routing: `Components/App.razor`, `Components/Routes.razor`
3. Review screen groups: `Components/Pages`, `Components/Shared`

Check questions:
- Is the boundary between page components and reusable components clear?
- Where is routing decided?

### Step 2: Read State

1. Open `Pages/Home.razor` or `Pages/PolicyFilter.razor`
2. Identify state variables and event handlers first
3. Trace which property each input value binds to

Check questions:
- Can you see all state change points at a glance?
- Can you predict which UI changes when a given state changes?

### Step 3: Read Services

1. Open `Services/PolicyAnalyzerService.cs`
2. Find the service call points in the page
3. Trace how `Models/ConditionalAccessPolicyModel.cs` maps to the UI

Check questions:
- Is UI logic cleanly separated from domain logic?
- Do the service method inputs/outputs match what the UI needs?

### Step 4: Make Changes

1. Add a small filter UI element to a page
2. Add a display condition based on existing service results
3. Verify the behavior before and after the change

Check questions:
- Was the change scope limited to within the component?
- If service changes were needed, were they minimal?

### Step 5: Explain It

Try to explain the following in plain language:

- What is the core state of this page?
- Which event changes which state?
- What maintenance benefit does service separation provide?

## Running the App

```bash
dotnet restore
dotnet run --project CAPolicyLab.csproj
```

After launching, navigate between pages and observe the state → rendering flow.

## Testing

1. Run the app and navigate to the main pages (Home, PolicyFilter, WhatIfSimulator).
2. Verify that changing input values updates the UI as expected.
3. Check the browser DevTools Network tab for requests/responses and errors.
4. Try edge cases (empty input, invalid values) and confirm validation messages and error handling.

Build and run commands:

```bash
dotnet build CAPolicyLab/CAPolicyLab.csproj
dotnet run --project CAPolicyLab/CAPolicyLab.csproj
```
