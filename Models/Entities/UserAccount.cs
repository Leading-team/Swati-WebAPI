namespace SWWebAPI.Models.Entities
{
    public class UserAccount
    {
        public int Id { get; set; }
        public required string FullName { get; set; }
        public string? UserName { get; set; }
        public required string Password { get; set; }
    }
}
