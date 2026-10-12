using System.ComponentModel.DataAnnotations;

namespace SWWebAPI.Models.Entities
{
    public class Language
    {
        [Key]
        public int language_id { get; set; }

        public string code { get; set; }

        public string description { get; set; }
        public bool active { get; set; }
    }
}
