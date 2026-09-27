using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWWebAPI.Data;
using SWWebAPI.Models;
using SWWebAPI.Models.Entities;

namespace SWWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SubscriberController : ControllerBase
    {
        private readonly ApplicationDbContext dbContext;
        public SubscriberController(ApplicationDbContext dbContext)
        {
            this.dbContext = dbContext;
        }
        [HttpGet]
        public IActionResult GetAllSubscribers()
        {
            return Ok(dbContext.subscribers.ToList());

        }
        [HttpPost]
        public IActionResult AddSubscriber(AddSubscriberDto addSubscriberDto)
        {
            var subscriberEntity = new subscribers()
            {
                subscriber_name = addSubscriberDto.subscriber_name,
                plan_id = addSubscriberDto.plan_id,
                issuedate = addSubscriberDto.issuedate,
                expirydate = addSubscriberDto.expirydate,
                address1 = addSubscriberDto.address1,
                address2 = addSubscriberDto?.address2,
                address3 = addSubscriberDto?.address3,
                state = addSubscriberDto.state,
                pincode = addSubscriberDto.pincode,
                mobilenumber = addSubscriberDto.mobilenumber

            };
            dbContext.subscribers.Add(subscriberEntity);
            dbContext.SaveChanges();
            return Ok(subscriberEntity);
        }
        [HttpPut]
        public IActionResult UpdateSubscriber(int subscriber_id, UpdateSubscriberDto updateSubscriberDto)
        {
            var subscriber = dbContext.subscribers.Find(subscriber_id);
            if (subscriber is null)
            {
                return NotFound();
            }
            subscriber.subscriber_name = updateSubscriberDto.subscriber_name;
            subscriber.expirydate = updateSubscriberDto.expirydate;
            subscriber.address1 = updateSubscriberDto.address1;
            subscriber.address2 = updateSubscriberDto.address2;
            subscriber.address3 = updateSubscriberDto.address3;
            subscriber.mobilenumber = updateSubscriberDto.mobilenumber;
            subscriber.state = updateSubscriberDto.state;
            subscriber.pincode = updateSubscriberDto.pincode;
            dbContext.SaveChanges();
            return Ok(subscriber);
        }
    }
}
