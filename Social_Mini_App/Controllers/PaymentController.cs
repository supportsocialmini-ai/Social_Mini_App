using System;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MiniSocialNetwork.Data;
using Social_Mini_App.Interfaces;
using Social_Mini_App.Models;
using Social_Mini_App.Services;

namespace Social_Mini_App.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class PaymentController : ControllerBase
    {
        private readonly IVnpayService _vnpayService;
        private readonly DataContext _context;
        private readonly IConfiguration _configuration;

        public PaymentController(IVnpayService vnpayService, DataContext context, IConfiguration configuration)
        {
            _vnpayService = vnpayService;
            _context = context;
            _configuration = configuration;
        }

        [HttpGet("packages")]
        public async Task<IActionResult> GetPackages()
        {
            var packages = await _context.SubscriptionPackages
                .Where(p => p.IsActive)
                .OrderBy(p => p.Price)
                .ToListAsync();
            return Ok(packages);
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreatePayment([FromBody] CreatePaymentRequest request)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr)) return Unauthorized();

            var userId = Guid.Parse(userIdStr);
            
            // 0. Lấy thông tin gói từ Database dựa trên ID gửi lên
            var targetPackage = await _context.SubscriptionPackages
                .FirstOrDefaultAsync(p => p.Id == request.PackageId && p.IsActive);

            if (targetPackage == null)
            {
                return BadRequest(new { message = "Gói dịch vụ không khả dụng hoặc đã bị xóa." });
            }

            decimal finalAmount = targetPackage.Price;
            if (request.CustomDays.HasValue && request.CustomDays.Value > 0)
            {
                double pricePerDay = (double)targetPackage.Price / (targetPackage.DurationDays > 0 ? targetPackage.DurationDays : 1);
                finalAmount = (decimal)Math.Round(pricePerDay * request.CustomDays.Value);
            }

            // 1. Tạo bản ghi giao dịch nháp
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PackageId = targetPackage.Id, 
                PostId = request.PostId, // Gán ID bài viết nếu có
                Amount = finalAmount, 
                OrderId = DateTime.Now.Ticks.ToString(),
                OrderInfo = request.PostId != null 
                    ? (request.CustomDays.HasValue && request.CustomDays.Value > 0
                        ? $"Quang-cao-bai-viet-{request.PostId}-goi-{targetPackage.Name?.Trim()}-customdays-{request.CustomDays.Value}".Replace(" ", "-")
                        : $"Quang-cao-bai-viet-{request.PostId}-goi-{targetPackage.Name?.Trim()}".Replace(" ", "-"))
                    : $"Thanh-toan-goi-{targetPackage.Name?.Trim()}-cho-user-{userId}".Replace(" ", "-"),
                Status = "Pending",
                CreatedAt = DateTime.Now
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // 2. Tạo URL thanh toán VNPay
            var paymentUrl = _vnpayService.CreatePaymentUrl(HttpContext, payment);

            Console.WriteLine("DEBUG - VNPay URL: " + paymentUrl);

            return Ok(new { paymentUrl });
        }

        [HttpGet("vnpay-return")]
        [AllowAnonymous]
        public async Task<IActionResult> VnpayReturn()
        {
            var collections = Request.Query;
            var isValid = _vnpayService.ValidateCallback(collections);

            if (!isValid)
            {
                return Redirect($"{GetFrontendUrl()}/payment-result?status=error&message=InvalidSignature");
            }

            var vnp_ResponseCode = collections["vnp_ResponseCode"].ToString();
            var vnp_TxnRef = collections["vnp_TxnRef"].ToString();
            var vnp_TransactionNo = collections["vnp_TransactionNo"].ToString();

            var payment = await _context.Payments
                .Include(p => p.Package)
                .FirstOrDefaultAsync(p => p.OrderId == vnp_TxnRef);

            if (payment == null) return NotFound();

            if (vnp_ResponseCode == "00")
            {
                // Thanh toán thành công
                payment.Status = "Success";
                payment.VnpayTranId = vnp_TransactionNo;

                // Lấy thời gian gia hạn từ gói
                int durationDays = payment.Package?.DurationDays ?? 30;
                string type = "premium";
                int days = durationDays;

                if (payment.PostId != null)
                {
                    type = "ads";
                    // Trường hợp mua quảng cáo cho bài viết cụ thể
                    var post = await _context.Posts.FindAsync(payment.PostId.Value);
                    if (post != null)
                    {
                        post.IsSponsored = true;
                        
                        // Parse custom duration days from OrderInfo if present
                        int actualDuration = durationDays;
                        if (payment.OrderInfo != null && payment.OrderInfo.Contains("-customdays-"))
                        {
                            var parts = payment.OrderInfo.Split("-customdays-");
                            if (parts.Length > 1 && int.TryParse(parts[1], out int parsedDays))
                            {
                                actualDuration = parsedDays;
                            }
                        }
                        days = actualDuration;

                        post.SponsorEndDate = (post.SponsorEndDate.HasValue && post.SponsorEndDate > DateTime.UtcNow) 
                            ? post.SponsorEndDate.Value.AddDays(actualDuration) 
                            : DateTime.UtcNow.AddDays(actualDuration);
                        post.UpdatedAt = DateTime.Now;
                        _context.Posts.Update(post);
                    }
                }
                else
                {
                    // Trường hợp mua gói nâng cấp tài khoản thông thường (Chat Random...)
                    var subscription = await _context.Subscriptions
                        .Include(s => s.Package)
                        .FirstOrDefaultAsync(s => s.UserId == payment.UserId);

                    var packageName = payment.Package?.Name ?? "Premium";

                    if (subscription == null)
                    {
                        subscription = new Subscription
                        {
                            Id = Guid.NewGuid(),
                            UserId = payment.UserId,
                            Tier = packageName,
                            PackageId = payment.PackageId,
                            IsActive = true,
                            StartDate = DateTime.Now,
                            EndDate = DateTime.Now.AddDays(durationDays),
                            CreatedAt = DateTime.Now
                        };
                        _context.Subscriptions.Add(subscription);
                    }
                    else
                    {
                        subscription.Tier = packageName;
                        subscription.PackageId = payment.PackageId;
                        subscription.IsActive = true;
                        subscription.StartDate = DateTime.Now;
                        subscription.EndDate = (subscription.EndDate > DateTime.Now ? subscription.EndDate : DateTime.Now).Value.AddDays(durationDays);
                        subscription.UpdatedAt = DateTime.Now;
                    }
                }

                await _context.SaveChangesAsync();
                return Redirect($"{GetFrontendUrl()}/payment-result?status=success&type={type}&days={days}");
            }
            else
            {
                // Thanh toán thất bại
                payment.Status = "Failed";
                await _context.SaveChangesAsync();
                return Redirect($"{GetFrontendUrl()}/payment-result?status=fail&code={vnp_ResponseCode}");
            }
        }

        private string GetFrontendUrl()
        {
            return _configuration["AppSettings:FrontendUrl"] ?? "http://localhost:3000";
        }
    }

    public class CreatePaymentRequest
    {
        public Guid PackageId { get; set; } // Nhận ID gói từ Frontend
        public Guid? PostId { get; set; } // Nhận ID bài viết từ Frontend nếu mua gói Ads
        public int? CustomDays { get; set; } // Số ngày tự chọn nếu mua gói Ads custom
    }
}
