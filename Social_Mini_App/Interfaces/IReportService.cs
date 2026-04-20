using Social_Mini_App.Dtos;

namespace Social_Mini_App.Interfaces
{
    public interface IReportService
    {
        Task<bool> CreateReportAsync(Guid reporterId, ReportCreateRequest request);
        Task<List<ReportResponse>> GetAllReportsAsync();
        Task<bool> ResolveReportAsync(Guid adminId, Guid reportId, string action); // action: "Resolved" or "Dismissed"
    }
}
