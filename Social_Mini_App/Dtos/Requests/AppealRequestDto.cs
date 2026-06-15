using System.ComponentModel.DataAnnotations;

namespace Social_Mini_App.Dtos.Requests
{
    public class AppealRequestDto
    {
        [Required(ErrorMessage = "Vui lòng nhập lý do giải trình kháng nghị!")]
        [MaxLength(500, ErrorMessage = "Lý do tối đa 500 ký tự")]
        public string Reason { get; set; } = string.Empty;
    }
}
