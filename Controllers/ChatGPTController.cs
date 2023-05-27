using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace atakafe_api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class ChatGPTController : ControllerBase
    {
        private readonly IChannelQueueService<UserActivity> _queueMessage;
        private readonly IChatGPTService _service;
        
        public ChatGPTController(
            IChannelQueueService<UserActivity> queueMessage,
            IChatGPTService service
        ) {
            _queueMessage = queueMessage;
            _service = service;
        }

        [HttpPost]
        [Route("Chat")]
        public async Task<ActionResult<dynamic>> Chat(IEnumerable<ChatGPTRoleAndContent> messages)
        {
            if (messages == null || !messages.Any()) {
                return null;
            }
            var result = await _service.SendMessageAsync(messages);
            return result;
        }
    }
}
