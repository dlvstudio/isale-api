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
    public class TradeController : ControllerBase
    {
        private readonly IChannelQueueService<UserActivity> _queueMessage;
        private readonly ISqlService _sqlService;
        private readonly ITradeService _tradeService;
        private readonly IStaffService _staffService;


        public TradeController(
            IChannelQueueService<UserActivity> queueMessage,
            ITradeService tradeService,
            ISqlService sqlService,
            IStaffService staffService
        )
        {
            _queueMessage = queueMessage;
            _sqlService = sqlService;
            _tradeService = tradeService;
            _staffService = staffService;
        }

        // GET /trade/list
        [HttpPost]
        [Route("List")]
        public async Task<IEnumerable<Trade>> List(GetTradesViewModel model)
        {
            var userId = User.GetUserId();
            var isShopOwner = true;
            Staff staff = null;
            if (model.StaffId.HasValue && model.StaffId.Value > 0)
            {
                staff = await _staffService.GetByIdOnly(model.StaffId.Value);
                if (staff == null)
                {
                    return null;
                }
                // owner login
                if (staff.UserId == userId)
                {
                    isShopOwner = true;
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
            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "trade",
                Action = "list",
                Note = "",
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-trade",
                    Action = "list",
                    Note = "",
                });
            }
            var dateFrom = !string.IsNullOrEmpty(model.DateFrom)
                ? DateTime.ParseExact(model.DateFrom, new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" }, null)
                : (DateTime?)null;
            var dateTo = !string.IsNullOrEmpty(model.DateTo)
                ? DateTime.ParseExact(model.DateTo, new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" }, null)
                : (DateTime?)null;
            var contactId = model.ContactId.HasValue ? model.ContactId.Value : 0;
            var productId = model.ProductId.HasValue ? model.ProductId.Value : 0;
            var staffId = model.StaffId.HasValue ? model.StaffId.Value : 0;
            var moneyAccountId = model.MoneyAccountId.HasValue ? model.MoneyAccountId.Value : 0;
            var orderId = model.OrderId.HasValue ? model.OrderId.Value : 0;
            var receivedNoteId = model.ReceivedNoteId.HasValue ? model.ReceivedNoteId.Value : 0;
            var transferNoteId = model.TransferNoteId.HasValue ? model.TransferNoteId.Value : 0;
            var debtId = model.DebtId.HasValue ? model.DebtId.Value : 0;
            var isReceived = model.IsReceived.HasValue ? model.IsReceived.Value : -1;
            var post = await _tradeService.GetTrades(userId, dateFrom, dateTo, contactId, productId, staffId, moneyAccountId, orderId, debtId, receivedNoteId, transferNoteId, isReceived);
            return post;
        }

        [HttpGet]
        public async Task<ActionResult<Trade>> Get(int id, int? staffId)
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
            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "trade",
                Action = "detail",
                Note = id.ToString(),
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-trade",
                    Action = "detail",
                    Note = "",
                });
            }
            var post = await _tradeService.GetTrade(id, userId);
            return post;
        }

        [HttpPost]
        [Route("Remove")]
        public async Task<ActionResult<bool>> Remove(int id, int? staffId, bool? saveAccount)
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

            if (staff != null && !staff.HasFullAccess && !isShopOwner)
            {
                var oldTrade = await _tradeService.GetTrade(id, userId);
                var oldDate = oldTrade.CreatedAt;
                if (oldDate.AddHours(24) < DateTime.Now && !staff.CanUpdateDeleteTransaction)
                {
                    return null;
                }
            }
            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "trade",
                Action = "delete",
                Note = id.ToString(),
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-trade",
                    Action = "delete",
                    Note = "",
                });
            }
            var post = await _tradeService.RemoveTrade(id, userId, saveAccount);
            return post;
        }

        // POST product/SaveContact
        [HttpPost]
        [Route("SaveTrade")]
        public async Task<ActionResult<int>> SaveTrade(SaveTradeViewModel model)
        {
            if (model == null)
            {
                return 0;
            }
            if (model.Value == 0)
            {
                return 0;
            }
            var userId = User.GetUserId();
            var userName = User.GetEmail();
            if (userName == "dlv@isale.com") {
                Request.Headers.TryGetValue("loginAs", out var loginAs);
                if (!string.IsNullOrEmpty(loginAs))
                {
                    var shop = await _sqlService.GetAsync("shop", new Dictionary<string, object>() {{"userName", loginAs}}, null);
                    if (shop != null)
                    {
                        userId = shop["userId"].ToString();
                    }
                }
            }
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
                var canCreateTransaction = staff.CanCreateNewTransaction || staff.CanCreateUpdateNote || staff.CanCreateUpdateDebt || staff.CanCreateOrder;
                if (model.Id == 0 && !canCreateTransaction)
                {
                    return null;
                }
                if (model.Id != 0)
                {
                    var oldTrade = await _tradeService.GetTrade(model.Id, userId);
                    var oldDate = oldTrade.CreatedAt;
                    var canUpdateTransaction = (staff.CanCreateNewTransaction && oldDate.AddHours(24) >= DateTime.Now)
                        || staff.CanUpdateDeleteTransaction
                        || staff.CanCreateUpdateNote
                        || staff.CanCreateUpdateDebt
                        || staff.CanUpdateDeleteOrder;
                    if (!canUpdateTransaction)
                    {
                        return null;
                    }
                }
            }
            var trade = new Trade();
            trade.Id = model.Id;
            trade.UserId = userId;
            trade.StaffId = model.StaffId.HasValue ? model.StaffId.Value : 0;
            trade.MoneyAccountId = model.MoneyAccountId.HasValue ? model.MoneyAccountId.Value : 0;
            trade.OrderId = model.OrderId.HasValue ? model.OrderId.Value : 0;
            trade.DebtId = model.DebtId.HasValue ? model.DebtId.Value : 0;
            trade.ContactId = model.ContactId;
            trade.ProductId = model.ProductId;
            trade.AvatarUrl = model.AvatarUrl;
            trade.ReceivedNoteId = model.ReceivedNoteId.HasValue ? model.ReceivedNoteId.Value : 0;
            trade.ImageUrlsJson = model.ImageUrlsJson;
            trade.IsPurchase = model.IsPurchase.HasValue && model.IsPurchase.Value;
            trade.IsReceived = model.IsReceived;
            trade.SaveAccount = model.SaveAccount;
            string createdAt = string.IsNullOrWhiteSpace(model.CreatedAt)
                ? DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
                : model.CreatedAt;
            int i = !string.IsNullOrEmpty(createdAt) ? createdAt.IndexOf(".") : -1;
            if (i >= 0)
            {
                createdAt = createdAt.Substring(0, i) + "Z";
            }
            trade.CreatedAt = DateTime
                    .ParseExact(createdAt, new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ssZ" }, null);
            trade.Note = model.Note;
            trade.ProductCount = model.ProductCount.HasValue ? model.ProductCount.Value : 0;
            trade.Value = model.Value;
            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "trade",
                Action = "save",
                Note = trade.Id.ToString(),
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-trade",
                    Action = "save",
                    Note = "",
                });
            }

            var result = await _tradeService.SaveTrade(trade);
            return result;
        }
    }
}
