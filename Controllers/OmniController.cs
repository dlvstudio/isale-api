using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace atakafe_api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class OmniController : ControllerBase
    {
        private readonly IChannelQueueService<UserActivity> _queueMessageActivity;
        private readonly IChannelQueueService<FbUpdateToken> _queueMessage;
        private readonly IStaffService _staffService;


        public OmniController(
            IChannelQueueService<UserActivity> queueMessageActivity,
            IChannelQueueService<FbUpdateToken> queueMessage,
            IStaffService staffService
        )
        {
            _queueMessageActivity = queueMessageActivity;
            _queueMessage = queueMessage;
            _staffService = staffService;
        }

        [HttpGet]
        [Route("Test")]
        public async Task<string> Test()
        {
            string token = "";
            string userId = "";
            string fbUserId = "";
            var updateObj = new FbUpdateToken() {UserId = userId, FbUserId = fbUserId, Token = token};
            await _queueMessage.WriteAsync(updateObj);
            return token;
        }

        [HttpPost]
        [Route("UpdateToken")]
        public async Task<bool> UpdateToken([FromBody] Dictionary<string, object> model)
        {
            if (model == null)
            {
                return false;
            }
            var userId = User.GetUserId();
            var isShopOwner = true;
            var hasFullAccess = false;
            var staffIdConverted = 0;
            var staffId = model.ContainsKey("staffId") && int.TryParse(model["staffId"].ToString(), out staffIdConverted)
                ? staffIdConverted
                : 0;
            if (staffId > 0)
            {
                var staff = await _staffService.GetByIdOnly(staffId);
                if (staff == null)
                {
                    return false;
                }
                hasFullAccess = staff.HasFullAccess;
                isShopOwner = staff.UserId == userId;
                var userName = User.GetEmail();
                if (userName != staff.UserName && !isShopOwner)
                {
                    return false;
                }
                userId = staff.UserId;
            }
            await _queueMessageActivity.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = "fbtoken",
                Action = "save",
                Note = "",
            });
            model["userId"] = userId;
            var token = model.ContainsKey("fbToken")
                ? model["fbToken"].ToString()
                : string.Empty;
            if (string.IsNullOrEmpty(token)) {
                return false;
            }
            var updateObj = new FbUpdateToken() {UserId = userId, FbUserId = model["fbUserId"].ToString(), Token = token};
            await _queueMessage.WriteAsync(updateObj);
            return true;
        }
    }
}
