using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWWebAPI.Data;
using SWWebAPI.Models.Entities;

namespace SWWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class UserAccountController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        public UserAccountController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        [HttpGet]
        public async Task<List<UserAccount>> GetAllUserAccounts()
        {
            return await dbContext.UserAccounts.ToListAsync();

        }
    }
}
