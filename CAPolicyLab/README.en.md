# CAPolicyLab

[한국어](README.md) | English

A hands-on project that visualizes and analyzes Microsoft Entra ID Conditional Access (CA) policies using Blazor Interactive Server.
Policies are fetched directly via the Graph API; conflict detection, What-If simulation, and Mermaid flowchart generation are all performed locally.

---

## Prerequisites

Familiarity with the concepts below will help you understand this project.

### Microsoft Entra ID / Azure AD

| Concept | Description |
|---|---|
| **Tenant** | An organization-scoped Azure AD directory, identified by a TenantId (GUID) |
| **App Registration** | Registering a client app in Entra ID so it can call APIs. Produces a ClientId |
| **Client Credentials Flow** | An OAuth 2.0 flow that calls APIs using the app's own permissions — no user sign-in required. Uses a ClientSecret |
| **Application Permissions** | Service-level permissions that require Admin Consent before they take effect |

Required application permissions for this project:

- `Policy.Read.All` — read Conditional Access policies
- `Directory.Read.All` — read users, groups, and app information

### Conditional Access

A CA policy defines **who (Users/Groups)** · **which apps (Applications)** · **under what conditions (Platforms, Locations)** are subject to **which controls (Grant Controls)**.

| State | Meaning |
|---|---|
| `enabled` | Actively enforced. Access is blocked if controls are not satisfied |
| `disabled` | Inactive. Excluded from policy evaluation |
| `enabledForReportingButNotEnforced` | Report-Only. Logs the result but does not block access |

Grant Controls examples: `mfa` (require MFA), `block` (deny access), `compliantDevice` (require compliant device)

### Microsoft Graph API

A REST-based API for querying and managing Microsoft 365 / Azure AD resources.

- CA policy endpoint: `GET /identity/conditionalAccess/policies`
- Users: `GET /users`
- Service principals (apps): `GET /servicePrincipals`
- Transitive group membership: `GET /users/{id}/transitiveMemberOf`

This project uses Graph SDK v5 (Kiota-based). Paginated responses are fully collected using `PageIterator`.

### Blazor Interactive Server

UI events are processed on the server and communicated to the browser over SignalR.
Key concepts: component lifecycle (`OnInitializedAsync`), two-way binding with `@bind`, dependency injection via `@inject`, and `@rendermode InteractiveServer`.

---

## Project Structure

```
CAPolicyLab/
├── Program.cs                   # App entry point, DI registration, middleware pipeline
├── Components/
│   ├── App.razor / Routes.razor # Routing root
│   ├── Layout/                  # MainLayout, NavMenu
│   ├── Pages/
│   │   ├── Home.razor           # Dashboard (KPI cards, policy list)
│   │   ├── PolicyFilter.razor   # Filter by user / app
│   │   ├── PolicyVisualizer.razor  # Policy Mermaid flowchart
│   │   ├── ConflictDetector.razor  # Inter-policy conflict detection
│   │   ├── WhatIfSimulator.razor   # Access policy simulation
│   │   └── Setup.razor          # Azure AD credential configuration UI
│   └── Shared/
│       ├── PolicyCard.razor     # Reusable policy card component
│       ├── ConfigGuard.razor    # Blocks access when app is not configured
│       └── MermaidDiagram.razor # Mermaid.js rendering wrapper
├── Services/
│   ├── GraphService.cs          # Graph API calls + 5-minute memory cache
│   ├── PolicyAnalyzerService.cs # Conflict detection, What-If, Mermaid generation (local)
│   ├── AppConfigService.cs      # Read/write appsettings, connection test
│   └── AppRegistrationService.cs
└── Models/
    ├── ConditionalAccessPolicyModel.cs  # Graph SDK type → internal view model
    ├── PolicyConflict.cs
    └── WhatIfResult.cs
```

---

## Features

| Feature | Description |
|---|---|
| **Dashboard** | KPI cards for total / active / report-only policy counts, Grant Control distribution, detected conflicts |
| **Policy Filter** | Filter the policy list by user ID, app ID, or state |
| **Policy Visualizer** | Renders a single policy as a Mermaid flowchart (condition → control flow) |
| **Conflict Detector** | Detects Block vs Allow, MFA vs No-MFA, and duplicate policies ranked by severity |
| **What-If Simulator** | Predicts which policies apply to a user/app combination and the final access outcome |
| **Setup Wizard** | Enter TenantId, ClientId, and ClientSecret from the browser UI and test the connection |

---

## Tech Stack

| Item | Details |
|---|---|
| Runtime | .NET 10, Blazor Interactive Server |
| Graph SDK | `Microsoft.Graph` 5.105.0 (Kiota-based) |
| Auth | `Azure.Identity` 1.13.2 — `ClientSecretCredential` |
| Cache | `IMemoryCache` (5-minute TTL) |
| UI | Bootstrap 5, Bootstrap Icons, Mermaid.js |

---

## Setup (Azure App Registration)

### 1. Register an App

1. [Azure Portal](https://portal.azure.com) → **Entra ID** → **App registrations** → **New registration**
2. Give it a name and register. Copy the **Application (client) ID** and **Directory (tenant) ID**
3. **Certificates & secrets** → create a new client secret. Copy the value immediately (it cannot be retrieved later)

### 2. Grant API Permissions

**API permissions** → **Add a permission** → **Microsoft Graph** → **Application permissions**, add the following, then click **Grant admin consent**:

- `Policy.Read.All`
- `Directory.Read.All`

### 3. Enter Credentials

After running the app, enter the values on the `/setup` page, or write them directly to `appsettings.Development.json`:

```json
{
  "AzureAd": {
    "TenantId": "your-tenant-id",
    "ClientId": "your-client-id",
    "ClientSecret": "your-client-secret"
  }
}
```

> The app runs without configuration. In the unconfigured state, a warning banner is shown on the dashboard and all Graph API calls are blocked.

---

## Running the App

```bash
dotnet restore
dotnet run --project CAPolicyLab/CAPolicyLab.csproj
```

---

## Root Document Mapping

Root: [README.md](../README.md)

| Code | Topic | Detail Doc | CAPolicyLab Application |
|---|---|---|---|
| B2 | Hosting Models and Rendering Modes | [02-hosting-render-modes.md](../docs/blazor/02-hosting-render-modes.md) | Interactive Server mode |
| B3 | Component Structure and Routing | [03-components-routing.md](../docs/blazor/03-components-routing.md) | Pages/Shared separation, App/Routes |
| B4 | State, Events, Data Binding | [04-state-events-binding.md](../docs/blazor/04-state-events-binding.md) | Page component state management |
| B6 | DI, Services, State Management | [06-di-services-state.md](../docs/blazor/06-di-services-state.md) | Services layer DI separation |
| B10 | Coding Style Guide | [10-coding-style.md](../docs/blazor/10-coding-style.md) | Component / service / model responsibility split |
