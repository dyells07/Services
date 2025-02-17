using JWT_Refresh.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace JWT_Refresh.Controllers
{

    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly IHubContext<ChatHub> _hubContext;

        public ChatController(IHubContext<ChatHub> hubContext)
        {
            _hubContext = hubContext;
        }

        [HttpPost("send")]
        public async Task<IActionResult> SendMessage([FromBody] ChatMessage message)
        {
            await _hubContext.Clients.All.SendAsync("ReceiveMessage", message.User, message.Message);
            return Ok(new { status = "Message sent!" });
        }

        public class ChatMessage
        {
            public string User { get; set; }
            public string Message { get; set; }
        }
    }
}
