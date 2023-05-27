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
    public class ProductController : ControllerBase
    {
        private readonly IChannelQueueService<UserActivity> _queueMessage;
        private readonly ICacheService _cacheService;
        private readonly IProductService _productService;
        private readonly IStaffService _staffService;
        private readonly ISqlService _sqlService;


        public ProductController(
            IChannelQueueService<UserActivity> queueMessage,
            ICacheService cacheService,
            ISqlService sqlService,
            IProductService productService,
            IStaffService staffService
        )
        {
            _queueMessage = queueMessage;
            _cacheService = cacheService;
            _sqlService = sqlService;
            _productService = productService;
            _staffService = staffService;
        }

        // GET /product/list
        [HttpGet]
        [Route("List")]
        public async Task<IEnumerable<Product>> List(int? staffId, int? storeId, bool? isMaterial, int? categoryId)
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
                Feature = "product",
                Action = "list",
                Note = "",
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-product",
                    Action = "list",
                    Note = "",
                });
            }
            var key = (storeId.HasValue ? storeId.Value : 0) + "-" + (isMaterial.HasValue ? isMaterial.Value : false) + "-" + (categoryId.HasValue ? categoryId.Value : 0) + "-" + (staffId.HasValue ? staffId.Value : 0);
            var cacheList = (CacheList<Product>)_cacheService.GetCacheList("product");
            var productsCached = cacheList.GetList(userId, key);
            if (productsCached != null)
            {
                return productsCached;
            }
            var products = await _productService.GetProducts(userId, storeId.HasValue ? storeId.Value : 0, isMaterial.HasValue ? isMaterial.Value : false, categoryId.HasValue ? categoryId.Value : 0);
            cacheList.SetList(products, userId, key);
            return products;
        }

        // GET /product/ListExpiries
        [HttpGet]
        [Route("ListExpiries")]
        public async Task<IEnumerable<Product>> ListExpiries(int? staffId, int? storeId, bool? isMaterial, int? categoryId)
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
                Feature = "product",
                Action = "list-expires",
                Note = "expires",
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-product",
                    Action = "list-expires",
                    Note = "expires",
                });
            }
            var post = await _productService.GetProductsWithExpiry(userId, storeId.HasValue ? storeId.Value : 0, isMaterial.HasValue ? isMaterial.Value : false, categoryId.HasValue ? categoryId.Value : 0);
            return post;
        }

        // GET /product/ListExpiries
        [HttpGet]
        [Route("ListQuantities")]
        public async Task<IEnumerable<Product>> ListQuantities(int? staffId, int? storeId, bool? isMaterial, int? categoryId)
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
                Feature = "product",
                Action = "list-quantity",
                Note = "quantity",
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-product",
                    Action = "list-quantity",
                    Note = "",
                });
            }
            var post = await _productService.GetProductsWithQuantity(userId, storeId.HasValue ? storeId.Value : 0, isMaterial.HasValue ? isMaterial.Value : false, categoryId.HasValue ? categoryId.Value : 0);
            return post;
        }

        // GET /product
        [HttpGet]
        [Route("Barcode")]
        public async Task<ActionResult<object>> Barcode(string barcode, int? staffId)
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
                Feature = "product",
                Action = "barcode",
                Note = "",
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-product",
                    Action = "barcode",
                    Note = "",
                });
            }
            var post = await _productService.SearchProductByBarcode(barcode, userId);
            if (post != null)
            {
                return post;
            }
            Dictionary<string, object> model = new Dictionary<string, object>();
            model["userId"] = userId;
            model["barcode"] = barcode;
            var productBarcode = await _sqlService.GetAsync("product_barcode", model, null);
            if (productBarcode != null)
            {
                var productObject = productBarcode["product"];
                var product = productObject != null ? productObject as Dictionary<string, object> : null;
                product["fromUnit"] = productBarcode["unit"];
                product["barcode"] = barcode;
                return product;
            }
            return null;
        }

        [HttpGet]
        public async Task<ActionResult<Product>> Get(int id, int? storeId, int? staffId)
        {
            var userId = User.GetUserId();
            var isShopOwner = true;
            Staff staff = null;
            if (staffId.HasValue && staffId.Value != 0)
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
                Feature = "product",
                Action = "detail",
                Note = id.ToString(),
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-product",
                    Action = "detail",
                    Note = "",
                });
            }

            Product ret = null;
            var cacheItem = (CacheItem<Product>)_cacheService.GetCacheItem("product");
            if (cacheItem.Contains(userId, id))
            {
                ret = cacheItem.GetItem(userId, id);
            }
            else
            {
                ret = await _productService.GetProduct(id, userId, storeId.HasValue ? storeId.Value : 0);
                cacheItem.SetItem(userId, id, ret);
            }
            return ret;
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
                    return false;
                }
                var userName = User.GetEmail();
                isShopOwner = userId == staff.UserId;
                if (userName != staff.UserName && !isShopOwner)
                {
                    return false;
                }
                userId = staff.UserId;
            }
            if (staff != null && !staff.HasFullAccess && !isShopOwner)
            {
                if (!staff.CanUpdateDeleteProduct && !staff.CanCreateUpdateNote)
                {
                    return null;
                }
            }
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = "product",
                Action = "delete",
                Note = id.ToString(),
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-product",
                    Action = "delete",
                    Note = "",
                });
            }
            var post = await _productService.RemoveProduct(id, userId);
            var cacheItem = (CacheItem<Product>)_cacheService.GetCacheItem("product");
            cacheItem.RemoveItem(userId, id.ToString());
            var cacheList = (CacheList<Product>)_cacheService.GetCacheList("product");
            cacheList.ClearAllLists(userId);
            return post;
        }

        // POST product/SaveProduct
        [HttpPost]
        [Route("SaveProduct")]
        public async Task<ActionResult<int>> SaveProduct(SaveProductViewModel productViewModel)
        {
            if (productViewModel == null)
            {
                return 0;
            }
            var userId = User.GetUserId();
            Staff staff = null;
            var isShopOwner = true;
            if (productViewModel.StaffId.HasValue && productViewModel.StaffId.Value > 0)
            {
                staff = await _staffService.GetByIdOnly(productViewModel.StaffId.Value);
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
                if (!staff.CanUpdateDeleteProduct && !staff.CanCreateUpdateNote)
                {
                    return null;
                }
            }
            var product = new Product();
            product.Id = productViewModel.Id;
            product.UserId = userId;
            product.Title = productViewModel.Title;
            product.Code = productViewModel.Code;
            product.Barcode = productViewModel.Barcode;
            product.Count = productViewModel.Count.HasValue && productViewModel.Count.Value > 0 ? productViewModel.Count.Value : 0;
            product.IsSale = productViewModel.IsSale;
            product.OriginalPrice = productViewModel.OriginalPrice.HasValue ? productViewModel.OriginalPrice.Value : 0;
            product.Price = productViewModel.Price.HasValue ? productViewModel.Price.Value : 0;
            product.ImageUrlsJson = productViewModel.ImageUrlsJson;
            product.Unit = productViewModel.Unit;
            product.UnitsJson = productViewModel.UnitsJson;
            product.AvatarUrl = productViewModel.AvatarUrl;
            product.CostPrice = productViewModel.CostPrice;
            product.CostPriceForeign = productViewModel.CostPriceForeign;
            product.ForeignCurrency = productViewModel.ForeignCurrency;
            product.IsOption = productViewModel.IsOption;
            product.IsCombo = productViewModel.IsCombo;
            product.IsPublic = productViewModel.IsPublic;
            product.IsService = productViewModel.IsService;
            product.Status = productViewModel.Status.HasValue ? productViewModel.Status.Value : 0;
            product.ItemsJson = productViewModel.ItemsJson;
            product.MaterialsJson = productViewModel.MaterialsJson;
            product.ShowOnWeb = productViewModel.ShowOnWeb.HasValue ? productViewModel.ShowOnWeb.Value : false;
            product.ShowPriceOnWeb = productViewModel.ShowPriceOnWeb.HasValue ? productViewModel.ShowPriceOnWeb.Value : false;
            product.IsHotProduct = productViewModel.IsHotProduct.HasValue ? productViewModel.IsHotProduct.Value : false;
            product.IsNewProduct = productViewModel.IsNewProduct.HasValue ? productViewModel.IsNewProduct.Value : false;
            product.Description = productViewModel.Description;
            product.ExpiredAt = productViewModel.ExpiredAt;
            product.IsMaterial = productViewModel.IsMaterial.HasValue ? productViewModel.IsMaterial.Value : false;
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = "product",
                Action = "save",
                Note = product.Id.ToString(),
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-product",
                    Action = "save",
                    Note = "",
                });
            }

            var result = await _productService.SaveProduct(product, false);
            var cacheItem = (CacheItem<Product>)_cacheService.GetCacheItem("product");
            cacheItem.RemoveItem(userId, result.ToString());
            var cacheList = (CacheList<Product>)_cacheService.GetCacheList("product");
            cacheList.ClearAllLists(userId);
            return result;
        }

        // POST product/SaveProductNote
        [HttpPost]
        [Route("SaveProductNote")]
        public async Task<ActionResult<int>> SaveProductNote(SaveProductNoteViewModel model)
        {
            if (model == null)
            {
                return 0;
            }
            Staff staff = null;
            var userId = User.GetUserId();
            var isShopOwner = true;
            if (model.StaffId.HasValue && model.StaffId.Value != 0)
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
            var note = new ProductNote();
            note.Id = model.Id;
            note.UserId = userId;
            note.Amount = model.Amount;
            note.AmountForeign = model.AmountForeign;
            note.Discount = model.Discount;
            note.DiscountType = model.DiscountType;
            note.ForeignCurrency = model.ForeignCurrency;
            note.OrderId = model.OrderId.HasValue ? model.OrderId.Value : 0;
            note.TradeId = model.TradeId.HasValue ? model.TradeId.Value : 0;
            note.ContactId = model.ContactId.HasValue ? model.ContactId.Value : 0;
            note.ReceivedNoteId = model.ReceivedNoteId.HasValue ? model.ReceivedNoteId.Value : 0;
            note.TransferNoteId = model.TransferNoteId.HasValue ? model.TransferNoteId.Value : 0;
            note.ProductId = model.ProductId.HasValue ? model.ProductId.Value : 0;
            note.ProductCode = model.ProductCode;
            note.ProductName = model.ProductName;
            note.Quantity = model.Quantity.HasValue ? model.Quantity.Value : 0;
            note.Unit = model.Unit;
            note.UnitPrice = model.UnitPrice.HasValue ? model.UnitPrice.Value : 0;
            note.UnitPriceForeign = model.UnitPriceForeign;
            note.Note = model.Note;
            note.StoreId = model.StoreId.HasValue ? model.StoreId.Value : 0;
            note.BasicUnit = model.BasicUnit;
            note.UnitExchange = model.UnitExchange;
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = "product_note",
                Action = "save",
                Note = note.Id.ToString(),
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-product_note",
                    Action = "save",
                    Note = "",
                });
            }

            var result = await _productService.SaveProductNote(note);
            var cacheList = (CacheList<Product>)_cacheService.GetCacheList("product");
            cacheList.ClearAllLists(userId);
            var cacheItem = (CacheItem<Product>)_cacheService.GetCacheItem("product");
            cacheItem.RemoveItem(userId, note.ProductId.ToString());
            return result;
        }

        [HttpPost]
        [Route("RemoveProductNote")]
        public async Task<ActionResult<bool>> RemoveProductNote(int id, int? staffId)
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
            var oldNote = await _productService.GetProductNoteById(id, userId);
            var productId = oldNote != null ? oldNote.ProductId : 0;
            if (productId > 0)
            {
                var cacheProductItem = (CacheItem<Product>)_cacheService.GetCacheItem("product");
                cacheProductItem.RemoveItem(userId, productId.ToString());
                var cacheList = (CacheList<Product>)_cacheService.GetCacheList("product");
                cacheList.ClearAllLists(userId);
            }
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = "product_note",
                Action = "delete",
                Note = id.ToString(),
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-product_note",
                    Action = "delete",
                    Note = "",
                });
            }
            var post = await _productService.RemoveProductNote(id, userId);
            return post;
        }

        // GET /product/listnote
        [HttpPost]
        [Route("ListNote")]
        public async Task<IEnumerable<ProductNote>> ListNote(GetProductNotesViewModel model)
        {
            var userId = User.GetUserId();
            var isShopOwner = true;
            var hasFullAccess = false;
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
                    isShopOwner = userId == staff.UserId;
                }
                else
                {
                    var userName = User.GetEmail();
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
            var productId = model.ProductId.HasValue ? model.ProductId.Value : 0;
            var orderId = model.OrderId.HasValue ? model.OrderId.Value : 0;
            var tradeId = model.TradeId.HasValue ? model.TradeId.Value : 0;
            var receivedNoteId = model.ReceivedNoteId.HasValue ? model.ReceivedNoteId.Value : 0;
            var transferNoteId = model.TransferNoteId.HasValue ? model.TransferNoteId.Value : 0;
            var staffId = model.StaffId.HasValue && !hasFullAccess && !isShopOwner
                ? model.StaffId.Value
                : (isShopOwner && model.StaffId.HasValue ? model.StaffId.Value : 0);
            var storeId = model.StoreId.HasValue && !hasFullAccess
                ? model.StoreId.Value
                : (isShopOwner && model.StoreId.HasValue ? model.StoreId.Value : 0);
            await _queueMessage.WriteAsync(new UserActivity() {
                UserId = userId,
                Feature = "product_note",
                Action = "list",
                Note = "",
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-product_note",
                    Action = "list",
                    Note = "",
                });
            }
            var post = await _productService.GetNotes(userId, dateFrom, dateTo, contactId, productId, orderId, receivedNoteId, tradeId, storeId, transferNoteId, staffId);
            return post;
        }
    }


}
