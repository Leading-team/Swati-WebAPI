namespace SWWebAPI.Models
{
    public class AddSubscriptionPlanDto
    {
        public required string plan_name { get; set; }
        public required int created_by { get; set; }
        public DateTime created_date { get; set; } = DateTime.Now;
        public int? modified_by { get; set; }

        public DateTime? modified_date { get; set; }

        public bool active { get; set; } = true;

    }
}
