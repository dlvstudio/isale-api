using System.Collections.Generic;
using Newtonsoft.Json;

public class HookObject
{
    [JsonProperty("app_id")]
    public string AppId { get; set; }

    [JsonProperty("sender")]
    public HookObjectUser Sender { get; set; }

    [JsonProperty("user_id_by_app")]
    public string UserIdByApp { get; set; }
    
    [JsonProperty("recipient")]
    public HookObjectUser Recipient { get; set; }

    [JsonProperty("event_name")]
    public string EventName { get; set; }

    [JsonProperty("message")]
    public HookObjectMessage Message { get; set; }

    [JsonProperty("timestamp")]
    public long Timestamp { get; set; }

    [JsonProperty("info")]
    public HookUserInfo Info { get; set; }

    public string Avatar { get; set; }
    public string Address { get; set; }
    public string City { get; set; }
    public string Phone { get; set; }
    public string Name { get; set; }
    public string Json { get; set; }
}

public class HookUserInfo {
    [JsonProperty("address")]
    public string Address { get; set; }

    [JsonProperty("phone")]
    public string Phone { get; set; }

    [JsonProperty("city")]
    public string City { get; set; }

    [JsonProperty("district")]
    public string District { get; set; }
    
    [JsonProperty("name")]
    public string Name { get; set; }
    
    [JsonProperty("ward")]
    public string Ward { get; set; }
}

public class HookObjectUser {
    [JsonProperty("id")]
    public string Id { get; set; }
}

public class HookObjectMessage {
    [JsonProperty("text")]
    public string Text { get; set; }

    [JsonProperty("msg_id")]
    public string MessageId { get; set; }

    [JsonProperty("attachments")]
    public IEnumerable<HookObjectAttachment> Attachments { get; set; }
}

public class HookObjectAttachment {
    [JsonProperty("payload")]
    public HookObjectAttachmentPayload Payload { get; set; }

    [JsonProperty("type")]
    public string Type { get; set; }
}

public class HookObjectAttachmentPayload {
    [JsonProperty("thumbnail")]
    public string Thumbnail { get; set; }

    [JsonProperty("url")]
    public string Url { get; set; }
}
