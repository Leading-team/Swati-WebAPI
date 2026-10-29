namespace SWWebAPI.Models.Entities
{
    public class ContactEmailDetails
    {

        public int id { get; set; }
        public required string description { get; set; }

        public string? email { get; set; }
        public bool active { get; set; } = true;
    }
}
