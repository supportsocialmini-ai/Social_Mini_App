using Microsoft.EntityFrameworkCore;
using MiniSocialNetwork.Data;
using Social_Mini_App.Dtos;
using Social_Mini_App.Interfaces;
using Social_Mini_App.Models;

namespace Social_Mini_App.Services
{
    public class ReportService : IReportService
    {
        private readonly DataContext _context;

        public ReportService(DataContext context)
        {
            _context = context;
        }

        public async Task<bool> CreateReportAsync(Guid reporterId, ReportCreateRequest request)
        {
            var report = new Report
            {
                ReporterId = reporterId,
                TargetId = request.TargetId,
                TargetType = request.TargetType,
                Reason = request.Reason,
                Description = request.Description,
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.Reports.Add(report);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<ReportResponse>> GetAllReportsAsync()
        {
            var reports = await _context.Reports
                .Include(r => r.Reporter)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            var result = new List<ReportResponse>();

            foreach (var r in reports)
            {
                var response = new ReportResponse
                {
                    ReportId = r.ReportId,
                    ReporterId = r.ReporterId,
                    ReporterName = r.Reporter?.FullName ?? r.Reporter?.Username ?? "Unknown",
                    TargetId = r.TargetId,
                    TargetType = r.TargetType,
                    Reason = r.Reason,
                    Description = r.Description,
                    Status = r.Status,
                    CreatedAt = r.CreatedAt
                };

                // Lấy thêm nội dung bài viết nếu là báo cáo Post
                if (r.TargetType == "Post")
                {
                    var post = await _context.Posts.FindAsync(r.TargetId);
                    response.TargetContent = post?.PostContent ?? "[Nội dung bài viết đã bị xóa]";
                }
                else if (r.TargetType == "User")
                {
                    var user = await _context.Users.FindAsync(r.TargetId);
                    response.TargetContent = $"Người dùng: {user?.FullName ?? user?.Username}";
                }

                result.Add(response);
            }

            return result;
        }

        public async Task<bool> ResolveReportAsync(Guid adminId, Guid reportId, string action)
        {
            var report = await _context.Reports.FindAsync(reportId);
            if (report == null) return false;

            report.Status = action; // "Resolved" or "Dismissed"
            report.ResolvedAt = DateTime.Now;
            report.ResolvedById = adminId;

            return await _context.SaveChangesAsync() > 0;
        }
    }
}
