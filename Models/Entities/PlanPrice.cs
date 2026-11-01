using System.ComponentModel.DataAnnotations;

namespace SWWebAPI.Models.Entities
{
    public class PlanPrice
    {
        [Key]
        public int planpriceid { get; set; }

        public int plan_id { get; set; }
        public decimal price { get; set; }
        public decimal courier_price { get; set; }

        public DateTime EffectiveFrom { get; set; }
        public DateTime EffectiveTo { get; set; }
    }
}
