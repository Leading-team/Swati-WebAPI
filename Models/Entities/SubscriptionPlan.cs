using System.ComponentModel.DataAnnotations;

namespace SWWebAPI.Models.Entities
{
    public class subscription_plan
    {
        [Key]
        public int plan_id { get; set; }
        public required string plan_name { get; set; }
        public required int created_by { get; set; }
        public DateTime created_date { get; set; }
        public int? modified_by { get; set; }

        public DateTime? modified_date { get; set; }

        public bool active { get; set; } = true;

    }
}
