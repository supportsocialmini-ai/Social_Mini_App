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

            // 1. Tạo bản ghi giao dịch nháp
            var payment = new Payment
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                PackageId = targetPackage.Id, 
                Amount = targetPackage.Price, 
                OrderId = DateTime.Now.Ticks.ToString(),
                OrderInfo = $"Thanh-toan-goi-{targetPackage.Name?.Trim()}-cho-user-{userId}".Replace(" ", "-"),
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

                // Cập nhật Subscription cho User
                var subscription = await _context.Subscriptions
                    .Include(s => s.Package)
                    .FirstOrDefaultAsync(s => s.UserId == payment.UserId);

                // Lấy tên gói để làm Tier (hoặc anh có thể tùy biến logic này)
                var packageName = payment.Package?.Name ?? "Premium";

                // Lấy thời gian gia hạn từ gói
                int durationDays = payment.Package?.DurationDays ?? 30;

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

                await _context.SaveChangesAsync();
                return Redirect($"{GetFrontendUrl()}/payment-result?status=success");
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
    }
}
