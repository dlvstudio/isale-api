using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using OfficeOpenXml;
using OfficeOpenXml.Style;

public class ExcelService : IExcelService
{
    private readonly IOrderRepository _repository;
    private readonly IProductRepository _productRepository;
    private readonly IOrderRepository _orderRepository;
    private readonly IContactRepository _contactRepository;
    private readonly IReceivedNoteRepository _receivedNoteRepository;
    private readonly IAccountRepository _accountRepository;
    private readonly ITradeRepository _tradeRepository;
    private readonly IDebtRepository _debtRepository;
    private readonly IStaffRepository _staffRepository;
    private readonly IProductService _productService;
    private readonly ISqlService _sqlService;

    public ExcelService(
        IOrderRepository repository,
        IProductRepository productRepository,
        IReceivedNoteRepository receivedNoteRepository,
        IAccountRepository accountRepository,
        ITradeRepository tradeRepository,
        IDebtRepository debtRepository,
        IContactRepository contactRepository,
        ISqlService sqlService,
        IOrderRepository orderRepository,
        IStaffRepository staffRepository,
        IProductService productService
    )
    {
        _repository = repository;
        _productRepository = productRepository;
        _receivedNoteRepository = receivedNoteRepository;
        _accountRepository = accountRepository;
        _tradeRepository = tradeRepository;
        _debtRepository = debtRepository;
        _contactRepository = contactRepository;
        _staffRepository = staffRepository;
        _productService = productService;
        _sqlService = sqlService;
        _orderRepository = orderRepository;
    }
    public async Task<string> CreateReceivedNoteFile(string lang, string userId, int noteId)
    {
        var templateFileName = "ImportProductNotesTemplate.xlsx";
        var pathTemplate = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
        var pathAndFileNameTemplate = Path.Combine(pathTemplate, templateFileName);
        using (var package = new ExcelPackage(new System.IO.FileInfo(pathAndFileNameTemplate)))
        {
            var sheet = package.Workbook.Worksheets[0];

            if (isVn(lang))
            {
                sheet.Cells[1, 1].Value = "PHIẾU NHẬP HÀNG";
                sheet.Cells[1, 4].Value = "* là trường bắt buộc";
                sheet.Cells[1, 5].Value = "Số phiếu:";
                sheet.Cells[1, 7].Value = "* Không bắt buộc, chỉ điền nếu bạn cần sửa, cập nhật một phiếu đã tồn tại trong hệ thống, xem Số phiếu ở trong App.";

                sheet.Cells[2, 1].Value = "Ngày:";
                sheet.Cells[3, 1].Value = "Tên nhà cung cấp:";
                sheet.Cells[4, 1].Value = "Số điện thoại:";
                sheet.Cells[5, 1].Value = "Người nhận:";
                sheet.Cells[6, 1].Value = "Người giao:";
                sheet.Cells[7, 1].Value = "Tên ngoại tệ (vd: EUR):";

                sheet.Cells[2, 3].Value = "Tổng thanh toán:";
                sheet.Cells[3, 3].Value = "Thanh toán:";
                sheet.Cells[4, 3].Value = "Thuế:";
                sheet.Cells[5, 3].Value = "Chiết khấu:";
                sheet.Cells[5, 5].Value = "Mã shop/kho:";
                sheet.Cells[5, 7].Value = "* Không bắt buộc, hãy xem mã shop/kho trong ứng dụng ISale, nếu điền phiếu này sẽ thuộc về shop cụ thể";
                sheet.Cells[6, 3].Value = "Phí vận chuyển:";
                sheet.Cells[7, 3].Value = "Thanh toán (ngoại tệ):";
                sheet.Cells[2, 5].Value = "Đã trả:";

                sheet.Cells[9, 2].Value = "Mã SP\n(Vd: IP01)";
                sheet.Cells[9, 3].Value = "Tên SP\n(Vd: Iphone 11)";
                sheet.Cells[9, 4].Value = "Đơn vị";
                sheet.Cells[9, 5].Value = "Giá bán";
                sheet.Cells[9, 6].Value = "Số lượng";
                sheet.Cells[9, 7].Value = "Chiết khấu";
                sheet.Cells[9, 8].Value = "Ghi chú";
                sheet.Cells[9, 9].Value = "Giá nhập(ngoại tệ)";
                sheet.Cells[9, 10].Value = "Thành tiền(ngoại tệ)";
                sheet.Cells[9, 11].Value = "Thành tiền";
                sheet.Cells[9, 12].Value = "Mã vạch SP\n(nếu có)";
                sheet.Cells[8, 12].Value = "Chỉ nhập cho sp mới";
                sheet.Cells[9, 13].Value = "Giá nhập";
                sheet.Cells[8, 13].Value = "Nếu để trắng, sẽ bằng giá bán";
                sheet.Cells[9, 14].Value = "Ngày nhập";
            }

            var fileName = Guid.NewGuid().ToString("N") + ".xlsx";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//downloads");
            var pathAndFileName = Path.Combine(path, fileName);

            var receivedNote = await _receivedNoteRepository.GetById(noteId, userId);
            if (receivedNote == null)
            {
                package.SaveAs(new System.IO.FileInfo(pathAndFileName));
                return fileName;
            }
            sheet.Cells[2, 2].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            sheet.Cells[2, 2].Value = receivedNote.CreatedAt;
            sheet.Cells[3, 2].Value = receivedNote.ContactName;
            sheet.Cells[4, 2].Value = receivedNote.ContactPhone;
            sheet.Cells[5, 2].Value = receivedNote.Receiver;
            sheet.Cells[6, 2].Value = receivedNote.DeliveryPerson;
            sheet.Cells[7, 2].Value = receivedNote.ForeignCurrency;
            sheet.Cells[2, 4].Value = receivedNote.Total;
            sheet.Cells[3, 4].Value = receivedNote.NetValue;
            sheet.Cells[4, 4].Value = receivedNote.Tax;
            sheet.Cells[5, 4].Value = receivedNote.Discount;
            sheet.Cells[6, 4].Value = receivedNote.ShippingFee;
            sheet.Cells[7, 4].Value = receivedNote.TotalForeign;
            sheet.Cells[1, 6].Value = receivedNote.Id;
            sheet.Cells[2, 6].Value = receivedNote.Paid;
            if (receivedNote.StoreId > 0)
            {
                sheet.Cells[5, 6].Value = receivedNote.StoreId;
            }
            var items = JsonConvert.DeserializeObject<IEnumerable<ReceivedNoteItem>>(receivedNote.ItemsJson);
            sheet.InsertRow(10, items.Count());
            var currentRow = 10;
            var i = 0;
            foreach (var noteItem in items)
            {
                i++;
                sheet.Cells[currentRow, 1].Value = i;
                sheet.Cells[currentRow, 2].Value = noteItem.ProductCode;
                sheet.Cells[currentRow, 3].Value = noteItem.ProductName;
                sheet.Cells[currentRow, 4].Value = noteItem.Unit;
                sheet.Cells[currentRow, 5].Value = noteItem.UnitPrice;
                sheet.Cells[currentRow, 6].Value = noteItem.Quantity;
                if (noteItem.Discount > 0)
                {
                    sheet.Cells[currentRow, 7].Value = noteItem.Discount;
                }
                sheet.Cells[currentRow, 8].Value = noteItem.Note;
                if (noteItem.Discount > 0)
                {
                    sheet.Cells[currentRow, 9].Value = noteItem.UnitPriceForeign;
                }
                if (noteItem.Discount > 0)
                {
                    sheet.Cells[currentRow, 10].Value = noteItem.AmountForeign;
                }
                sheet.Cells[currentRow, 11].Value = noteItem.Amount;
                if (!string.IsNullOrWhiteSpace(noteItem.Barcode))
                {
                    sheet.Cells[currentRow, 12].Value = noteItem.Barcode;
                }
                if (noteItem.CostPrice.HasValue && noteItem.CostPrice.Value > 0 && noteItem.CostPrice.Value != noteItem.UnitPrice)
                {
                    sheet.Cells[currentRow, 13].Value = noteItem.CostPrice;
                }
                sheet.Cells[currentRow, 14].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
                sheet.Cells[currentRow, 14].Value = noteItem.ReceivedDate;

                currentRow++;
            }
            package.SaveAs(new System.IO.FileInfo(pathAndFileName));
            return fileName;
        }
    }

    public async Task<string> CreateOrderReportFile(int reportType, string lang, string userId, DateTime? dateFrom, DateTime? dateTo)
    {
        if (reportType == 0)
        {
            return await CreateSalesReportSummary(lang, userId, dateFrom, dateTo);
        }
        return await CreateSalesReportDetail(lang, userId, dateFrom, dateTo);
    }

    public async Task<string> CreateSalesReportFile(IEnumerable<int> orderIds, int reportType, string lang, string userId, DateTime? dateFrom, DateTime? dateTo, int storeId, int staffId)
    {
        if (reportType == 0)
        {
            return await CreateSalesReportFileByOrder(orderIds, lang, userId, dateFrom, dateTo, storeId, staffId);
        }
        if (reportType == 1)
        {
            return await CreateSalesReportFileByProduct(orderIds, lang, userId, dateFrom, dateTo, storeId, staffId);
        }
        if (reportType == 2)
        {
            return await CreateSalesReportFileByCustomer(orderIds, lang, userId, dateFrom, dateTo, storeId, staffId);
        }
        if (reportType == 3)
        {
            return await CreateSalesReportFileByStaff(orderIds, lang, userId, dateFrom, dateTo, storeId, staffId);
        }
        return null;
    }

    public async Task<string> CreateInventoryReportFile(int reportType, string lang, string userId, DateTime? dateFrom, DateTime? dateTo, int productId, int storeId)
    {
        if (reportType == 0)
        {
            return await CreateInventoryReportSummary(lang, userId, dateFrom, dateTo, productId, storeId);
        }
        return await CreateInventoryReportDetail(lang, userId, dateFrom, dateTo, productId, storeId);
    }

    public async Task<string> CreateReceivedTemplate(string lang)
    {
        var templateFileName = "ImportProductNotesTemplate.xlsx";
        var pathTemplate = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
        var pathAndFileNameTemplate = Path.Combine(pathTemplate, templateFileName);
        using (var package = new ExcelPackage(new System.IO.FileInfo(pathAndFileNameTemplate)))
        {
            var sheet = package.Workbook.Worksheets[0];

            if (isVn(lang))
            {
                sheet.Cells[1, 1].Value = "PHIẾU NHẬP HÀNG";
                sheet.Cells[1, 4].Value = "* là trường bắt buộc";
                sheet.Cells[1, 5].Value = "Số phiếu:";
                sheet.Cells[1, 7].Value = "* Không bắt buộc, chỉ điền nếu bạn cần sửa, cập nhật một phiếu đã tồn tại trong hệ thống, xem Số phiếu ở trong App.";

                sheet.Cells[2, 1].Value = "Ngày:";
                sheet.Cells[3, 1].Value = "Tên nhà cung cấp:";
                sheet.Cells[4, 1].Value = "Số điện thoại:";
                sheet.Cells[5, 1].Value = "Người nhận:";
                sheet.Cells[6, 1].Value = "Người giao:";
                sheet.Cells[7, 1].Value = "Tên ngoại tệ (vd: EUR):";

                sheet.Cells[2, 3].Value = "Tổng thanh toán:";
                sheet.Cells[3, 3].Value = "Thanh toán:";
                sheet.Cells[4, 3].Value = "Thuế:";
                sheet.Cells[5, 3].Value = "Chiết khấu:";
                sheet.Cells[5, 5].Value = "Mã shop/kho:";
                sheet.Cells[5, 7].Value = "* Không bắt buộc, hãy xem mã shop/kho trong ứng dụng ISale, nếu điền phiếu này sẽ thuộc về shop cụ thể";
                sheet.Cells[6, 3].Value = "Phí vận chuyển:";
                sheet.Cells[7, 3].Value = "Thanh toán (ngoại tệ):";
                sheet.Cells[2, 5].Value = "Đã trả:";

                sheet.Cells[9, 2].Value = "Mã SP\n(Vd: IP01)";
                sheet.Cells[9, 3].Value = "Tên SP\n(Vd: Iphone 11)";
                sheet.Cells[9, 4].Value = "Đơn vị";
                sheet.Cells[9, 5].Value = "Giá bán";
                sheet.Cells[9, 6].Value = "Số lượng";
                sheet.Cells[9, 7].Value = "Chiết khấu";
                sheet.Cells[9, 8].Value = "Ghi chú";
                sheet.Cells[9, 9].Value = "Giá nhập(ngoại tệ)";
                sheet.Cells[9, 10].Value = "Thành tiền(ngoại tệ)";
                sheet.Cells[9, 11].Value = "Thành tiền";
                sheet.Cells[9, 12].Value = "Mã vạch SP\n(nếu có)";
                sheet.Cells[8, 12].Value = "Chỉ nhập cho sp mới";
                sheet.Cells[9, 13].Value = "Giá nhập";
                sheet.Cells[8, 13].Value = "Nếu để trắng, sẽ bằng giá bán";
                sheet.Cells[9, 14].Value = "Ngày nhập";
            }

            var fileName = Guid.NewGuid().ToString("N") + ".xlsx";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//downloads");
            var pathAndFileName = Path.Combine(path, fileName);

            package.SaveAs(new System.IO.FileInfo(pathAndFileName));
            return fileName;
        }
    }

