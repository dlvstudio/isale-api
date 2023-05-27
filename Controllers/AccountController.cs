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
    public class AccountController : ControllerBase
    {
        private readonly IChannelQueueService<UserActivity> _queueMessage;
        private readonly ICacheService _cacheService;
        private readonly IAccountService _accountService;
        private readonly IStaffService _staffService;

        
        public AccountController(
            IChannelQueueService<UserActivity> queueMessage,
            ICacheService cacheService,
            IAccountService accountService,
            IStaffService staffService
        ) {
            _queueMessage = queueMessage;
            _cacheService = cacheService;
            _accountService = accountService;
            _staffService = staffService;
        }

        // GET /trade/list
        [HttpPost]
        [Route("Accounts")]
        public async Task<IEnumerable<Account>> Accounts()
        {
            var userId = User.GetUserId();
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = "money_account",
                Action = "list",
                Note = "",
            });
            var post = await _accountService.GetAccounts(userId);
            return post;
        }

        [HttpPost]
        [Route("GetItemsByAccount")]
        public async Task<IEnumerable<AccountItem>> GetItemsByAccount(GetItemsByAccountViewModel model)
        {
            var userId = User.GetUserId();
            var dateFrom = !string.IsNullOrEmpty(model.DateFrom) 
                ? DateTime.ParseExact(model.DateFrom, new string[]{"yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd"}, null) 
                : (DateTime?)null;
            var dateTo = !string.IsNullOrEmpty(model.DateTo) 
                ? DateTime.ParseExact(model.DateTo, new string[]{"yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd"}, null) 
                : (DateTime?)null;
            var accountId = model.AccountId;
            var post = await _accountService.GetAccountItemsByAccount(userId, accountId, dateFrom, dateTo);
            return post;
        }

        [HttpGet]
        [Route("GetDefault")]
        public async Task<Account> GetDefault()
        {
            var userId = User.GetUserId();
            var post = await _accountService.GetDefault(userId);
            return post;
        }

        [HttpGet]
        [Route("GetStoreDefault")]
        public async Task<Account> GetStoreDefault(int storeId)
        {
            var userId = User.GetUserId();
            var post = await _accountService.GetStoreDefault(userId, storeId);
            return post;
        }

        [HttpGet]
        [Route("GetByTrade")]
        public async Task<AccountItem> GetByTrade(int tradeId, int? staffId)
        {
            var userId = User.GetUserId();
            var isShopOwner = true;
            if (staffId.HasValue && staffId.Value > 0) {
                var staff = await _staffService.GetByIdOnly(staffId.Value);
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
            var post = await _accountService.GetAccountItemByTrade(userId, tradeId);
            return post;
        }

        [HttpGet]
        [Route("GetByOrder")]
        public async Task<AccountItem> GetByOrder(int orderId)
        {
            var userId = User.GetUserId();
            var post = await _accountService.GetAccountItemByOrder(userId, orderId);
            return post;
        }

        [HttpGet]
        public async Task<Account> Get(int id)
        {
            var userId = User.GetUserId();
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = "money_account",
                Action = "detail",
                Note = id.ToString(),
            });
            var post = await _accountService.GetAccount(id, userId);
            return post;
        }

        [HttpGet]
        [Route("GetItem")]
        public async Task<AccountItem> GetItem(int id)
        {
            var userId = User.GetUserId();
            var post = await _accountService.GetAccountItem(id, userId);
            return post;
        }

        [HttpPost]
        [Route("Remove")]
        public async Task<bool> Remove(int id)
        {
            var userId = User.GetUserId();
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = "money_account",
                Action = "delete",
                Note = id.ToString(),
            });
            var post = await _accountService.RemoveAccount(id, userId);
            _cacheService.RemoveGetByIdItem("money_account", userId, id.ToString());
            _cacheService.RemoveListEqualItem("money_account", userId);
            return post;
        }

        [HttpPost]
        [Route("RemoveItems")]
        public async Task<bool> RemoveItems(int accountId)
        {
            var userId = User.GetUserId();
            var post = await _accountService.RemoveAccountItems(accountId, userId);
            _cacheService.RemoveGetByIdItem("money_account", userId, accountId.ToString());
            _cacheService.RemoveListEqualItem("money_account", userId);
            return post;
        }

        [HttpPost]
        [Route("RemoveItem")]
        public async Task<bool> RemoveItem(int itemId)
        {
            var userId = User.GetUserId();
            var post = await _accountService.RemoveAccountItem(itemId, userId);
            _cacheService.RemoveListEqualItem("money_account", userId);
            return post;
        }


        // POST product/SaveContact
        [HttpPost]
        [Route("SaveAccount")]
        public async Task<int> SaveAccount(SaveAccountViewModel model)
        {
            if (model == null){
                return 0;
            }
            var userId = User.GetUserId();
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = "money_account",
                Action = "save",
                Note = model.Id.ToString(),
            });
            var account = new Account();
            account.Id = model.Id;
            account.UserId = userId;
            account.AccountName = model.AccountName;
            account.BankAccountName = model.BankAccountName;
            account.BankName = model.BankName;
            account.BankNumber = model.BankNumber;
            account.IsDefault = model.IsDefault.HasValue && model.IsDefault.Value;
            account.Total = model.Total.HasValue ? model.Total.Value : 0;
            
            var result = await _accountService.SaveAccount(account);
            _cacheService.RemoveGetByIdItem("money_account", userId, result.ToString());
            _cacheService.RemoveListEqualItem("money_account", userId);
            return result;
        }

        [HttpPost]
        [Route("SaveAccountItem")]
        public async Task<int> SaveAccountItem(SaveAccountItemViewModel model)
        {
            if (model == null){
                return 0;
            }
            var userId = User.GetUserId();
            
            var account = new AccountItem();
            account.Id = model.Id;
            account.UserId = userId;
            account.TradeId = model.TradeId.HasValue ? model.TradeId.Value : 0;
            account.OrderId = model.OrderId.HasValue ? model.OrderId.Value : 0;
            account.Value = model.Value.HasValue ? model.Value.Value : 0;
            account.TransferFee = model.TransferFee.HasValue ? model.TransferFee.Value : 0;
            account.Note = model.Note;
            account.MoneyAccountId = model.MoneyAccountId;
            
            var result = await _accountService.SaveAccountItem(account);
            _cacheService.RemoveGetByIdItem("money_account", userId, model.MoneyAccountId.ToString());
            _cacheService.RemoveListEqualItem("money_account", userId);
            return result;
        }
    }
}
