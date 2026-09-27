using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWWebAPI.Data;
using SWWebAPI.Models;
using SWWebAPI.Models.Entities;

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
        [HttpGet("GetAllSubscriptionPlans")]
        public IActionResult GetAllSubscriptionPlans()
        {
            return Ok(dbContext.subscription_plan.ToList());

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