    public async Task<UploadReceivedNoteResponseViewModel> UploadReceivedNote(IFormFile file, string lang, string userId, int? storeId, int? staffId)
    {
        var account = await _accountRepository.GetDefault(userId);
        using (var stream = file.OpenReadStream())
        {
            using (var package = new ExcelPackage(stream))
            {
                var currentPlan = await GetCurrentPlanAsync(userId);
                var currentCount = await _sqlService.CountAsync("product", new Dictionary<string, object>() { { "userId", userId } }, null);
                var sheet = package.Workbook.Worksheets[0];
                var note = new ReceivedNote();

                if (!isDecimalOrEmpty(sheet.Cells[1, 6].Value))
                {
                    return new UploadReceivedNoteResponseViewModel() { Error = "{note-id-not-number}:1" };
                }

                var newCount = 0;
                var dicProduct = new Dictionary<string, bool>();
                bool isUpdate = false;
                if (sheet.Cells[1, 6].Value != null)
                {
                    var noteId = sheet.Cells[1, 6].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[1, 6].Value.ToString())
                        ? Convert.ToInt32(sheet.Cells[1, 6].Value)
                        : 0;
                    if (noteId != 0)
                    {
                        note = await _receivedNoteRepository.GetById(noteId, userId);
                        if (note == null)
                        {
                            return new UploadReceivedNoteResponseViewModel() { Error = "{note-is-not-exist}:1" };
                        }
                        isUpdate = true;
                    }
                    else
                    {
                        return new UploadReceivedNoteResponseViewModel() { Error = "{note-is-not-exist}:1" };
                    }
                }

                note.StaffId = staffId.HasValue ? staffId.Value : 0;
                note.ContactName = sheet.Cells[3, 2].Value != null ? sheet.Cells[3, 2].Value.ToString() : string.Empty;
                note.ContactPhone = sheet.Cells[4, 2].Value != null ? sheet.Cells[4, 2].Value.ToString() : string.Empty;
                note.Receiver = sheet.Cells[5, 2].Value != null ? sheet.Cells[5, 2].Value.ToString() : string.Empty;
                note.DeliveryPerson = sheet.Cells[6, 2].Value != null ? sheet.Cells[6, 2].Value.ToString() : string.Empty;
                note.ForeignCurrency = sheet.Cells[7, 2].Value != null ? sheet.Cells[7, 2].Value.ToString() : string.Empty;
                var receivedNoteString = isVn(lang) ? "Phiếu nhập hàng (từ Excel) #" : "Received note (from Excel) #";

                sheet.Cells[2, 4].Calculate();// if formula
                if (!isDecimal(sheet.Cells[2, 4].Value))
                {
                    return new UploadReceivedNoteResponseViewModel() { Error = "{total-not-number}:2" };
                }
                note.Total = sheet.Cells[2, 4].Value != null
                    ? Convert.ToDecimal(sheet.Cells[2, 4].Value)
                    : 0;
                if (!isDecimalOrEmpty(sheet.Cells[3, 4].Value))
                {
                    return new UploadReceivedNoteResponseViewModel() { Error = "{netvalue-not-number}:3" };
                }
                note.NetValue = sheet.Cells[3, 4].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[3, 4].Value.ToString())
                    ? Convert.ToDecimal(sheet.Cells[3, 4].Value)
                    : 0;
                if (!isDecimalOrEmpty(sheet.Cells[2, 4].Value))
                {
                    return new UploadReceivedNoteResponseViewModel() { Error = "{paid-not-number}:2" };
                }
                note.Paid = sheet.Cells[2, 6].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[2, 6].Value.ToString())
                    ? Convert.ToDecimal(sheet.Cells[2, 6].Value)
                    : (decimal?)null;
                if (!note.Paid.HasValue)
                {
                    note.Paid = note.Total;
                }
                if (!isDecimalOrEmpty(sheet.Cells[4, 4].Value))
                {
                    return new UploadReceivedNoteResponseViewModel() { Error = "{tax-not-number}:4" };
                }
                note.Tax = sheet.Cells[4, 4].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[4, 4].Value.ToString())
                    ? Convert.ToDecimal(sheet.Cells[4, 4].Value)
                    : 0;
                if (!isDecimalOrEmpty(sheet.Cells[5, 4].Value))
                {
                    return new UploadReceivedNoteResponseViewModel() { Error = "{discount-not-number}:5" };
                }
                note.Discount = sheet.Cells[5, 4].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[5, 4].Value.ToString())
                    ? Convert.ToDecimal(sheet.Cells[5, 4].Value)
                    : 0;
                if (storeId.HasValue)
                {
                    note.StoreId = storeId.Value;
                }
                else
                {
                    if (!isDecimalOrEmpty(sheet.Cells[5, 6].Value))
                    {
                        return new UploadReceivedNoteResponseViewModel() { Error = "{shop-id-not-number}:5" };
                    }
                    if (sheet.Cells[5, 6].Value != null)
                    {
                        var store = await _sqlService.GetAsync("store", new Dictionary<string, object>() { { "Id", sheet.Cells[5, 6].Value } }, null);
                        if (store == null)
                        {
                            return new UploadReceivedNoteResponseViewModel() { Error = "{shop-is-not-exist}:5" };
                        }
                    }
                    note.StoreId = sheet.Cells[5, 6].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[5, 6].Value.ToString())
                        ? Convert.ToInt32(sheet.Cells[5, 6].Value)
                        : 0;
                }
                if (!isDecimalOrEmpty(sheet.Cells[6, 4].Value))
                {
                    return new UploadReceivedNoteResponseViewModel() { Error = "{shipping-not-number}:6" };
                }
                note.ShippingFee = sheet.Cells[6, 4].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[6, 4].Value.ToString())
                    ? Convert.ToDecimal(sheet.Cells[6, 4].Value)
                    : 0;
                sheet.Cells[7, 4].Calculate();// if formula
                if (!isDecimalOrEmpty(sheet.Cells[7, 4].Value))
                {
                    return new UploadReceivedNoteResponseViewModel() { Error = "{totalforeign-not-number}:7" };
                }
                note.TotalForeign = sheet.Cells[7, 4].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[7, 4].Value.ToString()) ? Convert.ToDecimal(sheet.Cells[7, 4].Value)
                : (decimal?)null;
                if (note.NetValue == 0)
                {
                    note.NetValue = note.Total - note.Tax - note.Discount - note.ShippingFee;
                }
                var items = new List<ReceivedNoteItem>();

                var currentRow = 10;
                var hasData = sheet.Cells[currentRow, 2].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow, 2].Value.ToString())
                || sheet.Cells[currentRow + 1, 2].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow + 1, 2].Value.ToString());
                while (hasData)
                {
                    var item = new ReceivedNoteItem();
                    item.ProductCode = sheet.Cells[currentRow, 2].Value != null ? sheet.Cells[currentRow, 2].Value.ToString().ToUpper() : string.Empty;
                    if (string.IsNullOrWhiteSpace(item.ProductCode))
                    {
                        return new UploadReceivedNoteResponseViewModel() { Error = "{missing-code}:" + currentRow };
                    }
                    if (currentPlan == null)
                    {
                        var existingProduct = await _productRepository.GetByCode(item.ProductCode, userId);
                        if (existingProduct == null)
                        {
                            if (!dicProduct.ContainsKey(item.ProductCode))
                            {
                                newCount++;
                                if ((newCount + currentCount) > 30)
                                {
                                    return new UploadReceivedNoteResponseViewModel() { Error = "{total-product-more-than-30-please-upgrade-pro-plan}" };
                                }
                            }
                            else
                            {
                                dicProduct.Add(item.ProductCode.ToUpper(), true);
                            }
                        }
                    }

                    item.ProductName = sheet.Cells[currentRow, 3].Value != null ? sheet.Cells[currentRow, 3].Value.ToString() : string.Empty;
                    if (string.IsNullOrWhiteSpace(item.ProductName))
                    {
                        return new UploadReceivedNoteResponseViewModel() { Error = "{missing-name}:" + currentRow };
                    }
                    item.Unit = sheet.Cells[currentRow, 4].Value != null ? sheet.Cells[currentRow, 4].Value.ToString() : string.Empty;
                    if (string.IsNullOrWhiteSpace(item.Unit))
                    {
                        return new UploadReceivedNoteResponseViewModel() { Error = "{missing-unit}:" + currentRow };
                    }
                    decimal? unitPrice = null;
                    if (!isDecimalOrEmpty(sheet.Cells[currentRow, 5].Value))
                    {
                        return new UploadReceivedNoteResponseViewModel() { Error = "{unit-price-not-number}:" + currentRow };
                    }
                    unitPrice = sheet.Cells[currentRow, 5].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow, 5].Value.ToString())
                        ? Convert.ToDecimal(sheet.Cells[currentRow, 5].Value)
                        : (decimal?)null;
                    if (unitPrice.HasValue)
                    {
                        item.UnitPrice = unitPrice.Value;
                    }

                    if (!isDecimal(sheet.Cells[currentRow, 6].Value))
                    {
                        return new UploadReceivedNoteResponseViewModel() { Error = "{missing-quantity}:" + currentRow };
                    }
                    item.Quantity = sheet.Cells[currentRow, 6].Value != null ? Convert.ToInt32(sheet.Cells[currentRow, 6].Value) : 0;
                    if (!isDecimalOrEmpty(sheet.Cells[currentRow, 7].Value))
                    {
                        return new UploadReceivedNoteResponseViewModel() { Error = "{discount-not-number}:" + currentRow };
                    }
                    if (!string.IsNullOrEmpty(sheet.Cells[currentRow, 7].Formula))
                        sheet.Cells[currentRow, 7].Calculate();
                    item.Discount = sheet.Cells[currentRow, 7].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow, 7].Value.ToString())
                        ? Convert.ToDecimal(sheet.Cells[currentRow, 7].Value)
                        : 0;
                    item.Note = sheet.Cells[currentRow, 8].Value != null ? sheet.Cells[currentRow, 8].Value.ToString() : string.Empty;
                    if (string.IsNullOrWhiteSpace(item.Note))
                    {
                        item.Note = receivedNoteString;
                    }
                    if (!isDecimalOrEmpty(sheet.Cells[currentRow, 9].Value))
                    {
                        return new UploadReceivedNoteResponseViewModel() { Error = "{unit-price-foreign-not-number}:" + currentRow };
                    }
                    item.UnitPriceForeign = sheet.Cells[currentRow, 9].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow, 9].Value.ToString())
                        ? Convert.ToDecimal(sheet.Cells[currentRow, 9].Value)
                        : (decimal?)null;
                    if (!string.IsNullOrEmpty(sheet.Cells[currentRow, 10].Formula))
                        sheet.Cells[currentRow, 10].Calculate();// if formula
                    if (!isDecimalOrEmpty(sheet.Cells[currentRow, 10].Value))
                    {
                        return new UploadReceivedNoteResponseViewModel() { Error = "{amount-foreign-not-number}:" + currentRow };
                    }
                    item.AmountForeign = sheet.Cells[currentRow, 10].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow, 10].Value.ToString())
                        ? Convert.ToDecimal(sheet.Cells[currentRow, 10].Value)
                        : (decimal?)null;
                    if (!string.IsNullOrEmpty(sheet.Cells[currentRow, 11].Formula))
                        sheet.Cells[currentRow, 11].Calculate();// if formula
                    if (!isDecimal(sheet.Cells[currentRow, 11].Value))
                    {
                        return new UploadReceivedNoteResponseViewModel() { Error = "{missing-amount}:" + currentRow };
                    }
                    item.Amount = sheet.Cells[currentRow, 11].Value != null ? Convert.ToDecimal(sheet.Cells[currentRow, 11].Value) : 0;
                    item.Barcode = sheet.Cells[currentRow, 12].Value != null ? sheet.Cells[currentRow, 12].Value.ToString() : string.Empty;

                    if (!isDecimalOrEmpty(sheet.Cells[currentRow, 13].Value))
                    {
                        return new UploadReceivedNoteResponseViewModel() { Error = "{cost-price-not-number}:" + currentRow };
                    }
                    item.CostPrice = sheet.Cells[currentRow, 13].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow, 13].Value.ToString())
                        ? Convert.ToDecimal(sheet.Cells[currentRow, 13].Value)
                        : (decimal?)null;
                    if (!item.CostPrice.HasValue)
                    {
                        if (!unitPrice.HasValue)
                        {
                            return new UploadReceivedNoteResponseViewModel() { Error = "{missing-cost-price}:" + currentRow };
                        }
                        item.CostPrice = unitPrice.Value;
                    }
                    DateTime? receivedDate = null;
                    if (!isDateOrEmpty(sheet.Cells[currentRow, 14].Value, out receivedDate))
                    {
                        return new UploadReceivedNoteResponseViewModel() { Error = "{received-date-not-date-format}:" + currentRow };
                    }
                    item.ReceivedDate = receivedDate;

                    items.Add(item);
                    currentRow++;
                    hasData = sheet.Cells[currentRow, 2].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow, 2].Value.ToString())
                    || sheet.Cells[currentRow + 1, 2].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow + 1, 2].Value.ToString());
                }
                note.Items = items;
                note.MoneyAccountId = account != null ? account.Id : 0;
                var serializerSettings = new JsonSerializerSettings();
                serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                note.ItemsJson = JsonConvert.SerializeObject(note.Items, serializerSettings);
                note.UserId = userId;
                var id = await _receivedNoteRepository.Save(note);
                note.Id = id;
                if (!isUpdate)
                {
                    // create transaction
                    var trade = new Trade()
                    {
                        ReceivedNoteId = id,
                        UserId = userId,
                        IsReceived = false,
                        Note = receivedNoteString + id.ToString(),
                        Value = note.Paid.Value,
                        CreatedAt = DateTime.Now,
                        ModifiedAt = DateTime.Now,
                        MoneyAccountId = note.MoneyAccountId
                    };
                    await _tradeRepository.SaveTrade(trade);
                    //create debt
                    if (note.Total > note.Paid.Value)
                    {
                        var debt = new Debt()
                        {
                            Note = receivedNoteString + id.ToString(),
                            DebtType = 2,
                            CreatedAt = DateTime.Now,
                            ModifiedAt = DateTime.Now,
                            UserId = userId,
                            Value = note.Total - note.Paid.Value,
                            ReceivedNoteId = id,
                            StoreId = note.StoreId
                        };
                        await _debtRepository.SaveDebt(debt);
                    }
                }

                if (isUpdate)
                {
                    var notesToDel = await _productService.GetNotes(userId, (DateTime?)null, (DateTime?)null, 0, 0, 0, note.Id, 0, note.StoreId, 0, 0);
                    foreach (var noteToDel in notesToDel)
                    {
                        await _productService.RemoveProductNote(noteToDel.Id, userId);
                    }
                }
                // create product notes 
                foreach (var noteItem in note.Items)
                {
                    await UpdateProduct(noteItem, userId, 0, id, note.StoreId);
                }
                note.ItemsJson = JsonConvert.SerializeObject(note.Items, serializerSettings);
                await _receivedNoteRepository.Save(note);
                return new UploadReceivedNoteResponseViewModel() { Id = id, Error = string.Empty };
            }
        }
    }

    private bool isDecimalOrEmpty(object obj)
    {
        if (obj == null)
        {
            return true;
        }
        var inp = obj.ToString();
        if (string.IsNullOrWhiteSpace(inp))
        {
            return true;
        }
        decimal d = 0;
        return decimal.TryParse(inp, out d);
    }

    private bool isDateOrEmpty(object obj, out DateTime? result)
    {
        if (obj == null)
        {
            result = null;
            return true;
        }
        var inp = obj.ToString();
        if (string.IsNullOrWhiteSpace(inp))
        {
            result = null;
            return true;
        }
        DateTime d = DateTime.Now;
        bool isDate = DateTime.TryParse(inp, out d);
        if (isDate)
        {
            result = d;
            return true;
        }
        isDate = false;
        try
        {
            long dateNum = long.Parse(inp);
            result = DateTime.FromOADate(dateNum);
            return true;
        }
        catch (System.Exception)
        {
        }
        result = null;
        return false;
    }

    private bool isIntOrEmpty(object obj)
    {
        if (obj == null)
        {
            return true;
        }
        var inp = obj.ToString();
        if (string.IsNullOrWhiteSpace(inp))
        {
            return true;
        }
        int d = 0;
        return int.TryParse(inp, out d);
    }

    private bool isDecimal(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        var inp = obj.ToString();
        decimal d = 0;
        return decimal.TryParse(inp, out d);
    }

    private bool isInt(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        var inp = obj.ToString();
        decimal d = 0;
        return decimal.TryParse(inp, out d);
    }

    private bool isDateTime(object obj)
    {
        if (obj == null)
        {
            return false;
        }
        var inp = obj.ToString();
        DateTime d;
        return DateTime.TryParse(inp, out d);
    }

    private async Task<int> UpdateProduct(Product productImported, string userId, string lang, int? storeId)
    {
        var product = !string.IsNullOrWhiteSpace(productImported.Code)
            ? await _productRepository.GetByCode(productImported.Code, userId)
            : null;
        if (product == null)
        {
            var newProduct = productImported;
            newProduct.UserId = userId;

            var subNote = new ProductNote();
            subNote.Amount = 0; // amount is count to combo;
            subNote.UnitPrice = newProduct.Price;
            subNote.Unit = newProduct.Unit;
            subNote.Quantity = newProduct.Count;
            subNote.CreatedAt = DateTime.Now;
            subNote.ProductCode = newProduct.Code;
            subNote.ProductName = newProduct.Title;
            subNote.Discount = 0;
            subNote.DiscountType = 0;
            subNote.Note = !isVn(lang) ? "Update quantity manually" : "Cập nhật thủ công";
            subNote.UserId = userId;

            newProduct.Count = 0;

            var newId = await _productService.SaveProduct(newProduct, false);
            newProduct.Id = newId;

            if (newProduct.CollaboratorPrice.HasValue)
            {
                var cPrice = new Dictionary<string, object>();
                cPrice["productId"] = newId;
                cPrice["isCollaboratorPrice"] = true;
                cPrice["userId"] = userId;
                cPrice["price"] = newProduct.CollaboratorPrice.Value;
                await _sqlService.SaveAsync(cPrice, "customer_price", "id", new List<string> { "id", "userId" }, null);
            }

            subNote.ProductId = newProduct.Id;
            subNote.StoreId = storeId.HasValue ? storeId.Value : subNote.StoreId;
            if (subNote.Quantity != 0)
            {
                await _productService.SaveProductNote(subNote);
            }
            newProduct.Count = subNote.Quantity < 0 ? 0 : subNote.Quantity;
            await _productService.SaveProduct(newProduct, false);
            product = newProduct;
        }
        else
        {
            var updateProduct = productImported;
            updateProduct.UserId = userId;
            updateProduct.Id = product.Id;

            var oldQuantity = product.Count;
            var updatedQuantity = productImported.Count;

            var subNote = new ProductNote();
            subNote.Amount = 0;
            subNote.UnitPrice = updateProduct.Price;
            subNote.CreatedAt = DateTime.Now;
            subNote.Unit = updateProduct.Unit;
            subNote.Quantity = updatedQuantity - oldQuantity;
            subNote.ProductId = updateProduct.Id;
            subNote.ProductCode = updateProduct.Code;
            subNote.ProductName = updateProduct.Title;
            subNote.Discount = 0;
            subNote.DiscountType = 0;
            subNote.UserId = userId;
            subNote.Note = !isVn(lang) ? "Update quantity manually" : "Cập nhật thủ công";
            subNote.StoreId = storeId.HasValue ? storeId.Value : subNote.StoreId;

            updateProduct.Count = oldQuantity;
            await _productService.SaveProduct(updateProduct, false);

            if (updateProduct.CollaboratorPrice.HasValue)
            {
                var modelToDelete = new Dictionary<string, object>();
                modelToDelete["productId"] = updateProduct.Id;
                modelToDelete["isCollaboratorPrice"] = true;
                modelToDelete["collaboratorId"] = 0;
                modelToDelete["userId"] = userId;
                await _sqlService.RemoveAsync("customer_price", modelToDelete);

                var cPrice = new Dictionary<string, object>();
                cPrice["productId"] = updateProduct.Id;
                cPrice["isCollaboratorPrice"] = true;
                cPrice["userId"] = userId;
                cPrice["price"] = updateProduct.CollaboratorPrice.Value;
                await _sqlService.SaveAsync(cPrice, "customer_price", "id", new List<string> { "id", "userId" }, null);
            }

            if (subNote.Quantity != 0 && updatedQuantity > 0)
            {
                await _productService.SaveProductNote(subNote);
            }
            updateProduct.Count = updatedQuantity < 0 ? 0 : updatedQuantity;
            await _productService.SaveProduct(updateProduct, false);
        }
        return product.Id;
    }

    private async Task<int> UpdateContact(Contact contactImported, string userId)
    {
        var contact = string.IsNullOrWhiteSpace(contactImported.Code)
            ? null
            : await _contactRepository.GetByCode(contactImported.Code, userId);
        if (contact == null)
        {
            var newProduct = contactImported;
            newProduct.UserId = userId;
            newProduct.LastActive = DateTime.Now;
            var newId = await _contactRepository.SaveContact(newProduct);
            newProduct.Id = newId;
            contact = newProduct;
        }
        else
        {
            var updateProduct = contactImported;
            updateProduct.UserId = userId;
            updateProduct.Id = contact.Id;
            updateProduct.LastActive = DateTime.Now;

            await _contactRepository.SaveContact(updateProduct);
        }
        return contact.Id;
    }

    private async Task<int> UpdateProduct(ReceivedNoteItem noteItem, string userId, int contactId, int receivedNoteId, int storeId)
    {
        var product = string.IsNullOrWhiteSpace(noteItem.ProductCode)
            ? null
            : await _productRepository.GetByCode(noteItem.ProductCode, userId);
        if (product == null)
        {
            var newProduct = new Product()
            {
                Code = noteItem.ProductCode,
                Title = noteItem.ProductName,
                Count = 0,
                ForeignCurrency = noteItem.ForeignCurrency,
                CreatedAt = noteItem.ReceivedDate.HasValue ? noteItem.ReceivedDate.Value : DateTime.Now,
                Price = noteItem.UnitPrice,
                Unit = noteItem.Unit,
                CostPriceForeign = noteItem.UnitPriceForeign,
                CostPrice = noteItem.CostPrice.HasValue ? noteItem.CostPrice.Value : noteItem.UnitPrice,
                ModifiedAt = noteItem.ReceivedDate.HasValue ? noteItem.ReceivedDate.Value : DateTime.Now,
                OriginalPrice = noteItem.UnitPrice,
                Barcode = noteItem.Barcode,
                UserId = userId
            };
            var newId = await _productRepository.SaveProduct(newProduct, true);
            newProduct.Id = newId;
            product = newProduct;
        }
        else if (noteItem.ReceivedDate.HasValue)
        {
            product.ModifiedAt = noteItem.ReceivedDate.Value;
            await _productRepository.SaveProduct(product, true);
        }
        var note = new ProductNote()
        {
            Note = noteItem.Note,
            Amount = noteItem.Amount,
            AmountForeign = noteItem.AmountForeign,
            ContactId = contactId,
            ReceivedNoteId = receivedNoteId,
            Discount = noteItem.Discount,
            ForeignCurrency = noteItem.ForeignCurrency,
            ProductId = product.Id,
            ProductCode = noteItem.ProductCode,
            ProductName = noteItem.ProductName,
            Quantity = noteItem.Quantity.HasValue ? noteItem.Quantity.Value : 0,
            Unit = noteItem.Unit,
            UnitPrice = noteItem.CostPrice.HasValue ? noteItem.CostPrice.Value : noteItem.UnitPrice,
            UnitPriceForeign = noteItem.UnitPriceForeign,
            UserId = userId,
            Product = product,
            ReceivedDate = noteItem.ReceivedDate,
            StoreId = storeId
        };
        // noteItem.Product = product;
        noteItem.ProductId = product.Id;
        var noteId = await _productService.SaveProductNote(note);
        note.Id = noteId;
        return product.Id;
    }

    public async Task<OrderReportSummaryViewModel> BuildSalesReportByOrder(IEnumerable<int> orderIds, string lang, string userId, DateTime? dateFrom, DateTime? dateTo, int storeId, int staffId)
    {
        var from = dateFrom.HasValue ? dateFrom.Value : DateTime.Now.AddMonths(-6);
        var end = dateTo.HasValue ? dateTo.Value : DateTime.Now.AddMonths(3);

        var orders = await _repository.GetOrders(userId,
            from,
            end, 0, staffId, storeId, null, orderIds);

        if (orders == null || !orders.Any())
        {
            return new OrderReportSummaryViewModel() { FromDate = from, EndDate = end };
        }

        var arrOrders = orders.ToArray();
        var itemsCount = orders != null && orders.Any() ? orders.Count() : 0;

        decimal total = 0;
        decimal totalCost = 0;
        decimal totalDiscount = 0;
        decimal totalShip = 0;
        decimal totalChange = 0;
        decimal totalNoVat = 0;
        decimal totalVat = 0;
        decimal totalPaid = 0;
        decimal totalRevenue = 0;
        List<OrderReportItemViewModel> reportItems = new List<OrderReportItemViewModel>();
        for (int i = 0; i < itemsCount; i++)
        {
            var order = arrOrders[i];

            decimal orderCost = 0;
            decimal orderDiscount = 0;
            var items = JsonConvert.DeserializeObject<IEnumerable<OrderItem>>(order.ItemsJson);
            var itemsArr = items != null && items.Any() ? items.ToArray() : new OrderItem[] { };
            for (int j = 0; j < itemsArr.Count(); j++)
            {
                var product = itemsArr[j];
                var discount = product.Discount.HasValue ? product.Discount.Value : 0;
                if (product.TotalCostPrice.HasValue)
                {
                    orderCost += product.TotalCostPrice.Value;
                }
                orderDiscount += discount;
            }
            var item = new OrderReportItemViewModel
            {
                Id = order.Id,
                Code = order.OrderCode,
                Items = items,
                CreatedAt = order.CreatedAt,
                Discount = orderDiscount + order.DiscountOnTotal,
                NetValue = order.NetValue,
                Tax = order.Tax,
                ShippingFee = order.ShippingFee,
                Paid = order.Paid,
                Change = order.Change,
                ShipCostOnCustomer = order.ShipCostOnCustomer,
                Contact = order.Contact != null && order.ContactId > 0
                    ? order.Contact.FullName + (!string.IsNullOrWhiteSpace(order.Contact.Mobile) ? " | " + order.Contact.Mobile : string.Empty)
                    : order.ContactName
                        + (!string.IsNullOrWhiteSpace(order.ContactPhone) ? " | " + order.ContactPhone : string.Empty),
                ContactAddress = order.Contact != null && order.ContactId > 0
                    ? order.Contact.Address
                    : string.Empty,
                Status = order.Status,
                Total = order.Total,
                Cost = orderCost,
                BillOfLadingCode = order.BillOfLadingCode,
                ShippingPartner = order.ShippingPartner,
                ShipperName = order.ShipperName,
                ShipperPhone = order.ShipperPhone,
                DeliveryAddress = order.DeliveryAddress,
                Revenue = order.Total - orderCost - order.Tax - order.ShippingFee,
                Staff = order.Staff != null ? order.Staff.Name : string.Empty,
            };
            reportItems.Add(item);
            total += order.Total;
            totalCost += orderCost;
            totalShip += order.ShippingFee;
            totalPaid += order.Paid.HasValue ? order.Paid.Value : orderCost;
            totalDiscount += (orderDiscount + order.DiscountOnTotal);
            totalChange += order.Change.HasValue ? order.Change.Value : 0;
            totalVat += order.Tax;
            totalNoVat += order.NetValue;
            totalRevenue += order.Total - orderCost - order.Tax - order.ShippingFee;
        }
        var report = new OrderReportSummaryViewModel()
        {
            Total = total,
            TotalCost = totalCost,
            TotalChange = totalChange,
            TotalNoVAT = totalNoVat,
            TotalPaid = totalPaid,
            TotalRevenue = totalRevenue,
            TotalVAT = totalVat,
            TotalDiscount = totalDiscount,
            TotalShip = totalShip,
            TotalItems = reportItems.Count(),
            FromDate = from,
            EndDate = end,
            Items = reportItems
        };
        return report;
    }

    public async Task<OrderReportSummaryViewModel> BuildSalesReportByProduct(IEnumerable<int> orderIds, string lang, string userId, DateTime? dateFrom, DateTime? dateTo, int storeId, int staffId)
    {
        var from = dateFrom.HasValue ? dateFrom.Value : DateTime.Now.AddMonths(-6);
        var end = dateTo.HasValue ? dateTo.Value : DateTime.Now.AddMonths(3);

        var orders = await _repository.GetOrders(userId,
            from,
            end, 0, staffId, storeId, null, orderIds);

        if (orders == null || !orders.Any())
        {
            return new OrderReportSummaryViewModel() { FromDate = from, EndDate = end };
        }

        var arrOrders = orders.ToArray();
        var itemsCount = orders != null && orders.Any() ? orders.Count() : 0;
        decimal total = 0;
        decimal totalCost = 0;
        decimal totalDiscount = 0;
        decimal totalShip = 0;
        decimal totalChange = 0;
        decimal totalNoVat = 0;
        decimal totalVat = 0;
        decimal totalPaid = 0;
        decimal totalRevenue = 0;
        decimal totalQuantity = 0;
        List<OrderReportItemViewModel> reportItems = new List<OrderReportItemViewModel>();
        Dictionary<string, OrderReportItemViewModel> dic = new Dictionary<string, OrderReportItemViewModel>();
        for (int i = 0; i < itemsCount; i++)
        {
            var order = arrOrders[i];

            decimal orderCost = 0;
            decimal orderDiscount = 0;
            var items = JsonConvert.DeserializeObject<IEnumerable<OrderItem>>(order.ItemsJson);
            var itemsArr = items != null && items.Any() ? items.ToArray() : new OrderItem[] { };
            for (int j = 0; j < itemsArr.Count(); j++)
            {
                var product = itemsArr[j];
                var count = product.Count.HasValue ? product.Count.Value : 0;
                var isExists = dic.ContainsKey(product.ProductId + "-" + product.Unit);
                var netValue = (product.CostPrice.HasValue ? product.CostPrice.Value : 0) * count;
                var discount = product.Discount.HasValue ? product.Discount.Value : 0;
                var cost = product.TotalCostPrice.HasValue ? product.TotalCostPrice.Value : 0;
                var productTotal = product.Total.HasValue ? product.Total.Value : 0;
                OrderReportItemViewModel reportItem;
                if (!isExists)
                {
                    reportItem = new OrderReportItemViewModel
                    {
                        Id = product.ProductId,
                        Code = product.ProductCode,
                        Discount = discount,
                        Total = productTotal,
                        Cost = cost,
                        Unit = product.Unit,
                        Quantity = count,
                        Contact = product.ProductName,
                        Revenue = productTotal - cost,
                    };
                    dic.Add(product.ProductId + "-" + product.Unit, reportItem);
                }
                else
                {
                    reportItem = dic[product.ProductId + "-" + product.Unit];
                    reportItem.Discount += discount;
                    reportItem.Total += productTotal;
                    reportItem.Cost += cost;
                    reportItem.Quantity += count;
                    reportItem.Revenue += productTotal - cost;
                }
                if (product.TotalCostPrice.HasValue)
                {
                    orderCost += product.TotalCostPrice.Value;
                }
                orderDiscount += discount;
            }
            total += order.Total;
            totalCost += orderCost;
            totalShip += order.ShippingFee;
            totalPaid += order.Paid.HasValue ? order.Paid.Value : orderCost;
            totalDiscount += orderDiscount;
            totalChange += order.Change.HasValue ? order.Change.Value : 0;
            totalVat += order.Tax;
            totalNoVat += order.NetValue;
            totalRevenue += order.Total - orderCost - order.Tax - order.ShippingFee;
        }
        totalQuantity = 0;
        var totalProduct = 0;
        foreach (var key in dic.Keys)
        {
            var item = dic[key];
            totalQuantity += item.Quantity;
            totalProduct++;
            reportItems.Add(item);
        }
        var report = new OrderReportSummaryViewModel()
        {
            Total = total,
            TotalCost = totalCost,
            TotalChange = totalChange,
            TotalNoVAT = totalNoVat,
            TotalPaid = totalPaid,
            TotalRevenue = totalRevenue,
            TotalQuantity = totalQuantity,
            TotalProducts = totalProduct,
            TotalVAT = totalVat,
            TotalDiscount = totalDiscount,
            TotalShip = totalShip,
            TotalItems = reportItems.Count(),
            FromDate = from,
            EndDate = end,
            Items = reportItems
        };
        return report;
    }

    public async Task<OrderReportSummaryViewModel> BuildSalesReportByCustomerAndProduct(IEnumerable<int> orderIds, string lang, string userId, DateTime? dateFrom, DateTime? dateTo, int storeId, int staffId)
    {
        var from = dateFrom.HasValue ? dateFrom.Value : DateTime.Now.AddMonths(-6);
        var end = dateTo.HasValue ? dateTo.Value : DateTime.Now.AddMonths(3);

        var orders = await _repository.GetOrders(userId,
            from,
            end, 0, staffId, storeId, null, orderIds);

        if (orders == null || !orders.Any())
        {
            return new OrderReportSummaryViewModel() { FromDate = from, EndDate = end };
        }

        var arrOrders = orders.ToArray();
        var itemsCount = orders != null && orders.Any() ? orders.Count() : 0;
        decimal total = 0;
        decimal totalCost = 0;
        decimal totalDiscount = 0;
        decimal totalShip = 0;
        decimal totalChange = 0;
        decimal totalNoVat = 0;
        decimal totalVat = 0;
        decimal totalPaid = 0;
        decimal totalRevenue = 0;
        List<OrderReportItemViewModel> reportItems = new List<OrderReportItemViewModel>();
        Dictionary<int, OrderReportItemViewModel> dic = new Dictionary<int, OrderReportItemViewModel>();
        Dictionary<int, Dictionary<int, OrderReportItemViewModel>> dicProducts = new Dictionary<int, Dictionary<int, OrderReportItemViewModel>>();
        for (int i = 0; i < itemsCount; i++)
        {
            var order = arrOrders[i];

            decimal orderCost = 0;
            decimal orderDiscount = 0;
            var items = JsonConvert.DeserializeObject<IEnumerable<OrderItem>>(order.ItemsJson);
            var itemsArr = items != null && items.Any() ? items.ToArray() : new OrderItem[] { };
            for (int j = 0; j < itemsArr.Count(); j++)
            {
                var product = itemsArr[j];
                var count = product.Count.HasValue ? product.Count.Value : 0;
                var unit = product.Unit;
                if (!string.IsNullOrEmpty(product.BasicUnit)
                    && !string.IsNullOrEmpty(product.Unit)
                    && product.BasicUnit != product.Unit
                    && product.UnitExchange.HasValue)
                {
                    count = (product.Count.HasValue ? product.Count.Value : 0) * product.UnitExchange.Value;
                    unit = product.BasicUnit;
                }
                Dictionary<int, OrderReportItemViewModel> subDic = null;
                if (dicProducts.ContainsKey(order.ContactId))
                {
                    subDic = dicProducts[order.ContactId];
                }
                else
                {
                    subDic = new Dictionary<int, OrderReportItemViewModel>();
                    dicProducts.Add(order.ContactId, subDic);
                }
                var isExistsProduct = subDic.ContainsKey(product.ProductId);
                var netValue = (product.CostPrice.HasValue ? product.CostPrice.Value : 0) * count;
                var discount = product.Discount.HasValue ? product.Discount.Value : 0;
                var cost = product.TotalCostPrice.HasValue ? product.TotalCostPrice.Value : 0;
                var productTotal = product.Total.HasValue ? product.Total.Value : 0;
                OrderReportItemViewModel productReportItem;
                if (!isExistsProduct)
                {
                    productReportItem = new OrderReportItemViewModel
                    {
                        Id = product.ProductId,
                        Code = product.ProductCode,
                        Discount = discount,
                        Total = productTotal,
                        Cost = cost,
                        Unit = unit,
                        Quantity = count,
                        Contact = product.ProductName,
                        Revenue = productTotal - cost,
                    };
                    subDic.Add(product.ProductId, productReportItem);
                }
                else
                {
                    productReportItem = subDic[product.ProductId];
                    productReportItem.Discount += discount;
                    productReportItem.Total += productTotal;
                    productReportItem.Cost += cost;
                    productReportItem.Quantity += count;
                    productReportItem.Revenue += productTotal - cost;
                }
                if (product.TotalCostPrice.HasValue)
                {
                    orderCost += product.TotalCostPrice.Value;
                }
                orderDiscount += discount;
            }
            var isExists = dic.ContainsKey(order.ContactId);
            var contact = order.Contact;
            OrderReportItemViewModel reportItem = null;
            if (!isExists)
            {
                reportItem = new OrderReportItemViewModel
                {
                    Id = order.ContactId,
                    Total = order.Total,
                    Cost = orderCost,
                    Change = order.Change.HasValue ? order.Change.Value : 0,
                    NetValue = order.NetValue,
                    Paid = order.Paid.HasValue ? order.Paid.Value : orderCost,
                    Revenue = order.Total - orderCost - order.Tax - order.ShippingFee,
                    Tax = order.Tax,
                    Discount = orderDiscount + order.DiscountOnTotal,
                    ShippingFee = order.ShippingFee,
                    Quantity = 1,
                    Contact = contact != null ? contact.FullName : string.Empty
                };
                dic.Add(order.ContactId, reportItem);
            }
            else
            {
                reportItem = dic[order.ContactId];
                reportItem.Total += order.Total;
                reportItem.Cost += orderCost;
                reportItem.Change += order.Change.HasValue ? order.Change.Value : 0;
                reportItem.NetValue += order.NetValue;
                reportItem.Paid += order.Paid.HasValue ? order.Paid.Value : orderCost;
                reportItem.Revenue += order.Total - orderCost - order.Tax - order.ShippingFee;
                reportItem.Tax += order.Tax;
                reportItem.Discount += orderDiscount + order.DiscountOnTotal;
                reportItem.ShippingFee = order.ShippingFee;
                reportItem.Quantity++;
            }
            total += order.Total;
            totalCost += orderCost;
            totalShip += order.ShippingFee;
            totalPaid += order.Paid.HasValue ? order.Paid.Value : orderCost;
            totalDiscount += orderDiscount + order.DiscountOnTotal;
            totalChange += order.Change.HasValue ? order.Change.Value : 0;
            totalVat += order.Tax;
            totalNoVat += order.NetValue;
            totalRevenue += order.Total - orderCost - order.Tax - order.ShippingFee;
        }
        List<int> ids = new List<int>();
        foreach (var key in dic.Keys)
        {
            var item = dic[key];
            item.SubItems = new List<OrderReportItemViewModel>();
            var subDic = dicProducts[key];
            foreach (var key2 in subDic.Keys)
            {
                var subItem = subDic[key2];
                item.SubItems.Add(subItem);
                ids.Add(subItem.Id);
            }
            reportItems.Add(item);
        }
        var products = await _productRepository.GetProductsByIds(userId, ids);
        foreach (var item in reportItems)
        {
            var subItems = item.SubItems;
            foreach (var subItem in subItems)
            {
                var product = products != null && products.Any() ? products.FirstOrDefault(p => p.Id == subItem.Id) : null;
                if (product == null)
                {
                    continue;
                }
                var units = !string.IsNullOrWhiteSpace(product.UnitsJson)
                    ? JsonConvert.DeserializeObject<IEnumerable<UnitItem>>(product.UnitsJson)
                    : null;
                if (units == null || !units.Any())
                {
                    continue;
                }
                var mainUnit = units.FirstOrDefault(u => u.IsDefault.HasValue && u.IsDefault.Value);
                if (mainUnit == null)
                {
                    continue;
                }
                subItem.Unit = mainUnit.Unit;
                subItem.Quantity = decimal.Round(subItem.Quantity / mainUnit.Exchange, 2, MidpointRounding.AwayFromZero);
            }
        }
        var report = new OrderReportSummaryViewModel()
        {
            Total = total,
            TotalCost = totalCost,
            TotalChange = totalChange,
            TotalNoVAT = totalNoVat,
            TotalPaid = totalPaid,
            TotalRevenue = totalRevenue,
            TotalVAT = totalVat,
            TotalDiscount = totalDiscount,
            TotalShip = totalShip,
            TotalItems = reportItems.Count(),
            FromDate = from,
            EndDate = end,
            Items = reportItems
        };
        return report;
    }

    public async Task<OrderReportSummaryViewModel> BuildSalesReportByStaff(IEnumerable<int> orderIds, string lang, string userId, DateTime? dateFrom, DateTime? dateTo, int storeId, int staffId)
    {
        var from = dateFrom.HasValue ? dateFrom.Value : DateTime.Now.AddMonths(-6);
        var end = dateTo.HasValue ? dateTo.Value : DateTime.Now.AddMonths(3);

        var orders = await _repository.GetOrders(userId,
            from,
            end, 0, staffId, storeId, null, orderIds);

        if (orders == null || !orders.Any())
        {
            return new OrderReportSummaryViewModel() { FromDate = from, EndDate = end };
        }

        var arrOrders = orders.ToArray();
        var itemsCount = orders != null && orders.Any() ? orders.Count() : 0;

        decimal total = 0;
        decimal totalCost = 0;
        decimal totalDiscount = 0;
        decimal totalShip = 0;
        decimal totalChange = 0;
        decimal totalNoVat = 0;
        decimal totalVat = 0;
        decimal totalPaid = 0;
        decimal totalRevenue = 0;
        decimal totalStaffRevenue = 0;
        List<OrderReportItemViewModel> reportItems = new List<OrderReportItemViewModel>();
        Dictionary<int, OrderReportItemViewModel> dic = new Dictionary<int, OrderReportItemViewModel>();
        Dictionary<int, Dictionary<int, OrderReportItemViewModel>> dicProducts = new Dictionary<int, Dictionary<int, OrderReportItemViewModel>>();
        for (int i = 0; i < itemsCount; i++)
        {
            var order = arrOrders[i];

            decimal orderCost = 0;
            decimal orderDiscount = 0;
            decimal orderStaffRevenue = 0;
            var items = JsonConvert.DeserializeObject<IEnumerable<OrderItem>>(order.ItemsJson);
            var itemsArr = items != null && items.Any() ? items.ToArray() : new OrderItem[] { };
            for (int j = 0; j < itemsArr.Count(); j++)
            {
                var item = itemsArr[j];
                var priceInfo = item.PriceInfo;
                var count = item.Count.HasValue ? item.Count.Value : 0;
                var unit = item.Unit;
                if (!string.IsNullOrEmpty(item.BasicUnit)
                    && !string.IsNullOrEmpty(item.Unit)
                    && item.BasicUnit != item.Unit
                    && item.UnitExchange.HasValue)
                {
                    count = (item.Count.HasValue ? item.Count.Value : 0) * item.UnitExchange.Value;
                    unit = item.BasicUnit;
                }
                Dictionary<int, OrderReportItemViewModel> subDic = null;
                if (dicProducts.ContainsKey(order.StaffId))
                {
                    subDic = dicProducts[order.StaffId];
                }
                else
                {
                    subDic = new Dictionary<int, OrderReportItemViewModel>();
                    dicProducts.Add(order.StaffId, subDic);
                }
                var isExistsProduct = subDic.ContainsKey(item.ProductId);
                var netValue = (item.CostPrice.HasValue ? item.CostPrice.Value : 0) * count;
                var discount = item.Discount.HasValue ? item.Discount.Value : 0;
                var cost = item.TotalCostPrice.HasValue ? item.TotalCostPrice.Value : 0;
                var productTotal = item.Total.HasValue ? item.Total.Value : 0;
                var baseStaffAmount = priceInfo != null && (priceInfo.CollaboratorId != 0 || priceInfo.IsCollaboratorPrice)
                    ? priceInfo.Price * count 
                    : item.ShopPrice.HasValue 
                        ? item.ShopPrice.Value * count
                        : productTotal;
                OrderReportItemViewModel productReportItem;
                if (!isExistsProduct)
                {
                    productReportItem = new OrderReportItemViewModel
                    {
                        Id = item.ProductId,
                        Code = item.ProductCode,
                        Discount = discount,
                        Total = productTotal,
                        Cost = cost,
                        Unit = unit,
                        Quantity = count,
                        Contact = item.ProductName,
                        Revenue = productTotal - cost,
                        StaffRevenue = productTotal - baseStaffAmount,
                    };
                    subDic.Add(item.ProductId, productReportItem);
                }
                else
                {
                    productReportItem = subDic[item.ProductId];
                    productReportItem.Discount += discount;
                    productReportItem.Total += productTotal;
                    productReportItem.Cost += cost;
                    productReportItem.Quantity += count;
                    productReportItem.Revenue += productTotal - cost;
                    productReportItem.StaffRevenue += productTotal - baseStaffAmount;
                }
                if (item.TotalCostPrice.HasValue)
                {
                    orderCost += item.TotalCostPrice.Value;
                }
                orderDiscount += discount;
                orderStaffRevenue += productTotal - baseStaffAmount;
            }
            var isExists = dic.ContainsKey(order.StaffId);
            var staff = order.Staff;
            OrderReportItemViewModel reportItem = null;
            if (!isExists)
            {
                reportItem = new OrderReportItemViewModel
                {
                    Id = order.StaffId,
                    Total = order.Total,
                    Cost = orderCost,
                    Change = order.Change.HasValue ? order.Change.Value : 0,
                    NetValue = order.NetValue,
                    Paid = order.Paid.HasValue ? order.Paid.Value : orderCost,
                    Revenue = order.Total - orderCost - order.Tax - order.ShippingFee,
                    StaffRevenue = orderStaffRevenue,
                    Tax = order.Tax,
                    Discount = orderDiscount + order.DiscountOnTotal,
                    ShippingFee = order.ShippingFee,
                    Quantity = 1,
                    Staff = staff != null ? staff.Name : string.Empty
                };
                dic.Add(order.StaffId, reportItem);
            }
            else
            {
                reportItem = dic[order.StaffId];
                reportItem.Total += order.Total;
                reportItem.Cost += orderCost;
                reportItem.Change += order.Change.HasValue ? order.Change.Value : 0;
                reportItem.NetValue += order.NetValue;
                reportItem.Paid += order.Paid.HasValue ? order.Paid.Value : orderCost;
                reportItem.Revenue += order.Total - orderCost - order.Tax - order.ShippingFee;
                reportItem.StaffRevenue += orderStaffRevenue;
                reportItem.Tax += order.Tax;
                reportItem.Discount += orderDiscount + order.DiscountOnTotal;
                reportItem.ShippingFee = order.ShippingFee;
                reportItem.Quantity++;
            }
            total += order.Total;
            totalCost += orderCost;
            totalShip += order.ShippingFee;
            totalPaid += order.Paid.HasValue ? order.Paid.Value : orderCost;
            totalDiscount += orderDiscount + order.DiscountOnTotal;
            totalChange += order.Change.HasValue ? order.Change.Value : 0;
            totalVat += order.Tax;
            totalNoVat += order.NetValue;
            totalRevenue += order.Total - orderCost - order.Tax - order.ShippingFee;
            totalStaffRevenue += orderStaffRevenue;
        }
        List<int> ids = new List<int>();
        foreach (var key in dic.Keys)
        {
            var item = dic[key];
            item.SubItems = new List<OrderReportItemViewModel>();
            var subDic = dicProducts[key];
            foreach (var key2 in subDic.Keys)
            {
                var subItem = subDic[key2];
                item.SubItems.Add(subItem);
                ids.Add(subItem.Id);
            }
            reportItems.Add(item);
        }
        var products = await _productRepository.GetProductsByIds(userId, ids);
        foreach (var item in reportItems)
        {
            var subItems = item.SubItems;
            foreach (var subItem in subItems)
            {
                var product = products != null && products.Any() ? products.FirstOrDefault(p => p.Id == subItem.Id) : null;
                if (product == null)
                {
                    continue;
                }
                var units = !string.IsNullOrWhiteSpace(product.UnitsJson)
                    ? JsonConvert.DeserializeObject<IEnumerable<UnitItem>>(product.UnitsJson)
                    : null;
                if (units == null || !units.Any())
                {
                    continue;
                }
                var mainUnit = units.FirstOrDefault(u => u.IsDefault.HasValue && u.IsDefault.Value);
                if (mainUnit == null)
                {
                    continue;
                }
                subItem.Unit = mainUnit.Unit;
                subItem.Quantity = decimal.Round(subItem.Quantity / mainUnit.Exchange, 2, MidpointRounding.AwayFromZero);
            }
        }
        var report = new OrderReportSummaryViewModel()
        {
            Total = total,
            TotalCost = totalCost,
            TotalChange = totalChange,
            TotalNoVAT = totalNoVat,
            TotalPaid = totalPaid,
            TotalRevenue = totalRevenue,
            TotalStaffRevenue = totalStaffRevenue,
            TotalVAT = totalVat,
            TotalDiscount = totalDiscount,
            TotalShip = totalShip,
            TotalItems = reportItems.Count(),
            FromDate = from,
            EndDate = end,
            Items = reportItems
        };
        return report;
    }

    private async Task<string> CreateSalesReportSummary(string lang, string userId, DateTime? dateFrom, DateTime? dateTo)
    {
        var templateFileName = "SalesReport.xlsx";
        var pathTemplate = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
        var pathAndFileNameTemplate = Path.Combine(pathTemplate, templateFileName);
        using (var package = new ExcelPackage(new System.IO.FileInfo(pathAndFileNameTemplate)))
        {
            var sheet = package.Workbook.Worksheets[0];

            if (isVn(lang))
            {
                sheet.Cells[1, 1].Value = "BÁO CÁO DOANH THU - tổng hợp theo đơn hàng";
                sheet.Cells[2, 1].Value = "Từ ngày:";
                sheet.Cells[2, 3].Value = "Đến ngày:";
                sheet.Cells[4, 2].Value = "Mã đơn";
                sheet.Cells[4, 3].Value = "Ngày";
                sheet.Cells[4, 4].Value = "Chiết khấu";
                sheet.Cells[4, 5].Value = "Tổng";
                sheet.Cells[4, 6].Value = "Tiền ship";
                sheet.Cells[4, 7].Value = "Khách đưa";
                sheet.Cells[4, 8].Value = "Tiền thừa";
                sheet.Cells[4, 9].Value = "Tên khách";
                sheet.Cells[4, 10].Value = "Ghi chú";
                sheet.Cells[4, 11].Value = "Trạng thái";
                sheet.Cells[4, 12].Value = "Tổng toàn bộ";
                sheet.Cells[4, 13].Value = "Chi phí";
                sheet.Cells[6, 1].Value = "TỔNG:";
                sheet.Cells[7, 1].Value = "Chi phí:";
            }

            var fileName = Guid.NewGuid().ToString("N") + ".xlsx";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//downloads");
            var pathAndFileName = Path.Combine(path, fileName);

            var orders = await _repository.GetOrders(userId,
                dateFrom.HasValue ? dateFrom.Value : DateTime.Now.AddMonths(-6),
                dateTo.HasValue ? dateTo.Value : DateTime.Now.AddMonths(3), 0, 0, 0, null);
            if (orders == null || !orders.Any())
            {
                package.SaveAs(new System.IO.FileInfo(pathAndFileName));
                return fileName;
            }
            var arrOrders = orders.ToArray();
            var itemsCount = orders != null && orders.Any() ? orders.Count() : 0;
            sheet.Cells[2, 2].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            sheet.Cells[2, 2].Value = dateFrom;
            sheet.Cells[2, 4].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            sheet.Cells[2, 4].Value = dateTo;
            sheet.InsertRow(5, orders.Count());
            var currentRow = 5;
            decimal total = 0;
            decimal totalCost = 0;
            for (int i = 0; i < itemsCount; i++)
            {
                var order = arrOrders[i];
                sheet.Cells[currentRow, 1].Value = i + 1;
                sheet.Cells[currentRow, 2].Value = order.OrderCode;
                sheet.Cells[currentRow, 3].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
                sheet.Cells[currentRow, 3].Value = order.CreatedAt;
                sheet.Cells[currentRow, 4].Value = order.Discount;
                sheet.Cells[currentRow, 5].Value = order.NetValue;
                sheet.Cells[currentRow, 6].Value = order.ShippingFee;
                sheet.Cells[currentRow, 7].Value = order.Paid;
                sheet.Cells[currentRow, 8].Value = order.Change;
                sheet.Cells[currentRow, 9].Value = order.Contact != null && order.ContactId > 0
                    ? order.Contact.FullName + (!string.IsNullOrWhiteSpace(order.Contact.Mobile) ? " | " + order.Contact.Mobile : string.Empty)
                    : order.ContactName
                        + (!string.IsNullOrWhiteSpace(order.ContactPhone) ? " | " + order.ContactPhone : string.Empty);
                //sheet.Cells[currentRow, 10].Value = or;
                sheet.Cells[currentRow, 11].Value = GetOrderStatus(order.Status, lang);
                sheet.Cells[currentRow, 12].Value = order.Total;
                decimal orderCost = 0;
                var items = JsonConvert.DeserializeObject<IEnumerable<OrderItem>>(order.ItemsJson);
                var itemsArr = items != null && items.Any() ? items.ToArray() : new OrderItem[] { };
                for (int j = 0; j < itemsArr.Count(); j++)
                {
                    var product = itemsArr[j];
                    if (product.TotalCostPrice.HasValue)
                    {
                        orderCost += product.TotalCostPrice.Value;
                    }
                }
                if (orderCost > 0)
                {
                    sheet.Cells[currentRow, 13].Value = orderCost;
                }
                totalCost += orderCost;
                total += order.Total;
                currentRow++;
            }
            sheet.Cells[6 + itemsCount, 2].Value = total;
            if (totalCost >= 0)
            {
                sheet.Cells[6 + itemsCount + 1, 2].Value = totalCost;
            }
            package.SaveAs(new System.IO.FileInfo(pathAndFileName));
            return fileName;
        }
    }

    private async Task<string> CreateSalesReportFileByOrder(IEnumerable<int> orderIds, string lang, string userId, DateTime? dateFrom, DateTime? dateTo, int storeId, int staffId)
    {
        var templateFileName = "SalesReportByOrder.xlsx";
        var pathTemplate = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
        var pathAndFileNameTemplate = Path.Combine(pathTemplate, templateFileName);
        using (var package = new ExcelPackage(new System.IO.FileInfo(pathAndFileNameTemplate)))
        {
            var sheet = package.Workbook.Worksheets[0];

            if (isVn(lang))
            {
                sheet.Cells[1, 1].Value = "BÁO CÁO BÁN HÀNG - theo đơn hàng";
                sheet.Cells[2, 1].Value = "Từ ngày:";
                sheet.Cells[2, 3].Value = "Đến ngày:";
                sheet.Cells[4, 1].Value = "Mã đơn";
                sheet.Cells[4, 2].Value = "Sản phẩm";
                sheet.Cells[4, 3].Value = "Đơn giá";
                sheet.Cells[4, 4].Value = "Số lượng";
                sheet.Cells[4, 5].Value = "Tổng";
                sheet.Cells[4, 6].Value = "Tổng (k VAT)";
                sheet.Cells[4, 7].Value = "VAT";
                sheet.Cells[4, 8].Value = "Phí ship (COD)";
                sheet.Cells[4, 9].Value = "Khách đưa";
                sheet.Cells[4, 10].Value = "Tiền thừa";
                sheet.Cells[4, 11].Value = "Chiết khấu";
                sheet.Cells[4, 12].Value = "Chi phí";
                sheet.Cells[4, 13].Value = "Lợi nhuận";
                sheet.Cells[4, 14].Value = "Trạng thái";
                sheet.Cells[4, 15].Value = "Khách";
                sheet.Cells[4, 16].Value = "Địa chỉ khách";
                sheet.Cells[4, 17].Value = "Nhân viên";
                sheet.Cells[4, 18].Value = "Mã vận đơn";
                sheet.Cells[4, 19].Value = "Đối tác vận";
                sheet.Cells[4, 20].Value = "Người giao hàng";
                sheet.Cells[4, 21].Value = "SĐT Shipper";
                sheet.Cells[4, 22].Value = "Địa chỉ giao";
                sheet.Cells[4, 23].Value = "Ngày";
                sheet.Cells[4, 24].Value = "Lợi nhuận nhân viên";
                sheet.Cells[3, 24].Value = "* Tính bởi: Tổng thực bán - tổng theo giá dành cho nhân viên (hoặc giá của shop) - phí ship (nếu shop trả ship)";
                sheet.Cells[6, 1].Value = "TỔNG:";
            }

            var fileName = Guid.NewGuid().ToString("N") + ".xlsx";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//downloads");
            var pathAndFileName = Path.Combine(path, fileName);

            var report = await this.BuildSalesReportByOrder(orderIds, lang, userId, dateFrom, dateTo, storeId, staffId);
            if (report == null)
            {
                package.SaveAs(new System.IO.FileInfo(pathAndFileName));
                return fileName;
            }

            var itemsCount = 0;
            var arrOrders = report.Items;
            itemsCount = report.Items != null && report.Items.Any() ? report.Items.Count() : 0;
            sheet.Cells[2, 2].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            sheet.Cells[2, 2].Value = dateFrom;
            sheet.Cells[2, 4].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            sheet.Cells[2, 4].Value = dateTo;
            decimal totalStaffRevenue = 0;
            sheet.InsertRow(5, report.Items != null && report.Items.Any() ? report.Items.Count() : 0);
            var currentRow = 5;
            for (int i = 0; i < itemsCount; i++)
            {
                var order = arrOrders[i];
                sheet.Cells[currentRow, 1].Value = order.Code;
                sheet.Cells[currentRow, 5].Value = order.Total;
                sheet.Cells[currentRow, 6].Value = order.NetValue;
                sheet.Cells[currentRow, 7].Value = order.Tax;
                sheet.Cells[currentRow, 8].Value = order.ShippingFee;
                sheet.Cells[currentRow, 9].Value = order.Paid;
                sheet.Cells[currentRow, 10].Value = order.Change;
                sheet.Cells[currentRow, 11].Value = order.Discount;
                sheet.Cells[currentRow, 12].Value = order.Cost;
                sheet.Cells[currentRow, 13].Value = order.Revenue;
                sheet.Cells[currentRow, 14].Value = GetOrderStatus(order.Status, lang);
                sheet.Cells[currentRow, 15].Value = order.Contact;
                sheet.Cells[currentRow, 16].Value = order.ContactAddress;
                sheet.Cells[currentRow, 17].Value = order.Staff;
                sheet.Cells[currentRow, 18].Value = order.BillOfLadingCode;
                sheet.Cells[currentRow, 19].Value = order.ShippingPartner;
                sheet.Cells[currentRow, 20].Value = order.ShipperName;
                sheet.Cells[currentRow, 21].Value = order.ShipperPhone;
                sheet.Cells[currentRow, 22].Value = order.DeliveryAddress;
                sheet.Cells[currentRow, 23].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
                sheet.Cells[currentRow, 23].Value = order.CreatedAt;
                var oldRow = currentRow;
                currentRow++;
                decimal orderStaffProfit = 0;
                if (order.Items != null && order.Items.Any())
                {
                    sheet.InsertRow(currentRow, order.Items != null && order.Items.Any() ? order.Items.Count() : 0);
                    var j = 0;
                    foreach (var product in order.Items)
                    {
                        j++;
                        var productName = !string.IsNullOrEmpty(product.ProductCode) ? product.ProductCode + "-" + product.ProductName : product.ProductName;
                        if (!string.IsNullOrEmpty(product.Unit)) {
                            productName += " (" + product.Unit + ")";
                        }
                        sheet.Cells[currentRow, 2].Style.WrapText = true;
                        sheet.Cells[currentRow, 2].Value = productName;
                        sheet.Cells[currentRow, 3].Value = product.Price;
                        sheet.Cells[currentRow, 4].Value = product.Count;
                        sheet.Cells[currentRow, 5].Value = product.Total;
                        sheet.Cells[currentRow, 5].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                        
                        var total = product.Total.HasValue ? product.Total.Value : 0;
                        var staffAmount = product.PriceInfo != null && (product.PriceInfo.CollaboratorId != 0 || product.PriceInfo.IsCollaboratorPrice)  
                            ? product.PriceInfo.Price * product.Count
                            : product.ShopPrice.HasValue 
                                ? product.ShopPrice.Value  * product.Count
                                : total;
                        var staffProfit = total - staffAmount;
                        orderStaffProfit += staffProfit.Value;
                        sheet.Cells[currentRow, 24].Value = staffProfit;
                        currentRow++;                        
                    }
                    orderStaffProfit -= !order.ShipCostOnCustomer  ? order.ShippingFee : 0;
                    sheet.Cells[oldRow, 24].Value = orderStaffProfit;
                    totalStaffRevenue += orderStaffProfit;
                }
            }
            sheet.Cells[1+ currentRow, 5].Value = report.Total;
            sheet.Cells[1+ currentRow, 6].Value = report.TotalNoVAT;
            sheet.Cells[1+ currentRow, 7].Value = report.TotalVAT;
            sheet.Cells[1+ currentRow, 8].Value = report.TotalShip;
            sheet.Cells[1+ currentRow, 9].Value = report.TotalPaid;
            sheet.Cells[1+ currentRow, 10].Value = report.TotalChange;
            sheet.Cells[1+ currentRow, 11].Value = report.TotalDiscount;
            sheet.Cells[1+ currentRow, 12].Value = report.TotalCost;
            sheet.Cells[1+ currentRow, 13].Value = report.TotalRevenue;
            sheet.Cells[1+ currentRow, 24].Value = totalStaffRevenue;
            for (int i = 1; i<= 24; i++) {
                for (int j = 5; j <= currentRow; j++) {
                    sheet.Cells[j, i].Style.WrapText = true;
                    sheet.Cells[j, i].Style.Border.BorderAround(ExcelBorderStyle.Thin);
                }
            }
            package.SaveAs(new System.IO.FileInfo(pathAndFileName));
            return fileName;
        }
    }

    private async Task<string> CreateSalesReportFileByProduct(IEnumerable<int> orderIds, string lang, string userId, DateTime? dateFrom, DateTime? dateTo, int storeId, int staffId)
    {
        var templateFileName = "SalesReportByProduct.xlsx";
        var pathTemplate = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
        var pathAndFileNameTemplate = Path.Combine(pathTemplate, templateFileName);
        using (var package = new ExcelPackage(new System.IO.FileInfo(pathAndFileNameTemplate)))
        {
            var sheet = package.Workbook.Worksheets[0];

            if (isVn(lang))
            {
                sheet.Cells[1, 1].Value = "BÁO CÁO BÁN HÀNG - theo sản phẩm";
                sheet.Cells[2, 1].Value = "Từ ngày:";
                sheet.Cells[2, 3].Value = "Đến ngày:";
                sheet.Cells[4, 1].Value = "Sản phẩm";
                sheet.Cells[4, 2].Value = "Tổng";
                sheet.Cells[4, 3].Value = "Số lượng";
                sheet.Cells[4, 4].Value = "Chi phí";
                sheet.Cells[4, 5].Value = "Chiết khấu";
                sheet.Cells[4, 6].Value = "Lợi nhuận";
                sheet.Cells[4, 7].Value = "Đơn vị";
                sheet.Cells[4, 8].Value = "Đơn giá bán";
                sheet.Cells[4, 9].Value = "Đơn giá nhập";
                sheet.Cells[6, 1].Value = "TỔNG:";
                sheet.Cells[7, 1].Value = "Tổng (ko VAT):";
                sheet.Cells[8, 1].Value = "Tổng VAT:";
                sheet.Cells[9, 1].Value = "Tổng ship (COD):";
                sheet.Cells[10, 1].Value = "Tổng chiết khấu:";
                sheet.Cells[11, 1].Value = "Tổng chi phí:";
                sheet.Cells[12, 1].Value = "Tổng lợi nhuận:";
            }

            var fileName = Guid.NewGuid().ToString("N") + ".xlsx";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//downloads");
            var pathAndFileName = Path.Combine(path, fileName);

            var report = await this.BuildSalesReportByProduct(orderIds, lang, userId, dateFrom, dateTo, storeId, staffId);
            if (report == null)
            {
                package.SaveAs(new System.IO.FileInfo(pathAndFileName));
                return fileName;
            }

            var arrOrders = report.Items;
            var itemsCount = report.Items != null && report.Items.Any() ? report.Items.Count() : 0;
            sheet.Cells[2, 2].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            sheet.Cells[2, 2].Value = dateFrom;
            sheet.Cells[2, 4].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            sheet.Cells[2, 4].Value = dateTo;
            sheet.InsertRow(5, itemsCount);
            var currentRow = 5;
            for (int i = 0; i < itemsCount; i++)
            {
                var product = arrOrders[i];
                sheet.Cells[currentRow, 1].Value = product.Contact;
                sheet.Cells[currentRow, 2].Value = product.Total;
                sheet.Cells[currentRow, 3].Value = product.Quantity;
                sheet.Cells[currentRow, 4].Value = product.Cost;
                sheet.Cells[currentRow, 5].Value = product.Discount;
                sheet.Cells[currentRow, 6].Value = product.Revenue;
                sheet.Cells[currentRow, 7].Value = product.Unit;
                sheet.Cells[currentRow, 8].Value = product.Quantity > 0 ? product.Total / product.Quantity : 0;
                sheet.Cells[currentRow, 9].Value = product.Quantity > 0 ? product.Cost / product.Quantity : 0;
                currentRow++;
            }
            sheet.Cells[6 + itemsCount, 2].Value = report.Total;
            sheet.Cells[7 + itemsCount, 2].Value = report.TotalNoVAT;
            sheet.Cells[8 + itemsCount, 2].Value = report.TotalVAT;
            sheet.Cells[9 + itemsCount, 2].Value = report.TotalShip;
            sheet.Cells[10 + itemsCount, 2].Value = report.TotalDiscount;
            sheet.Cells[11 + itemsCount, 2].Value = report.TotalCost;
            sheet.Cells[12 + itemsCount, 2].Value = report.TotalRevenue;
            package.SaveAs(new System.IO.FileInfo(pathAndFileName));
            return fileName;
        }
    }

    private async Task<string> CreateSalesReportFileByCustomer(IEnumerable<int> orderIds, string lang, string userId, DateTime? dateFrom, DateTime? dateTo, int storeId, int staffId)
    {
        var templateFileName = "SalesReportByCustomerAndProduct.xlsx";
        var pathTemplate = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
        var pathAndFileNameTemplate = Path.Combine(pathTemplate, templateFileName);
        using (var package = new ExcelPackage(new System.IO.FileInfo(pathAndFileNameTemplate)))
        {
            var sheet = package.Workbook.Worksheets[0];

            if (isVn(lang))
            {
                sheet.Cells[1, 1].Value = "BÁO CÁO BÁN HÀNG - theo khách hàng";
                sheet.Cells[2, 1].Value = "Từ ngày:";
                sheet.Cells[2, 3].Value = "Đến ngày:";
                sheet.Cells[4, 1].Value = "Khách";
                sheet.Cells[4, 2].Value = "Sản phẩm";
                sheet.Cells[4, 3].Value = "Số lượng";
                sheet.Cells[4, 4].Value = "Tổng";
                sheet.Cells[4, 5].Value = "Tổng (k VAT)";
                sheet.Cells[4, 6].Value = "VAT";
                sheet.Cells[4, 7].Value = "Phí ship (COD)";
                sheet.Cells[4, 8].Value = "Chiết khấu";
                sheet.Cells[4, 9].Value = "Chi phí";
                sheet.Cells[4, 10].Value = "Lợi nhuận";
                sheet.Cells[6, 1].Value = "TỔNG:";
            }

            var fileName = Guid.NewGuid().ToString("N") + ".xlsx";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//downloads");
            var pathAndFileName = Path.Combine(path, fileName);

            var report = await this.BuildSalesReportByCustomerAndProduct(orderIds, lang, userId, dateFrom, dateTo, storeId, staffId);
            if (report == null)
            {
                package.SaveAs(new System.IO.FileInfo(pathAndFileName));
                return fileName;
            }

            var arrOrders = report.Items;
            var itemsCount = report.Items != null && report.Items.Any() ? report.Items.Count() : 0;
            sheet.Cells[2, 2].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            sheet.Cells[2, 2].Value = dateFrom;
            sheet.Cells[2, 4].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            sheet.Cells[2, 4].Value = dateTo;
            sheet.InsertRow(5, itemsCount);
            var currentRow = 5;
            for (int i = 0; i < itemsCount; i++)
            {
                var order = arrOrders[i];
                sheet.Cells[currentRow, 1].Value = !string.IsNullOrEmpty(order.Contact)
                    ? order.Contact
                    : isVn(lang)
                        ? "Khách vãng lai"
                        : "Guests"
                ;
                sheet.Cells[currentRow, 4].Value = order.Total;
                sheet.Cells[currentRow, 5].Value = order.NetValue;
                sheet.Cells[currentRow, 6].Value = order.Tax;
                sheet.Cells[currentRow, 7].Value = order.ShippingFee;
                sheet.Cells[currentRow, 8].Value = order.Discount;
                sheet.Cells[currentRow, 9].Value = order.Cost;
                sheet.Cells[currentRow, 10].Value = order.Revenue;
                currentRow++;
                var arrProducts = order.SubItems;
                sheet.InsertRow(currentRow, arrProducts.Count());
                for (int j = 0; j < arrProducts.Count(); j++)
                {
                    var product = arrProducts[j];
                    sheet.Cells[currentRow, 2].Value = product.Contact;
                    sheet.Cells[currentRow, 4].Value = product.Total;
                    sheet.Cells[currentRow, 3].Value = product.Quantity;
                    sheet.Cells[currentRow, 9].Value = product.Cost;
                    sheet.Cells[currentRow, 8].Value = product.Discount;
                    sheet.Cells[currentRow, 10].Value = product.Revenue;
                    currentRow++;
                }
            }
            sheet.Cells[currentRow + 1, 4].Value = report.Total;
            sheet.Cells[currentRow + 1, 5].Value = report.TotalNoVAT;
            sheet.Cells[currentRow + 1, 6].Value = report.TotalVAT;
            sheet.Cells[currentRow + 1, 7].Value = report.TotalShip;
            sheet.Cells[currentRow + 1, 8].Value = report.TotalDiscount;
            sheet.Cells[currentRow + 1, 9].Value = report.TotalCost;
            sheet.Cells[currentRow + 1, 10].Value = report.TotalRevenue;
            package.SaveAs(new System.IO.FileInfo(pathAndFileName));
            return fileName;
        }
    }

    private async Task<string> CreateSalesReportFileByStaff(IEnumerable<int> orderIds, string lang, string userId, DateTime? dateFrom, DateTime? dateTo, int storeId, int staffId)
    {
        var templateFileName = "SalesReportByStaff.xlsx";
        var pathTemplate = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
        var pathAndFileNameTemplate = Path.Combine(pathTemplate, templateFileName);
        using (var package = new ExcelPackage(new System.IO.FileInfo(pathAndFileNameTemplate)))
        {
            var sheet = package.Workbook.Worksheets[0];

            if (isVn(lang))
            {
                sheet.Cells[1, 1].Value = "BÁO CÁO BÁN HÀNG - theo nhân viên";
                sheet.Cells[2, 1].Value = "Từ ngày:";
                sheet.Cells[2, 3].Value = "Đến ngày:";
                sheet.Cells[4, 1].Value = "Nhân viên";
                sheet.Cells[4, 2].Value = "Sản phẩm";
                sheet.Cells[4, 3].Value = "Số lượng";
                sheet.Cells[4, 4].Value = "Tổng";
                sheet.Cells[4, 5].Value = "Tổng (k VAT)";
                sheet.Cells[4, 6].Value = "VAT";
                sheet.Cells[4, 7].Value = "Phí ship (COD)";
                sheet.Cells[4, 8].Value = "Chiết khấu";
                sheet.Cells[4, 9].Value = "Chi phí";
                sheet.Cells[4, 10].Value = "Lợi nhuận";
                sheet.Cells[4, 11].Value = "Lợi nhuận nhân viên";
                sheet.Cells[6, 1].Value = "TỔNG:";
            }

            var fileName = Guid.NewGuid().ToString("N") + ".xlsx";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//downloads");
            var pathAndFileName = Path.Combine(path, fileName);

            var report = await this.BuildSalesReportByStaff(orderIds, lang, userId, dateFrom, dateTo, storeId, staffId);
            if (report == null)
            {
                package.SaveAs(new System.IO.FileInfo(pathAndFileName));
                return fileName;
            }

            var arrOrders = report.Items;
            var itemsCount = report.Items != null && report.Items.Any() ? report.Items.Count() : 0;
            sheet.Cells[2, 2].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            sheet.Cells[2, 2].Value = dateFrom;
            sheet.Cells[2, 4].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            sheet.Cells[2, 4].Value = dateTo;
            sheet.InsertRow(5, itemsCount);
            var currentRow = 5;
            for (int i = 0; i < itemsCount; i++)
            {
                var order = arrOrders[i];
                sheet.Cells[currentRow, 1].Value = !string.IsNullOrEmpty(order.Staff)
                    ? order.Staff
                    : isVn(lang)
                        ? "Khác"
                        : "Others"
                ;
                sheet.Cells[currentRow, 4].Value = order.Total;
                sheet.Cells[currentRow, 5].Value = order.NetValue;
                sheet.Cells[currentRow, 6].Value = order.Tax;
                sheet.Cells[currentRow, 7].Value = order.ShippingFee;
                sheet.Cells[currentRow, 8].Value = order.Discount;
                sheet.Cells[currentRow, 9].Value = order.Cost;
                sheet.Cells[currentRow, 10].Value = order.Revenue;
                sheet.Cells[currentRow, 11].Value = order.StaffRevenue;
                currentRow++;
                var arrProducts = order.SubItems;
                sheet.InsertRow(currentRow, arrProducts.Count());
                for (int j = 0; j < arrProducts.Count(); j++)
                {
                    var product = arrProducts[j];
                    sheet.Cells[currentRow, 2].Value = product.Contact;
                    sheet.Cells[currentRow, 4].Value = product.Total;
                    sheet.Cells[currentRow, 3].Value = product.Quantity;
                    sheet.Cells[currentRow, 9].Value = product.Cost;
                    sheet.Cells[currentRow, 8].Value = product.Discount;
                    sheet.Cells[currentRow, 10].Value = product.Revenue;
                    sheet.Cells[currentRow, 11].Value = product.StaffRevenue;
                    currentRow++;
                }
            }
            sheet.Cells[currentRow + 1, 4].Value = report.Total;
            sheet.Cells[currentRow + 1, 5].Value = report.TotalNoVAT;
            sheet.Cells[currentRow + 1, 6].Value = report.TotalVAT;
            sheet.Cells[currentRow + 1, 7].Value = report.TotalShip;
            sheet.Cells[currentRow + 1, 8].Value = report.TotalDiscount;
            sheet.Cells[currentRow + 1, 9].Value = report.TotalCost;
            sheet.Cells[currentRow + 1, 10].Value = report.TotalRevenue;
            sheet.Cells[currentRow + 1, 11].Value = report.TotalStaffRevenue;
            package.SaveAs(new System.IO.FileInfo(pathAndFileName));
            return fileName;
        }
    }

    private async Task<string> CreateSalesReportDetail(string lang, string userId, DateTime? dateFrom, DateTime? dateTo)
    {
        var templateFileName = "SalesReportDetail.xlsx";
        var pathTemplate = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
        var pathAndFileNameTemplate = Path.Combine(pathTemplate, templateFileName);
        using (var package = new ExcelPackage(new System.IO.FileInfo(pathAndFileNameTemplate)))
        {
            var sheet = package.Workbook.Worksheets[0];

            if (isVn(lang))
            {
                sheet.Cells[1, 1].Value = "BÁO CÁO DOANH THU - chi tiết theo đơn hàng";
                sheet.Cells[2, 1].Value = "Từ ngày:";
                sheet.Cells[2, 3].Value = "Đến ngày:";
                sheet.Cells[4, 2].Value = "Mã đơn";
                sheet.Cells[4, 3].Value = "Ngày";
                sheet.Cells[4, 4].Value = "Sản phẩm";
                sheet.Cells[4, 5].Value = "Đơn vị";
                sheet.Cells[4, 6].Value = "Đơn giá";
                sheet.Cells[4, 7].Value = "Số lượng";
                sheet.Cells[4, 8].Value = "Chiết khấu";
                sheet.Cells[4, 9].Value = "Khách hàng";
                sheet.Cells[4, 10].Value = "THANH TOÁN";
                sheet.Cells[4, 11].Value = "Chi phí";
                sheet.Cells[6, 1].Value = "TỔNG:";
                sheet.Cells[7, 1].Value = "Tổng chi phí:";
            }

            var fileName = Guid.NewGuid().ToString("N") + ".xlsx";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//downloads");
            var pathAndFileName = Path.Combine(path, fileName);

            var orders = await _repository.GetOrders(userId,
                dateFrom.HasValue ? dateFrom.Value : DateTime.Now.AddMonths(-6),
                dateTo.HasValue ? dateTo.Value : DateTime.Now.AddMonths(3), 0, 0, 0, null);
            if (orders == null || !orders.Any())
            {
                package.SaveAs(new System.IO.FileInfo(pathAndFileName));
                return fileName;
            }
            var arrOrders = orders.ToArray();
            var itemsCount = orders != null && orders.Any() ? orders.Count() : 0;
            sheet.Cells[2, 2].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            sheet.Cells[2, 2].Value = dateFrom;
            sheet.Cells[2, 4].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            sheet.Cells[2, 4].Value = dateTo;
            var currentRow = 5;
            decimal total = 0;
            decimal totalCost = 0;
            int index = 0;
            for (int i = 0; i < itemsCount; i++)
            {
                var order = arrOrders[i];
                var items = JsonConvert.DeserializeObject<IEnumerable<OrderItem>>(order.ItemsJson);
                var itemsArr = items != null && items.Any() ? items.ToArray() : new OrderItem[] { };
                sheet.InsertRow(currentRow, itemsArr.Count());
                for (int j = 0; j < itemsArr.Count(); j++)
                {
                    var product = itemsArr[j];
                    sheet.Cells[currentRow, 1].Value = index + 1;
                    sheet.Cells[currentRow, 2].Value = order.OrderCode;
                    sheet.Cells[currentRow, 3].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
                    sheet.Cells[currentRow, 3].Value = order.CreatedAt;
                    sheet.Cells[currentRow, 4].Value = product.ProductName;
                    sheet.Cells[currentRow, 5].Value = product.Unit;
                    sheet.Cells[currentRow, 6].Value = product.Price;
                    sheet.Cells[currentRow, 7].Value = product.Count;
                    sheet.Cells[currentRow, 8].Value = product.Discount.HasValue ? product.Discount.Value : 0;
                    sheet.Cells[currentRow, 9].Value = order.Contact != null && order.ContactId > 0
                        ? order.Contact.FullName + (!string.IsNullOrWhiteSpace(order.Contact.Mobile) ? " | " + order.Contact.Mobile : string.Empty)
                        : order.ContactName
                            + (!string.IsNullOrWhiteSpace(order.ContactPhone) ? " | " + order.ContactPhone : string.Empty);
                    sheet.Cells[currentRow, 10].Value = product.Total;
                    if (product.TotalCostPrice.HasValue)
                    {
                        totalCost += product.TotalCostPrice.Value;
                        sheet.Cells[currentRow, 11].Value = product.TotalCostPrice.Value;
                    }
                    index++;
                    currentRow++;
                }
                total += order.Total;
            }
            sheet.Cells[currentRow + 1, 2].Value = total;
            if (totalCost > 0)
            {
                sheet.Cells[currentRow + 2, 2].Value = totalCost;
            }
            package.SaveAs(new System.IO.FileInfo(pathAndFileName));
            return fileName;
        }
    }

    private async Task<string> CreateInventoryReportDetail(string lang, string userId, DateTime? dateFrom, DateTime? dateTo, int productId, int storeId)
    {
        var templateFileName = "ProductNotes.xlsx";
        var pathTemplate = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
        var pathAndFileNameTemplate = Path.Combine(pathTemplate, templateFileName);
        using (var package = new ExcelPackage(new System.IO.FileInfo(pathAndFileNameTemplate)))
        {
            var sheet = package.Workbook.Worksheets[0];

            if (isVn(lang))
            {
                sheet.Cells[1, 1].Value = "BÁO CÁO NHẬP/XUẤT HÀNG";
                sheet.Cells[2, 1].Value = "Từ ngày:";
                sheet.Cells[2, 3].Value = "Đến ngày:";
                sheet.Cells[4, 2].Value = "Mã SP";
                sheet.Cells[4, 3].Value = "Tên SP";
                sheet.Cells[4, 4].Value = "Đơn vị";
                sheet.Cells[4, 5].Value = "Đơn giá";
                sheet.Cells[4, 6].Value = "Số lượng";
                sheet.Cells[4, 7].Value = "Chiết khấu";
                sheet.Cells[4, 8].Value = "Ghi chú";
                sheet.Cells[4, 9].Value = "Giá nhập (ngoại tệ)";
                sheet.Cells[4, 10].Value = "Thanh toán (ngoại tệ)";
                sheet.Cells[4, 11].Value = "Nhà cung cấp";
                sheet.Cells[4, 12].Value = "Ngày";
                sheet.Cells[4, 13].Value = "Thanh toán";
                sheet.Cells[4, 14].Value = "Nhân viên";
                sheet.Cells[4, 15].Value = "Giá cho nhân viên";
                sheet.Cells[4, 16].Value = "Lợi nhuận cho nv";
                sheet.Cells[6, 1].Value = "TỔNG:";
            }

            var fileName = Guid.NewGuid().ToString("N") + ".xlsx";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//downloads");
            var pathAndFileName = Path.Combine(path, fileName);

            var notes = await _productRepository.GetNotes(userId,
                dateFrom.HasValue ? dateFrom.Value : DateTime.Now.AddMonths(-6),
                dateTo.HasValue ? dateTo.Value : DateTime.Now.AddMonths(3), 0, productId, 0, 0, 0, storeId, 0, 0, true);
            if (notes == null || !notes.Any())
            {
                package.SaveAs(new System.IO.FileInfo(pathAndFileName));
                return fileName;
            }
            var arr = notes.ToArray();
            sheet.Cells[2, 2].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            sheet.Cells[2, 2].Value = dateFrom;
            sheet.Cells[2, 4].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            sheet.Cells[2, 4].Value = dateTo;
            var currentRow = 5;
            sheet.InsertRow(currentRow, arr.Count());
            decimal total = 0;
            int index = 0;
            for (int i = 0; i < arr.Count(); i++)
            {
                var note = arr[i];
                var quantity = note.OrderId > 0
                    ? -note.Quantity
                    : note.ReceivedNoteId > 0
                        ? note.Quantity
                        : note.TradeId > 0 && note.Trade != null
                            ? note.Trade.IsReceived
                                ? -note.Quantity
                                : note.Quantity
                            : note.Quantity;
                var amount = note.OrderId > 0
                    ? note.Amount
                    : note.ReceivedNoteId > 0
                        ? -note.Amount
                        : note.Amount;
                var amountForeign = note.OrderId > 0
                    ? note.AmountForeign
                    : note.ReceivedNoteId > 0
                        ? -note.AmountForeign
                        : note.AmountForeign;
                var description = note.Note;
                if (string.IsNullOrWhiteSpace(description))
                {
                    description = note.OrderId > 0
                    ? (isVn(lang) ? "Đơn hàng #" : "Order #") + note.Order.OrderCode.ToString()
                    : note.ReceivedNoteId > 0
                        ? (isVn(lang) ? "Phiếu nhập hàng #" : "Product received note #") + note.ReceivedNoteId.ToString()
                        : note.TradeId > 0 && note.Trade != null
                            ? note.Trade.IsReceived
                                ? (isVn(lang) ? "Bán lẻ, giao dịch #" : "Sale, transaction #") + note.Trade.Id.ToString()
                                : (isVn(lang) ? "Mua lẻ, giao dịch #" : "Buy, transaction #") + note.Trade.Id.ToString()
                            : string.Empty;
                }
                PriceInfo priceInfo = null;
                string staff = note.Order != null && note.Order.Staff != null ? note.Order.Staff.Name : string.Empty;
                if (note.OrderId > 0)
                {
                    var items = JsonConvert.DeserializeObject<IEnumerable<OrderItem>>(note.Order.ItemsJson);
                    var itemsArr = items != null && items.Any() ? items.ToArray() : new OrderItem[] { };
                    var item = itemsArr.FirstOrDefault(ia => ia.ProductId == note.ProductId && ia.Unit == note.Unit);
                    if (item != null)
                    {
                        priceInfo = item.PriceInfo;
                    }
                }

                var productCode = note.ProductCode;
                if (!string.IsNullOrWhiteSpace(productCode))
                {
                    productCode = productCode.ToUpper();
                }
                sheet.Cells[currentRow, 1].Value = index + 1;
                sheet.Cells[currentRow, 2].Value = productCode;
                sheet.Cells[currentRow, 3].Value = note.ProductName;
                sheet.Cells[currentRow, 4].Value = note.Unit;
                sheet.Cells[currentRow, 5].Value = note.UnitPrice;
                sheet.Cells[currentRow, 6].Value = quantity;
                sheet.Cells[currentRow, 7].Value = note.Discount;
                sheet.Cells[currentRow, 8].Value = description;
                sheet.Cells[currentRow, 9].Value = note.UnitPriceForeign;
                sheet.Cells[currentRow, 10].Value = amountForeign;
                sheet.Cells[currentRow, 11].Value = note.Contact != null && note.ContactId > 0
                    ? note.Contact.FullName + (!string.IsNullOrWhiteSpace(note.Contact.Mobile) ? " | " + note.Contact.Mobile : string.Empty)
                    : string.Empty;
                sheet.Cells[currentRow, 12].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
                sheet.Cells[currentRow, 12].Value = note.CreatedAt;
                sheet.Cells[currentRow, 13].Value = amount;
                sheet.Cells[currentRow, 14].Value = staff;
                if (priceInfo != null)
                {
                    sheet.Cells[currentRow, 15].Value = priceInfo.Price;
                    sheet.Cells[currentRow, 16].Value = amount + priceInfo.Price * quantity;
                }

                index++;
                currentRow++;
                total += amount;
            }
            sheet.Cells[currentRow + 1, 2].Value = total;
            package.SaveAs(new System.IO.FileInfo(pathAndFileName));
            return fileName;
        }
    }

    public async Task<ProductReportViewModel> BuildProductReportSummary(string lang, string userId, DateTime? dateFrom, DateTime? dateTo, int storeId, bool autoUpdateQuantity)
    {
        var from = dateFrom.HasValue ? dateFrom.Value : DateTime.Now.AddYears(-10);
        var end = dateTo.HasValue ? dateTo.Value : DateTime.Now.AddMonths(3);
        var notes = await _productRepository.GetNotes(userId,
            from,
            end, 0, 0, 0, 0, 0, storeId, 0, 0);
        var report = new ProductReportViewModel() { FromDate = from, EndDate = end };
        if (notes == null || !notes.Any())
        {
            return report;
        }
        var arr = notes.ToArray();
        var dic = new Dictionary<string, ProductReportItemViewModel>();

        decimal totalCost = 0;
        decimal totalSale = 0;
        for (int i = 0; i < arr.Count(); i++)
        {
            var note = arr[i];
            var isOrder = note.OrderId > 0 || (note.TradeId > 0 && note.Amount > 0);
            var isReceived = note.ReceivedNoteId > 0 || (note.TradeId > 0 && note.Amount < 0);
            var unit = note.Unit;
            var quantity = note.Quantity;
            if (!string.IsNullOrEmpty(note.BasicUnit)
                && !string.IsNullOrEmpty(note.Unit)
                && note.BasicUnit != note.Unit
                && note.UnitExchange.HasValue)
            {
                quantity = note.Quantity * note.UnitExchange.Value;
                unit = note.BasicUnit;
            }
            if (dic.ContainsKey(note.ProductId.ToString().ToUpper()))
            {
                var oldItem = dic[note.ProductId.ToString().ToUpper()];
                var item = new ProductReportItemViewModel
                {
                    Closing = oldItem.Closing,
                    TotalSale = oldItem.TotalSale + (isOrder
                        ? note.Amount
                        : 0),
                    TotalCost = oldItem.TotalCost + (isReceived
                        ? note.Amount
                        : 0),
                    Issued = oldItem.Issued + (isOrder
                        ? -quantity
                        : (quantity < 0 && !isReceived ? quantity : 0)),
                    Received = oldItem.Received + (isReceived
                        ? quantity
                        : (quantity > 0 && !isOrder ? quantity : 0)),
                    Code = note.ProductCode,
                    Name = note.ProductName,
                    Unit = unit,
                    Id = note.ProductId
                };
                item.Opening = oldItem.Opening - item.Issued - item.Received;
                dic[note.ProductId.ToString().ToUpper()] = item;
            }
            else
            {
                var product = await _productRepository.GetById(note.ProductId, userId, storeId);
                if (product == null) {
                    continue;
                }
                decimal totalClosing = product.Count;
                var newNotes = await _productRepository.GetNotes(userId, end.AddDays(+1), DateTime.Now.AddMonths(2), 0, note.ProductId, 0, 0, 0, storeId, 0, 0);
                if (newNotes != null && newNotes.Any())
                {
                    foreach (var newNote in newNotes)
                    {
                        var isOldNoteReceived = newNote.ReceivedNoteId > 0 || (newNote.TradeId > 0 && newNote.Amount < 0);
                        var isOldOrder = newNote.OrderId > 0 || (newNote.TradeId > 0 && newNote.Amount > 0);
                        var oldQuantity = newNote.Quantity;
                        if (!string.IsNullOrEmpty(newNote.BasicUnit)
                            && !string.IsNullOrEmpty(newNote.Unit)
                            && newNote.BasicUnit != newNote.Unit
                            && newNote.UnitExchange.HasValue)
                        {
                            oldQuantity = newNote.Quantity * newNote.UnitExchange.Value;
                        }
                        var issued = isOldOrder
                            ? -oldQuantity
                            : (oldQuantity < 0 && !isOldNoteReceived ? oldQuantity : 0);
                        var received = isOldNoteReceived
                            ? oldQuantity
                            : (!isOldOrder && oldQuantity > 0 ? oldQuantity : 0);
                        totalClosing -= (issued + received);
                    }
                }
                var item = new ProductReportItemViewModel
                {
                    Closing = totalClosing,
                    TotalSale = isOrder
                        ? note.Amount
                        : 0,
                    TotalCost = isReceived
                        ? note.Amount
                        : 0,
                    Issued = isOrder
                        ? -quantity
                        : (quantity < 0 && !isReceived ? quantity : 0),
                    Received = isReceived
                        ? quantity
                        : (!isOrder && quantity > 0 ? quantity : 0),
                    Code = note.ProductCode,
                    Name = note.ProductName,
                    Id = note.ProductId,
                    Unit = unit
                };
                item.Opening = item.Closing - item.Issued - item.Received;
                dic.Add(note.ProductId.ToString().ToUpper(), item);
            }
        }
        report.Items = new List<ProductReportItemViewModel>();
        List<int> ids = new List<int>();
        foreach (var key in dic.Keys)
        {
            var item = dic[key];
            ids.Add(Convert.ToInt32(key));
        }
        foreach (var itemKey in dic.Keys)
        {
            var item = dic[itemKey];
            report.Items.Add(item);
            totalCost += item.TotalCost;
            totalSale += item.TotalSale;
        }
        var products = await _productRepository.GetProductsByIds(userId, ids);
        foreach (var item in report.Items)
        {
            var product = products != null && products.Any() ? products.FirstOrDefault(p => p.Id == item.Id) : null;
            if (product == null)
            {
                continue;
            }
            // item.Closing = item.Closing > 0 ? item.Closing : 0;
            item.Opening = item.Closing - item.Received - item.Issued;
            // item.Opening = item.Opening > 0 ? item.Opening : 0;
            if (autoUpdateQuantity && DateTime.Now.Date <= end.Date)
            {
                if (storeId == 0)
                {
                    product.Count = item.Closing > 0 ? item.Closing : 0;
                    await _productRepository.SaveProduct(product, false);
                }
                else
                {
                    var productQuantity = await _productRepository.GetProductStoreQuantity(product.Id, storeId, userId);
                    if (productQuantity != null)
                    {
                        productQuantity.Quantity = item.Closing > 0 ? item.Closing : 0;
                    }
                    else
                    {
                        productQuantity = new ProductStoreQuantity()
                        {
                            Quantity = item.Closing > 0 ? item.Closing : 0,
                            ProductId = product.Id,
                            StoreId = storeId,
                            UserId = userId
                        };
                    }
                    await _productRepository.SaveProductStoreQuantity(productQuantity, false); ;
                }
            }
            var units = !string.IsNullOrWhiteSpace(product.UnitsJson)
                ? JsonConvert.DeserializeObject<IEnumerable<UnitItem>>(product.UnitsJson)
                : null;
            if (units == null || !units.Any())
            {
                continue;
            }
            var mainUnit = units.FirstOrDefault(u => u.IsDefault.HasValue && u.IsDefault.Value);
            if (mainUnit == null)
            {
                continue;
            }
            item.Unit = mainUnit.Unit;
            item.Name += " (" + mainUnit.Unit + ")";
            item.Opening = decimal.Round(item.Opening / mainUnit.Exchange, 2, MidpointRounding.AwayFromZero);
            item.Closing = decimal.Round(item.Closing / mainUnit.Exchange, 2, MidpointRounding.AwayFromZero);
            item.Issued = decimal.Round(item.Issued / mainUnit.Exchange, 2, MidpointRounding.AwayFromZero);
            item.Received = decimal.Round(item.Received / mainUnit.Exchange, 2, MidpointRounding.AwayFromZero);
        }
        report.TotalCost = totalCost;
        report.TotalSale = totalSale;
        return report;
    }

    public async Task<ProductNoteReportViewModel> BuildProductReportDetail(string lang, string userId, DateTime? dateFrom, DateTime? dateTo, int productId, int storeId)
    {
        var from = dateFrom.HasValue ? dateFrom.Value : DateTime.Now.AddMonths(-6);
        var end = dateTo.HasValue ? dateTo.Value : DateTime.Now.AddMonths(3);

        var notes = await _productRepository.GetNotes(userId,
            dateFrom.HasValue ? dateFrom.Value : DateTime.Now.AddMonths(-6),
            dateTo.HasValue ? dateTo.Value : DateTime.Now.AddMonths(3), 0, productId, 0, 0, 0, storeId, 0, 0, true);

        var report = new ProductNoteReportViewModel() { FromDate = from, EndDate = end };

        if (notes == null || !notes.Any())
        {
            return report;
        }
        var arr = notes.ToArray();
        decimal total = 0;
        report.Items = new List<ProductNoteReportItemViewModel>();
        for (int i = 0; i < arr.Count(); i++)
        {
            var note = arr[i];
            var quantity = note.OrderId > 0
                ? -note.Quantity
                : note.ReceivedNoteId > 0
                    ? note.Quantity
                    : note.TradeId > 0 && note.Trade != null
                        ? note.Trade.IsReceived
                            ? -note.Quantity
                            : note.Quantity
                        : note.Quantity;
            var amount = note.OrderId > 0
                ? note.Amount
                : note.ReceivedNoteId > 0
                    ? -note.Amount
                    : note.Amount;
            var amountForeign = note.OrderId > 0
                ? note.AmountForeign
                : note.ReceivedNoteId > 0
                    ? -note.AmountForeign
                    : note.AmountForeign;
            var description = note.Note;
            if (string.IsNullOrWhiteSpace(description))
            {
                description = note.OrderId > 0
                ? (isVn(lang) ? "Đơn hàng #" : "Order #") + note.Order.OrderCode.ToString()
                : note.ReceivedNoteId > 0
                    ? (isVn(lang) ? "Phiếu nhập hàng #" : "Product received note #") + note.ReceivedNoteId.ToString()
                    : note.TradeId > 0 && note.Trade != null
                        ? note.Trade.IsReceived
                            ? (isVn(lang) ? "Bán lẻ, giao dịch #" : "Sale, transaction #") + note.Trade.Id.ToString()
                            : (isVn(lang) ? "Mua lẻ, giao dịch #" : "Buy, transaction #") + note.Trade.Id.ToString()
                        : string.Empty;
            }

            var productCode = note.ProductCode;
            if (note.ProductId > 0 && string.IsNullOrWhiteSpace(productCode))
            {
                var product = await _productRepository.GetById(note.ProductId, userId, storeId);
                productCode = product.Code;
            }
            if (!string.IsNullOrWhiteSpace(productCode))
            {
                productCode = productCode.ToUpper();
            }

            PriceInfo priceInfo = null;
            string staff = note.Order != null && note.Order.Staff != null ? note.Order.Staff.Name : string.Empty;
            if (note.OrderId > 0)
            {
                var items = JsonConvert.DeserializeObject<IEnumerable<OrderItem>>(note.Order.ItemsJson);
                var itemsArr = items != null && items.Any() ? items.ToArray() : new OrderItem[] { };
                var item = itemsArr.FirstOrDefault(ia => ia.ProductId == note.ProductId && ia.Unit == note.Unit);
                if (item != null)
                {
                    priceInfo = item.PriceInfo;
                }
            }

            report.Items.Add(new ProductNoteReportItemViewModel
            {
                Code = productCode,
                Name = note.ProductName,
                Unit = note.Unit,
                UnitPrice = note.UnitPrice,
                Quantity = quantity,
                Discount = note.Discount,
                Description = description,
                UnitPriceForeign = note.UnitPriceForeign,
                AmountForeign = amountForeign,
                Contact = note.Contact != null && note.ContactId > 0
                ? note.Contact.FullName + (!string.IsNullOrWhiteSpace(note.Contact.Mobile) ? " | " + note.Contact.Mobile : string.Empty)
                : string.Empty,
                CreatedAt = note.CreatedAt,
                Amount = amount,
                Staff = staff,
                StaffUnitPrice = priceInfo != null ? priceInfo.Price : 0,
                StaffProfit = priceInfo != null ? amount + priceInfo.Price * quantity : 0,
            });
            total += amount;
        }
        report.Total = total;
        return report;
    }

    private async Task<string> CreateInventoryReportSummary(string lang, string userId, DateTime? dateFrom, DateTime? dateTo, int productId, int storeId)
    {
        var templateFileName = "ProductNotesSummary.xlsx";
        var pathTemplate = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
        var pathAndFileNameTemplate = Path.Combine(pathTemplate, templateFileName);
        using (var package = new ExcelPackage(new System.IO.FileInfo(pathAndFileNameTemplate)))
        {
            var sheet = package.Workbook.Worksheets[0];

            if (isVn(lang))
            {
                sheet.Cells[1, 1].Value = "BÁO CÁO TỒN KHO";
                sheet.Cells[2, 1].Value = "Từ ngày:";
                sheet.Cells[2, 3].Value = "Đến ngày:";
                sheet.Cells[4, 2].Value = "Mã SP";
                sheet.Cells[4, 3].Value = "Tên SP";
                sheet.Cells[4, 4].Value = "Đơn vị";
                sheet.Cells[4, 5].Value = "Tồn đầu kỳ";
                sheet.Cells[4, 6].Value = "Nhập";
                sheet.Cells[4, 7].Value = "Xuất";
                sheet.Cells[4, 8].Value = "Tồn cuối kỳ";
                sheet.Cells[4, 9].Value = "Tổng tiền nhập";
                sheet.Cells[4, 10].Value = "Tổng bán";
                sheet.Cells[6, 1].Value = "TỔNG TIỀN NHẬP:";
                sheet.Cells[7, 1].Value = "TỔNG BÁN:";
            }

            var fileName = Guid.NewGuid().ToString("N") + ".xlsx";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//downloads");
            var pathAndFileName = Path.Combine(path, fileName);

            var notes = await _productRepository.GetNotes(userId,
                dateFrom.HasValue ? dateFrom.Value : DateTime.Now.AddYears(-10),
                dateTo.HasValue ? dateTo.Value : DateTime.Now.AddMonths(3), 0, productId, 0, 0, 0, storeId, 0, 0);
            if (notes == null || !notes.Any())
            {
                package.SaveAs(new System.IO.FileInfo(pathAndFileName));
                return fileName;
            }
            var arr = notes.ToArray();
            sheet.Cells[2, 2].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            sheet.Cells[2, 2].Value = dateFrom;
            sheet.Cells[2, 4].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
            sheet.Cells[2, 4].Value = dateTo;
            var currentRow = 5;
            var dic = new Dictionary<string, dynamic>();

            decimal totalCost = 0;
            decimal totalSale = 0;
            int index = 0;
            for (int i = 0; i < arr.Count(); i++)
            {
                var note = arr[i];
                var isOrder = note.OrderId > 0 || (note.TradeId > 0 && note.Amount > 0);
                var isReceived = note.ReceivedNoteId > 0 || (note.TradeId > 0 && note.Amount < 0);
                var unit = note.Unit;
                var quantity = note.Quantity;
                if (!string.IsNullOrEmpty(note.BasicUnit)
                    && !string.IsNullOrEmpty(note.Unit)
                    && note.BasicUnit != note.Unit
                    && note.UnitExchange.HasValue)
                {
                    quantity = note.Quantity * note.UnitExchange.Value;
                    unit = note.BasicUnit;
                }
                if (dic.ContainsKey(note.ProductId.ToString().ToUpper()))
                {
                    var oldItem = dic[note.ProductId.ToString().ToUpper()];
                    var item = new ProductReportItemViewModel
                    {
                        Id = note.ProductId,
                        Opening = oldItem.Opening,
                        TotalSale = oldItem.TotalSale + (isOrder
                            ? note.Amount
                            : 0),
                        TotalCost = oldItem.TotalCost + (isReceived
                            ? note.Amount
                            : 0),
                        Issued = oldItem.Issued + (isOrder
                            ? -quantity
                            : (quantity < 0 && !isReceived ? quantity : 0)),
                        Received = oldItem.Received + (isReceived
                            ? quantity
                            : (quantity > 0 && !isOrder ? quantity : 0)),
                        Code = note.ProductCode,
                        Name = note.ProductName,
                        Unit = unit
                    };
                    item.Closing = item.Opening + item.Issued + item.Received;
                    dic[note.ProductId.ToString().ToUpper()] = item;
                }
                else
                {
                    decimal totalOpening = 0;
                    var oldNotes = await _productRepository.GetNotes(userId, DateTime.Now.AddMonths(-6), (dateFrom.HasValue ? dateFrom.Value : DateTime.Now.AddYears(-10)).AddMinutes(-1), 0, note.ProductId, 0, 0, 0, storeId, 0, 0);
                    if (oldNotes != null && oldNotes.Any())
                    {
                        foreach (var oldNote in oldNotes)
                        {
                            var isOldNoteReceived = oldNote.ReceivedNoteId > 0 || (oldNote.TradeId > 0 && oldNote.Amount < 0);
                            var isOldOrder = oldNote.OrderId > 0 || (oldNote.TradeId > 0 && oldNote.Amount > 0);
                            var oldQuantity = oldNote.Quantity;
                            if (!string.IsNullOrEmpty(oldNote.BasicUnit)
                                && !string.IsNullOrEmpty(oldNote.Unit)
                                && oldNote.BasicUnit != oldNote.Unit
                                && oldNote.UnitExchange.HasValue)
                            {
                                oldQuantity = oldNote.Quantity * oldNote.UnitExchange.Value;
                            }
                            var issued = isOldOrder
                            ? -oldQuantity
                            : (oldQuantity < 0 && !isOldNoteReceived ? oldQuantity : 0);
                            var received = isOldNoteReceived
                                ? oldQuantity
                                : (!isOldOrder && oldQuantity > 0 ? oldQuantity : 0);
                            totalOpening += issued + received;
                        }
                    }
                    totalOpening = totalOpening < 0 ? 0 : totalOpening;
                    var item = new ProductReportItemViewModel
                    {
                        Id = note.ProductId,
                        Opening = totalOpening,
                        TotalSale = isOrder
                            ? note.Amount
                            : 0,
                        TotalCost = isReceived
                            ? note.Amount
                            : 0,
                        Issued = isOrder
                            ? -quantity
                            : (quantity < 0 && !isReceived ? quantity : 0),
                        Received = isReceived
                            ? quantity
                            : (!isOrder && quantity > 0 ? quantity : 0),
                        Code = note.ProductCode,
                        Name = note.ProductName,
                        Unit = unit
                    };
                    item.Closing = item.Opening + item.Issued + item.Received;
                    dic.Add(note.ProductId.ToString().ToUpper(), item);
                }
            }
            List<int> ids = new List<int>();
            foreach (var key in dic.Keys)
            {
                var item = dic[key];
                ids.Add(Convert.ToInt32(key));
            }
            var products = await _productRepository.GetProductsByIds(userId, ids);
            foreach (var itemKey in dic.Keys)
            {
                var item = dic[itemKey];
                var product = products != null && products.Any() ? products.FirstOrDefault(p => p.Id == item.Id) : null;
                if (product == null)
                {
                    continue;
                }
                var units = !string.IsNullOrWhiteSpace(product.UnitsJson)
                    ? JsonConvert.DeserializeObject<IEnumerable<UnitItem>>(product.UnitsJson)
                    : null;
                if (units == null || !units.Any())
                {
                    continue;
                }
                var mainUnit = units.FirstOrDefault(u => u.IsDefault.HasValue && u.IsDefault.Value);
                if (mainUnit == null)
                {
                    continue;
                }
                item.Unit = mainUnit.Unit;
                item.Name += " (" + mainUnit.Unit + ")";
                item.Opening = decimal.Round(item.Opening / mainUnit.Exchange, 2, MidpointRounding.AwayFromZero);
                item.Closing = decimal.Round(item.Closing / mainUnit.Exchange, 2, MidpointRounding.AwayFromZero);
                item.Issued = decimal.Round(item.Issued / mainUnit.Exchange, 2, MidpointRounding.AwayFromZero);
                item.Received = decimal.Round(item.Received / mainUnit.Exchange, 2, MidpointRounding.AwayFromZero);
            }

            sheet.InsertRow(currentRow, dic.Count());

            foreach (var itemKey in dic.Keys)
            {
                var item = dic[itemKey];
                sheet.Cells[currentRow, 1].Value = index + 1;
                sheet.Cells[currentRow, 2].Value = item.Code;
                sheet.Cells[currentRow, 3].Value = item.Name;
                sheet.Cells[currentRow, 4].Value = item.Unit;
                sheet.Cells[currentRow, 5].Value = item.Opening;
                sheet.Cells[currentRow, 6].Value = item.Received;
                sheet.Cells[currentRow, 7].Value = item.Issued;
                sheet.Cells[currentRow, 8].Value = item.Closing;
                sheet.Cells[currentRow, 9].Value = item.TotalCost;
                sheet.Cells[currentRow, 10].Value = item.TotalSale;
                index++;
                currentRow++;
                totalCost += item.TotalCost;
                totalSale += item.TotalSale;
            }

            sheet.Cells[currentRow + 1, 2].Value = totalCost;
            sheet.Cells[currentRow + 2, 2].Value = totalSale;
            package.SaveAs(new System.IO.FileInfo(pathAndFileName));
            return fileName;
        }
    }

    public async Task<string> CreateProductsReportFile(string lang, string userId, int storeId, bool? isMaterial)
    {
        var templateFileName = "Products.xlsx";
        var pathTemplate = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
        var pathAndFileNameTemplate = Path.Combine(pathTemplate, templateFileName);
        using (var package = new ExcelPackage(new System.IO.FileInfo(pathAndFileNameTemplate)))
        {
            var sheet = package.Workbook.Worksheets[0];

            if (isVn(lang))
            {
                sheet.Cells[1, 1].Value = isMaterial.HasValue && isMaterial.Value ? "NGUYÊN VẬT LIỆU" : "SẢN PHẨM";
                sheet.Cells[4, 2].Value = isMaterial.HasValue && isMaterial.Value ? "Mã NVL" : "Mã SP";
                sheet.Cells[4, 3].Value = isMaterial.HasValue && isMaterial.Value ? "Tên NVL" : "Tên SP";
                sheet.Cells[4, 4].Value = "Đơn vị";
                sheet.Cells[4, 5].Value = "Giá bán";
                sheet.Cells[4, 6].Value = "Giá nhập";
                sheet.Cells[4, 7].Value = "Mã vạch";
                sheet.Cells[4, 8].Value = "Số lượng";
                sheet.Cells[4, 9].Value = "Mô tả";
                sheet.Cells[4, 10].Value = "Ngày hết hạn";
                sheet.Cells[4, 11].Value = "Ngày tạo";
            }

            var fileName = Guid.NewGuid().ToString("N") + ".xlsx";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//downloads");
            var pathAndFileName = Path.Combine(path, fileName);

            var products = await _productRepository.GetProducts(userId, storeId, isMaterial.HasValue && isMaterial.Value, 0);
            if (products == null || !products.Any())
            {
                package.SaveAs(new System.IO.FileInfo(pathAndFileName));
                return fileName;
            }
            var currentRow = 5;

            sheet.InsertRow(currentRow, products.Count());
            var index = 0;
            foreach (var item in products)
            {
                sheet.Cells[currentRow, 1].Value = index + 1;
                sheet.Cells[currentRow, 2].Value = item.Code;
                sheet.Cells[currentRow, 3].Value = item.Title;
                sheet.Cells[currentRow, 4].Value = item.Unit;
                sheet.Cells[currentRow, 5].Value = item.Price;
                sheet.Cells[currentRow, 6].Value = item.CostPrice;
                sheet.Cells[currentRow, 7].Value = item.Barcode;
                sheet.Cells[currentRow, 8].Value = storeId != 0 ? item.StoreQuantity : item.Count;
                sheet.Cells[currentRow, 9].Value = item.Description;
                if (item.ExpiredAt.HasValue)
                {
                    sheet.Cells[currentRow, 10].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
                    sheet.Cells[currentRow, 10].Value = item.ExpiredAt;
                }
                sheet.Cells[currentRow, 11].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
                sheet.Cells[currentRow, 11].Value = item.CreatedAt;
                index++;
                currentRow++;
            }
            package.SaveAs(new System.IO.FileInfo(pathAndFileName));
            return fileName;
        }
    }

    public async Task<string> CreateContactsFile(string lang, string userId)
    {
        var templateFileName = "ImportContactsTemplate.xlsx";
        var pathTemplate = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
        var pathAndFileNameTemplate = Path.Combine(pathTemplate, templateFileName);
        using (var package = new ExcelPackage(new System.IO.FileInfo(pathAndFileNameTemplate)))
        {
            var sheet = package.Workbook.Worksheets[0];

            if (isVn(lang))
            {
                sheet.Cells[1, 1].Value = "DANH BẠ";
                sheet.Cells[1, 3].Value = "";

                sheet.Cells[3, 2].Value = "Mã danh bạ\n(Vd: C01)";
                sheet.Cells[3, 3].Value = "Họ và tên";
                sheet.Cells[3, 4].Value = "Điện thoại";
                sheet.Cells[3, 5].Value = "Email";
                sheet.Cells[3, 6].Value = "Địa chỉ";
                sheet.Cells[3, 7].Value = "Ngày sinh";
                sheet.Cells[3, 8].Value = "Giới tính\n(Vd: Nam hoặc Nữ)";
                sheet.Cells[3, 9].Value = "Nhân viên phụ trách";
                sheet.Cells[2, 9].Value = "Điền tên hoặc mã ID của nhân viên";
            }

            var fileName = Guid.NewGuid().ToString("N") + ".xlsx";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//downloads");
            var pathAndFileName = Path.Combine(path, fileName);

            var contacts = await _contactRepository.GetContacts(userId);
            if (contacts == null || !contacts.Any())
            {
                package.SaveAs(new System.IO.FileInfo(pathAndFileName));
                return fileName;
            }
            var currentRow = 4;

            sheet.InsertRow(currentRow, contacts.Count());
            var index = 0;
            foreach (var item in contacts)
            {
                sheet.Cells[currentRow, 1].Value = index + 1;
                sheet.Cells[currentRow, 2].Value = item.Code;
                sheet.Cells[currentRow, 3].Value = item.FullName;
                sheet.Cells[currentRow, 4].Value = item.Mobile;
                sheet.Cells[currentRow, 5].Value = item.Email;
                sheet.Cells[currentRow, 6].Value = item.Address;
                sheet.Cells[currentRow, 7].Value = item.DateOfBirth.HasValue
                    ? item.DateOfBirth.Value.ToString(isVn(lang) ? "dd/MM/yyyy" : "MM/dd/yyyy")
                    : string.Empty;
                sheet.Cells[currentRow, 8].Value = item.Gender == "female"
                    ? (isVn(lang) ? "Nữ" : "Female")
                    : (isVn(lang) ? "Nam" : "Male");
                sheet.Cells[currentRow, 9].Value = item.StaffId != 0 && item.Staff != null
                    ? item.Staff.Name
                    : string.Empty;
                index++;
                currentRow++;
            }
            package.SaveAs(new System.IO.FileInfo(pathAndFileName));
            return fileName;
        }
    }

    private bool isVn(string lang)
    {
        return string.IsNullOrWhiteSpace(lang) || lang.ToLower() == "vn";
    }

    private string GetOrderStatus(int orderStatus, string lang)
    {
        switch (orderStatus)
        {
            case 0:
                return isVn(lang) ? "Nháp" : "Draft";
            case 1:
                return isVn(lang) ? "Đang xử lý" : "In Progress";
            case 2:
                return isVn(lang) ? "Đang vận chuyển" : "Shipping";
            case 3:
                return isVn(lang) ? "Hoàn thành" : "Done";
            case 4:
                return isVn(lang) ? "Đã hủy" : "Cancel";
            case 5:
                return isVn(lang) ? "Có công nợ" : "Has debt";
        }
        return string.Empty;
    }

    public async Task<string> CreateProductsTemplate(string lang, bool? isMaterial)
    {
        var templateFileName = "ImportProductsTemplate.xlsx";
        var pathTemplate = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
        var pathAndFileNameTemplate = Path.Combine(pathTemplate, templateFileName);
        using (var package = new ExcelPackage(new System.IO.FileInfo(pathAndFileNameTemplate)))
        {
            var sheet = package.Workbook.Worksheets[0];

            if (isVn(lang))
            {
                sheet.Cells[1, 1].Value = !(isMaterial.HasValue && isMaterial.Value) ? "SẢN PHẨM" : "NGUYÊN VẬT LIỆU";
                sheet.Cells[2, 1].Value = "Chú ý: Nếu tên nguyên vật liệu trùng nhau mà không điền mã, hệ thống có thể tạo ra các nguyên vật liệu trùng lặp";
                sheet.Cells[1, 3].Value = "* Trường phải nhập";
                sheet.Cells[1, 7].Value = "Không đổi thứ tự các trường";

                sheet.Cells[3, 2].Value = isMaterial.HasValue && isMaterial.Value ? "Mã NVL\n(Vd: IP01)" : "Mã SP\n(Vd: IP01)";
                sheet.Cells[3, 3].Value = isMaterial.HasValue && isMaterial.Value ? "Tên NVL\n(Vd: Iphone 11)" : "Tên SP\n(Vd: Iphone 11)";
                sheet.Cells[3, 4].Value = "Đơn vị\n(Vd: chiếc)";
                sheet.Cells[3, 5].Value = "Đơn giá";
                sheet.Cells[3, 6].Value = "Giá nhập";
                sheet.Cells[3, 7].Value = "Loại ngoại tệ\n(Vd: EUR)";
                sheet.Cells[3, 8].Value = "Giá nhập (ngoại tệ)";
                sheet.Cells[3, 9].Value = "Ghi chú";
                sheet.Cells[3, 10].Value = "Mã vạch";
                sheet.Cells[3, 11].Value = "Số lượng";
                sheet.Cells[3, 12].Value = "Giá cho CTV/Nhân viên";
                sheet.Cells[2, 11].Value = "* Chỉ thay đổi trường này nếu muốn cập nhật lại thủ công Số lượng trong kho";
            }

            var fileName = Guid.NewGuid().ToString("N") + ".xlsx";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//downloads");
            var pathAndFileName = Path.Combine(path, fileName);

            package.SaveAs(new System.IO.FileInfo(pathAndFileName));
            return fileName;
        }
    }

    public async Task<string> CreateOrdersTemplate(string lang)
    {
        var templateFileName = "ImportOrderTemplate.xlsx";
        var pathTemplate = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
        var pathAndFileNameTemplate = Path.Combine(pathTemplate, templateFileName);
        using (var package = new ExcelPackage(new System.IO.FileInfo(pathAndFileNameTemplate)))
        {
            var sheet = package.Workbook.Worksheets[0];

            if (isVn(lang))
            {
                sheet.Cells[1, 1].Value = "Nhập đơn hàng";
                sheet.Cells[1, 4].Value = "* là trường bắt buộc";

                sheet.Cells[3, 1].Value = "Mã đơn\n(Vd: 210719-12345)";
                sheet.Cells[3, 2].Value = "Ngày";
                sheet.Cells[2, 2].Value = "Chỉ điền một lần cho một Mã đơn hàng";
                sheet.Cells[3, 3].Value = "Mã SP\n(VD: IP01)";
                sheet.Cells[3, 4].Value = "Tên SP\n(Vd: Iphone 12)";
                sheet.Cells[3, 5].Value = "Đơn vị";
                sheet.Cells[3, 6].Value = "Đơn giá";
                sheet.Cells[3, 7].Value = "Số lượng";
                sheet.Cells[3, 8].Value = "Chiết khấu";
                sheet.Cells[3, 9].Value = "Ghi chú";
                sheet.Cells[3, 10].Value = "Thành tiền";
                sheet.Cells[3, 11].Value = "Nhân viên";
                sheet.Cells[2, 11].Value = "Điền tên nhân viên hoặc số của nhân viên (xem trên app). Chỉ điền một lần cho một Mã đơn hàng.";
            }

            var fileName = Guid.NewGuid().ToString("N") + ".xlsx";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//downloads");
            var pathAndFileName = Path.Combine(path, fileName);

            package.SaveAs(new System.IO.FileInfo(pathAndFileName));
            return fileName;
        }
    }

    private async Task<int> CountCurrentOrders(string userId)
    {
        var model = new Dictionary<string, object>{
                {"userId", userId},
            };
        var query = new QueryModelOnSearch()
        {
            WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"userId", new List<string>{EnumSearchFunctions.EQUALS}},
                    {"createdAt", new List<string>{EnumSearchFunctions.IS_TODAY}}
                }
        };
        return await _sqlService.CountAsync("order", model, query);
    }

    public async Task<ImportOrdersViewModel> UploadOrders(IFormFile file, string lang, string userId, int? storeId)
    {
        var account = await _accountRepository.GetDefault(userId);
        using (var stream = file.OpenReadStream())
        {
            using (var package = new ExcelPackage(stream))
            {
                var sheet = package.Workbook.Worksheets[0];
                var currentPlan = await GetCurrentPlanAsync(userId);
                var currentOrders = 0;
                if (currentPlan == null)
                {
                    currentOrders = await CountCurrentOrders(userId);
                    if (currentOrders > 10)
                    {
                        return new ImportOrdersViewModel() { Error = "{total-orders-today-is-more-than-10-please-upgrade}" };
                    }
                }

                Dictionary<string, Order> dic = new Dictionary<string, Order>();

                var items = new List<OrderItem>();
                var moneyAccount = await _accountRepository.GetDefault(userId);
                var currentRow = 4;
                var hasData = sheet.Cells[currentRow, 1].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow, 1].Value.ToString())
                || sheet.Cells[currentRow + 1, 1].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow + 1, 1].Value.ToString());
                while (hasData)
                {
                    var item = new OrderItem();
                    var orderCode = sheet.Cells[currentRow, 1].Value != null ? sheet.Cells[currentRow, 1].Value.ToString().ToUpper() : string.Empty;
                    if (string.IsNullOrWhiteSpace(orderCode))
                    {
                        return new ImportOrdersViewModel() { Error = "{missing-order-code}:" + currentRow };
                    }
                    var order = dic.ContainsKey(orderCode.ToUpper()) ? dic[orderCode.ToUpper()] : null;
                    if (order == null)
                    {
                        order = await _orderRepository.GetByCode(orderCode.ToUpper(), userId);
                        if (order != null)
                        {
                            return new ImportOrdersViewModel() { Error = "{order-code-is-not-exists}:" + currentRow };
                        }
                        else
                        {
                            order = new Order();
                            order.OrderCode = orderCode.ToUpper();
                            order.UserId = userId;
                            order.MoneyAccountId = moneyAccount != null ? moneyAccount.Id : 0;
                            order.Items = new List<OrderItem>();
                            dic.Add(orderCode.ToUpper(), order);
                        }
                    }

                    if (!order.CreatedAt.HasValue)
                    {
                        DateTime? orderDate = null;
                        if (!isDateOrEmpty(sheet.Cells[currentRow, 2].Value, out orderDate))
                        {
                            return new ImportOrdersViewModel() { Error = "{order-date-not-date-format}:" + currentRow };
                        }
                        order.CreatedAt = orderDate;
                    }

                    item.ProductCode = sheet.Cells[currentRow, 3].Value != null ? sheet.Cells[currentRow, 3].Value.ToString().ToUpper() : string.Empty;
                    item.ProductName = sheet.Cells[currentRow, 4].Value != null ? sheet.Cells[currentRow, 4].Value.ToString() : string.Empty;
                    if (string.IsNullOrWhiteSpace(item.ProductCode))
                    {
                        return new ImportOrdersViewModel() { Error = "{missing-product-code}:" + currentRow };
                    }
                    var product = await _productRepository.GetByCode(item.ProductCode, userId);
                    if (product == null)
                    {
                        return new ImportOrdersViewModel() { Error = "{product-code-is-not-exists}:" + currentRow };
                    }
                    if (string.IsNullOrWhiteSpace(item.ProductName))
                    {
                        return new ImportOrdersViewModel() { Error = "{missing-product-name}:" + currentRow };
                    }
                    item.ProductId = product.Id;
                    item.CostPrice = product.CostPrice;
                    item.Unit = sheet.Cells[currentRow, 5].Value != null ? sheet.Cells[currentRow, 5].Value.ToString() : string.Empty;
                    if (!isDecimal(sheet.Cells[currentRow, 6].Value))
                    {
                        return new ImportOrdersViewModel() { Error = "{missing-price}:" + currentRow };
                    }
                    item.Price = sheet.Cells[currentRow, 6].Value != null ? Convert.ToDecimal(sheet.Cells[currentRow, 6].Value) : 0;
                    if (item.Price == 0)
                    {
                        return new ImportOrdersViewModel() { Error = "{missing-price}:" + currentRow };
                    }

                    if (!isDecimal(sheet.Cells[currentRow, 7].Value))
                    {
                        return new ImportOrdersViewModel() { Error = "{missing-quantity}:" + currentRow };
                    }
                    item.Count = sheet.Cells[currentRow, 7].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow, 7].Value.ToString())
                        ? Convert.ToDecimal(sheet.Cells[currentRow, 7].Value)
                        : 0;

                    item.TotalCostPrice = (item.CostPrice.HasValue ? item.CostPrice.Value : 0) * item.Count;

                    if (!isDecimalOrEmpty(sheet.Cells[currentRow, 8].Value))
                    {
                        return new ImportOrdersViewModel() { Error = "{discount-not-number}:" + currentRow };
                    }
                    item.Discount = sheet.Cells[currentRow, 8].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow, 8].Value.ToString())
                        ? Convert.ToDecimal(sheet.Cells[currentRow, 8].Value)
                        : 0;

                    item.Note = sheet.Cells[currentRow, 9].Value != null ? sheet.Cells[currentRow, 9].Value.ToString() : string.Empty;

                    if (!isDecimal(sheet.Cells[currentRow, 10].Value))
                    {
                        return new ImportOrdersViewModel() { Error = "{missing-amount}:" + currentRow };
                    }
                    item.Total = sheet.Cells[currentRow, 10].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow, 10].Value.ToString())
                        ? Convert.ToDecimal(sheet.Cells[currentRow, 10].Value)
                        : 0;
                    if (order.StaffId == 0)
                    {
                        var staff = sheet.Cells[currentRow, 11].Value != null ? sheet.Cells[currentRow, 11].Value.ToString().ToUpper() : string.Empty;
                        if (!string.IsNullOrWhiteSpace(staff))
                        {
                            var staffObj = await _staffRepository.GetByName(staff.Trim(), userId);
                            if (staffObj == null && isInt(staff))
                            {
                                staffObj = await _staffRepository.GetById(Convert.ToInt32(staff), userId);
                            }
                            if (staffObj != null)
                            {
                                order.StaffId = staffObj.Id;
                                order.StoreId = staffObj.StoreId;
                            }
                            else
                            {
                                return new ImportOrdersViewModel() { Error = "{staff-is-not-exists}:" + currentRow };
                            }
                        }
                    }
                    item.UserId = userId;

                    items.Add(item);
                    var orderItems = (List<OrderItem>)order.Items;
                    orderItems.Add(item);
                    order.Total += item.Total.HasValue ? item.Total.Value : 0;
                    if (moneyAccount != null)
                    {
                        moneyAccount.Total += order.Total;
                    }
                    order.Discount += item.Discount.HasValue ? item.Discount.Value : 0;
                    currentRow++;
                    hasData = sheet.Cells[currentRow, 3].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow, 3].Value.ToString())
                    || sheet.Cells[currentRow + 1, 3].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow + 1, 3].Value.ToString());
                }

                if (!items.Any())
                {
                    return new ImportOrdersViewModel() { Error = "{nothing-to-import}" };
                }

                if (currentPlan == null)
                {
                    currentOrders = await CountCurrentOrders(userId);
                    if ((currentOrders + items.Count()) > 10)
                    {
                        return new ImportOrdersViewModel() { Error = "{total-orders-today-is-more-than-10-please-upgrade}" };
                    }
                }

                var serializerSettings = new JsonSerializerSettings();
                serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();

                var arr = new List<Task>();
                if (moneyAccount != null)
                {
                    arr.Add(_accountRepository.SaveAccount(moneyAccount));
                }
                foreach (var order in dic.Values)
                {
                    order.Status = 3;
                    order.ItemsJson = JsonConvert.SerializeObject(order.Items, serializerSettings);
                    var orderId = await _orderRepository.SaveOrder(order);
                    var trade = new Trade()
                    {
                        UserId = userId,
                        OrderId = orderId,
                        StaffId = order.StaffId,
                        IsPurchase = true,
                        IsReceived = true,
                        Value = order.Total,
                        MoneyAccountId = order.MoneyAccountId,
                        CreatedAt = order.CreatedAt.HasValue ? order.CreatedAt.Value : DateTime.Now,
                    };
                    arr.Add(_tradeRepository.SaveTrade(trade));
                    foreach (var item in order.Items)
                    {
                        var note = new ProductNote()
                        {
                            OrderId = orderId,
                            UserId = userId,
                            Amount = item.Total.HasValue ? item.Total.Value : 0,
                            UnitPrice = item.Price.HasValue ? item.Price.Value : 0,
                            Unit = item.Unit,
                            Quantity = item.Count.HasValue ? item.Count.Value : 0,
                            ProductId = item.ProductId,
                            ProductCode = item.ProductCode,
                            ProductName = item.ProductName,
                            Discount = item.Discount.HasValue ? item.Discount.Value : 0,
                            StoreId = order.StoreId,
                            CreatedAt = order.CreatedAt.HasValue ? order.CreatedAt.Value : DateTime.Now,
                            Note = item.Note
                        };
                        arr.Add(_productService.SaveProductNote(note));
                    }
                }
                Task.WaitAll(arr.ToArray());

                return new ImportOrdersViewModel() { Count = dic.Count(), Error = string.Empty, Orders = dic.Values };
            }
        }
    }

    public async Task<ImportProductsViewModel> UploadProducts(IFormFile file, string lang, string userId, int? storeId, bool isMaterial)
    {
        var account = await _accountRepository.GetDefault(userId);
        using (var stream = file.OpenReadStream())
        {
            using (var package = new ExcelPackage(stream))
            {
                var sheet = package.Workbook.Worksheets[0];
                var currentPlan = await GetCurrentPlanAsync(userId);
                var currentCount = await _sqlService.CountAsync("product", new Dictionary<string, object>() { { "userId", userId } }, null);
                if (currentPlan == null && currentCount > 30)
                {
                    return new ImportProductsViewModel() { Error = "{total-product-more-than-30-please-upgrade-pro-plan}" };
                }

                var items = new List<Product>();
                var countNewProduct = 0;
                var dic = new Dictionary<string, bool>();

                var currentRow = 4;
                var hasData = sheet.Cells[currentRow, 3].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow, 3].Value.ToString())
                || sheet.Cells[currentRow + 1, 3].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow + 1, 3].Value.ToString());
                while (hasData)
                {
                    var item = new Product();
                    item.Code = sheet.Cells[currentRow, 2].Value != null ? sheet.Cells[currentRow, 2].Value.ToString().ToUpper() : string.Empty;
                    if (currentPlan == null)
                    {
                        if (string.IsNullOrWhiteSpace(item.Code))
                        {
                            countNewProduct++;
                            if ((currentCount + countNewProduct) > 30)
                            {
                                return new ImportProductsViewModel() { Error = "{total-product-more-than-30-please-upgrade-pro-plan}" };
                            }
                        }
                        else
                        {
                            var existProduct = await _productRepository.GetByCode(item.Code, userId);
                            if (existProduct == null && !dic.ContainsKey(item.Code.ToUpper()))
                            {
                                if (!dic.ContainsKey(item.Code.ToUpper()))
                                {
                                    countNewProduct++;
                                    if ((currentCount + countNewProduct) > 30)
                                    {
                                        return new ImportProductsViewModel() { Error = "{total-product-more-than-30-please-upgrade-pro-plan}" };
                                    }
                                }
                                else
                                {
                                    dic.Add(item.Code.ToUpper(), true);
                                }
                            }
                        }
                    }
                    item.Title = sheet.Cells[currentRow, 3].Value != null ? sheet.Cells[currentRow, 3].Value.ToString() : string.Empty;
                    if (string.IsNullOrWhiteSpace(item.Title))
                    {
                        return new ImportProductsViewModel() { Error = "{missing-name}:" + currentRow };
                    }
                    item.Unit = sheet.Cells[currentRow, 4].Value != null ? sheet.Cells[currentRow, 4].Value.ToString() : string.Empty;
                    if (string.IsNullOrWhiteSpace(item.Unit))
                    {
                        return new ImportProductsViewModel() { Error = "{missing-unit}:" + currentRow };
                    }
                    if (!isDecimal(sheet.Cells[currentRow, 5].Value))
                    {
                        return new ImportProductsViewModel() { Error = "{missing-price}:" + currentRow };
                    }
                    item.Price = sheet.Cells[currentRow, 5].Value != null ? Convert.ToDecimal(sheet.Cells[currentRow, 5].Value) : 0;
                    if (item.Price == 0)
                    {
                        return new ImportProductsViewModel() { Error = "{missing-price}:" + currentRow };
                    }
                    if (!isDecimalOrEmpty(sheet.Cells[currentRow, 6].Value))
                    {
                        return new ImportProductsViewModel() { Error = "{cost-price-not-number}:" + currentRow };
                    }
                    item.CostPrice = sheet.Cells[currentRow, 6].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow, 6].Value.ToString())
                        ? Convert.ToDecimal(sheet.Cells[currentRow, 6].Value)
                        : (decimal?)null;
                    item.ForeignCurrency = sheet.Cells[currentRow, 7].Value != null ? Convert.ToString(sheet.Cells[currentRow, 7].Value) : string.Empty;
                    if (!isDecimalOrEmpty(sheet.Cells[currentRow, 8].Value))
                    {
                        return new ImportProductsViewModel() { Error = "{unit-price-foreign-not-number}:" + currentRow };
                    }
                    item.CostPriceForeign = sheet.Cells[currentRow, 8].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow, 8].Value.ToString())
                        ? Convert.ToDecimal(sheet.Cells[currentRow, 8].Value)
                        : (decimal?)null;
                    item.Description = sheet.Cells[currentRow, 9].Value != null ? Convert.ToString(sheet.Cells[currentRow, 9].Value) : string.Empty;
                    item.Barcode = sheet.Cells[currentRow, 10].Value != null ? Convert.ToString(sheet.Cells[currentRow, 10].Value) : string.Empty;

                    if (!isDecimalOrEmpty(sheet.Cells[currentRow, 11].Value))
                    {
                        return new ImportProductsViewModel() { Error = "{quantity-not-number}:" + currentRow };
                    }
                    item.Count = sheet.Cells[currentRow, 11].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow, 11].Value.ToString())
                        ? Convert.ToDecimal(sheet.Cells[currentRow, 11].Value)
                        : 0;

                    item.CollaboratorPrice = sheet.Cells[currentRow, 12].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow, 12].Value.ToString())
                        ? Convert.ToDecimal(sheet.Cells[currentRow, 12].Value)
                        : (decimal?)null;
                    item.ShowOnWeb = false;
                    item.IsMaterial = isMaterial;
                    items.Add(item);
                    currentRow++;
                    hasData = sheet.Cells[currentRow, 3].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow, 3].Value.ToString())
                    || sheet.Cells[currentRow + 1, 3].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow + 1, 3].Value.ToString());
                }

                if (!items.Any())
                {
                    return new ImportProductsViewModel() { Error = "{nothing-to-import}" };
                }

                // create product notes
                foreach (var product in items)
                {
                    await UpdateProduct(product, userId, lang, storeId);
                }
                return new ImportProductsViewModel() { Id = items.Count(), Count = items.Count(), Error = string.Empty, Products = items };
            }
        }
    }

    private async Task<Dictionary<string, object>> GetCurrentShop(string userId)
    {
        var model = new Dictionary<string, object>{
                {"userId", userId},
            };
        var shops = await _sqlService.ListAsync("shop", model, null);
        if (shops != null && shops.Any())
        {
            return shops.First();
        }
        return null;
    }

    private async Task<Dictionary<string, object>> GetCurrentPlanAsync(string userId)
    {
        var currentShop = await GetCurrentShop(userId);
        if (currentShop != null && currentShop.ContainsKey("createdAt"))
        {
            var createdAt = (DateTime)currentShop["createdAt"];
            if (createdAt.AddDays(6).Date >= DateTime.Now.Date)
            {
                return new Dictionary<string, object>();
            }
        }
        var model = new Dictionary<string, object>{
                {"userId", userId},
                {"subscriptionType", "PRO"},
            };
        var query = new QueryModelOnSearch()
        {
            WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"userId", new List<string>{EnumSearchFunctions.EQUALS}},
                    {"startDate", new List<string>{EnumSearchFunctions.SMALLER_THAN_TODAY}},
                    {"endDate", new List<string>{EnumSearchFunctions.BIGGER_THAN_TODAY}},
                }
        };
        var subscriptions = await _sqlService.ListAsync("subscription", model, query);
        if (subscriptions != null && subscriptions.Any())
        {
            return subscriptions.First();
        }
        return null;
    }

    public async Task<string> CreateContactsTemplate(string lang)
    {
        var templateFileName = "ImportContactsTemplate.xlsx";
        var pathTemplate = Path.Combine(Directory.GetCurrentDirectory(), "Templates");
        var pathAndFileNameTemplate = Path.Combine(pathTemplate, templateFileName);
        using (var package = new ExcelPackage(new System.IO.FileInfo(pathAndFileNameTemplate)))
        {
            var sheet = package.Workbook.Worksheets[0];

            if (isVn(lang))
            {
                sheet.Cells[1, 1].Value = "DANH BẠ";
                sheet.Cells[1, 3].Value = "Không đổi thứ tự các trường";

                sheet.Cells[3, 2].Value = "Mã danh bạ\n(Vd: C01)";
                sheet.Cells[3, 3].Value = "Họ và tên";
                sheet.Cells[3, 4].Value = "Điện thoại";
                sheet.Cells[3, 5].Value = "Email";
                sheet.Cells[3, 6].Value = "Địa chỉ";
                sheet.Cells[3, 7].Value = "Ngày sinh";
                sheet.Cells[3, 8].Value = "Giới tính\n(Vd: Nam hoặc Nữ)";
            }

            var fileName = Guid.NewGuid().ToString("N") + ".xlsx";
            var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot//downloads");
            var pathAndFileName = Path.Combine(path, fileName);

            package.SaveAs(new System.IO.FileInfo(pathAndFileName));
            return fileName;
        }
    }

    public async Task<ImportContactsViewModel> UploadContacts(IFormFile file, string lang, string userId)
    {
        var account = await _accountRepository.GetDefault(userId);
        using (var stream = file.OpenReadStream())
        {
            using (var package = new ExcelPackage(stream))
            {
                var sheet = package.Workbook.Worksheets[0];

                var items = new List<Contact>();

                var currentRow = 4;
                var hasData = sheet.Cells[currentRow, 3].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow, 3].Value.ToString())
                || sheet.Cells[currentRow + 1, 3].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow + 1, 3].Value.ToString());
                while (hasData)
                {
                    var item = new Contact();
                    item.Code = sheet.Cells[currentRow, 2].Value != null ? sheet.Cells[currentRow, 2].Value.ToString().ToUpper() : string.Empty;
                    // if (string.IsNullOrWhiteSpace(item.Code)) {
                    //     return new UploadReceivedNoteResponseViewModel() {Error = "{missing-code}:" + currentRow};
                    // }
                    item.FullName = sheet.Cells[currentRow, 3].Value != null ? sheet.Cells[currentRow, 3].Value.ToString() : string.Empty;
                    if (string.IsNullOrWhiteSpace(item.FullName))
                    {
                        return new ImportContactsViewModel() { Error = "{missing-name}:" + currentRow };
                    }
                    item.Mobile = sheet.Cells[currentRow, 4].Value != null ? sheet.Cells[currentRow, 4].Value.ToString() : null;
                    item.Email = sheet.Cells[currentRow, 5].Value != null ? sheet.Cells[currentRow, 5].Value.ToString() : null;
                    item.Address = sheet.Cells[currentRow, 6].Value != null ? sheet.Cells[currentRow, 6].Value.ToString() : null;
                    DateTime? birthDate = null;
                    if (!isDateOrEmpty(sheet.Cells[currentRow, 7].Value, out birthDate))
                    {
                        return new ImportContactsViewModel() { Error = "{dateofbirth-not-date}:" + currentRow };
                    }
                    item.DateOfBirth = birthDate;
                    item.Gender = sheet.Cells[currentRow, 8].Value != null
                        ? isVn(lang)
                            ? (sheet.Cells[currentRow, 8].Value.ToString().ToUpper() == "MALE" ? "male" : "female")
                            : (sheet.Cells[currentRow, 8].Value.ToString().ToUpper() == "NAM" ? "male" : "female")
                        : null;
                    var staff = sheet.Cells[currentRow, 9].Value != null ? sheet.Cells[currentRow, 9].Value.ToString().ToUpper() : string.Empty;
                    if (!string.IsNullOrWhiteSpace(staff))
                    {
                        var staffObj = await _staffRepository.GetByName(staff.Trim(), userId);
                        if (staffObj == null && isInt(staff))
                        {
                            staffObj = await _staffRepository.GetById(Convert.ToInt32(staff), userId);
                        }
                        if (staffObj != null)
                        {
                            item.StaffId = staffObj.Id;
                        }
                        else
                        {
                            return new ImportContactsViewModel() { Error = "{staff-is-not-exists}:" + currentRow };
                        }
                    }

                    items.Add(item);
                    currentRow++;
                    hasData = sheet.Cells[currentRow, 3].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow, 3].Value.ToString())
                    || sheet.Cells[currentRow + 1, 3].Value != null && !string.IsNullOrWhiteSpace(sheet.Cells[currentRow + 1, 3].Value.ToString());
                }

                if (!items.Any())
                {
                    return new ImportContactsViewModel() { Error = "{nothing-to-import}" };
                }

                // create contact notes
                var arContactJobs = new List<Task>();
                foreach (var contact in items)
                {
                    var t = UpdateContact(contact, userId);
                    arContactJobs.Add(t);
                }
                Task.WaitAll(arContactJobs.ToArray());
                return new ImportContactsViewModel() { Id = arContactJobs.Count(), Count = arContactJobs.Count(), Error = string.Empty, Contacts = items };
            }
        }
    }
}