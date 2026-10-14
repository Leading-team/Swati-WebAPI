namespace SWWebAPI.Models
{
    public class CreateOrderRequest
    {
        public decimal Amount { get; set; }
        public string Name { get; set; } = "";
        public string Email { get; set; } = "";
    }
}
