using System;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Linq;

[Route("[controller]")]
[ApiController]
public class ZaloHookController : ControllerBase
{
    private readonly IChannelQueueService<HookObject> _queueMessage;
    private readonly HttpClient _httpClient;


    public ZaloHookController(
        IChannelQueueService<HookObject> queueMessage,
        IHttpClientFactory httpClientFactory
    )
    {
        _queueMessage = queueMessage;
        _httpClient = httpClientFactory.CreateClient();
    }

    [HttpGet]
    public string Get()
    {
        return "123";
    }

    [HttpGet]
    [Route("TestUser")]
    public async Task<string> TestUser(string token)
    {
        var json = @"
            {
                ""app_id"": ""3172407167023250080"",
                ""user_id_by_app"": ""582379654628259956"",
                ""event_name"": ""user_submit_info"",
                ""timestamp"": ""1680942200839"",
                ""sender"": {
                    ""id"": ""1695259959489112035""
                },
                ""recipient"": {
                    ""id"": ""579745863508352884""
                },
                ""info"": {
                    ""address"": ""address"",
                    ""phone"": ""012345688"",
                    ""city"": ""city"",
                    ""district"": ""district"",
                    ""name"": ""name"",
                    ""ward"": ""ward""
                }
            }
        ";
        var updateObj = JsonConvert.DeserializeObject<HookObject>(json);
        updateObj.Json = json;
        await _queueMessage.WriteAsync(updateObj);
        return token;
    }

    [HttpGet]
    [Route("TestMess")]
    public async Task<string> TestMess(string token)
    {
        var json = @"
            {
                ""app_id"": ""360846524940903967"",
                ""sender"": {
                    ""id"": ""246845883529197922""
                },
                ""user_id_by_app"": ""552177279717587730"",
                ""recipient"": {
                    ""id"": ""388613280878808645""
                },
                ""event_name"": ""user_send_text"",
                ""message"": {
                    ""text"": ""message"",
                    ""msg_id"": ""96d3cdf3af150460909""
                },
                ""timestamp"": ""154390853474""
            }
        ";
        var updateObj = JsonConvert.DeserializeObject<HookObject>(json);
        updateObj.Json = json;
        await _queueMessage.WriteAsync(updateObj);
        return token;
    }

    [HttpPost]
    public async Task<string> Post()
    {
        string json;
        try
        {
            using (var sr = new StreamReader(this.Request.Body))
            {
                json = await sr.ReadToEndAsync();
                var updateObj = JsonConvert.DeserializeObject<HookObject>(json);
                updateObj.Json = json;
                await _queueMessage.WriteAsync(updateObj);
            }
        }
        catch (Exception ex)
        {
        }
        return "OK";
    }
}