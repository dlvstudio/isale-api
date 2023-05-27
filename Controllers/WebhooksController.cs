using System;
using Microsoft.AspNetCore.Mvc;
using System.IO;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace atakafe_api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class WebHooksController : ControllerBase
    {
        private readonly IChannelQueueService<FbUpdateObject> _queueMessage;
        private readonly ISqlService _sqlService;

        public WebHooksController(
            IChannelQueueService<FbUpdateObject> queueMessage,
            ISqlService sqlService
        )
        {
            _queueMessage = queueMessage;
            _sqlService = sqlService;
        }

        // GET: api/webhooks
        [HttpGet]
        public string Get([FromQuery(Name = "hub.mode")] string hub_mode,
            [FromQuery(Name = "hub.challenge")] string hub_challenge,
            [FromQuery(Name = "hub.verify_token")] string hub_verify_token)
        {
            return hub_challenge;
        }

        [HttpGet]
        [Route("TestMess")]
        public async Task<string> TestMess(string token)
        {
            var json = "{\"object\":\"page\",\"entry\":[{\"id\":\"1178682335584024\",\"time\":1673006900724,\"messaging\":[{\"sender\":{\"id\":\"3806769879344881\"},\"recipient\":{\"id\":\"1178682335584024\"},\"timestamp\":1673006900381,\"message\":{\"mid\":\"m_xUzn75XKEE8jSg6_Zr0sP1QE8Vs1XEmFNcfuEVFC7IXmrMDbz_yllfiT55hzX6MLNANmiZCqAjE1UJuVH2SSSg\",\"text\":\"0938212188 DELL01\",\"tags\":{\"source\":\"customer_chat_plugin\"}}}]}]}";
            var updateObj = JsonConvert.DeserializeObject<FbUpdateObject>(json);
            updateObj.Json = json;
            await _queueMessage.WriteAsync(updateObj);
            return token;
        }

        

        [HttpGet]
        [Route("TestComm")]
        public async Task<string> TestComm(string token)
        {
            var json = "{\"entry\": [{\"id\": \"1178682335584024\", \"time\": 1673942028, \"changes\": [{\"value\": {\"from\": {\"id\": \"9076773499006938\", \"name\": \"Tam T\u00e0i Lu\u1eadn\"}, \"post\": {\"status_type\": \"added_photos\", \"is_published\": true, \"updated_time\": \"2023-01-17T07:53:45+0000\", \"permalink_url\": \"https://www.facebook.com/dlv.isale/posts/pfbid02oeSK524aP4PxgPNAqZZNQK6LBEmgyTNudCCBeHLJ6LwnCnQWbLmRwRMpUZ4bbC6nl\", \"promotion_status\": \"inactive\", \"id\": \"1178682335584024_4566560853462805\"}, \"message\": \"em ch\u00e0o shop \u1ea1\", \"post_id\": \"1178682335584024_4566560853462805\", \"comment_id\": \"4566560853462805_3196978633946723\", \"created_time\": 1673942025, \"item\": \"comment\", \"parent_id\": \"1178682335584024_4566560853462805\", \"verb\": \"add\"}, \"field\": \"feed\"}]}], \"object\": \"page\"}";
            var updateObj = JsonConvert.DeserializeObject<FbUpdateObject>(json);
            updateObj.Json = json;
            await _queueMessage.WriteAsync(updateObj);
            return token;
        }

        [HttpPost]
        public async Task Post()
        {
            string json;
            try
            {
                using (var sr = new StreamReader(this.Request.Body))
                {
                    json = sr.ReadToEnd();
                    var updateObj = JsonConvert.DeserializeObject<FbUpdateObject>(json);
                    updateObj.Json = json;
                    await _queueMessage.WriteAsync(updateObj);
                }
            }
            catch (Exception ex)
            {
                return;
            }
        }
    }
}
