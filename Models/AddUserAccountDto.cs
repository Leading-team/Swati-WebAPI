namespace SWWebAPI.Models
{
    public class AddUserAccountDto
    {
        public required string UserName { get; set; }
        public required string Password { get; set; }
    }
}
