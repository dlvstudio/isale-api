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
    public class CategoryController : ControllerBase
    {
        private readonly IChannelQueueService<UserActivity> _queueMessage;
        private readonly ICategoryService _categoryService;
        private readonly IStaffService _staffService;


        public CategoryController(
            IChannelQueueService<UserActivity> queueMessage,
            ICategoryService categoryService,
            IStaffService staffService
        )
        {
            _queueMessage = queueMessage;
            _categoryService = categoryService;
            _staffService = staffService;
        }

        // GET /product/list
        [HttpGet]
        [Route("List")]
        public async Task<IEnumerable<Category>> List(int? staffId)
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
                Feature = "trade_category",
                Action = "list",
                Note = "",
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = userId,
                    Feature = "staff-trade_category",
                    Action = "list",
                    Note = "",
                });
            }
            var post = await _categoryService.GetCategories(userId);
            return post;
        }

        [HttpGet]
        public async Task<ActionResult<Category>> Get(int id, int? staffId)
        {
            var userId = User.GetUserId();
            Staff staff = null;
            bool hasFullAccess = false;
            if (staffId.HasValue && staffId.Value > 0)
            {
                staff = await _staffService.GetByIdOnly(staffId.Value);
                if (staff == null)
                {
                    return null;
                }
                hasFullAccess = staff.HasFullAccess;
                // owner login
                if (staff.UserId == userId)
                {
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
            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "trade_category",
                Action = "detail",
                Note = id.ToString(),
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = userId,
                    Feature = "staff-trade_category",
                    Action = "detail",
                    Note = "",
                });
            }
            var post = await _categoryService.GetCategory(id, userId);
            return post;
        }

        [HttpPost]
        [Route("Remove")]
        public async Task<ActionResult<bool>> Remove(int id, int? staffId)
        {
            var userId = User.GetUserId();
            Staff staff = null;
            bool hasFullAccess = false;
            bool isOwner = false;
            if (staffId.HasValue && staffId.Value > 0)
            {
                staff = await _staffService.GetByIdOnly(staffId.Value);
                if (staff == null)
                {
                    return null;
                }
                hasFullAccess = staff.HasFullAccess;
                // owner login
                if (staff.UserId == userId)
                {
                    isOwner = true;
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
            if (staff != null && !staff.HasFullAccess && !isOwner)
            {
                return null;
            }
            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "trade_category",
                Action = "delete",
                Note = id.ToString(),
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = userId,
                    Feature = "staff-trade_category",
                    Action = "delete",
                    Note = "",
                });
            }
            var post = await _categoryService.RemoveCategory(id, userId);
            return post;
        }

        // POST product/SaveCategory
        [HttpPost]
        [Route("SaveCategory")]
        public async Task<ActionResult<int>> SaveCategory(SaveCategoryViewModel model)
        {
            if (model == null)
            {
                return 0;
            }
            var userId = User.GetUserId();
            Staff staff = null;
            bool hasFullAccess = false;
            bool isOwner = false;
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
                    isOwner = true;
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

            if (staff != null && !staff.HasFullAccess && !isOwner)
            {
                return null;
            }
            var product = new Category();
            product.Id = model.Id;
            product.OrderIndex = model.OrderIndex.HasValue ? model.OrderIndex.Value : 0;
            product.UserId = userId;
            product.Title = model.Title;

            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "trade_category",
                Action = "save",
                Note = product.Id.ToString(),
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = userId,
                    Feature = "staff-trade_category",
                    Action = "save",
                    Note = "",
                });
            }
            var result = await _categoryService.SaveCategory(product);
            return result;
        }

        [HttpGet]
        [Route("GetCategoriesToTrade")]
        public async Task<IEnumerable<TradeToCategory>> GetCategoriesToTrade(int tradeId, int? staffId)
        {
            var userId = User.GetUserId();
            var isShopOwner = true;
            if (staffId.HasValue && staffId.Value > 0)
            {
                var staff = await _staffService.GetByIdOnly(staffId.Value);
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
            return await _categoryService.GetCategoriesToTrade(tradeId, userId);
        }

        [HttpPost]
        [Route("GetCategoriesOfProducts")]
        public async Task<IEnumerable<TradeToCategory>> GetCategoriesOfProducts(GetCategoryViewModel model)
        {
            var userId = User.GetUserId();
            var isShopOwner = true;
            if (model.StaffId.HasValue && model.StaffId.Value > 0)
            {
                var staff = await _staffService.GetByIdOnly(model.StaffId.Value);
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
            return await _categoryService.GetCategoriesToTrade(model.Ids, userId);
        }

        [HttpGet]
        [Route("GetCategoriesToDebt")]
        public async Task<IEnumerable<DebtToCategory>> GetCategoriesToDebt(int debtId, int? staffId)
        {
            var userId = User.GetUserId();
            Staff staff = null;
            bool hasFullAccess = false;
            if (staffId.HasValue && staffId.Value > 0)
            {
                staff = await _staffService.GetByIdOnly(staffId.Value);
                if (staff == null)
                {
                    return null;
                }
                hasFullAccess = staff.HasFullAccess;
                // owner login
                if (staff.UserId == userId)
                {
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
            return await _categoryService.GetCategoriesToDebt(debtId, userId);
        }

        [HttpGet]
        [Route("GetAllCategoriesToTrade")]
        public async Task<IEnumerable<TradeToCategory>> GetAllCategoriesToTrade(int? staffId)
        {
            var userId = User.GetUserId();
            Staff staff = null;
            bool hasFullAccess = false;
            if (staffId.HasValue && staffId.Value > 0)
            {
                staff = await _staffService.GetByIdOnly(staffId.Value);
                if (staff == null)
                {
                    return null;
                }
                hasFullAccess = staff.HasFullAccess;
                // owner login
                if (staff.UserId == userId)
                {
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
            return await _categoryService.GetAllCategoriesToTrade(userId);
        }

        [HttpGet]
        [Route("GetAllCategoriesToDebt")]
        public async Task<IEnumerable<DebtToCategory>> GetAllCategoriesToDebt(int? staffId)
        {
            var userId = User.GetUserId();
            Staff staff = null;
            bool hasFullAccess = false;
            if (staffId.HasValue && staffId.Value > 0)
            {
                staff = await _staffService.GetByIdOnly(staffId.Value);
                if (staff == null)
                {
                    return null;
                }
                hasFullAccess = staff.HasFullAccess;
                // owner login
                if (staff.UserId == userId)
                {
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
            return await _categoryService.GetAllCategoriesToDebt(userId);
        }

        [HttpPost]
        [Route("GetTradesByCategory")]
        public async Task<IEnumerable<Trade>> GetTradesByCategory(GetTradesByCategoryViewModel model)
        {
            var userId = User.GetUserId();
            Staff staff = null;
            bool hasFullAccess = false;
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
            var categoryId = model.CategoryId.HasValue ? model.CategoryId.Value : 0;
            var post = await _categoryService.GetTradesByCategory(categoryId, dateFrom, dateTo, userId);
            return post;
        }

        [HttpPost]
        [Route("GetProductsByCategory")]
        public async Task<IEnumerable<Product>> GetProductsByCategory(GetTradesByCategoryViewModel model)
        {
            var userId = User.GetUserId();
            Staff staff = null;
            bool hasFullAccess = false;
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
            var categoryId = model.CategoryId.HasValue ? model.CategoryId.Value : 0;
            var post = await _categoryService.GetProductsByCategory(categoryId, userId);
            return post;
        }

        [HttpPost]
        [Route("GetDebtsByCategory")]
        public async Task<IEnumerable<Debt>> GetDebtsByCategory(GetTradesByCategoryViewModel model)
        {
            var userId = User.GetUserId();
            Staff staff = null;
            bool hasFullAccess = false;
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
            var categoryId = model.CategoryId.HasValue ? model.CategoryId.Value : 0;
            var post = await _categoryService.GetDebtsByCategory(categoryId, dateFrom, dateTo, userId);
            return post;
        }

        [HttpPost]
        [Route("SaveCategoriesToTrade")]
        public async Task<IEnumerable<int>> SaveCategoriesToTrade(SaveCategoriesToTradeViewModel model)
        {
            var userId = User.GetUserId();
            var isShopOwner = true;
            if (model.StaffId.HasValue && model.StaffId.Value > 0)
            {
                var staff = await _staffService.GetByIdOnly(model.StaffId.Value);
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
            return await _categoryService.SaveCategoriesToTrade(TradeToCategoryViewModel.ConvertToModel(model.Categories), model.TradeId.HasValue ? model.TradeId.Value : 0, userId);
        }

        [HttpPost]
        [Route("SaveCategoriesToDebt")]
        public async Task<IEnumerable<int>> SaveCategoriesToDebt(SaveCategoriesToDebtViewModel model)
        {
            var userId = User.GetUserId();
            Staff staff = null;
            bool hasFullAccess = false;
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
            return await _categoryService.SaveCategoriesToDebt(DebtToCategoryViewModel.ConvertToModel(model.Categories), model.TradeId.HasValue ? model.TradeId.Value : 0, userId);
        }

        [HttpPost]
        [Route("DeleteCategoriesToTrade")]
        public async Task<ActionResult<bool>> DeleteCategoriesToTrade(DeleteCategoriesToTradeViewModel model)
        {
            var userId = User.GetUserId();
            Staff staff = null;
            bool hasFullAccess = false;
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
            return await _categoryService.DeleteCategoriesToTrade(TradeToCategoryViewModel.ConvertToModel(model.Categories), userId);
        }

        [HttpPost]
        [Route("DeleteCategoriesToDebt")]
        public async Task<ActionResult<bool>> DeleteCategoriesToDebt(DeleteCategoriesToDebtViewModel model)
        {
            var userId = User.GetUserId();
            Staff staff = null;
            bool hasFullAccess = false;
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
            return await _categoryService.DeleteCategoriesToDebt(DebtToCategoryViewModel.ConvertToModel(model.Categories), userId);
        }
    }
}
