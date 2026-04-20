namespace Social_Mini_App.Dtos
{
    public class ReportCreateRequest
    {
        public Guid TargetId { get; set; }
        public string TargetType { get; set; } = "Post"; // "Post" or "User"
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    public class ReportResponse
    {
        public Guid ReportId { get; set; }
        public Guid ReporterId { get; set; }
        public string ReporterName { get; set; } = string.Empty;
        public Guid TargetId { get; set; }
        public string TargetType { get; set; } = string.Empty;
        public string Reason { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        
        // Thông tin thêm cho Admin dễ xem
        public string? TargetContent { get; set; } // Ví dụ nội dung bài viết bị báo cáo
    }
}
