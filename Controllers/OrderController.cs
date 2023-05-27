using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace atakafe_api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class OrderController : ControllerBase
    {
        private readonly IChannelQueueService<UserActivity> _queueMessage;
        private readonly ICacheService _cacheService;
        private readonly IOrderService _orderService;
        private readonly IStaffService _staffService;
        private readonly ISqlService _sqlService;
        private readonly IProductService _productService;
        private readonly ITradeService _tradeService;
        private readonly IAccountService _accountService;
        private readonly DataController _dataController;
        private readonly AccountController _accountController;


        public OrderController(
            IChannelQueueService<UserActivity> queueMessage,
            ICacheService cacheService,
            IOrderService orderService,
            IStaffService staffService,
            IProductService productService,
            ITradeService tradeService,
            IAccountService accountService,
            ISqlService sqlService,
            DataController dataController,
            AccountController accountController
        )
        {
            _queueMessage = queueMessage;
            _cacheService = cacheService;
            _orderService = orderService;
            _staffService = staffService;
            _sqlService = sqlService;
            _productService = productService;
            _accountService = accountService;
            _tradeService = tradeService;
            _dataController = dataController;
            _accountController = accountController;
        }

        [HttpPost]
        [Route("NewOrderData")]
        public async Task<dynamic> NewOrderData([FromBody] Dictionary<string, object> model)
        {
            if (model == null)
            {
                return null;
            }

            _dataController.ControllerContext = ControllerContext;
            _accountController.ControllerContext = ControllerContext;

            var userId = User.GetUserId();
            var isShopOwner = true;
            var canViewAll = false;
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
                    return null;
                }
                hasFullAccess = staff.HasFullAccess;
                isShopOwner = staff.UserId == userId;
                var userName = User.GetEmail();
                if (userName != staff.UserName && !isShopOwner)
                {
                    return null;
                }
                userId = staff.UserId;
            }
            var filterByStaffConverted = 0;
            var filterByStaff = model.ContainsKey("filterByStaff") && int.TryParse(model["filterByStaff"].ToString(), out filterByStaffConverted)
                ? filterByStaffConverted
                : 0;
            var staffRealId = staffId > 0 && !hasFullAccess && !isShopOwner && !canViewAll
                ? staffId
                : ((staffId > 0 && hasFullAccess || isShopOwner || canViewAll) && filterByStaffConverted > 0)
                    ? filterByStaffConverted
                    : 0;
            if (staffRealId > 0)
            {
                model["staffId"] = staffRealId;
            }
            model["userId"] = userId;

            var shopConfigs = await _dataController.ListEqual(CloneAndAddToDic(model, "table", "shop_config"));
            var pointConfigs = await _dataController.ListEqual(CloneAndAddToDic(model, "table", "point_config"));
            var customerPrices = await _dataController.ListEqual(CloneAndAddToDic(model, "table", "customer_price"));
            var customerDiscounts = await _dataController.ListEqual(CloneAndAddToDic(model, "table", "customer_discount"));
            var defaultAccount = await _accountController.GetDefault();
            return new { shopConfigs, pointConfigs, customerPrices, customerDiscounts, defaultAccount };
        }

        // GET /order/list
        [HttpPost]
        [Route("List")]
        public async Task<IEnumerable<Order>> List(GetOrdersViewModel model)
        {
            var userId = User.GetUserId();
            Staff staff = null;
            bool hasFullAccess = false;
            bool updateStatusExceptDone = false;
            bool isOwner = false;
            if (model.StaffId.HasValue && model.StaffId.Value > 0)
            {
                staff = await _staffService.GetByIdOnly(model.StaffId.Value);
                if (staff == null)
                {
                    return null;
                }
                hasFullAccess = staff.HasFullAccess;
                updateStatusExceptDone = staff.UpdateStatusExceptDone;
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
            var dateFrom = !string.IsNullOrEmpty(model.DateFrom)
                ? DateTime.ParseExact(model.DateFrom, new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" }, null)
                : (DateTime?)null;
            var dateTo = !string.IsNullOrEmpty(model.DateTo)
                ? DateTime.ParseExact(model.DateTo, new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" }, null)
                : (DateTime?)null;
            var contactId = model.ContactId.HasValue ? model.ContactId.Value : 0;
            var staffId = model.StaffId.HasValue && !hasFullAccess && !updateStatusExceptDone
                ? model.StaffId.Value
                : (isOwner && model.StaffId.HasValue ? model.StaffId.Value : 0);
            var storeId = model.StoreId.HasValue && !hasFullAccess
                ? model.StoreId.Value
                : (isOwner && model.StoreId.HasValue ? model.StoreId.Value : 0);
            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "order",
                Action = "list",
                Note = "",
            });
            if (staff != null) {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-order",
                    Action = "list",
                    Note = "",
                });
            }
            var post = await _orderService.GetOrders(userId, dateFrom, dateTo, contactId, staffId, storeId, model.Status);
            if (post != null && post.Any())
            {
                foreach (var order in post)
                {
                    try
                    {
                        order.Items = JsonConvert.DeserializeObject<IEnumerable<OrderItem>>(order.ItemsJson);
                    }
                    catch (System.Exception)
                    {
                    }
                    // 
                }
            }
            return post;
        }

        [HttpGet]
        [Route("GetByCode")]
        public async Task<ActionResult<Order>> GetByCode(string code, int? staffId)
        {
            if (string.IsNullOrEmpty(code)) {
                return null;
            }
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
            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "order",
                Action = "detail-by-code",
                Note = code,
            });
            if (staff != null) {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-order",
                    Action = "detail-by-code",
                    Note = "",
                });
            }
            Order ret = null;
            ret = await _orderService.GetOrderByCode(code, userId);
            if (staff != null && !staff.HasFullAccess && !isOwner && ret.StaffId != staff.Id && !staff.UpdateStatusExceptDone)
            {
                return null;
            }
            return ret;
        }

        [HttpGet]
        public async Task<ActionResult<Order>> Get(int id, int? staffId)
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
            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "order",
                Action = "detail",
                Note = id.ToString(),
            });
            if (staff != null) {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-order",
                    Action = "detail",
                    Note = "",
                });
            }
            Order ret = null;
            var cacheItem = (CacheItem<Order>)_cacheService.GetCacheItem("order");
            if (cacheItem.Contains(userId, id))
            {
                ret = cacheItem.GetItem(userId, id);
            }
            else
            {
                ret = await _orderService.GetOrder(id, userId);
                cacheItem.SetItem(userId, id, ret);
            }
            if (staff != null && !staff.HasFullAccess && !isOwner && ret.StaffId != staff.Id && !staff.UpdateStatusExceptDone)
            {
                return null;
            }
            return ret;
        }

        [HttpPost]
        [Route("Remove")]
        public async Task<ActionResult<bool>> Remove(int id, int? staffId)
        {
            var userId = User.GetUserId();
            Staff staff = null;
            var isShopOwner = true;
            if (staffId.HasValue && staffId.Value > 0)
            {
                staff = await _staffService.GetByIdOnly(staffId.Value);
                if (staff == null)
                {
                    return null;
                }
                var userName = User.GetEmail();
                isShopOwner = userId == staff.UserId;
                var isStaff = userName == staff.UserName;
                if (!isStaff && !isShopOwner)
                {
                    return null;
                }
                userId = staff.UserId;
            }
            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "order",
                Action = "delete",
                Note = id.ToString(),
            });
            if (staff != null) {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-order",
                    Action = "delete",
                    Note = "",
                });
            }
            var oldOrder = await _orderService.GetOrder(id, userId);

            var cacheItem = (CacheItem<Order>)_cacheService.GetCacheItem("order");
            cacheItem.RemoveItem(userId, oldOrder.Id.ToString());
            if (oldOrder.TableId > 0)
            {
                _cacheService.RemoveListEqualItem("table", userId);
            }
            _cacheService.RemoveListEqualItem("product", userId);
            var cacheProductItem = (CacheItem<Product>)_cacheService.GetCacheItem("product");
            var items = JsonConvert.DeserializeObject<IEnumerable<OrderItem>>(oldOrder.ItemsJson);
            if (items != null && items.Any())
            {
                cacheProductItem.RemoveItems(userId, items.Select(i => i.ProductId.ToString()).ToList());
            }
            var cacheProductList = (CacheList<Product>)_cacheService.GetCacheList("product");
            cacheProductList.ClearAllLists(userId);

            // check is staff with access
            if (staff != null && !staff.HasFullAccess && !isShopOwner)
            {
                var oldOrderDate = oldOrder.CreatedAt.HasValue ? oldOrder.CreatedAt.Value : DateTime.Now;
                if (oldOrderDate.AddHours(staff.HourLimit) < DateTime.Now && !staff.CanUpdateDeleteOrder)
                {
                    return null;
                }
                if (!staff.CanCreateOrder)
                {
                    return null;
                }
            }
            await UpdateBuyCountAsync(oldOrder, -1);
            await RemovePointAsync(oldOrder);
            var post = await _orderService.RemoveOrder(id, userId);
            return post;
        }

        // POST order/SaveOrder
        [HttpPost]
        [Route("SaveOrder")]
        public async Task<ActionResult<int>> SaveOrder(SaveOrderViewModel model)
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
                var isStaff = userName == staff.UserName;
                if (!isStaff && !isShopOwner)
                {
                    return null;
                }
                userId = staff.UserId;
            }
            // check is staff with access
            var oldOrder = await _orderService.GetOrder(model.Id, userId);
            if (staff != null && !staff.HasFullAccess && !isShopOwner)
            {
                if (model.Id == 0 && !staff.CanCreateOrder)
                {
                    return null;
                }
                if (model.Id != 0)
                {
                    var oldOrderDate = oldOrder.CreatedAt.HasValue ? oldOrder.CreatedAt.Value : DateTime.Now;
                    if (oldOrderDate.AddHours(staff.HourLimit) < DateTime.Now && !staff.CanUpdateDeleteOrder)
                    {
                        return null;
                    }
                    if (!staff.CanCreateOrder)
                    {
                        return null;
                    }
                }
            }
            var order = new Order();
            order.Id = model.Id;
            order.UserId = userId;
            order.ContactId = model.ContactId.HasValue ? model.ContactId.Value : 0;
            order.CreatedAt = model.CreatedAt;
            order.StaffId = model.CollaboratorId.HasValue && model.CollaboratorId.Value != 0
                ? model.CollaboratorId.Value
                : model.StaffId.HasValue
                    ? model.StaffId.Value
                    : 0;
            order.MoneyAccountId = model.MoneyAccountId.HasValue ? model.MoneyAccountId.Value : 0;
            order.ContactName = model.ContactName;
            order.ContactPhone = model.ContactPhone;
            order.ContactAddress = model.ContactAddress;
            order.ItemsJson = model.ItemsJson;
            order.NetValue = model.NetValue.HasValue ? model.NetValue.Value : 0;
            order.Tax = model.Tax.HasValue ? model.Tax.Value : 0;
            order.TaxType = model.TaxType.HasValue ? model.TaxType.Value : 0;
            order.Total = model.Total.HasValue ? model.Total.Value : 0;
            order.Paid = model.Paid;
            order.Change = model.Change;
            order.OrderCode = model.OrderCode;
            order.ShippingFee = model.ShippingFee.HasValue ? model.ShippingFee.Value : 0;
            order.Status = model.Status.HasValue ? model.Status.Value : 0;
            order.Discount = model.Discount.HasValue ? model.Discount.Value : 0;
            order.DiscountOnTotal = model.DiscountOnTotal.HasValue ? model.DiscountOnTotal.Value : 0;
            order.TableId = model.TableId.HasValue ? model.TableId.Value : 0;
            order.Note = model.Note;
            order.StoreId = model.StoreId.HasValue ? model.StoreId.Value : 0;
            order.ShipperId = model.ShipperId.HasValue ? model.ShipperId.Value : 0;
            order.ShipperName = model.ShipperName;
            order.ShipperPhone = model.ShipperPhone;
            order.ShippingPartner = model.ShippingPartner;
            order.DeliveryAddress = model.DeliveryAddress;
            order.BillOfLadingCode = model.BillOfLadingCode;
            order.HasShipInfo = model.HasShipInfo.HasValue ? model.HasShipInfo.Value : false;
            order.PointAmount = model.PointAmount.HasValue ? model.PointAmount.Value : 0;
            order.PointPaymentExchange = model.PointPaymentExchange.HasValue ? model.PointPaymentExchange.Value : 0;
            order.AmountFromPoint = model.AmountFromPoint.HasValue ? model.AmountFromPoint.Value : 0; ;
            order.ShipCostOnCustomer = model.ShipCostOnCustomer.HasValue ? model.ShipCostOnCustomer.Value : true;
            order.TotalPromotionDiscount = model.TotalPromotionDiscount.HasValue ? model.TotalPromotionDiscount.Value : 0;
            order.PromotionsJson = model.PromotionsJson;
            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "order",
                Action = "save",
                Note = order.Id.ToString(),
            });
            if (staff != null) {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-order",
                    Action = "save",
                    Note = "",
                });
            }

            var result = await _orderService.SaveOrder(order);
            order.Id = result;
            var cacheItem = (CacheItem<Order>)_cacheService.GetCacheItem("order");
            cacheItem.RemoveItem(userId, order.Id.ToString());
            if (order.TableId > 0)
            {
                _cacheService.RemoveListEqualItem("table", userId);
            }
            _cacheService.RemoveListEqualItem("product", userId);
            var cacheProductItem = (CacheItem<Product>)_cacheService.GetCacheItem("product");
            var items = JsonConvert.DeserializeObject<IEnumerable<OrderItem>>(order.ItemsJson);
            if (items != null && items.Any())
            {
                cacheProductItem.RemoveItems(userId, items.Select(i => i.ProductId.ToString()).ToList());
            }
            var cacheProductList = (CacheList<Product>)_cacheService.GetCacheList("product");
            cacheProductList.ClearAllLists(userId);
            if (model.StaffId.HasValue && model.StaffId.Value > 0)
            {
                try
                {
                    await SendOrderMessageToShopOwnerAsync(userId, staff, order);
                }
                catch (System.Exception)
                {
                }
            }

            if (model.SaveProductNotes.HasValue && model.SaveProductNotes.Value)
            {
                await CreateTransactionsAsync(order, model.Lang);
                await AddProductNotesAsync(order, model.Lang);
            }
            if (oldOrder != null)
            {
                await UpdateBuyCountAsync(oldOrder, -1);
                await RemovePointAsync(oldOrder);
            }
            await UpdateBuyCountAsync(order, 1);
            await SavePointAsync(order);

            return result;
        }

        // POST order/SaveStatus
        [HttpPost]
        [Route("SaveStatus")]
        public async Task<ActionResult<int>> SaveStatus(SaveOrderStatusViewModel model)
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
                var isStaff = userName == staff.UserName;
                if (!isStaff && !isShopOwner)
                {
                    return null;
                }
                userId = staff.UserId;
            }
            // check is staff with access
            var oldOrder = await _orderService.GetOrder(model.Id, userId);
            if (staff != null && !staff.HasFullAccess && !isShopOwner)
            {
                if (model.Id == 0)
                {
                    return null;
                }
                if (model.Id != 0)
                {
                    if (!staff.CanCreateOrder && !staff.UpdateStatusExceptDone)
                    {
                        return null;
                    }
                }
            }
            var order = new Order();
            order.Id = model.Id;
            order.UserId = userId;
            order.Status = model.Status.HasValue ? model.Status.Value : 0;
            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "order",
                Action = "save-status",
                Note = order.Id.ToString(),
            });
            if (staff != null) {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-order",
                    Action = "save-status",
                    Note = "",
                });
            }

            var result = await _orderService.SaveOrderStatus(order);
            order.Id = result;
            var cacheItem = (CacheItem<Order>)_cacheService.GetCacheItem("order");
            cacheItem.RemoveItem(userId, order.Id.ToString());
            if (order.TableId > 0)
            {
                _cacheService.RemoveListEqualItem("table", userId);
            }
            _cacheService.RemoveListEqualItem("product", userId);
            var cacheProductList = (CacheList<Product>)_cacheService.GetCacheList("product");
            cacheProductList.ClearAllLists(userId);

            return result;
        }

        private async Task CreateTransactionsAsync(Order order, string lang)
        {
            if (order.Paid.HasValue && order.Paid.Value == 0)
            {
                return;
            }
            if (!order.Paid.HasValue && order.Status != 3)
            {
                return;
            }
            var arr = new List<Task>();
            var trades = await _tradeService.GetTrades(order.UserId, null, null, 0, 0, 0, 0, order.Id, 0, 0, 0, -1);
            foreach (var trade in trades)
            {
                if (trade.DebtId != 0)
                {
                    continue;
                }
                arr.Add(_tradeService.RemoveTrade(trade.Id, trade.UserId, true));
            }
            var change = order.Change.HasValue && order.Change.Value != 0 ? order.Change.Value : 0;
            var newTrade = new Trade();
            newTrade.ContactId = order.ContactId;
            newTrade.StaffId = order.StaffId;
            newTrade.IsPurchase = true;
            newTrade.IsReceived = true;
            newTrade.Value = order.Paid.HasValue ? (order.Paid.Value - change) : order.Total;
            newTrade.OrderId = order.Id;
            newTrade.MoneyAccountId = order.MoneyAccountId;
            newTrade.SaveAccount = true;
            newTrade.Note = GetOrderStringByLang(lang) + " #" + order.OrderCode;
            newTrade.CreatedAt = order.CreatedAt.HasValue ? order.CreatedAt.Value : DateTime.Now;
            newTrade.UserId = order.UserId;
            arr.Add(_tradeService.SaveTrade(newTrade));
            await Task.WhenAll(arr.ToArray());
        }

        private async Task UpdateBuyCountAsync(Order oldOrder, int buyCount)
        {
            await _sqlService.UpdateAsync("contact",
                new Dictionary<string, object>() { { "id", oldOrder.ContactId }, { "userId", oldOrder.UserId }, { "buyCount", buyCount } },
                new QueryModelOnSearch()
                {
                    WhereFieldQuerys = new Dictionary<string, List<string>>() {
                            {"id", new List<string>{EnumSearchFunctions.EQUALS}},
                            {"userid", new List<string>{EnumSearchFunctions.EQUALS}}
                    },
                    UpdateFieldQuerys = new Dictionary<string, List<string>>() {
                            {"buyCount", new List<string>{EnumUpdateFunctions.INCREASE}}
                    },
                }
            );
            _cacheService.RemoveListEqualItem("contact", oldOrder.UserId);
        }

        private async Task SavePointAsync(Order order)
        {
            if (order.ContactId <= 0)
            {
                return;
            }
            if (order.AmountFromPoint > 0)
            {
                await _sqlService.UpdateAsync("contact",
                    new Dictionary<string, object>() { { "id", order.ContactId }, { "userId", order.UserId }, { "point", order.PointAmount } },
                    new QueryModelOnSearch()
                    {
                        WhereFieldQuerys = new Dictionary<string, List<string>>() {
                            {"id", new List<string>{EnumSearchFunctions.EQUALS}},
                            {"userid", new List<string>{EnumSearchFunctions.EQUALS}}
                        },
                        UpdateFieldQuerys = new Dictionary<string, List<string>>() {
                            {"point", new List<string>{EnumUpdateFunctions.DECREASE}}
                        },
                    }
                );
                _cacheService.RemoveListEqualItem("contact", order.UserId);
                return;
            }
            var pointConfigs = await _sqlService.ListAsync("point_config", new Dictionary<string, object>() { { "userId", order.UserId } }, null);
            if (pointConfigs == null || !pointConfigs.Any())
            {
                return;
            }
            // reset arr task
            var arr = new List<Task>();
            if ((order.Items == null || !order.Items.Any()) && !string.IsNullOrEmpty(order.ItemsJson))
            {
                order.Items = JsonConvert.DeserializeObject<List<OrderItem>>(order.ItemsJson);
            }
            var productIds = new List<int>();
            productIds = order.Items.Select(i => i.ProductId).ToList();
            var productCategoriesResult = await _sqlService.ListAsync("trade_to_category", new Dictionary<string, object> { { "ids", productIds }, { "userId", order.UserId } },
                new QueryModelOnSearch()
                {
                    WhereFieldQuerys = new Dictionary<string, List<string>>() {
                        {"tradeid", new List<string> {EnumSearchFunctions.IN, "ids"}},
                        {"userid", new List<string> {EnumSearchFunctions.EQUALS}}
                    }
                }
            );
            var productCategories = productCategoriesResult != null && productCategoriesResult.Any() ? productCategoriesResult.ToList() : new List<Dictionary<string, object>>();
            Decimal? totalAmount = 0;
            foreach (var input in order.Items)
            {
                Dictionary<string, object> configSelected = null;
                foreach (var config in pointConfigs)
                {
                    if (!config.ContainsKey("forAllCustomer") || Convert.ToInt32(config["forAllCustomer"]) != 1)
                    {
                        if (!config.ContainsKey("contactId") || Convert.ToInt32(config["contactId"]) != order.ContactId)
                        {
                            continue;
                        }
                    }
                    // find by product
                    if (config.ContainsKey("productId") && Convert.ToInt32(config["productId"]) == input.ProductId)
                    {
                        configSelected = config;
                        break;
                    }
                    // exclude if not product
                    if (config.ContainsKey("productId") && Convert.ToInt32(config["productId"]) != 0)
                    {
                        continue;
                    }
                    // find by category
                    if (config.ContainsKey("categoryId") && productCategories.Any(p => Convert.ToInt32(p["tradeId"]) == input.ProductId && Convert.ToInt32(p["categoryId"]) == Convert.ToInt32(config["categoryId"])))
                    {
                        configSelected = config;
                        break;
                    }
                    // exclude if not category
                    if (config.ContainsKey("categoryId") && Convert.ToInt32(config["categoryId"]) != 0)
                    {
                        continue;
                    }
                    // find by contact
                    if (config.ContainsKey("contactId") && Convert.ToInt32(config["contactId"]) == order.ContactId)
                    {
                        configSelected = config;
                        break;
                    }
                    // exclude the other 
                    if (config.ContainsKey("contactId") && Convert.ToInt32(config["contactId"]) != 0)
                    {
                        continue;
                    }
                    configSelected = config;
                }
                if (configSelected == null)
                {
                    continue;
                }
                var amount = input.Total / Convert.ToDecimal(configSelected["exchange"]);
                totalAmount += amount;
                var t = _sqlService.SaveAsync(new Dictionary<string, object>() {
                    {"userId", order.UserId},
                    {"orderId", order.Id},
                    {"amount", amount},
                    {"pointConfigId", configSelected["id"]},
                    {"contactId", order.ContactId}, 
                    // {"createdAt", order.CreatedAt}, 
                    {"id", 0}},
                    "point_history",
                    "Id",
                    new List<string>() { "Id", "UserId" }, null);
                arr.Add(t);
                _cacheService.RemoveListEqualItem("point_history", order.UserId);
            }
            await _sqlService.UpdateAsync("contact",
                new Dictionary<string, object>() { { "id", order.ContactId }, { "userId", order.UserId }, { "point", totalAmount } },
                new QueryModelOnSearch()
                {
                    WhereFieldQuerys = new Dictionary<string, List<string>>() {
                        {"id", new List<string>{EnumSearchFunctions.EQUALS}},
                        {"userid", new List<string>{EnumSearchFunctions.EQUALS}}
                    },
                    UpdateFieldQuerys = new Dictionary<string, List<string>>() {
                        {"point", new List<string>{EnumUpdateFunctions.INCREASE}}
                    },
                }
            );
            var contact = await _sqlService.GetByIdAsync("contact", order.ContactId, order.UserId);
            if (contact != null)
            {
                var levelConfigs = await _sqlService.ListAsync("level_config",
                    new Dictionary<string, object>() {
                    {"userId", order.UserId},
                    {"point", contact["point"]},
                    {"buyCount", contact["buyCount"]},
                    },
                    new QueryModelOnSearch()
                    {
                        WhereFieldQuerys = new Dictionary<string, List<string>>() {
                            {"userId", new List<string>{EnumSearchFunctions.EQUALS}},
                            {"point", new List<string>{EnumSearchFunctions.SMALLER_OR_EQUALS_THAN}},
                            {"buyCount", new List<string>{EnumSearchFunctions.SMALLER_OR_EQUALS_THAN}},
                        }
                    }
                );
                if (levelConfigs != null && levelConfigs.Any())
                {
                    var maxLevelConfig = levelConfigs.FirstOrDefault();
                    foreach (var levelConfig in levelConfigs)
                    {
                        if (Convert.ToDecimal(levelConfig["point"]) > Convert.ToDecimal(maxLevelConfig["point"]))
                        {
                            maxLevelConfig = levelConfig;
                        }
                    }
                    if (maxLevelConfig != null)
                    {
                        var currentLevelConfig = Convert.ToInt32(contact["levelId"]) != 0
                            ? await _sqlService.GetByIdAsync("level_config", Convert.ToInt32(contact["levelId"]), order.UserId)
                            : null;
                        var oldMaxPoint = currentLevelConfig != null ? Convert.ToInt32(currentLevelConfig["point"]) : 0;
                        if (Convert.ToInt32(maxLevelConfig["point"]) > oldMaxPoint)
                        {
                            await _sqlService.SaveAsync(
                                new Dictionary<string, object>() { { "id", order.ContactId }, { "userId", order.UserId }, { "levelId", maxLevelConfig["id"] } },
                                    "contact",
                                    "Id",
                                    new List<string>() { "Id", "UserId" },
                                null
                            );
                        }
                    }
                }
            }
            _cacheService.RemoveListEqualItem("contact", order.UserId);
            await Task.WhenAll(arr.ToArray());
        }

        private async Task RemovePointAsync(Order oldOrder)
        {
            if (oldOrder.ContactId <= 0)
            {
                return;
            }
            if (oldOrder.AmountFromPoint > 0
                && oldOrder.PointAmount > 0)
            {
                await _sqlService.UpdateAsync("contact",
                    new Dictionary<string, object>() { { "id", oldOrder.ContactId }, { "userId", oldOrder.UserId }, { "point", oldOrder.PointAmount } },
                    new QueryModelOnSearch()
                    {
                        WhereFieldQuerys = new Dictionary<string, List<string>>() {
                                {"id", new List<string>{EnumSearchFunctions.EQUALS}},
                                {"userid", new List<string>{EnumSearchFunctions.EQUALS}}
                        },
                        UpdateFieldQuerys = new Dictionary<string, List<string>>() {
                                {"point", new List<string>{EnumUpdateFunctions.INCREASE}}
                        },
                    }
                );
                _cacheService.RemoveListEqualItem("contact", oldOrder.UserId);
                return;
            }
            var pointHistories = await _sqlService.ListAsync("point_history", new Dictionary<string, object> { { "orderId", oldOrder.Id }, { "userId", oldOrder.UserId } }, null);
            if (pointHistories == null || !pointHistories.Any())
            {
                return;
            }
            var totalPoint = pointHistories.Sum((p) => Convert.ToDecimal(p["amount"]));
            var arr = new List<Task>();
            arr.Add(_sqlService.UpdateAsync("contact",
                new Dictionary<string, object>() { { "id", oldOrder.ContactId }, { "userId", oldOrder.UserId }, { "point", totalPoint } },
                new QueryModelOnSearch()
                {
                    WhereFieldQuerys = new Dictionary<string, List<string>>() {
                        {"id", new List<string>{EnumSearchFunctions.EQUALS}},
                        {"userid", new List<string>{EnumSearchFunctions.EQUALS}}
                    },
                    UpdateFieldQuerys = new Dictionary<string, List<string>>() {
                        {"point", new List<string>{EnumUpdateFunctions.DECREASE}}
                    },
                }
            ));
            arr.Add(_sqlService.RemoveAsync("point_history", new Dictionary<string, object> { { "orderId", oldOrder.Id }, { "userId", oldOrder.UserId } }));
            _cacheService.RemoveListEqualItem("point_history", oldOrder.UserId);
            _cacheService.RemoveListEqualItem("contact", oldOrder.UserId);
            await Task.WhenAll(arr);
        }

        private async Task AddProductNotesAsync(Order order, string lang)
        {
            if (order.Id == 0)
            {
                return;
            }
            var dic = new Dictionary<int, bool>();
            var notesToDelete = await _productService.GetNotes(order.UserId, null, null, 0, 0, order.Id, 0, 0, 0, 0, 0);
            var arr = new List<Task>();
            foreach (var note in notesToDelete)
            {
                if (dic.ContainsKey(note.ProductId))
                {
                    continue;
                }
                dic[note.ProductId] = true;
                arr.Add(DeleteProductNoteFromArrAsync(notesToDelete, note));
            }
            if (arr.Any())
            {
                await Task.WhenAll(arr.ToArray());
            }
            arr = new List<Task>();
            dic = new Dictionary<int, bool>();
            if ((order.Items == null || !order.Items.Any()) && !string.IsNullOrEmpty(order.ItemsJson))
            {
                order.Items = JsonConvert.DeserializeObject<List<OrderItem>>(order.ItemsJson);
            }
            foreach (var input in order.Items)
            {
                if (dic.ContainsKey(input.ProductId))
                {
                    continue;
                }
                dic[input.ProductId] = true;
                arr.Add(SaveProductNoteFromArrAsync(order.Items, input, order, lang));
            }
            if (arr.Any())
            {
                await Task.WhenAll(arr.ToArray());
            }
        }

        private async Task SaveProductNoteFromArrAsync(IEnumerable<OrderItem> items, OrderItem input, Order order, string lang)
        {
            foreach (var item in items)
            {
                if (item.ProductId != input.ProductId)
                {
                    continue;
                }
                await SaveNoteForItemAsync(item, order, lang);
            }
        }

        private string GetOrderStringByLang(string lang)
        {
            return !string.IsNullOrEmpty(lang) && lang.ToUpper() == "VN" ? "Đơn hàng" : "Order";
        }

        private async Task SaveNoteForItemAsync(OrderItem item, Order order, string lang)
        {
            var arr = new List<Task>();
            if (item.IsCombo.HasValue && item.IsCombo.Value && item.Items != null && item.Items.Any())
            {
                foreach (var subItem in item.Items)
                {
                    IEnumerable<OrderSubItem> materialsSubItem = !string.IsNullOrEmpty(subItem.MaterialsJson)
                        ? JsonConvert.DeserializeObject<List<OrderSubItem>>(subItem.MaterialsJson)
                        : subItem.Materials;
                    if (materialsSubItem != null && materialsSubItem.Any())
                    {
                        foreach (var material in materialsSubItem)
                        {
                            var materialNote = NewProductNote(order, lang, 0, material.Price.HasValue ? material.Price.Value : 0, material.Unit, null, null, subItem.Count * material.Count, material.ProductId, !string.IsNullOrEmpty(material.ProductCode) ? material.ProductCode.ToUpper() : string.Empty, material.ProductName);
                            arr.Add(_productService.SaveProductNote(materialNote));
                        }
                    }
                    else
                    {
                        var subNote = NewProductNote(order, lang, 0, subItem.Price.HasValue ? subItem.Price.Value : 0, subItem.Unit, subItem.UnitExchange, subItem.BasicUnit, subItem.Count, subItem.ProductId, !string.IsNullOrEmpty(subItem.ProductCode) ? subItem.ProductCode.ToUpper() : string.Empty, subItem.ProductName);
                        arr.Add(_productService.SaveProductNote(subNote));
                    }
                    if (subItem.Options != null && subItem.Options.Any())
                    {
                        foreach (var topping in subItem.Options)
                        {
                            IEnumerable<OrderSubItem> materials = !string.IsNullOrEmpty(topping.MaterialsJson)
                                ? JsonConvert.DeserializeObject<List<OrderSubItem>>(topping.MaterialsJson)
                                : topping.Materials;
                            if (materials != null && materials.Any())
                            {
                                foreach (var material in materials)
                                {
                                    var materialNote = NewProductNote(order, lang, 0, material.Price.HasValue ? material.Price.Value : 0, material.Unit, null, null, topping.Count * material.Count, material.ProductId, !string.IsNullOrEmpty(material.ProductCode) ? material.ProductCode.ToUpper() : string.Empty, material.ProductName);
                                    arr.Add(_productService.SaveProductNote(materialNote));
                                }
                            }
                            else
                            {
                                var subObtionNote = NewProductNote(order, lang, 0, topping.Price.HasValue ? topping.Price.Value : 0, topping.Unit, topping.UnitExchange, topping.BasicUnit, topping.Count, topping.Id, !string.IsNullOrEmpty(topping.Code) ? topping.Code.ToUpper() : string.Empty, topping.Title);
                                arr.Add(_productService.SaveProductNote(subObtionNote));
                            }
                        }
                    }
                }
                if (arr.Any())
                {
                    await Task.WhenAll(arr.ToArray());
                }
                return;
            }
            if (item.Materials != null && item.Materials.Any())
            {
                foreach (var material in item.Materials)
                {
                    var materialNote = NewProductNote(order, lang, 0, material.Price.HasValue ? material.Price.Value : 0, material.Unit, null, null, (item.Count.HasValue ? item.Count.Value : 0) * material.Count, material.ProductId, !string.IsNullOrEmpty(material.ProductCode) ? material.ProductCode.ToUpper() : string.Empty, material.ProductName);
                    arr.Add(_productService.SaveProductNote(materialNote));
                }
            }
            else
            {
                var productNote = NewProductNote(order, lang, item.Total.HasValue ? item.Total.Value : 0, item.Price.HasValue ? item.Price.Value : 0, item.Unit, item.UnitExchange, item.BasicUnit, item.Count.HasValue ? item.Count.Value : 0, item.ProductId, !string.IsNullOrEmpty(item.ProductCode) ? item.ProductCode.ToUpper() : string.Empty, item.ProductName, item.Discount.HasValue ? item.Discount.Value : 0, item.DiscountType);
                arr.Add(_productService.SaveProductNote(productNote));
            }
            if (item.Options != null && item.Options.Any())
            {
                foreach (var topping in item.Options)
                {
                    IEnumerable<OrderSubItem> materials = !string.IsNullOrEmpty(topping.MaterialsJson)
                        ? JsonConvert.DeserializeObject<List<OrderSubItem>>(topping.MaterialsJson)
                        : topping.Materials;
                    if (materials != null && materials.Any())
                    {
                        foreach (var material in materials)
                        {
                            var materialNote = NewProductNote(order, lang, 0, material.Price.HasValue ? material.Price.Value : 0, material.Unit, null, null, topping.Count * material.Count, material.ProductId, !string.IsNullOrEmpty(material.ProductCode) ? material.ProductCode.ToUpper() : string.Empty, material.ProductName);
                            arr.Add(_productService.SaveProductNote(materialNote));
                        }
                    }
                    else
                    {
                        var subObtionNote = NewProductNote(order, lang, 0, topping.Price.HasValue ? topping.Price.Value : 0, topping.Unit, topping.UnitExchange, topping.BasicUnit, topping.Count, topping.Id, !string.IsNullOrEmpty(topping.Code) ? topping.Code.ToUpper() : string.Empty, topping.Title);
                        arr.Add(_productService.SaveProductNote(subObtionNote));
                    }
                }
            }
            if (arr.Any())
            {
                await Task.WhenAll(arr.ToArray());
            }
        }

        private ProductNote NewProductNote(Order order, string lang, decimal amount, decimal price, string unit, decimal? unitExchange, string basicUnit, decimal quantity, int productId, string productCode, string productName, decimal discount = 0, int discountType = 0)
        {
            var note = new ProductNote();
            note.OrderId = order.Id;
            note.ContactId = order.ContactId;
            note.Amount = amount;
            note.UnitPrice = price;
            note.Unit = unit;
            note.UnitExchange = unitExchange;
            note.BasicUnit = basicUnit;
            note.Quantity = quantity;
            note.ProductId = productId;
            note.ProductCode = !string.IsNullOrEmpty(productCode) ? productCode.ToUpper() : string.Empty;
            note.ProductName = productName;
            note.Discount = discount;
            note.DiscountType = discountType;
            note.StoreId = order.StoreId;
            note.Note = GetOrderStringByLang(lang) + " #" + order.OrderCode;
            note.UserId = order.UserId;
            return note;
        }

        private async Task DeleteProductNoteFromArrAsync(IEnumerable<ProductNote> notesToDelete, ProductNote note)
        {
            foreach (var item in notesToDelete)
            {
                if (item.ProductId != note.ProductId)
                {
                    continue;
                }
                await _productService.RemoveProductNote(note.Id, note.UserId);
            }
        }

        private async Task SendOrderMessageToShopOwnerAsync(String userId, Staff staff, Order order)
        {
            var tokenModel = new Dictionary<string, object>();
            tokenModel["userId"] = userId;

            var tokens = await _sqlService.ListAsync("fcmtoken", tokenModel, null);
            if (tokens != null && tokens.Any())
            {
                foreach (var token in tokens)
                {
                    var message = new Message()
                    {
                        Token = token["token"].ToString(),
                        Notification = new Notification
                        {
                            Title = "Đơn hàng mới",
                            Body = staff.Name + " vừa tạo mới đơn hàng #" + order.OrderCode + ". Tổng thanh toán: " + order.Total.ToString() + "."
                        },
                        Data = new Dictionary<string, string>(){
                        {"notification_foreground", "true"},
                        {"notification_title", "Đơn hàng mới"},
                        {"notification_body", staff.Name + " vừa tạo mới đơn hàng #" + order.OrderCode + ". Tổng thanh toán: " + order.Total.ToString() + "."},
                        {"page", "order"},
                        {"id", order.Id.ToString()},
                    }
                    };

                    var messaging = FirebaseMessaging.DefaultInstance;
                    var sendResult = await messaging.SendAsync(message);
                }
            }
        }

        private Dictionary<string, object> CloneAndAddToDic(Dictionary<string, object> original, string key, object value)
        {
            var newDic = new Dictionary<string, object>(original);
            newDic[key] = value;
            return newDic;
        }
    }
}
