namespace SWWebAPI.Models.Entities
{
    public class ContactUsRequest
    {
        public string FullName { get; set; } = "";
        public string Email { get; set; } = "";
        public string? Phone { get; set; }
        public string? EnquiryType { get; set; }
        public string Message { get; set; } = "";
        public string? Language { get; set; }
    }
}
