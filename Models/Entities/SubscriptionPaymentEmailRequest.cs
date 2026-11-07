namespace SWWebAPI.Models.Entities
{
    public class SubscriptionPaymentEmailRequest
    {
        public string SubscriberName { get; set; }
        public string Email { get; set; }
        public string PlanName { get; set; }
        public string Edition { get; set; }
        public string SubscriptionTerm { get; set; }

        public decimal SubscriptionAmount { get; set; }
        public decimal CourierCharges { get; set; }
        public decimal ProcessingFee { get; set; }
        public decimal GstAmount { get; set; }
        public decimal TotalAmount { get; set; }

        public string PaymentId { get; set; }
        public string OrderId { get; set; }

        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
    }
}
