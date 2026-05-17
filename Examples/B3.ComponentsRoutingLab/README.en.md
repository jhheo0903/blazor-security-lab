# B3 Components Routing Lab

[한국어](README.md) | English

A minimal example project for quickly experiencing B3 (Component Structure and Routing) from the root study guide.

## Root Document Mapping

Root reference: [README.md](../../README.md)

| Code | Topic | Detail Doc | B3 Application | Source File |
| --- | --- | --- | --- | --- |
| B3 | Component Structure and Routing | [03-components-routing.md](../../docs/blazor/03-components-routing.md) | `@page`, route parameters, multiple routes, NavLink setup | [Components/Routes.razor](Components/Routes.razor), [Components/Layout/NavMenu.razor](Components/Layout/NavMenu.razor), [Components/Pages/RouteParameterDemo.razor](Components/Pages/RouteParameterDemo.razor), [Components/Pages/MultiRouteDemo.razor](Components/Pages/MultiRouteDemo.razor) |
| B4 | State, Events, Data Binding | [04-state-events-binding.md](../../docs/blazor/04-state-events-binding.md) | Shared component parameters and EventCallback interaction | [Components/Pages/ComponentCompositionDemo.razor](Components/Pages/ComponentCompositionDemo.razor), [Components/Shared/PolicyCard.razor](Components/Shared/PolicyCard.razor) |

## Demo Pages

- **Home**: Purpose and page overview
- **Route Parameter**: Route parameter binding via `/policy/{id:int}`
- **Multi Route**: Multiple and optional routes (`/policies`, `/policies/all`, `/items/{page:int?}`)
- **Composition**: Shared component parameters and EventCallback handling

## Testing

1. Run the project with the command below.
2. Review the overview on Home, then navigate to Route Parameter, Multi Route, and Composition in order.
3. On Route Parameter, change the URL to `policy/101` and `policy/999` — confirm the bound ID displays correctly.
4. On Multi Route, confirm `/policies`, `/policies/all`, `/items`, and `/items/3` all render correctly.
5. On Composition, click the Apply button on a card — confirm the selected policy message updates.

```bash
dotnet restore
dotnet run --project Examples/B3.ComponentsRoutingLab/B3.ComponentsRoutingLab.csproj
```

Build only:

```bash
dotnet build Examples/B3.ComponentsRoutingLab/B3.ComponentsRoutingLab.csproj
```
