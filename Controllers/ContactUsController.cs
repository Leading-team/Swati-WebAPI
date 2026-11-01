using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SWWebAPI.Models.Entities;
using SWWebAPI.Services;
using System.ComponentModel.DataAnnotations;
namespace SWWebAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ContactUsController : ControllerBase
    {
        private readonly ContactUsService _contactService;

        public ContactUsController(ContactUsService contactService)
        {
            _contactService = contactService;
        }

        [HttpPost("Submit")]
        public async Task<IActionResult> Submit(
        [FromBody] ContactUsRequest request,
        CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Name, email and message are required."
                });
            }

            if (!new EmailAddressAttribute().IsValid(request.Email))
            {
                return BadRequest(new
                {
                    success = false,
                    message = "Please provide a valid email address."
                });
            }

            try
            {
                long contactId = await _contactService.SubmitContactAsync(
                    request, cancellationToken);

                return Ok(new
                {
                    success = true,
                    contactId,
                    message = "Your enquiry has been received successfully."
                });
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "Unable to submit your enquiry. Please try again."
                });
            }
        }

    }
}
