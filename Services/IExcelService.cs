using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

public interface IExcelService
{
    Task<string> CreateOrderReportFile(int reportType, string lang, string userId, DateTime? dateFrom, DateTime? dateTo);
    Task<string> CreateSalesReportFile(IEnumerable<int> orderIds, int reportType, string lang, string userId, DateTime? dateFrom, DateTime? dateTo, int storeId, int staffId);
    Task<string> CreateInventoryReportFile(int reportType, string lang, string userId, DateTime? dateFrom, DateTime? dateTo, int productId, int storeId);
    Task<string> CreateProductsReportFile(string lang, string userId, int storeId, bool? isMaterial);
    Task<string> CreateReceivedTemplate(string lang);
    Task<string> CreateProductsTemplate(string lang, bool? isMaterial);
    Task<string> CreateOrdersTemplate(string lang);
    Task<string> CreateContactsTemplate(string lang);
    Task<UploadReceivedNoteResponseViewModel> UploadReceivedNote(IFormFile file, string lang, string userId, int? storeId, int? staffId);
    Task<ImportProductsViewModel> UploadProducts(IFormFile file, string lang, string userId, int? storeId, bool isMaterial);
    Task<ImportContactsViewModel> UploadContacts(IFormFile file, string lang, string userId);
    Task<ImportOrdersViewModel> UploadOrders(IFormFile file, string lang, string userId, int? storeId);
    Task<OrderReportSummaryViewModel> BuildSalesReportByOrder(IEnumerable<int> orderIds, string lang, string userId, DateTime? dateFrom, DateTime? dateTo, int storeId, int staffId);
    Task<OrderReportSummaryViewModel> BuildSalesReportByProduct(IEnumerable<int> orderIds, string lang, string userId, DateTime? dateFrom, DateTime? dateTo, int storeId, int staffId);
    Task<OrderReportSummaryViewModel> BuildSalesReportByCustomerAndProduct(IEnumerable<int> orderIds, string lang, string userId, DateTime? dateFrom, DateTime? dateTo, int storeId, int staffId);
    Task<OrderReportSummaryViewModel> BuildSalesReportByStaff(IEnumerable<int> orderIds, string lang, string userId, DateTime? dateFrom, DateTime? dateTo, int storeId, int staffId);
    Task<ProductReportViewModel> BuildProductReportSummary(string lang, string userId, DateTime? dateFrom, DateTime? dateTo, int storeId, bool autoUpdateQuantity);
    Task<ProductNoteReportViewModel> BuildProductReportDetail(string lang, string userId, DateTime? dateFrom, DateTime? dateTo, int productId, int storeId);
    Task<string> CreateReceivedNoteFile(string lang, string userId, int noteId);
    Task<string> CreateContactsFile(string lang, string userId);
}