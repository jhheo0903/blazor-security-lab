# B2-WebAssembly

[한국어](README.md) | [English](README.en.md)

**문서**: `docs/blazor/02-hosting-render-modes.md`

Blazor **WebAssembly** 독립 호스팅 모델 예제 프로젝트입니다.

## 호스팅 모델 특징

- **실행 위치**: 브라우저 (WebAssembly)
- **서버 연결**: 초기 파일 다운로드 이후 불필요
- **초기 로딩**: 느림 (.NET 런타임 + 앱 번들 다운로드)
- **오프라인**: 가능 (다운로드 후)

## Program.cs 핵심 설정

```csharp
using B2_WebAssembly.Components;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
await builder.Build().RunAsync();
```

> 서버가 없습니다! `WebApplication.CreateBuilder` 대신 `WebAssemblyHostBuilder` 사용.

## 프로젝트 구조 차이

| 항목 | Blazor Server | Blazor WebAssembly |
|------|--------------|-------------------|
| SDK | `Microsoft.NET.Sdk.Web` | `Microsoft.NET.Sdk.BlazorWebAssembly` |
| 진입점 | `App.razor` (서버 렌더) | `wwwroot/index.html` + `App.razor` |
| 라우터 | `Routes.razor` / `<Routes>` | `App.razor`에 `<Router>` 직접 |
| rendermode | `@rendermode InteractiveServer` | 불필요 (전체가 클라이언트) |
| ReconnectModal | 있음 | 없음 |

## 실행 방법

```bash
dotnet run --project Examples/B2-WebAssembly/B2-WebAssembly.csproj
```

## 테스트 방법

1. 실행 후 브라우저에서 `https://localhost:포트` 열기
2. 브라우저 개발자 도구 → Network → `_framework/` 파일들 확인 (WASM 번들)
3. 첫 로딩이 Server보다 느린 것 확인 (8~30MB 번들 다운로드)
4. Counter 페이지 → 버튼 클릭 → Network에 서버 요청 없음 확인 (브라우저에서 처리)
5. 서버를 종료해도 이미 로딩된 페이지는 동작 확인

## 다른 호스팅 모델과 비교

| 항목 | B2-Server | B2-WebAssembly | B2-WebApp |
|------|-----------|----------------|-----------|
| 실행 위치 | 서버 | 브라우저 | 선택 가능 |
| 서버 연결 | 항상 필요 | 불필요 | 모드별 |
| 초기 로딩 | 빠름 | 느림 (번들 다운로드) | 혼합 |
| 오프라인 | 안 됨 | 가능 | 모드별 |
