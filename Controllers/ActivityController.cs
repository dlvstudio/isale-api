using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace atakafe_api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class ActivityController : ControllerBase
    {
        private readonly IChannelQueueService<UserActivity> _queueMessage;
        
        public ActivityController(
            IChannelQueueService<UserActivity> queueMessage
        ) {
            _queueMessage = queueMessage;
        }

        [HttpPost]
        [Route("Log")]
        public async Task<int> Log(LogViewModel logModel)
        {
            string userId = string.Empty;
            if (User != null) {
                userId = User.GetUserId();
                if (string.IsNullOrWhiteSpace(userId)) {
                    userId = HttpContext.Session.Id;
                }
            } else {
                userId = HttpContext.Session.Id;
            }
            if (logModel.Feature.ToLower() == "login") {
                return 0;
            }
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = logModel.Feature.ToLower(),
                Action = logModel.Action,
                Note = logModel.Note,
                Session = HttpContext.Session.Id
            });
            return 1;
        }
    }
}
