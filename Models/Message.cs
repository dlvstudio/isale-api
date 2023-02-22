using System;

public class UserMessage {
    public int Id { get; set; }

    public string UserId { get; set; }

    public string Message { get; set; }

    public string Content { get; set; }

    public bool IsNotification { get; set; }

    public bool IsRead { get; set; }
}