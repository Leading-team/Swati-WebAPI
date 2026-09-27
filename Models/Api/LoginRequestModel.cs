using System.ComponentModel.DataAnnotations;

namespace SWWebAPI.Models.Api
{
    public class LoginRequestModel
    {
        [Required]
        public string? UserName { get; set; }
        [Required]
        public string? Password { get; set; }
    }
}
