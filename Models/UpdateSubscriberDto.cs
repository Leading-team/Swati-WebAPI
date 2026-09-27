namespace SWWebAPI.Models
{
    public class UpdateSubscriberDto
    {
        public int subscriber_id { get; set; }
        public string? subscriber_name { get; set; }
        public int plan_id { get; set; }
        public DateTime expirydate { get; set; }
        public required string address1 { get; set; }
        public string? address2 { get; set; }
        public string? address3 { get; set; }
        public string state { get; set; }
        public int pincode { get; set; }
        public required string mobilenumber { get; set; }
    }
}
