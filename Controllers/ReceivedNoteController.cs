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
    public class ReceivedNoteController : ControllerBase
    {
        private readonly IChannelQueueService<UserActivity> _queueMessage;
        private readonly ICacheService _cacheService;
        private readonly IReceivedNoteService _service;
        private readonly IStaffService _staffService;
        private readonly IProductService _productService;
        private readonly ITradeService _tradeService;

        public ReceivedNoteController(
            IChannelQueueService<UserActivity> queueMessage,
            ICacheService cacheService,
            IReceivedNoteService service,
            IStaffService staffService,
            IProductService productService,
            ITradeService tradeService
        )
        {
            _queueMessage = queueMessage;
            _cacheService = cacheService;
            _service = service;
            _staffService = staffService;
            _productService = productService;
            _tradeService = tradeService;
        }

        // GET /receivednote/list
        [HttpPost]
        [Route("List")]
        public async Task<IEnumerable<ReceivedNote>> List(GetReceivedNotesViewModel model)
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
            var dateFrom = !string.IsNullOrEmpty(model.DateFrom)
                ? DateTime.ParseExact(model.DateFrom, new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" }, null)
                : (DateTime?)null;
            var dateTo = !string.IsNullOrEmpty(model.DateTo)
                ? DateTime.ParseExact(model.DateTo, new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" }, null)
                : (DateTime?)null;
            var contactId = model.ContactId.HasValue ? model.ContactId.Value : 0;
            var staffId = model.StaffId.HasValue && !hasFullAccess ? model.StaffId.Value : 0;
            var storeId = model.StoreId.HasValue && !hasFullAccess ? model.StoreId.Value : 0;
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = "received_note",
                Action = "list",
                Note = "",
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-received_note",
                    Action = "list",
                    Note = "",
                });
            }
            var post = await _service.List(userId, dateFrom, dateTo, contactId, staffId, storeId);
            return post;
        }

        [HttpGet]
        public async Task<ReceivedNote> Get(int id, int? staffId)
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
                Feature = "received_note",
                Action = "detail",
                Note = id.ToString(),
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-received_note",
                    Action = "detail",
                    Note = "",
                });
            }
            ReceivedNote ret = null;
            var cacheItem = (CacheItem<ReceivedNote>)_cacheService.GetCacheItem("receivedNote");
            if (cacheItem.Contains(userId, id)) {
                ret = cacheItem.GetItem(userId, id);
            } else {
                ret = await _service.Get(id, userId);
                cacheItem.SetItem(userId, id, ret);
            }
            if (staff != null && !staff.HasFullAccess && !isShopOwner && ret.StaffId != staff.Id)
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
                if (userName != staff.UserName && !isShopOwner)
                {
                    return null;
                }
                userId = staff.UserId;
            }
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = "received_note",
                Action = "delete",
                Note = id.ToString(),
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-received_note",
                    Action = "delete",
                    Note = "",
                });
            }
            var oldOrder = await _service.Get(id, userId);
            if (staff != null && !staff.HasFullAccess && !isShopOwner)
            {
                var oldOrderDate = oldOrder.CreatedAt.HasValue ? oldOrder.CreatedAt.Value : DateTime.Now;
                if (oldOrderDate.AddHours(24) < DateTime.Now && !staff.CanCreateUpdateNote)
                {
                    return null;
                }
                if (!staff.CanCreateUpdateNote)
                {
                    return null;
                }
            }
            var post = await _service.Remove(id, userId);
            var cacheItem = (CacheItem<ReceivedNote>)_cacheService.GetCacheItem("receivedNote");
            cacheItem.RemoveItem(userId, id.ToString());
            _cacheService.RemoveListEqualItem("product", userId);
            var cacheProductItem = (CacheItem<Product>)_cacheService.GetCacheItem("product");
            var items = JsonConvert.DeserializeObject<IEnumerable<ReceivedNoteItem>>(oldOrder.ItemsJson);
            if (items != null && items.Any()) {
                cacheProductItem.RemoveItems(userId, items.Select(i => i.ProductId.ToString()).ToList());
            }
            var cacheProductList = (CacheList<Product>)_cacheService.GetCacheList("product");
            cacheProductList.ClearAllLists(userId);
            return post;
        }

        [HttpPost]
        [Route("Save")]
        public async Task<ActionResult<int>> Save(SaveReceivedNoteViewModel model)
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
                    var oldOrder = await _service.Get(model.Id, userId);
                    var oldOrderDate = oldOrder.CreatedAt.HasValue ? oldOrder.CreatedAt.Value : DateTime.Now;
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

            var note = new ReceivedNote();
            note.Id = model.Id;
            note.UserId = userId;
            note.CreatedAt = model.CreatedAt;
            note.ContactId = model.ContactId.HasValue ? model.ContactId.Value : 0;
            note.StaffId = model.StaffId.HasValue ? model.StaffId.Value : 0;
            note.MoneyAccountId = model.MoneyAccountId.HasValue ? model.MoneyAccountId.Value : 0;
            note.ContactName = model.ContactName;
            note.ContactPhone = model.ContactPhone;
            note.ItemsJson = model.ItemsJson;
            note.NetValue = model.NetValue.HasValue ? model.NetValue.Value : 0;
            note.Tax = model.Tax.HasValue ? model.Tax.Value : 0;
            note.TaxType = model.TaxType.HasValue ? model.TaxType.Value : 0;
            note.Total = model.Total.HasValue ? model.Total.Value : 0;
            note.Paid = model.Paid;
            note.ShippingFee = model.ShippingFee.HasValue ? model.ShippingFee.Value : 0;
            note.Discount = model.Discount.HasValue ? model.Discount.Value : 0;
            note.DiscountOnTotal = model.DiscountOnTotal.HasValue ? model.DiscountOnTotal.Value : 0;
            note.TotalForeign = model.TotalForeign;
            note.DeliveryPerson = model.DeliveryPerson;
            note.ForeignCurrency = model.ForeignCurrency;
            note.Receiver = model.Receiver;
            note.StoreId = model.StoreId.HasValue ? model.StoreId.Value : 0;
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = "received_note",
                Action = "save",
                Note = note.Id.ToString(),
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-received_note",
                    Action = "save",
                    Note = "",
                });
            }

            var result = await _service.Save(note);
            var cacheItem = (CacheItem<ReceivedNote>)_cacheService.GetCacheItem("receivedNote");
            cacheItem.RemoveItem(userId, result.ToString());
            _cacheService.RemoveListEqualItem("product", userId);
            var cacheProductItem = (CacheItem<Product>)_cacheService.GetCacheItem("product");
            var items = JsonConvert.DeserializeObject<IEnumerable<ReceivedNoteItem>>(note.ItemsJson);
            if (items != null && items.Any()) {
                cacheProductItem.RemoveItems(userId, items.Select(i => i.ProductId.ToString()).ToList());
            }
            var cacheProductList = (CacheList<Product>)_cacheService.GetCacheList("product");
            cacheProductList.ClearAllLists(userId);
            note.Id = result;
            if (model.SaveProductNotes.HasValue && model.SaveProductNotes.Value)
            {
                await CreateTransactionsAsync(note, model.Lang);
                await AddProductNotesAsync(note, model.Lang);
            }
            return result;
        }

        private async Task CreateTransactionsAsync(ReceivedNote note, string lang)
        {
            if (note.Paid.HasValue && note.Paid.Value == 0)
            {
                return;
            }
            if (!note.Paid.HasValue)
            {
                return;
            }

            var arr = new List<Task>();
            var trades = await _tradeService.GetTrades(note.UserId, null, null, 0, 0, 0, 0, 0, 0, note.Id, 0, -1);
            foreach (var trade in trades)
            {
                if (trade.DebtId != 0)
                {
                    continue;
                }
                arr.Add(_tradeService.RemoveTrade(trade.Id, trade.UserId, true));
            }
            var newTrade = new Trade();
            newTrade.ContactId = note.ContactId;
            newTrade.StaffId = note.StaffId;
            newTrade.IsPurchase = false;
            newTrade.IsReceived = false;
            newTrade.Value = note.Paid.HasValue ? note.Paid.Value : note.Total;
            newTrade.ReceivedNoteId = note.Id;
            newTrade.MoneyAccountId = note.MoneyAccountId;
            newTrade.Note = GetOrderStringByLang(lang) + " #" + note.Id;
            newTrade.CreatedAt = note.CreatedAt.HasValue ? note.CreatedAt.Value : DateTime.Now;
            newTrade.UserId = note.UserId;
            arr.Add(_tradeService.SaveTrade(newTrade));
            await Task.WhenAll(arr.ToArray());
        }

        private async Task AddProductNotesAsync(ReceivedNote receivedNote, string lang)
        {
            if (receivedNote.Id == 0)
            {
                return;
            }
            var dic = new Dictionary<int, bool>();
            var notesToDelete = await _productService.GetNotes(receivedNote.UserId, null, null, 0, 0, 0, receivedNote.Id, 0, 0, 0, 0);
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
            if ((receivedNote.Items == null || !receivedNote.Items.Any()) && !string.IsNullOrEmpty(receivedNote.ItemsJson))
            {
                receivedNote.Items = JsonConvert.DeserializeObject<List<ReceivedNoteItem>>(receivedNote.ItemsJson);
            }
            foreach (var input in receivedNote.Items)
            {
                if (dic.ContainsKey(input.ProductId))
                {
                    continue;
                }
                dic[input.ProductId] = true;
                arr.Add(SaveProductNoteFromArrAsync(receivedNote.Items, input, receivedNote, lang));
            }
            if (arr.Any())
            {
                await Task.WhenAll(arr.ToArray());
            }
        }

        private async Task SaveProductNoteFromArrAsync(IEnumerable<ReceivedNoteItem> items, ReceivedNoteItem input, ReceivedNote receivedNote, string lang)
        {
            foreach (var item in items)
            {
                if (item.ProductId != input.ProductId)
                {
                    continue;
                }
                await SaveNoteForItemAsync(item, receivedNote, lang);
            }
        }

        private string GetOrderStringByLang(string lang)
        {
            return !string.IsNullOrEmpty(lang) && lang.ToUpper() == "VN" ? "Phiếu nhập" : "Received Note";
        }

        private async Task SaveNoteForItemAsync(ReceivedNoteItem item, ReceivedNote receivedNote, string lang)
        {
            var arr = new List<Task>();
            var productNote = NewProductNote(receivedNote, lang, item.Amount, item.CostPrice.HasValue ? item.CostPrice.Value : 0, item.Unit, item.UnitExchange, item.BasicUnit, item.Quantity.HasValue ? item.Quantity.Value : 0, item.ProductId, !string.IsNullOrEmpty(item.ProductCode) ? item.ProductCode.ToUpper() : string.Empty, item.ProductName, item.Discount, item.DiscountType);
            productNote.Note = !string.IsNullOrEmpty(item.Note)
                ? item.Note
                : GetOrderStringByLang(lang) + " #" + receivedNote.Id;
            productNote.AmountForeign = item.AmountForeign;
            productNote.UnitPriceForeign = item.UnitPriceForeign;
            productNote.ForeignCurrency = item.ForeignCurrency;
            arr.Add(_productService.SaveProductNote(productNote));
            if (arr.Any())
            {
                await Task.WhenAll(arr.ToArray());
            }
        }

        private ProductNote NewProductNote(ReceivedNote receivedNote, string lang, decimal amount, decimal price, string unit, decimal? unitExchange, string basicUnit, decimal quantity, int productId, string productCode, string productName, decimal discount = 0, int discountType = 0)
        {
            var note = new ProductNote();
            note.ReceivedNoteId = receivedNote.Id;
            note.ContactId = receivedNote.ContactId;
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
            note.StoreId = receivedNote.StoreId;
            note.Note = GetOrderStringByLang(lang) + " #" + receivedNote.Id;
            note.UserId = receivedNote.UserId;
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
                await _productService.RemoveProductNote(note.Id, note.UserId, true);
            }
        }
    }
}
