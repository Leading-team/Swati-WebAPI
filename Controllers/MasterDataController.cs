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
            try
            {
                var languages = dbContext.language
                                         .Where(r => r.active == true)
                                         .ToList();

                return Ok(languages);
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                // Log database connection or SQL errors
                return StatusCode(503, new
                {
                    success = false,
                    message = "Unable to connect to the database. Please try again later."
                });
            }
            catch (Exception ex)
            {
                // Log unexpected errors
                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred while retrieving languages."
                });
            }
        }
        [HttpGet("GetAllCountries")]
        public IActionResult GetAllCountries()
        {
            try
            {
                var languages = dbContext.country.Where(r => r.active == true).ToList();

                return Ok(languages);
            }
            catch (Microsoft.Data.SqlClient.SqlException ex)
            {
                // Log database connection or SQL errors
                return StatusCode(503, new
                {
                    success = false,
                    message = "Unable to connect to the database. Please try again later."
                });
            }
            catch (Exception ex)
            {
                // Log unexpected errors
                return StatusCode(500, new
                {
                    success = false,
                    message = "An unexpected error occurred while retrieving countries."
                });
            }
        }
    }
}
