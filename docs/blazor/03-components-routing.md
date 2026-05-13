# B3. 컴포넌트 구조와 라우팅

## 권장 폴더 전략

- Pages: 라우팅되는 화면 컴포넌트
- Shared: 재사용 UI 컴포넌트
- Layout: 앱 공통 프레임

## 라우팅 기본

```razor
@page "/policies"
<h3>Policies</h3>
```

- @page는 URL과 컴포넌트를 매핑합니다.
- 라우팅은 Routes/App 구성의 영향을 받습니다.

## 컴포넌트 책임 분리

- 페이지 컴포넌트: 화면 흐름, 사용자 액션 오케스트레이션
- 공유 컴포넌트: 반복 UI/표현 규칙
- 서비스: 계산/외부 연동/데이터 변환

## 파라미터 전달 패턴

```razor
<PolicyCard Policy="selected" OnApply="HandleApply" />
```

```razor
[Parameter] public PolicyModel? Policy { get; set; }
[Parameter] public EventCallback OnApply { get; set; }
```

## 체크리스트

- 재사용 가능한 부분이 Shared로 분리되어 있는가?
- 페이지가 서비스 구현 상세를 과도하게 알고 있지 않은가?
- 라우트 이름이 도메인 의미를 잘 드러내는가?
