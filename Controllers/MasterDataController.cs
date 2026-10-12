using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWWebAPI.Data;
using SWWebAPI.Models.Entities;

namespace SWWebAPI.Controllers
{
    [Route("api/master")]
    [ApiController]
    public class MasterDataController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;

        public MasterDataController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        [HttpGet("GetAllLanguages")]
        public IActionResult GetAllLanguages()
        {
            return Ok(dbContext.language.Where(r => r.active == true).ToList());
        }
        [HttpGet("GetAllCountries")]
        public IActionResult GetAllCountries()
        {
            return Ok(dbContext.country.Where(r => r.active == true).ToList());
        }
    }
}
