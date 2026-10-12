using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWWebAPI.Models.Entities
{
    public class Country
    {

        [Key]
        public int country_id { get; set; }

        public string? countrycode { get; set; }

        [Column("currencyname")]
        [StringLength(100)]
        public string? currency { get; set; }
        public bool active { get; set; }
    }
}
