using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SWWebAPI.Models.Entities
{
    public class swati_images
    {
        [Key]
        public int img_id { get; set; }
       
        public string filename { get; set; } = string.Empty;
        public string source { get; set; }
       
    }
}
