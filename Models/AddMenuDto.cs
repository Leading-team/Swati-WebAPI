using System.ComponentModel.DataAnnotations;

namespace SWWebAPI.Models
{
    public class AddMenuDto
    {
        [Required]
        [MaxLength(500)]
        public string menu_name { get; set; } = string.Empty;
        public string route_name { get; set; } = string.Empty;
        public string language { get; set; } = "te";

    }
}
