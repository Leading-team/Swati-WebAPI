using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWWebAPI.Models.Entities
{
    public class menu_list
    {
        [Key]
        public int menu_id { get; set; }
        [Required]
        [MaxLength(500)]
        public string menu_name { get; set; } = string.Empty;
        [Column(TypeName = "char(2)")]
        [MaxLength(2)]
        public string language { get; set; } = "te";

        public string route_name { get; set; }
        public DateTime created_date { get; set; } = DateTime.UtcNow;
        public bool active { get; set; } = true;
    }
}
