# B9. 성능 최적화 포인트

## 먼저 측정

최적화는 체감이 아니라 계측 기준으로 수행합니다.
브라우저 DevTools Performance, .NET 진단 도구, Blazor 렌더 로그를 활용합니다.

- 렌더링 빈도 이상 여부
- 네트워크 호출 횟수
- 큰 목록 렌더 비용

## 1. 불필요한 렌더링 방지

### ShouldRender override

상태 변경이 실제로 없을 때 렌더링을 건너뜁니다.

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

### @key 지시문

목록 렌더링 시 항목을 추적해 불필요한 DOM 재생성을 방지합니다.

```razor
@foreach (var item in items)
{
	<PolicyCard @key="item.Id" Policy="item" />
}
```

`@key`가 없으면 목록 변경 시 모든 항목을 재렌더링할 수 있습니다.

## 2. 큰 목록 가상화 (Virtualize)

수천 개의 항목을 렌더링할 때 뷰포트에 보이는 항목만 렌더링합니다.

```razor
<div style="height:500px; overflow-y:auto">
	<Virtualize Items="policies" Context="policy" OverscanCount="3">
		<PolicyCard Policy="policy" />
	</Virtualize>
</div>
```

### 서버 측 데이터 페이징 연동

```razor
<Virtualize Context="item"
			ItemsProvider="LoadItemsAsync"
			ItemSize="50">
	<ItemContent>
		<PolicyRow Item="item" />
	</ItemContent>
	<Placeholder>
		<div>로딩 중...</div>
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

## 3. 스트리밍 렌더링 (Streaming Rendering)

.NET 8+에서 Static SSR 페이지의 긴 작업을 스트리밍으로 분할합니다.

```razor
@page "/reports"
@attribute [StreamRendering]

@if (report is null)
{
	<p>보고서를 생성하는 중입니다...</p>
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

## 4. 병렬 비동기 로드

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

## 5. 상태 변경 최소화

```razor
@code {
	// 나쁜 예: 여러 번 상태 변경 -> 여러 번 렌더링 가능
	private async Task BadUpdate()
	{
		isLoading = true;
		items = await LoadAsync();
		isLoading = false;
	}

	// 좋은 예: 한 번에 상태 완성 후 종료
	private async Task GoodUpdate()
	{
		var loaded = await LoadAsync();
		items = loaded;
		isLoading = false;
		// 이벤트 핸들러 종료 후 1회 StateHasChanged 자동 호출
	}
}
```

## 렌더링 관점 체크리스트

- 같은 입력에서 매번 새 객체를 생성해 불필요 렌더를 유발하는가?
- 목록에 `@key`를 사용하는가?
- 큰 목록에 `<Virtualize>`를 사용하는가?
- 초기 로딩에서 꼭 필요한 데이터만 먼저 가져오는가?

## 실무 팁

- 성능 이슈는 페이지 단위가 아니라 사용자 시나리오 단위로 분석합니다.
- 한 번에 여러 최적화를 적용하지 말고 단계적으로 검증합니다.
- 최적화 전후 수치로 효과를 확인합니다.

## 참고

- https://learn.microsoft.com/aspnet/core/blazor/performance
- https://learn.microsoft.com/aspnet/core/blazor/components/virtualization
