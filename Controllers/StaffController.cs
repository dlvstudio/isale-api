using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace atakafe_api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class StaffController : ControllerBase
    {
        private readonly IChannelQueueService<UserActivity> _queueMessage;
        private readonly ICacheService _cacheService;
        private readonly IStaffService _staffService;
        
        public StaffController(
            IChannelQueueService<UserActivity> queueMessage,
            IStaffService staffService,
            ICacheService cacheService
        ) {
            _queueMessage = queueMessage;
            _staffService = staffService;
            _cacheService = cacheService;
        }

        // GET /staff/list
        [HttpGet]
        [Route("List")]
        public async Task<IEnumerable<Staff>> List()
        {
            var userId = User.GetUserId();
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = "staff",
                Action = "list",
                Note = "",
            });
            var post = await _staffService.List(userId);
            return post;
        }

        [HttpGet]
        public async Task<ActionResult<Staff>> Get(int id)
        {
            var userId = User.GetUserId();
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = "staff",
                Action = "detail",
                Note = id.ToString(),
            });
            var post = await _staffService.Get(id, userId);
            return post;
        }

        [HttpGet]
        [Route("CheckPermissions")]
        public async Task<IEnumerable<Dictionary<string, object>>> CheckPermissions()
        {
            var userId = User.GetUserId();
            var userName = User.GetEmail();
            var post = await _staffService.CheckPermissions(userName);
            return post;
        }

        [HttpPost]
        [Route("Remove")]
        public async Task<ActionResult<bool>> Remove(int id)
        {
            var userId = User.GetUserId();
            var post = await _staffService.Remove(id, userId);
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = "staff",
                Action = "delete",
                Note = id.ToString(),
            });
            _cacheService.RemoveListEqualItem("staff", userId);
            _cacheService.RemoveGetByIdItem("staff", userId, id.ToString());
            return post;
        }

        // POST staff/Save
        [HttpPost]
        [Route("Save")]
        public async Task<ActionResult<int>> Save(SaveStaffViewModel model)
        {
            if (model == null){
                return 0;
            }
            var userId = User.GetUserId();
            
            var contact = new Staff();
            contact.Id = model.Id;
            contact.UserId = userId;
            contact.AvatarUrl = model.AvatarUrl;
            contact.Name = model.Name;
            contact.UserName = model.UserName;
            contact.ShopName = model.ShopName;
            contact.HasFullAccess = model.HasFullAccess.HasValue ? model.HasFullAccess.Value : false;
            contact.CanCreateNewTransaction = model.CanCreateNewTransaction.HasValue ? model.CanCreateNewTransaction.Value : false;
            contact.CanCreateOrder = model.CanCreateOrder.HasValue ? model.CanCreateOrder.Value : false;
            contact.CanUpdateDeleteOrder = model.CanUpdateDeleteOrder.HasValue ? model.CanUpdateDeleteOrder.Value : false;
            contact.CanUpdateDeleteTransaction = model.CanUpdateDeleteTransaction.HasValue ? model.CanUpdateDeleteTransaction.Value : false;
            contact.CanCreateUpdateDebt = model.CanCreateUpdateDebt.HasValue ? model.CanCreateUpdateDebt.Value : false;
            contact.CanCreateUpdateNote = model.CanCreateUpdateNote.HasValue ? model.CanCreateUpdateNote.Value : false;
            contact.CanUpdateDeleteProduct = model.CanUpdateDeleteProduct.HasValue ? model.CanUpdateDeleteProduct.Value : false;
            contact.CanViewProductCostPrice = model.CanViewProductCostPrice.HasValue ? model.CanViewProductCostPrice.Value : false;
            contact.CanUpdateProductCostPrice = model.CanUpdateProductCostPrice.HasValue ? model.CanUpdateProductCostPrice.Value : false;
            contact.CanViewAllContacts= model.CanViewAllContacts.HasValue ? model.CanViewAllContacts.Value : false;
            contact.CanManageContacts= model.CanManageContacts.HasValue ? model.CanManageContacts.Value : false;
            contact.HourLimit= model.HourLimit.HasValue ? model.HourLimit.Value : 1;
            contact.StoreId = model.StoreId.HasValue ? model.StoreId.Value : 0;
            contact.ShiftId = model.ShiftId.HasValue ? model.ShiftId.Value : 0;
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = "staff",
                Action = "save",
                Note = contact.Id.ToString(),
            });
            
            var result = await _staffService.Save(contact);
            _cacheService.RemoveListEqualItem("staff", userId);
            _cacheService.RemoveGetByIdItem("staff", userId, result.ToString());
            return result;
        }
    }
}
