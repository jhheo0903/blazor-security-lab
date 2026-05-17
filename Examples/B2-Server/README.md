# B2-Server

[한국어](README.md) | [English](README.en.md)

**문서**: `docs/blazor/02-hosting-render-modes.md`

Blazor **Interactive Server** 호스팅 모델 예제 프로젝트입니다.

## 호스팅 모델 특징

- **실행 위치**: 서버
- **서버 연결**: SignalR Circuit 유지 필요
- **초기 로딩**: 빠름 (서버에서 렌더)
- **오프라인**: 지원 안 됨

## Program.cs 핵심 설정

```csharp
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();  // Server 전용

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();
```

## 실행 방법

```bash
dotnet run --project Examples/B2-Server/B2-Server.csproj
```

## 테스트 방법

1. 실행 후 브라우저에서 `https://localhost:포트` 열기
2. Counter 페이지에서 버튼 클릭 → 카운트 변경 확인 (SignalR로 서버 처리)
3. 브라우저 개발자 도구 Network 탭 → WebSocket 연결 확인 (SignalR)
4. 서버 종료 시 화면에 "재연결 중..." 메시지 표시됨

## 다른 호스팅 모델과 비교

| 항목 | B2-Server | B2-WebAssembly | B2-WebApp |
|------|-----------|----------------|-----------|
| 실행 위치 | 서버 | 브라우저 | 선택 가능 |
| 서버 연결 | 항상 필요 | 불필요 | 모드별 |
| 초기 로딩 | 빠름 | 느림 (번들 다운로드) | 혼합 |
| 오프라인 | 안 됨 | 가능 | 모드별 |
