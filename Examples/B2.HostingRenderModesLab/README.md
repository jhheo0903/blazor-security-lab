# B2 Hosting Render Modes Lab

이 프로젝트는 루트 기준서의 B2(호스팅 모델과 렌더링 모드)를 빠르게 체험하기 위한 최소 예제입니다.

## 루트 문서 연계

루트 기준 문서: [README.md](../../README.md)

| 루트 코드 | 루트 주제 | 상세 문서 | B2 예제 적용 | 근거 파일 |
| --- | --- | --- | --- | --- |
| B2 | 호스팅 모델과 렌더링 모드 | [02-hosting-render-modes.md](../../docs/blazor/02-hosting-render-modes.md) | Static SSR + Interactive Server 비교 | [Program.cs](Program.cs), [Components/Pages/Counter.razor](Components/Pages/Counter.razor), [Components/Pages/PrerenderProbe.razor](Components/Pages/PrerenderProbe.razor) |
| B5 | 컴포넌트 생명주기 | [05-lifecycle.md](../../docs/blazor/05-lifecycle.md) | prerender 이후 interactive 상태 전환 관찰 | [Components/Pages/PrerenderProbe.razor](Components/Pages/PrerenderProbe.razor) |

## 예제 구성

- Home: 예제 목적과 페이지 안내
- Counter: `@rendermode InteractiveServer` 기반 이벤트 처리
- Weather: streaming SSR 렌더링 예시
- Prerender Probe: `RendererInfo.IsInteractive`로 interactive 전환 확인

## 테스트 방법

1. 아래 명령으로 프로젝트를 실행합니다.
2. Home에서 안내를 확인한 뒤 Counter, Weather, Prerender Probe 순서로 이동합니다.
3. Counter/Prerender Probe에서 버튼 클릭이 즉시 반영되는지 확인합니다.
4. Prerender Probe 첫 진입 시 연결 메시지가 잠깐 보였다가 interactive 상태로 전환되는지 확인합니다.

```bash
dotnet restore
dotnet run --project Examples/B2.HostingRenderModesLab/B2.HostingRenderModesLab.csproj
```

빌드만 확인하려면 아래 명령을 사용합니다.

```bash
dotnet build Examples/B2.HostingRenderModesLab/B2.HostingRenderModesLab.csproj
```
