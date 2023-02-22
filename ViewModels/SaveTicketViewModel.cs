public class SaveTicketViewModel {
    public int Id { get; set; }
    public string Email { get; set; }
    public string Content { get; set; }
    public string Subject { get; set; }
    public int? CategoryId { get; set; }
    public int? StaffId { get; set; }
    public string UserId { get; set; }
}