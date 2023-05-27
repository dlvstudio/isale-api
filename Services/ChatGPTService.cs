using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

public class ChatGPTService : IChatGPTService
{
    private readonly string instanceUrl = "https://api.openai.com/v1";
    private readonly string ChatGPTKey = "***";
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ISqlService _sqlService;
    public ChatGPTService(
        ISqlService sqlService,
        IHttpClientFactory httpClientFactory
    )
    {
        _sqlService = sqlService;
        _httpClientFactory = httpClientFactory;
    }

    public async Task<dynamic> SendMessageAsync(IEnumerable<ChatGPTRoleAndContent> messages)
    {
        if (messages == null || !messages.Any())
        {
            return null;
        }
        using (var client = _httpClientFactory.CreateClient())
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", ChatGPTKey);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            List<ChatGPTRoleAndContent> chats = new List<ChatGPTRoleAndContent>();
            chats.Add(new ChatGPTRoleAndContent { Role = "system", Content = "You are a helpful assistant." });
            if (messages != null && messages.Any())
            {
                foreach (var roleAndContent in messages)
                {
                    chats.Add(roleAndContent);
                }
            }

            var chatLog = new
            {
                Model = "gpt-3.5-turbo",
                Messages = chats,
            };
            string json = Serialize(chatLog);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await client.PostAsync(instanceUrl + "/chat/completions", content);

            var responseContent = await response.Content.ReadAsStringAsync();
            dynamic responseData = JsonConvert.DeserializeObject(responseContent);
            if (responseData == null || responseData.choices == null || responseData.usage == null)
            {
                return null;
            }
            IEnumerable<dynamic> choices = (IEnumerable<dynamic>)responseData.choices;
            dynamic choice = choices.FirstOrDefault();
            if (choice.message == null)
            {
                return null;
            }
            dynamic messageOutput = choice.message;
            string contentOutput = (string)messageOutput.content;
            dynamic usage = responseData.usage;
            var usageOutput = new
            {
                prompt_tokens = usage.prompt_tokens,
                completion_tokens = usage.completion_tokens,
                total_tokens = usage.total_tokens,
            };
            var output = new
            {
                message = contentOutput,
                usage = usageOutput
            };
            return output;
        }
    }

    private string Serialize(dynamic obj)
    {
        string json = JsonConvert.SerializeObject(obj, new JsonSerializerSettings
        {
            ContractResolver = new Newtonsoft.Json.Serialization.CamelCasePropertyNamesContractResolver()
        });
        return json;
    }
}