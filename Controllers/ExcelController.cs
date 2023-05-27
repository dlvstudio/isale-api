using System;
using System.IO;
using System.Threading.Tasks;
using HtmlAgilityPack;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PdfSharpCore;

namespace atakafe_api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    [Authorize]
    public class ExcelController : ControllerBase
    {
        private readonly IChannelQueueService<UserActivity> _queueMessage;
        private readonly ICacheService _cacheService;
        private readonly IExcelService _excelService;
        private readonly IStaffService _staffService;


        public ExcelController(
            IChannelQueueService<UserActivity> queueMessage,
            ICacheService cacheService,
            IExcelService excelService,
            IStaffService staffService
        )
        {
            _queueMessage = queueMessage;
            _cacheService = cacheService;
            _excelService = excelService;
            _staffService = staffService;
        }

        [HttpPost]
        [Route("Download")]
        public async Task<ActionResult<string>> Download(GetOrdersViewModel model)
        {
            var userId = User.GetUserId();
            var staffId = model.StaffId;
            if (staffId.HasValue && staffId.Value > 0)
            {
                var staff = await _staffService.GetByIdOnly(staffId.Value);
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
            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "excel",
                Action = "download",
                Note = "",
            });
            var dateFrom = !string.IsNullOrEmpty(model.DateFrom)
                ? DateTime.ParseExact(model.DateFrom, new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" }, null)
                : (DateTime?)null;
            var dateTo = !string.IsNullOrEmpty(model.DateTo)
                ? DateTime.ParseExact(model.DateTo, new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" }, null)
                : (DateTime?)null;
            return await _excelService.CreateOrderReportFile(model.ReportType.HasValue ? model.ReportType.Value : 0, model.Lang, userId, dateFrom, dateTo);
        }

        //CreateReceivedNoteFile
        [HttpPost]
        [Route("CreateReceivedNoteFile")]
        public async Task<ActionResult<string>> CreateReceivedNoteFile(GetOrdersViewModel model)
        {
            if (!model.ProductId.HasValue)
            {
                return null;
            }
            var userId = User.GetUserId();
            var staffId = model.StaffId;
            if (staffId.HasValue && staffId.Value > 0)
            {
                var staff = await _staffService.GetByIdOnly(staffId.Value);
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
            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "excel",
                Action = "CreateReceivedNoteFile",
                Note = "",
            });
            return await _excelService.CreateReceivedNoteFile(model.Lang, userId, model.ProductId.Value);
        }

        [HttpPost]
        [Route("DownloadSalesReportExcel")]
        public async Task<ActionResult<string>> DownloadSalesReportExcel(GetOrdersViewModel model)
        {
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
                isOwner = staff.UserId == userId;
                var userName = User.GetEmail();
                if (userName != staff.UserName && !isOwner)
                {
                    return null;
                }
                userId = staff.UserId;
            }
            var staffId = model.StaffId.HasValue && !hasFullAccess
                ? model.StaffId.Value
                : (isOwner && model.StaffId.HasValue ? model.StaffId.Value : 0);
            var storeId = model.StoreId.HasValue && !hasFullAccess
                ? model.StoreId.Value
                : (isOwner && model.StoreId.HasValue ? model.StoreId.Value : 0);
            var dateFrom = !string.IsNullOrEmpty(model.DateFrom)
                ? DateTime.ParseExact(model.DateFrom, new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" }, null)
                : (DateTime?)null;
            var dateTo = !string.IsNullOrEmpty(model.DateTo)
                ? DateTime.ParseExact(model.DateTo, new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" }, null)
                : (DateTime?)null;
            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "excel",
                Action = "DownloadSalesReportExcel",
                Note = "",
            });
            return await _excelService.CreateSalesReportFile(model.OrderIds, model.ReportType.HasValue ? model.ReportType.Value : 0, model.Lang, userId, dateFrom, dateTo, storeId, staffId);
        }

        [HttpPost]
        [Route("SalesReport")]
        public async Task<ActionResult<OrderReportSummaryViewModel>> SalesReport(GetOrdersViewModel model)
        {
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
                isOwner = staff.UserId == userId;
                var userName = User.GetEmail();
                if (userName != staff.UserName && !isOwner)
                {
                    return null;
                }
                userId = staff.UserId;
            }
            var staffId = model.StaffId.HasValue && !hasFullAccess
                ? model.StaffId.Value
                : (isOwner && model.StaffId.HasValue ? model.StaffId.Value : 0);
            var storeId = model.StoreId.HasValue && !hasFullAccess
                ? model.StoreId.Value
                : (isOwner && model.StoreId.HasValue ? model.StoreId.Value : 0);
            var reportType = model.ReportType.HasValue ? model.ReportType.Value : 0;
            var dateFrom = !string.IsNullOrEmpty(model.DateFrom)
                ? DateTime.ParseExact(model.DateFrom, new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" }, null)
                : (DateTime?)null;
            var dateTo = !string.IsNullOrEmpty(model.DateTo)
                ? DateTime.ParseExact(model.DateTo, new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" }, null)
                : (DateTime?)null;
            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "excel",
                Action = "SalesReport",
                Note = "",
            });
            if (reportType == 0)
            {
                return await _excelService.BuildSalesReportByOrder(model.OrderIds, model.Lang, userId, dateFrom, dateTo, storeId, staffId);
            }
            if (reportType == 1)
            {
                return await _excelService.BuildSalesReportByProduct(model.OrderIds, model.Lang, userId, dateFrom, dateTo, storeId, staffId);
            }
            if (reportType == 2)
            {
                return await _excelService.BuildSalesReportByCustomerAndProduct(model.OrderIds, model.Lang, userId, dateFrom, dateTo, storeId, staffId);
            }
            if (reportType == 3)
            {
                return await _excelService.BuildSalesReportByStaff(model.OrderIds, model.Lang, userId, dateFrom, dateTo, storeId, staffId);
            }
            return null;
        }

        [HttpPost]
        [Route("ProductReport")]
        public async Task<ActionResult<object>> ProductReport(GetOrdersViewModel model)
        {
            var userId = User.GetUserId();
            var staffId = model.StaffId;
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
            var reportType = model.ReportType.HasValue ? model.ReportType.Value : 0;
            var dateFrom = !string.IsNullOrEmpty(model.DateFrom)
                ? DateTime.ParseExact(model.DateFrom, new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" }, null)
                : (DateTime?)null;
            var dateTo = !string.IsNullOrEmpty(model.DateTo)
                ? DateTime.ParseExact(model.DateTo, new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" }, null)
                : (DateTime?)null;
            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "excel",
                Action = "ProductReport",
                Note = "",
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-excel",
                    Action = "ProductReport",
                    Note = "",
                });
            }
            var storeId = model.StoreId.HasValue ? model.StoreId.Value : 0;
            if (reportType == 0)
            {
                return await _excelService.BuildProductReportSummary(model.Lang, userId, dateFrom, dateTo, storeId, model.AutoUpdateQuantity.HasValue && model.AutoUpdateQuantity.Value);
            }
            if (reportType == 1)
            {
                return await _excelService.BuildProductReportDetail(model.Lang, userId, dateFrom, dateTo, model.ProductId.HasValue ? model.ProductId.Value : 0, storeId);
            }
            return null;
        }

        [HttpPost]
        [Route("Inventory")]
        public async Task<ActionResult<string>> Inventory(GetOrdersViewModel model)
        {
            var userId = User.GetUserId();
            var staffId = model.StaffId;
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
            var dateFrom = !string.IsNullOrEmpty(model.DateFrom)
                ? DateTime.ParseExact(model.DateFrom, new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" }, null)
                : (DateTime?)null;
            var dateTo = !string.IsNullOrEmpty(model.DateTo)
                ? DateTime.ParseExact(model.DateTo, new string[] { "yyyy-MM-dd HH:mm:ss", "yyyy-MM-dd" }, null)
                : (DateTime?)null;
            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "excel",
                Action = "Inventory",
                Note = "",
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-excel",
                    Action = "Inventory",
                    Note = "",
                });
            }
            var storeId = model.StoreId.HasValue ? model.StoreId.Value : 0;
            return await _excelService.CreateInventoryReportFile(model.ReportType.HasValue ? model.ReportType.Value : 0, model.Lang, userId, dateFrom, dateTo, model.ProductId.HasValue ? model.ProductId.Value : 0, storeId);
        }

        [HttpPost]
        [Route("Products")]
        public async Task<ActionResult<string>> Products(GetOrdersViewModel model)
        {
            var userId = User.GetUserId();
            var staffId = model.StaffId;
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
            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "excel",
                Action = "Products",
                Note = "",
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-excel",
                    Action = "Products",
                    Note = "",
                });
            }
            var storeId = model.StoreId.HasValue ? model.StoreId.Value : 0;
            return await _excelService.CreateProductsReportFile(model.Lang, userId, storeId, model.IsMaterial);
        }

        [HttpPost]
        [Route("Contacts")]
        public async Task<ActionResult<string>> Contacts(GetOrdersViewModel model)
        {
            var userId = User.GetUserId();
            var staffId = model.StaffId;
            Staff staff = null;
            if (staffId.HasValue && staffId.Value > 0)
            {
                staff = await _staffService.GetByIdOnly(staffId.Value);
                if (staff == null)
                {
                    return null;
                }
                if (!staff.HasFullAccess && !staff.CanManageContacts)
                {
                    return null;
                }
                userId = staff.UserId;
            }
            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "excel",
                Action = "Contacts",
                Note = "",
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-excel",
                    Action = "Contacts",
                    Note = "",
                });
            }
            return await _excelService.CreateContactsFile(model.Lang, userId);
        }

        [HttpPost]
        [Route("ReceivedTemplate")]
        public async Task<ActionResult<string>> ReceivedTemplate(GetOrdersViewModel model)
        {
            var userId = User.GetUserId();
            return await _excelService.CreateReceivedTemplate(model.Lang);
        }

        [HttpPost]
        [Route("ProductsTemplate")]
        public async Task<ActionResult<string>> ProductsTemplate(GetOrdersViewModel model)
        {
            var userId = User.GetUserId();
            return await _excelService.CreateProductsTemplate(model.Lang, model.IsMaterial);
        }

        [HttpPost]
        [Route("CreateOrdersTemplate")]
        public async Task<ActionResult<string>> CreateOrdersTemplate(GetOrdersViewModel model)
        {
            var userId = User.GetUserId();
            return await _excelService.CreateOrdersTemplate(model.Lang);
        }

        [HttpPost]
        [Route("ContactsTemplate")]
        public async Task<ActionResult<string>> ContactsTemplate(GetOrdersViewModel model)
        {
            var userId = User.GetUserId();
            return await _excelService.CreateContactsTemplate(model.Lang);
        }

        [HttpPost]
        [Route("UploadReceivedNote")]
        public async Task<ActionResult<UploadReceivedNoteResponseViewModel>> UploadReceivedNote([FromForm] UploadReceivedNoteViewModel uploadImageModel)
        {
            if (uploadImageModel == null)
            {
                return new UploadReceivedNoteResponseViewModel() { Id = 0, Error = "{missing-file}" };
            }

            var file = uploadImageModel.File;
            if (file == null || string.IsNullOrEmpty(file.FileName))
            {
                return new UploadReceivedNoteResponseViewModel() { Id = 0, Error = "{missing-file}" };
            }

            var userId = User.GetUserId();
            var staffId = uploadImageModel.StaffId;
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
            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "excel",
                Action = "UploadReceivedNote",
                Note = "",
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-excel",
                    Action = "UploadReceivedNote",
                    Note = "",
                });
            }
            _cacheService.RemoveListEqualItem("product", userId);
            var cacheProductItem = (CacheItem<Product>)_cacheService.GetCacheItem("product");
            cacheProductItem.Clear(userId);
            var cacheProductList = (CacheList<Product>)_cacheService.GetCacheList("product");
            cacheProductList.ClearAllLists(userId);
            return await _excelService.UploadReceivedNote(file, uploadImageModel.Lang, userId, uploadImageModel.StoreId, uploadImageModel.StaffId);
        }

        [HttpPost]
        [Route("UploadOrders")]
        public async Task<ActionResult<ImportOrdersViewModel>> UploadOrders([FromForm] UploadReceivedNoteViewModel uploadImageModel)
        {
            if (uploadImageModel == null)
            {
                return new ImportOrdersViewModel() { Error = "{missing-file}" };
            }

            var file = uploadImageModel.File;
            if (file == null || string.IsNullOrEmpty(file.FileName))
            {
                return new ImportOrdersViewModel() { Error = "{missing-file}" };
            }

            var userId = User.GetUserId();
            var staffId = uploadImageModel.StaffId;
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
            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "excel",
                Action = "UploadOrders",
                Note = "",
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-excel",
                    Action = "UploadOrders",
                    Note = "",
                });
            }

            _cacheService.RemoveListEqualItem("product", userId);
            var cacheProductItem = (CacheItem<Product>)_cacheService.GetCacheItem("product");
            cacheProductItem.Clear(userId);
            var cacheProductList = (CacheList<Product>)_cacheService.GetCacheList("product");
            cacheProductList.ClearAllLists(userId);
            return await _excelService.UploadOrders(file, uploadImageModel.Lang, userId, uploadImageModel.StoreId);
        }

        [HttpPost]
        [Route("UploadProducts")]
        public async Task<ActionResult<ImportProductsViewModel>> UploadProducts([FromForm] UploadReceivedNoteViewModel uploadImageModel)
        {
            if (uploadImageModel == null)
            {
                return new ImportProductsViewModel() { Id = 0, Error = "{missing-file}" };
            }

            var file = uploadImageModel.File;
            if (file == null || string.IsNullOrEmpty(file.FileName))
            {
                return new ImportProductsViewModel() { Id = 0, Error = "{missing-file}" };
            }

            var userId = User.GetUserId();
            var staffId = uploadImageModel.StaffId;
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
            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "excel",
                Action = "UploadProducts",
                Note = "",
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-excel",
                    Action = "UploadProducts",
                    Note = "",
                });
            }

            _cacheService.RemoveListEqualItem("product", userId);
            var cacheProductItem = (CacheItem<Product>)_cacheService.GetCacheItem("product");
            cacheProductItem.Clear(userId);
            var cacheProductList = (CacheList<Product>)_cacheService.GetCacheList("product");
            cacheProductList.ClearAllLists(userId);
            return await _excelService.UploadProducts(file, uploadImageModel.Lang, userId, uploadImageModel.StoreId, false);
        }

        [HttpPost]
        [Route("UploadMaterials")]
        public async Task<ActionResult<ImportProductsViewModel>> UploadMaterials([FromForm] UploadReceivedNoteViewModel uploadImageModel)
        {
            if (uploadImageModel == null)
            {
                return new ImportProductsViewModel() { Id = 0, Error = "{missing-file}" };
            }

            var file = uploadImageModel.File;
            if (file == null || string.IsNullOrEmpty(file.FileName))
            {
                return new ImportProductsViewModel() { Id = 0, Error = "{missing-file}" };
            }

            var userId = User.GetUserId();
            var staffId = uploadImageModel.StaffId;
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
            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "excel",
                Action = "UploadMaterials",
                Note = "",
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-excel",
                    Action = "UploadMaterials",
                    Note = "",
                });
            }
            _cacheService.RemoveListEqualItem("product", userId);
            var cacheProductItem = (CacheItem<Product>)_cacheService.GetCacheItem("product");
            cacheProductItem.Clear(userId);
            var cacheProductList = (CacheList<Product>)_cacheService.GetCacheList("product");
            cacheProductList.ClearAllLists(userId);
            return await _excelService.UploadProducts(file, uploadImageModel.Lang, userId, uploadImageModel.StoreId, true);
        }

        [HttpPost]
        [Route("UploadContacts")]
        public async Task<ActionResult<ImportContactsViewModel>> UploadContacts([FromForm] UploadReceivedNoteViewModel uploadImageModel)
        {
            if (uploadImageModel == null)
            {
                return new ImportContactsViewModel() { Id = 0, Error = "{missing-file}" };
            }

            var file = uploadImageModel.File;
            if (file == null || string.IsNullOrEmpty(file.FileName))
            {
                return new ImportContactsViewModel() { Id = 0, Error = "{missing-file}" };
            }

            var userId = User.GetUserId();
            var staffId = uploadImageModel.StaffId;
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
            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "excel",
                Action = "UploadContacts",
                Note = "",
            });
            if (staff != null)
            {
                await _queueMessage.WriteAsync(new UserActivity()
                {
                    UserId = User.GetUserId(),
                    Feature = "staff-excel",
                    Action = "UploadContacts",
                    Note = "",
                });
            }

            _cacheService.RemoveListEqualItem("contact", userId);
            var cacheOrderItem = (CacheItem<Order>)_cacheService.GetCacheItem("order");
            cacheOrderItem.Clear(userId);
            var cacheNoteItem = (CacheItem<ReceivedNote>)_cacheService.GetCacheItem("receivedNote");
            cacheNoteItem.Clear(userId);

            return await _excelService.UploadContacts(file, uploadImageModel.Lang, userId);
        }

        [HttpPost]
        [Route("PdfConvert")]
        public ActionResult<dynamic> PdfConvert(ConvertHtmlModel htmlModel)
        {
            String file;
            using (MemoryStream ms = new MemoryStream())
            {
                Byte[] res = null;
                var pdf = TheArtOfDev.HtmlRenderer.PdfSharp.PdfGenerator.GeneratePdf(htmlModel.Html, htmlModel.Size == 5 ? PageSize.A4 : PageSize.Letter, 5);
                pdf.Save(ms);
                res = ms.ToArray();
                file = Convert.ToBase64String(res);
            }
            return new { Base64 = file };
        }

        [HttpPost]
        [Route("PdfConvertShare")]
        public ActionResult<string> PdfConvertShare(ConvertHtmlModel htmlModel)
        {
            var html = ExtractInnerHtml(htmlModel.Html);
            var pdf = TheArtOfDev.HtmlRenderer.PdfSharp.PdfGenerator.GeneratePdf(html, htmlModel.Size == 5 || htmlModel.Size == 6 ? PageSize.A5 : PageSize.Letter, 20);

            var fileName = Guid.NewGuid().ToString("N") + ".pdf";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//downloads");
            var pathAndFileName = Path.Combine(path, fileName);
            pdf.Save(pathAndFileName);

            return fileName;
        }

        public string ExtractInnerHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) {
                return html;
            }
            var doc = new HtmlDocument();
            doc.LoadHtml(html);

            var orderPrintElement = doc.DocumentNode.SelectSingleNode("//order-print");
            if (orderPrintElement != null)
            {
                return orderPrintElement.InnerHtml;
            }
            else
            {
                return string.Empty;
            }
        }

    }
}
