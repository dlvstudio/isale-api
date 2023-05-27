using Newtonsoft.Json;
using System.Collections.Generic;

public class FbPageTokenResponse {
    [JsonProperty("data")]
    public IEnumerable<FbPageTokenDataItem> Data { get; set; }
}

public class FbPageTokenDataItem {
    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("name")]
    public string Name { get; set; }

    [JsonProperty("id")]
    public string Id { get; set; }
}