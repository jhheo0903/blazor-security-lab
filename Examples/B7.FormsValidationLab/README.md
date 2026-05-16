# B7.FormsValidationLab

**문서**: `docs/blazor/07-forms-validation-errors.md`

Blazor의 폼 입력, 데이터 바인딩, 유효성검사를 다루는 예제 프로젝트입니다.

## 문서-코드 매핑

| 문서 섹션 | 구현 파일 | 설명 |
|-----------|-----------|------|
| 기본 폼 입력 | `Components/Pages/BasicFormDemo.razor` | @bind로 상태 갱신 |
| 유효성 검사 | `Components/Pages/ValidationSimpleDemo.razor` | DataAnnotations 검증 |
| EditForm 제출 | `Components/Pages/FormSubmitDemo.razor` | EditForm + DataAnnotationsValidator |
| 폼 모델 | `Models/UserForm.cs` | 검증 규칙 정의 |

## 실행 방법

\`\`\`bash
dotnet run --project Examples/B7.FormsValidationLab/B7.FormsValidationLab.csproj
\`\`\`

## 빌드 방법

\`\`\`bash
dotnet build Examples/B7.FormsValidationLab/B7.FormsValidationLab.csproj
\`\`\`

## 3개 데모 페이지

1. **기본 폼** (`/basic-form`)
   - @bind="변수" 사용
   - 입력 즉시 상태 갱신
   - 간단한 UI 표시

2. **유효성 검사** (`/validation-simple`)
   - UserForm 모델 검증
   - 수동 Validate() 호출
   - 에러 메시지 표시

3. **EditForm 제출** (`/form-submit`)
   - EditForm 래퍼
   - DataAnnotationsValidator
   - ValidationSummary / ValidationMessage
   - OnValidSubmit 콜백
