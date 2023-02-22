using Microsoft.AspNetCore.Http;

public class UploadImageViewModel {
    public string Title { get; set; }
    public string[] ChannelAliases { get; set; }
    public string[] Tags { get; set; }
    public string Code { get; set; }
}