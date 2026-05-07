using System.ComponentModel.DataAnnotations;

namespace Social_Mini_App.Dtos.Requests
{
    public class ShareRequest
    {
        [MaxLength(500)]
        public string? Content { get; set; } // Nội dung người dùng viết thêm khi share (tùy chọn)
        
        public Guid? GroupId { get; set; } // Nếu share vào nhóm
    }
}
