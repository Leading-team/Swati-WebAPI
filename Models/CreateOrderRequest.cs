namespace SWWebAPI.Models
{
    public class CreateOrderRequest
    {
        // Plan details
        public int PlanId { get; set; }
        public string PlanSlug { get; set; } = "";
        public decimal Amount { get; set; }
        public string CountryCode { get; set; } = "";

        // Subscription details
        public string? Edition { get; set; }
        public string? Term { get; set; }
        public List<string>? ChargeIds { get; set; }

        public string? LanguageCode { get; set; }

        // Shipping / Customer details
        public ShippingAddressRequest? ShippingAddress { get; set; }
    }

    public class ShippingAddressRequest
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";

        public string Address { get; set; } = "";
        public string City { get; set; } = "";
        public string State { get; set; } = "";
        public string PostalCode { get; set; } = "";
        public string Country { get; set; } = "";
    }
}
