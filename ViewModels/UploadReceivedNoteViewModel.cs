using Microsoft.AspNetCore.Http;

public class UploadReceivedNoteViewModel {
    public IFormFile File { get; set; }
    public string Lang { get; set; }
    public int? StoreId { get; set; }
    public int? StaffId { get; set; }
}