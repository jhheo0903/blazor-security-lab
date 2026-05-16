using System.ComponentModel.DataAnnotations;

namespace B7.FormsValidationLab.Models;

public class UserForm
{
    [Required(ErrorMessage = "이름은 필수입니다")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "이름은 2자 이상 50자 이하여야 합니다")]
    public string Name { get; set; } = "";

    [Required(ErrorMessage = "이메일은 필수입니다")]
    [EmailAddress(ErrorMessage = "유효한 이메일 형식이 아닙니다")]
    public string Email { get; set; } = "";

    [Range(18, 120, ErrorMessage = "나이는 18세 이상 120세 이하여야 합니다")]
    public int Age { get; set; } = 20;
}
