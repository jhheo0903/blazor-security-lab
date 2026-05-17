# B9. Performance Optimization

[한국어](09-performance.md) | English

## Measure First

Optimize based on measurement, not intuition.
Use browser DevTools Performance, .NET diagnostic tools, and Blazor render logs.

- Abnormal render frequency
- Number of network calls
- Cost of rendering large lists

## 1. Preventing Unnecessary Re-renders

### ShouldRender Override

Skip rendering when state hasn't actually changed.

```razor
@code {
	private int _lastCount = -1;
	private int count;

	protected override bool ShouldRender()
	{
		if (_lastCount == count)
			return false;

		_lastCount = count;
		return true;
	}
}
```

### @key Directive

Track list items during rendering to prevent unnecessary DOM recreation.

```razor
@foreach (var item in items)
{
	<PolicyCard @key="item.Id" Policy="item" />
}
```

Without `@key`, changing a list may re-render all items.

## 2. Virtualization for Large Lists

When rendering thousands of items, only render what's visible in the viewport.

```razor
<div style="height:500px; overflow-y:auto">
	<Virtualize Items="policies" Context="policy" OverscanCount="3">
		<PolicyCard Policy="policy" />
	</Virtualize>
</div>
```

### Server-Side Data Paging Integration

```razor
<Virtualize Context="item"
			ItemsProvider="LoadItemsAsync"
			ItemSize="50">
	<ItemContent>
		<PolicyRow Item="item" />
	</ItemContent>
	<Placeholder>
		<div>Loading...</div>
	</Placeholder>
</Virtualize>

@code {
	private async ValueTask<ItemsProviderResult<PolicyModel>> LoadItemsAsync(
		ItemsProviderRequest request)
	{
		var result = await PolicyService.GetPagedAsync(
			request.StartIndex, request.Count, request.CancellationToken);

		return new ItemsProviderResult<PolicyModel>(result.Items, result.TotalCount);
	}
}
```

## 3. Streaming Rendering

In .NET 8+, split long-running operations on Static SSR pages using streaming.

```razor
@page "/reports"
@attribute [StreamRendering]

@if (report is null)
{
	<p>Generating report...</p>
}
else
{
	<ReportView Report="report" />
}

@code {
	private ReportModel? report;

	protected override async Task OnInitializedAsync()
	{
		report = await ReportService.GenerateAsync();
	}
}
```

## 4. Parallel Async Loading

```razor
@code {
	private PolicyModel? policy;
	private UserInfo? user;

	protected override async Task OnInitializedAsync()
	{
		var policyTask = PolicyService.GetAsync(id);
		var userTask = UserService.GetCurrentAsync();

		await Task.WhenAll(policyTask, userTask);

		policy = policyTask.Result;
		user = userTask.Result;
	}
}
```

## 5. Minimizing State Changes

```razor
@code {
	// Bad: multiple state changes → potential multiple renders
	private async Task BadUpdate()
	{
		isLoading = true;
		items = await LoadAsync();
		isLoading = false;
	}

	// Good: complete the state in one go before returning
	private async Task GoodUpdate()
	{
		var loaded = await LoadAsync();
		items = loaded;
		isLoading = false;
		// one automatic StateHasChanged call after event handler exits
	}
}
```

## Rendering Checklist

- Does the same input create new objects every time, triggering unnecessary re-renders?
- Is `@key` used on list items?
- Is `<Virtualize>` used for large lists?
- Is only the necessary data fetched first during initial load?

## Practical Tips

- Analyze performance issues at the user scenario level, not just the page level.
- Apply optimizations one at a time and verify the effect incrementally.
- Always confirm improvements with before/after measurements.

## References

- https://learn.microsoft.com/aspnet/core/blazor/performance
- https://learn.microsoft.com/aspnet/core/blazor/components/virtualization
