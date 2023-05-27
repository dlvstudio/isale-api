
using System;

public class Contact {
    public int Id { get; set; }
    public int StaffId { get; set; }
    public string AvatarUrl { get; set; }
    public string FullName { get; set; }
    public string Mobile { get; set; }
    public bool IsImportant { get; set; }
    public string Gender { get; set; }
    public string Email { get; set; }
    public string Address { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public DateTime LastActive { get; set; }
    public string LastAction { get; set; }
    public string UserId { get; set; }
    public string Code { get; set; }
    public Staff Staff { get; set; }
    public string Note { get; set; }
    public int BusinessTypeId { get; set; }
    public int SalesLineId { get; set; }
    public decimal Point { get; set; }
    public int LevelId { get; set; }
    public int BuyCount {get;set;}
    public string FbUserId {get;set;}
    public string Source {get;set;}
}