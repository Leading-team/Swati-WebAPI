using System.ComponentModel.DataAnnotations;

namespace SWWebAPI.Models.Entities
{
    public class SubscriptionPayment
    {
        [Key]
        public int subscription_payment_id { get; set; }

        public string RazorpayOrderId { get; set; }

        public string CurrencyCode { get; set; }
        public decimal Amount { get; set; }
        public string paymentstatus { get; set; }

    }
}
