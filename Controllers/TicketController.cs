using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace atakafe_api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class TicketController : ControllerBase
    {
        private readonly IChannelQueueService<UserActivity> _queueMessage;
        private readonly ITicketService _service;
        private readonly IStaffService _staffService;

        
        public TicketController(
            IChannelQueueService<UserActivity> queueMessage,
            ITicketService service,
            IStaffService staffService
        ) {
            _queueMessage = queueMessage;
            _service = service;
            _staffService = staffService;
        }

        // POST product/SaveContact
        [HttpPost]
        [Route("Save")]
        public async Task<ActionResult<int>> Save(SaveTicketViewModel model)
        {
            if (model == null){
                return 0;
            }
            var userId = User.GetUserId();
            var isShopOwner = true;
            Staff staff = null;
            if (model.StaffId.HasValue && model.StaffId.Value > 0) {
                staff = await _staffService.GetByIdOnly(model.StaffId.Value);
                if (staff == null) {
                    return null;
                }
                var userName = User.GetEmail();
                isShopOwner = userId == staff.UserId;
                if (userName != staff.UserName && !isShopOwner) {
                    return null;
                }
                userId = staff.UserId;
            }
            var ticket = new Ticket();
            ticket.Id = model.Id;
            ticket.UserId = userId;
            ticket.Content = model.Content;
            ticket.Email = model.Email;
            ticket.Subject = model.Subject;
            ticket.CategoryId = model.CategoryId.HasValue ? model.CategoryId.Value : 0;
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = "ticket",
                Action = "save",
                Note = ticket.Id.ToString()
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-ticket",
                    Action = "save",
                    Note = "",
                });
            }
            
            var result = await _service.Save(ticket);
            return result;
        }
    }
}
