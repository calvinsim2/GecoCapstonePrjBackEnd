using CapstoneProjectBlog.Models;
using CapstoneProjectBlog.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace CapstoneProjectBlog.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class MailController : ControllerBase
    {
        private readonly IMailService _mailService;

        // we inject our mail service interface, inside our mailController.
        public MailController(IMailService mailService)
        {
            this._mailService = mailService;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMail([FromForm] MailModel mailRequest)
        {
            // remember in our mailService, we said to implement SendEmailAsync, and it accepts a mailrequest object.
            await _mailService.SendEmailAsync(mailRequest);
            return Ok(new
            {
                StatusCode = 200,
                Message = "Mail Sent!"
            });
        }
    }
}
