using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace atakafe_api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class TransferNoteController : ControllerBase
    {
        private readonly IChannelQueueService<UserActivity> _queueMessage;
        private readonly ICacheService _cacheService;
        private readonly ISqlService _service;
        private readonly IStaffService _staffService;

        public TransferNoteController(
            IChannelQueueService<UserActivity> queueMessage,
            ICacheService cacheService,
            ISqlService service,
            IStaffService staffService
        )
        {
            _queueMessage = queueMessage;
            _cacheService = cacheService;
            _service = service;
            _staffService = staffService;
        }

        // GET /receivednote/list
        [HttpPost]
        [Route("List")]
        public async Task<IEnumerable<Dictionary<string, object>>> List(GetReceivedNotesViewModel model)
        {
            var userId = User.GetUserId();
            var hasFullAccess = false;
            var isShopOwner = true;
            Staff staff = null;
            if (model.StaffId.HasValue && model.StaffId.Value > 0)
            {
                staff = await _staffService.GetByIdOnly(model.StaffId.Value);
                if (staff == null)
                {
                    return null;
                }
                hasFullAccess = staff.HasFullAccess;
                // owner login
                if (staff.UserId == userId)
                {
                    // do nothing
                }
                else
                {
                    var userName = User.GetEmail();
                    isShopOwner = userId == staff.UserId;
                    if (userName != staff.UserName && !isShopOwner)
                    {
                        return null;
                    }
                    userId = staff.UserId;
                }
            }
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = "transfer_note",
                Action = "list",
                Note = "",
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-transfer_note",
                    Action = "list",
                    Note = "",
                });
            }
            var dateFrom = !string.IsNullOrEmpty(model.DateFrom)
                ? DateTime.ParseExact(model.DateFrom, new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" }, null)
                : DateTime.Now.AddMonths(-3);
            var dateTo = !string.IsNullOrEmpty(model.DateTo)
                ? DateTime.ParseExact(model.DateTo, new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" }, null)
                : DateTime.Now;
            var modelQuery = new Dictionary<string, object>{
                {"userId", userId},
                {"dateFrom", dateFrom},
                {"dateTo", dateTo},
            };
            var query = new QueryModelOnSearch()
            {
                WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"userId", new List<string>{EnumSearchFunctions.EQUALS}},
                    {"createdAt", new List<string>{EnumSearchFunctions.BETWEENS, "dateFrom", "dateTo"}},
                }
            };
            var items = await _service.ListAsync("transfer_note", modelQuery, query);
            return items;
        }

        [HttpGet]
        public async Task<ActionResult<Dictionary<string, object>>> Get(int id, int? staffId)
        {
            var userId = User.GetUserId();
            var isShopOwner = true;
            Staff staff = null;
            if (staffId.HasValue && staffId.Value > 0)
            {
                staff = await _staffService.GetByIdOnly(staffId.Value);
                if (staff == null)
                {
                    return null;
                }
                var userName = User.GetEmail();
                isShopOwner = userId == staff.UserId;
                if (userName != staff.UserName && !isShopOwner)
                {
                    return null;
                }
                userId = staff.UserId;
            }
            var model = new Dictionary<string, object>() {
                {"userId", userId},
                {"id", id},
            };
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = "transfer_note",
                Action = "detail",
                Note = id.ToString(),
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-transfer_note",
                    Action = "detail",
                    Note = "",
                });
            }
            var item = await _service.GetByIdAsync("transfer_note", id, userId);
            return item;
        }

        [HttpPost]
        [Route("Remove")]
        public async Task<ActionResult<bool>> Remove(int id, int? staffId)
        {
            var userId = User.GetUserId();
            var isShopOwner = true;
            Staff staff = null;
            if (staffId.HasValue && staffId.Value > 0)
            {
                staff = await _staffService.GetByIdOnly(staffId.Value);
                if (staff == null)
                {
                    return null;
                }
                var userName = User.GetEmail();
                isShopOwner = userId == staff.UserId;
                if (userName != staff.UserName && !isShopOwner)
                {
                    return null;
                }
                userId = staff.UserId;
            }
                var oldOrder = await _service.GetByIdAsync("transfer_note", id, userId);
            if (staff != null && !staff.HasFullAccess && !isShopOwner)
            {
                var oldOrderDate = (DateTime)oldOrder["createdAt"];
                if (oldOrderDate.AddHours(24) < DateTime.Now && !staff.CanCreateUpdateNote)
                {
                    return null;
                }
                if (!staff.CanCreateUpdateNote)
                {
                    return null;
                }
            }
            var model = new Dictionary<string, object>() {
                {"userId", userId},
                {"id", id},
            };
            var isSuccess = await _service.RemoveAsync("transfer_note", model);
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = "transfer_note",
                Action = "delete",
                Note = id.ToString(),
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-transfer_note",
                    Action = "delete",
                    Note = "",
                });
            }
            
            var cacheItem = (CacheItem<ReceivedNote>)_cacheService.GetCacheItem("receivedNote");
            cacheItem.RemoveItem(userId, id.ToString());
            _cacheService.RemoveListEqualItem("product", userId);
            var cacheProductItem = (CacheItem<Product>)_cacheService.GetCacheItem("product");
            var items = JsonConvert.DeserializeObject<IEnumerable<Dictionary<string, object>>>(oldOrder["itemsJson"].ToString());
            if (items != null && items.Any()) {
                cacheProductItem.RemoveItems(userId, items.Select(i => i["productId"].ToString()).ToList());
            }
            var cacheProductList = (CacheList<Product>)_cacheService.GetCacheList("product");
            cacheProductList.ClearAllLists(userId);
            return isSuccess;
        }

        [HttpPost]
        [Route("Save")]
        public async Task<ActionResult<int>> Save(SaveTransferNoteViewModel model)
        {
            if (model == null)
            {
                return 0;
            }
            var userId = User.GetUserId();
            Staff staff = null;
            var isShopOwner = true;
            if (model.StaffId.HasValue && model.StaffId.Value > 0)
            {
                staff = await _staffService.GetByIdOnly(model.StaffId.Value);
                if (staff == null)
                {
                    return null;
                }
                var userName = User.GetEmail();
                isShopOwner = userId == staff.UserId;
                if (userName != staff.UserName && !isShopOwner)
                {
                    return null;
                }
                userId = staff.UserId;
            }
            if (staff != null && !staff.HasFullAccess && !isShopOwner)
            {
                if (model.Id == 0 && !staff.CanCreateUpdateNote)
                {
                    return null;
                }
                if (model.Id != 0)
                {
                    var oldOrder = await _service.GetByIdAsync("transfer_note", model.Id, userId);
                    var oldOrderDate = (DateTime)oldOrder["createdAt"];
                    if (oldOrderDate.AddHours(24) < DateTime.Now && !staff.CanCreateUpdateNote)
                    {
                        return null;
                    }
                    if (!staff.CanCreateUpdateNote)
                    {
                        return null;
                    }
                }
            }
            var note = new TransferNote();
            note.Id = model.Id;
            note.UserId = userId;
            note.Code = model.Code;
            note.ModifiedAt = DateTime.Now;
            if (model.CreatedAt.HasValue)
            {
                note.CreatedAt = model.CreatedAt;
            }
            note.StaffId = model.StaffId.HasValue ? model.StaffId.Value : 0;
            note.ItemsJson = model.ItemsJson;
            note.Deliverer = model.Deliverer;
            note.DeliveryAddress = model.DeliveryAddress;
            note.Transportation = model.Transportation;
            note.Receiver = model.Receiver;
            note.HasPayment = model.HasPayment.HasValue && model.HasPayment.Value;
            note.ImportStoreId = model.ImportStoreId.HasValue ? model.ImportStoreId.Value : 0;
            note.ExportStoreId = model.ExportStoreId.HasValue ? model.ExportStoreId.Value : 0;
            note.ExportMoneyAccountId = model.ExportMoneyAccountId.HasValue ? model.ExportMoneyAccountId.Value : 0;
            note.ImportMoneyAccountId = model.ImportMoneyAccountId.HasValue ? model.ImportMoneyAccountId.Value : 0;
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = "transfer_note",
                Action = "save",
                Note = note.Id.ToString(),
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-transfer_note",
                    Action = "save",
                    Note = "",
                });
            }

            var result = await _service.SaveAsync(note.ToDictionary(), "transfer_note", "id", new List<string>() { "Id", "UserId" }, null);

            var cacheItem = (CacheItem<ReceivedNote>)_cacheService.GetCacheItem("receivedNote");
            cacheItem.RemoveItem(userId, result.ToString());
            _cacheService.RemoveListEqualItem("product", userId);
            var cacheProductItem = (CacheItem<Product>)_cacheService.GetCacheItem("product");
            var items = JsonConvert.DeserializeObject<IEnumerable<Dictionary<string, object>>>(note.ItemsJson);
            if (items != null && items.Any()) {
                cacheProductItem.RemoveItems(userId, items.Select(i => i["productId"].ToString()).ToList());
            }
            var cacheProductList = (CacheList<Product>)_cacheService.GetCacheList("product");
            cacheProductList.ClearAllLists(userId);

            return result;
        }
    }
}
