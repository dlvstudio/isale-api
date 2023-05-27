using System;

public class SaveContactViewModel {
    public int Id { get; set; }
    public int? StaffId { get; set; }
    public string AvatarUrl { get; set; }
    public string Code { get; set; }
    public string FullName { get; set; }
    public string Mobile { get; set; }
    public bool? IsImportant { get; set; }
    public string Gender { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime? LastActive { get; set; }
    public string LastAction { get; set; }
}