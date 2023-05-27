using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace atakafe_api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class DebtController : ControllerBase
    {
        private readonly IChannelQueueService<UserActivity> _queueMessage;
        private readonly IDebtService _debtService;
        private readonly IStaffService _staffService;


        public DebtController(
            IChannelQueueService<UserActivity> queueMessage,
            IDebtService debtService,
            IStaffService staffService
        )
        {
            _queueMessage = queueMessage;
            _debtService = debtService;
            _staffService = staffService;
        }

        // GET /debt/list
        [HttpPost]
        [Route("List")]
        public async Task<IEnumerable<Debt>> List(GetDebtsViewModel model)
        {
            var userId = User.GetUserId();
            bool hasFullAccess = false;
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
                    if (userName != staff.UserName)
                    {
                        return null;
                    }
                    userId = staff.UserId;
                }
            }
            var dateFrom = !string.IsNullOrEmpty(model.DateFrom)
                ? DateTime.ParseExact(model.DateFrom, new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" }, null)
                : (DateTime?)null;
            var dateTo = !string.IsNullOrEmpty(model.DateTo)
                ? DateTime.ParseExact(model.DateTo, new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" }, null)
                : (DateTime?)null;
            var contactId = model.ContactId.HasValue ? model.ContactId.Value : 0;
            var productId = model.ProductId.HasValue ? model.ProductId.Value : 0;
            var orderId = model.OrderId.HasValue ? model.OrderId.Value : 0;
            var receivedNoteId = model.ReceivedNoteId.HasValue ? model.ReceivedNoteId.Value : 0;
            var debtType = model.DebtType.HasValue ? model.DebtType.Value : 0;
            var staffId = model.StaffId.HasValue && !hasFullAccess ? model.StaffId.Value : 0;
            var storeId = model.StoreId.HasValue ? model.StoreId.Value : 0;
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = "debt",
                Action = "list",
                Note = "",
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-debt",
                    Action = "list",
                    Note = "",
                });
            }
            var post = await _debtService.GetDebts(userId, dateFrom, dateTo, contactId, productId, debtType, orderId, receivedNoteId, staffId, storeId);
            return post;
        }

        [HttpGet]
        public async Task<ActionResult<Debt>> Get(int id, int? staffId)
        {
            var userId = User.GetUserId();
            Staff staff = null;
            if (staffId.HasValue && staffId.Value > 0)
            {
                staff = await _staffService.GetByIdOnly(staffId.Value);
                if (staff == null)
                {
                    return null;
                }
                var userName = User.GetEmail();
                if (userName != staff.UserName)
                {
                    return null;
                }
                userId = staff.UserId;
            }
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = "debt",
                Action = "detail",
                Note = id.ToString(),
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-debt",
                    Action = "detail",
                    Note = "",
                });
            }
            var post = await _debtService.GetDebt(id, userId);
            return post;
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
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = "debt",
                Action = "delete",
                Note = id.ToString(),
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-debt",
                    Action = "delete",
                    Note = "",
                });
            }
            var post = await _debtService.RemoveDebt(id, userId);
            return post;
        }

        // POST debt/SaveDebt
        [HttpPost]
        [Route("SaveDebt")]
        public async Task<ActionResult<int>> SaveDebt(SaveDebtViewModel model)
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
                if (model.Id == 0 && !staff.CanCreateUpdateDebt)
                {
                    return null;
                }
                if (model.Id != 0)
                {
                    var old = await _debtService.GetDebt(model.Id, userId);
                    var oldDate = old.CreatedAt.Value;
                    if (oldDate.AddHours(24) < DateTime.Now && !staff.CanCreateUpdateDebt)
                    {
                        return null;
                    }
                    if (!staff.CanCreateUpdateDebt)
                    {
                        return null;
                    }
                }
            }
            var debt = new Debt();
            debt.Id = model.Id;
            debt.UserId = userId;
            debt.ContactId = model.ContactId;
            debt.ProductId = model.ProductId;
            debt.StaffId = model.StaffId.HasValue ? model.StaffId.Value : 0;
            debt.IsPurchase = model.IsPurchase.HasValue && model.IsPurchase.Value;
            debt.OrderId = model.OrderId.HasValue ? model.OrderId.Value : 0;
            debt.ReceivedNoteId = model.ReceivedNoteId.HasValue ? model.ReceivedNoteId.Value : 0;
            debt.DebtType = model.DebtType;
            debt.CreatedAt = model.CreatedAt;
            debt.MaturityDate = model.MaturityDate;
            debt.IsPaid = model.IsPaid.HasValue ? model.IsPaid.Value : false;
            debt.InterestRate = model.InterestRate.HasValue ? model.InterestRate.Value : 0;
            debt.Note = model.Note;
            debt.ProductCount = model.ProductCount.HasValue ? model.ProductCount.Value : 0;
            debt.Value = model.Value;
            debt.ValuePaid = model.ValuePaid.HasValue ? model.ValuePaid.Value : 0;
            debt.CountPaid = model.CountPaid.HasValue ? model.CountPaid.Value : 0;
            debt.StoreId = model.StoreId.HasValue ? model.StoreId.Value : 0;

            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = "debt",
                Action = "save",
                Note = debt.Id.ToString(),
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-debt",
                    Action = "save",
                    Note = "",
                });
            }
            var result = await _debtService.SaveDebt(debt);
            return result;
        }
    }
}
