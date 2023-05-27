using System;

public class Category {
    public int Id { get; set; }
    public string Title { get; set; }
    public int OrderIndex { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ModifiedAt { get; set; }
    public string UserId { get; set; }
}