using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWWebAPI.Models.Entities
{
    public class Country
    {
        private string _currency;
        private string _symbol;

        [Key]
        public int country_id { get; set; }

        [Column("country_code")]
        [StringLength(3)]
        public string? countrycode { get; set; }

        public string? currency
        {
            get => _currency?.TrimEnd();
            set => _currency = value;
        }
        public string? symbol
        {
            get => _symbol?.TrimEnd();
            set => _symbol = value;
        }
        public bool active { get; set; }
    }
}
