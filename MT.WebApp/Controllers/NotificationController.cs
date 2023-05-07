using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using MT.WebApp.Hubs;

namespace MT.WebApp.Controllers
{
    [Route("api/[controller]/[action]")]
    [ApiController]
    public class NotificationController : ControllerBase
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public NotificationController(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }
        [HttpGet("{connectionId}")]
        public  async Task<IActionResult> CompleteWritingProcess(string connectionId)
        {

            _hubContext.Clients.Client(connectionId).SendAsync("NotifyCompleteWritingProcess");
            return Ok();
        }
    }
}
