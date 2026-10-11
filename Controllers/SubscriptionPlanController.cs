using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SWWebAPI.Data;
using SWWebAPI.Models;
using SWWebAPI.Models.Entities;
using System.Data;
using System.Text;

namespace SWWebAPI.Controllers
{
    [Route("api/subscriptionplan")]
    [ApiController]
    public class SubscriptionPlanController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        public SubscriptionPlanController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        //[HttpGet("GetAllSubscriptionPlans")]
        //public IActionResult GetAllSubscriptionPlans()
        //{
        //    return Ok(dbContext.subscription_plan.ToList());

        //}
        [HttpGet("GetAllSubscriptionPlans")]
        public async Task<IActionResult> GetAllSubscriptionPlans()
        //[FromQuery] int countryId,
        //[FromQuery] int? languageId = null)
        {
            var json = new StringBuilder();

            var conn = dbContext.Database.GetDbConnection();
            var wasClosed = conn.State == ConnectionState.Closed;
            if (wasClosed) await conn.OpenAsync();

            try
            {
                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "dbo.GetAllSubscriptionPlans";
                cmd.CommandType = CommandType.StoredProcedure;

                //var pCountry = cmd.CreateParameter();
                //pCountry.ParameterName = "@CountryId";
                //pCountry.Value = countryId;
                //cmd.Parameters.Add(pCountry);

                //var pLang = cmd.CreateParameter();
                //pLang.ParameterName = "@LanguageId";
                //pLang.Value = (object?)languageId ?? DBNull.Value;
                //cmd.Parameters.Add(pLang);

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    json.Append(reader.GetString(0));   // FOR JSON comes back in chunks of ~2 KB
                }
            }
            finally
            {
                if (wasClosed) await conn.CloseAsync();
            }

            // No rows come back when nothing matches, so return an empty array
            var result = json.Length == 0 ? "[]" : json.ToString();
            return Content(result, "application/json");
        }

            [HttpPost("AddSubscriptionPlan")]
        public IActionResult AddSubscriptionPlan(AddSubscriptionPlanDto addSubscriptionPlanDto)
        {
            var subscriptionEntity = new subscription_plan()
            {
                plan_name = addSubscriptionPlanDto.plan_name,
                created_by = addSubscriptionPlanDto.created_by,

            };
            dbContext.subscription_plan.Add(subscriptionEntity);
            dbContext.SaveChanges();
            return Ok(subscriptionEntity);
        }


    }
}
