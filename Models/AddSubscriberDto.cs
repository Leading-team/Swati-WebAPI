namespace SWWebAPI.Models.Entities
{
    public class AddSubscriberDto
    {
        public string? subscriber_name { get; set; }
        public int plan_id { get; set; }
        public required DateTime issuedate { get; set; }
        public DateTime expirydate { get; set; }
        public required string address1 { get; set; }
        public string? address2 { get; set; }
        public string? address3 { get; set; }
        public string state { get; set; }
        public int pincode { get; set; }
        public required string mobilenumber { get; set; }

    }
    public class AddSubscribtionPlanDto
    {
        public int plan_id { get; set; }
        public required string plan_name { get; set; }
        public required int created_by { get; set; }
        public DateTime created_date { get; set; }
        public int? modified_by { get; set; }
        public DateTime? modified_date { get; set; }
        public bool active { get; set; } = true;

    }
}
