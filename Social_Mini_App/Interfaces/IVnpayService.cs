using Microsoft.AspNetCore.Http;
using Social_Mini_App.Models;

namespace Social_Mini_App.Interfaces
{
    public interface IVnpayService
    {
        string CreatePaymentUrl(HttpContext context, Payment payment);
        bool ValidateCallback(IQueryCollection collections);
    }
}
