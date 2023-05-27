using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using FirebaseAdmin.Messaging;
using Microsoft.AspNetCore.Mvc;

namespace atakafe_api.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class CustomerController : ControllerBase
    {
        private readonly IChannelQueueService<UserActivity> _queueMessage;
        private readonly ISqlService _sqlService;

        private const string Key = "123456ABC999";


        public CustomerController(
            IChannelQueueService<UserActivity> queueMessage,
            ISqlService sqlService
        )
        {
            _queueMessage = queueMessage;
            _sqlService = sqlService;
        }

        [HttpGet]
        [Route("UpgradeByShop")]
        public async Task<ActionResult<string>> UpgradeByShop(string key, int shopId, int month)
        {
            if (key != Key)
            {
                return NotFound();
            }

            var shops = await _sqlService.ListAsync("shop", new Dictionary<string, object>() { { "id", shopId } }, null);
            if (shops == null || !shops.Any())
            {
                return "NO SHOP!";
            }
            var shop = shops.First();

            var userId = shop["userId"].ToString();

            var table = "subscription";
            Dictionary<string, string> exceptionFunctions = BuildExceptionFunctions(table.ToString());

            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "request-pro",
                Action = "upgrade",
                Note = month.ToString(),
            });

            var currentPlan = await GetCurrentPlan(userId);
            if (currentPlan != null)
            {
                var lastDate = Convert.ToDateTime(currentPlan["endDate"]);
                if (lastDate < DateTime.Today)
                {
                    lastDate = DateTime.Today;
                }
                currentPlan["endDate"] = lastDate.AddMonths(month);
                currentPlan["isTrial"] = false;

                await _sqlService.SaveAsync(currentPlan, table,
                    "Id",
                    new List<string>() { "Id", "UserId" },
                    exceptionFunctions);
                await SendOrderMessageToShopOwnerAsync(userId);
                return "Exists. Add new OK: " + userId + "; Month: " + month;
            }

            var model = new Dictionary<string, object>();
            model["userId"] = userId;
            model["startDate"] = DateTime.Today;
            model["endDate"] = DateTime.Today.AddMonths(month);
            model["subscriptionType"] = "PRO";

            await _sqlService.SaveAsync(model, table,
                "Id",
                new List<string>() { "Id", "UserId" },
                exceptionFunctions);
            await SendOrderMessageToShopOwnerAsync(userId);
            return "OK: " + userId + "; Month: " + month;
        }

        [HttpGet]
        [Route("Upgrade")]
        public async Task<ActionResult<string>> Upgrade(string key, string userId, int month)
        {
            if (key != Key)
            {
                return NotFound();
            }
            if (string.IsNullOrWhiteSpace(userId))
            {
                return NotFound();
            }

            var table = "subscription";
            Dictionary<string, string> exceptionFunctions = BuildExceptionFunctions(table.ToString());
            
            await _queueMessage.WriteAsync(new UserActivity()
            {
                UserId = userId,
                Feature = "request-pro",
                Action = "upgrade",
                Note = month.ToString(),
            });

            var currentPlan = await GetCurrentPlan(userId);
            if (currentPlan != null)
            {
                var lastDate = Convert.ToDateTime(currentPlan["endDate"]);
                if (lastDate < DateTime.Today)
                {
                    lastDate = DateTime.Today;
                }
                currentPlan["endDate"] = lastDate.AddMonths(month);
                currentPlan["isTrial"] = false;

                await _sqlService.SaveAsync(currentPlan, table,
                    "Id",
                    new List<string>() { "Id", "UserId" },
                    exceptionFunctions);
                await SendOrderMessageToShopOwnerAsync(userId);
                return "Exists. Add new OK: " + userId + "; Month: " + month;
            }

            var shops = await _sqlService.ListAsync("shop", new Dictionary<string, object>() { { "userId", userId } }, null);
            if (shops == null || !shops.Any())
            {
                // auto create shop
                var shop = new Dictionary<string, object>();
                shop["name"] = "Shop";
                shop["userId"] = userId;
                Dictionary<string, string> exceptionFunctionsForShop = BuildExceptionFunctions("shop");
                await _sqlService.SaveAsync(shop, "shop",
                    "Id",
                    new List<string>() { "Id", "UserId" },
                exceptionFunctionsForShop);
            }

            var model = new Dictionary<string, object>();
            model["userId"] = userId;
            model["startDate"] = DateTime.Today;
            model["endDate"] = DateTime.Today.AddMonths(month);
            model["subscriptionType"] = "PRO";

            await _sqlService.SaveAsync(model, table,
                "Id",
                new List<string>() { "Id", "UserId" },
                exceptionFunctions);
            await SendOrderMessageToShopOwnerAsync(userId);
            return "OK: " + userId + "; Month: " + month;
        }

        [HttpGet]
        [Route("Mess")]
        public async Task<ActionResult<string>> Mess(string key, string userId, string mess, string title)
        {
            if (key != Key)
            {
                return NotFound();
            }
            if (string.IsNullOrWhiteSpace(userId))
            {
                return NotFound();
            }
            string titleShow = string.IsNullOrEmpty(title) ? "Cảm ơn bạn đã đóng góp ý kiến!" : title;
            await SendOrderMessageToShopOwnerAsync(userId, mess, titleShow);
            return "OK: " + userId + "; Mess: " + mess + "; Title: " + titleShow;
        }

        [HttpGet]
        [Route("SuperSaleOff")]
        public async Task<ActionResult<string>> SuperSaleOff(string key)
        {
            if (key != Key)
            {
                return NotFound();
            }
            var model = new Dictionary<string, object>{
                {"createdAt", DateTime.Now.AddDays(-3)},
                // {"userId", "F480982901E347B9BEABDAFBFBDE9080"},
            };
            var query = new QueryModelOnSearch()
            {
                WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"createdAt", new List<string>{EnumSearchFunctions.SMALLER_THAN}},
                    // {"userId", new List<string>{EnumSearchFunctions.EQUALS}},
                }
            };
            var tokens = await _sqlService.ListAsync("fcmtoken", model, query);

            if (tokens == null || !tokens.Any())
            {
                return "No tokens";
            }

            var modelSub = new Dictionary<string, object>{
                {"id", 0},
                // {"userId", "F480982901E347B9BEABDAFBFBDE9080"},
            };
            var querySub = new QueryModelOnSearch()
            {
                WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"id", new List<string>{EnumSearchFunctions.BIGGER_THAN}},
                    // {"userId", new List<string>{EnumSearchFunctions.EQUALS}},
                }
            };

            var subs = await _sqlService.ListAsync("subscription", modelSub, querySub);

            var modelConfig = new Dictionary<string, object>{
                {"id", 0},
            };
            var queryConfig = new QueryModelOnSearch()
            {
                WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"id", new List<string>{EnumSearchFunctions.BIGGER_THAN}},
                    // {"userId", new List<string>{EnumSearchFunctions.EQUALS}},
                }
            };
            var shopConfigs = await _sqlService.ListAsync("shop_config", modelConfig, queryConfig);

            var tokensToSend = new Dictionary<string, string>();

            foreach (var token in tokens)
            {
                string userId = token["userId"].ToString();
                var hasSub = false;
                if (subs != null && subs.Any())
                {
                    foreach (var sub in subs)
                    {
                        string subUserId = sub["userId"].ToString();
                        if (subUserId == userId)
                        {
                            hasSub = true;
                            break;
                        }
                    }
                }
                if (hasSub)
                {
                    continue;
                }
                if (!tokensToSend.ContainsKey(token["token"].ToString()))
                {
                    var language = "vn";
                    foreach (var cf in shopConfigs)
                    {
                        string cfUserId = cf["userId"].ToString();
                        if (cfUserId == userId)
                        {
                            language = cf["language"].ToString();
                            break;
                        }
                    }
                    tokensToSend.Add(token["token"].ToString(), language);
                }
            }

            if (tokensToSend.Any())
            {
                string title = "ISALE thông báo";
                string titleEn = "ISALE";
                string body = "Chỉ 86K/tháng (giá gốc 99K) cho bản CHUYÊN NGHIỆP, không giới hạn tính năng, không quảng cáo, miễn phí nâng cấp tính năng mới. Mua càng nhiều tháng càng được KHUYẾN MẠI!";
                string bodyEn = "Only 3.7$/month (original: 4.3$) for PRO PLAN, no limitations, no ads and free upgrade new features forever. Buy more to get more SALES OFF!";
                await SendMultiMessagesAsync(tokensToSend, title, body, titleEn, bodyEn, "request-pro");
            }

            return "OK";
        }

        [HttpGet]
        [Route("Giamsoc")]
        public async Task<ActionResult<string>> Giamsoc(string key)
        {
            if (key != Key)
            {
                return NotFound();
            }
            var model = new Dictionary<string, object>{
                {"lastDate", DateTime.Now.AddDays(-7)},
                {"age", 3},
            };
            var query = new QueryModelOnSearch()
            {
                WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"lastDate", new List<string>{EnumSearchFunctions.SMALLER_THAN}},
                    {"age", new List<string>{EnumSearchFunctions.SMALLER_THAN}},
                }
            };
            var tokens = await _sqlService.ListAsync("fcmtoken", model, query);

            if (tokens == null || !tokens.Any())
            {
                return "No tokens";
            }

            var modelSub = new Dictionary<string, object>{
                {"id", 0},
                // {"userId", "F480982901E347B9BEABDAFBFBDE9080"},
            };
            var querySub = new QueryModelOnSearch()
            {
                WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"id", new List<string>{EnumSearchFunctions.BIGGER_THAN}},
                    // {"userId", new List<string>{EnumSearchFunctions.EQUALS}},
                }
            };

            var subs = await _sqlService.ListAsync("subscription", modelSub, querySub);

            var modelConfig = new Dictionary<string, object>{
                {"id", 0},
            };
            var queryConfig = new QueryModelOnSearch()
            {
                WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"id", new List<string>{EnumSearchFunctions.BIGGER_THAN}},
                    // {"userId", new List<string>{EnumSearchFunctions.EQUALS}},
                }
            };
            var shopConfigs = await _sqlService.ListAsync("shop_config", modelConfig, queryConfig);

            var tokensToSend = new Dictionary<string, string>();

            foreach (var token in tokens)
            {
                string userId = token["userId"].ToString();
                var hasSub = false;
                if (subs != null && subs.Any())
                {
                    foreach (var sub in subs)
                    {
                        string subUserId = sub["userId"].ToString();
                        if (subUserId == userId)
                        {
                            hasSub = true;
                            break;
                        }
                    }
                }
                if (hasSub)
                {
                    continue;
                }
                if (!tokensToSend.ContainsKey(token["token"].ToString()))
                {
                    var language = "vn";
                    foreach (var cf in shopConfigs)
                    {
                        string cfUserId = cf["userId"].ToString();
                        if (cfUserId == userId)
                        {
                            language = cf["language"].ToString();
                            break;
                        }
                    }
                    tokensToSend.Add(token["token"].ToString(), language);
                }
            }

            if (tokensToSend.Any())
            {
                string title = "ISALE giảm sốc!";
                string titleEn = "ISALE super sales!";
                string body = "Mua gói PRO ngay hôm nay, bạn được lì xì ngay 1 tháng để kiếm vận may cả năm Quý Mão - khi mua từ 6 tháng - 1 năm. Còn chờ gì nữa!";
                string bodyEn = "Buy today to get 1 Month plus to get the luck for all of New Year coming - when you buy 6 month or 1 year PRO Plan. Don't hesitate!";
                await SendMultiMessagesAsync(tokensToSend, title, body, titleEn, bodyEn, "request-pro");
            }

            return "OK";
        }

        [HttpGet]
        [Route("Lixi")]
        public async Task<ActionResult<string>> Lixi(string key)
        {
            if (key != Key)
            {
                return NotFound();
            }
            var model = new Dictionary<string, object>{
                {"id", 0},
                // {"userId", "F480982901E347B9BEABDAFBFBDE9080"},
            };
            var query = new QueryModelOnSearch()
            {
                WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"id", new List<string>{EnumSearchFunctions.BIGGER_THAN}},
                    // {"userId", new List<string>{EnumSearchFunctions.EQUALS}},
                }
            };
            var tokens = await _sqlService.ListAsync("fcmtoken", model, query);

            if (tokens == null || !tokens.Any())
            {
                return "No tokens";
            }

            var modelSub = new Dictionary<string, object>{
                {"id", 0},
                // {"userId", "F480982901E347B9BEABDAFBFBDE9080"},
            };
            var querySub = new QueryModelOnSearch()
            {
                WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"id", new List<string>{EnumSearchFunctions.BIGGER_THAN}},
                    // {"userId", new List<string>{EnumSearchFunctions.EQUALS}},
                }
            };

            var subs = await _sqlService.ListAsync("subscription", modelSub, querySub);

            var modelConfig = new Dictionary<string, object>{
                {"id", 0},
            };
            var queryConfig = new QueryModelOnSearch()
            {
                WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"id", new List<string>{EnumSearchFunctions.BIGGER_THAN}},
                    // {"userId", new List<string>{EnumSearchFunctions.EQUALS}},
                }
            };
            var shopConfigs = await _sqlService.ListAsync("shop_config", modelConfig, queryConfig);

            var tokensToSend = new Dictionary<string, string>();

            foreach (var token in tokens)
            {
                string userId = token["userId"].ToString();
                var hasSub = false;
                if (subs != null && subs.Any())
                {
                    foreach (var sub in subs)
                    {
                        string subUserId = sub["userId"].ToString();
                        if (subUserId == userId)
                        {
                            hasSub = true;
                            break;
                        }
                    }
                }
                if (hasSub)
                {
                    continue;
                }
                if (!tokensToSend.ContainsKey(token["token"].ToString()))
                {
                    var language = "vn";
                    foreach (var cf in shopConfigs)
                    {
                        string cfUserId = cf["userId"].ToString();
                        if (cfUserId == userId)
                        {
                            language = cf["language"].ToString();
                            break;
                        }
                    }
                    tokensToSend.Add(token["token"].ToString(), language);
                }
            }

            if (tokensToSend.Any())
            {
                string title = "Lì xì đầu năm";
                string titleEn = "Gift! Happy new luna year!";
                string body = "Bạn có lì xì! Bạn sẽ được tặng thêm 1 tháng khi mua Gói Chuyên Nghiệp từ 3 tháng trở lên! Chương trình chỉ giới hạn trong tháng giêng, hãy nhanh tay để có cơ hội trải nghiệm gói chuyên nghiệp với không giới hạn chức năng bạn nhé!";
                string bodyEn = "For a new Luna Year, ISale wish you all the best and luck. ISale give you a half of month Pro Plan when you buy a 3 months Pro Plan. Regards!";
                await SendMultiMessagesAsync(tokensToSend, title, body, titleEn, bodyEn, "request-pro");
            }

            return "OK";
        }

        [HttpGet]
        [Route("RequestUpdate")]
        public async Task<ActionResult<string>> RequestUpdate(string key)
        {
            if (key != Key)
            {
                return NotFound();
            }
            // var userIdTest = "";
            var userIdTest = "F480982901E347B9BEABDAFBFBDE9080";
            var model = new Dictionary<string, object>{
                {"id", 0},
            };
            if (!string.IsNullOrWhiteSpace(userIdTest))
            {
                model["userId"] = userIdTest;
            }
            var query = new QueryModelOnSearch()
            {
                WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"id", new List<string>{EnumSearchFunctions.BIGGER_THAN}},
                }
            };
            if (!string.IsNullOrWhiteSpace(userIdTest))
            {
                query.WhereFieldQuerys["userId"] = new List<string> { EnumSearchFunctions.EQUALS };
            }
            var tokens = await _sqlService.ListAsync("fcmtoken", model, query);

            if (tokens == null || !tokens.Any())
            {
                return "No tokens";
            }

            var modelConfig = new Dictionary<string, object>{
                {"id", 0},
            };
            var queryConfig = new QueryModelOnSearch()
            {
                WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"id", new List<string>{EnumSearchFunctions.BIGGER_THAN}},
                    // {"userId", new List<string>{EnumSearchFunctions.EQUALS}},
                }
            };
            var shopConfigs = await _sqlService.ListAsync("shop_config", modelConfig, queryConfig);

            var tokensToSend = new Dictionary<string, string>();

            foreach (var token in tokens)
            {
                string userId = token["userId"].ToString();
                if (!tokensToSend.ContainsKey(token["token"].ToString()))
                {
                    var language = "vn";
                    foreach (var cf in shopConfigs)
                    {
                        string cfUserId = cf["userId"].ToString();
                        if (cfUserId == userId)
                        {
                            language = cf["language"].ToString();
                            break;
                        }
                    }
                    tokensToSend.Add(token["token"].ToString(), language);
                }
            }

            if (tokensToSend.Any())
            {
                string title = "ISALE thông báo";
                string titleEn = "ISALE";
                string body = "ISale đã có phiên bản mới, hãy tắt bật ứng dụng này, và đồng ý cập nhật để nhận phiên bản mới nhất bạn nhé. Xin không tắt app cho đến khi có thông báo cập nhật xong (có thể đến hơn 1 phút).";
                string bodyEn = "We have a new version, please close and reopen this app, and agree to get the new update. Please don't turn off the app until you got a finish alert (may over 1 minute).";
                await SendMultiMessagesAsync(tokensToSend, title, body, titleEn, bodyEn, "request-update");
            }

            return "OK";
        }

        [HttpGet]
        [Route("RequestVersion")]
        public async Task<ActionResult<string>> RequestVersion(string key)
        {
            if (key != Key)
            {
                return NotFound();
            }
            var userIdTest = "";
            // var userIdTest = "A14D904713A449ABB6420AEA2E32C7BB";
            var model = new Dictionary<string, object>{
                {"id", 0},
            };
            if (!string.IsNullOrWhiteSpace(userIdTest))
            {
                model["userId"] = userIdTest;
            }
            var query = new QueryModelOnSearch()
            {
                WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"id", new List<string>{EnumSearchFunctions.BIGGER_THAN}},
                }
            };
            if (!string.IsNullOrWhiteSpace(userIdTest))
            {
                query.WhereFieldQuerys["userId"] = new List<string> { EnumSearchFunctions.EQUALS };
            }
            var tokens = await _sqlService.ListAsync("fcmtoken", model, query);

            if (tokens == null || !tokens.Any())
            {
                return "No tokens";
            }

            var modelConfig = new Dictionary<string, object>{
                {"id", 0},
            };
            var queryConfig = new QueryModelOnSearch()
            {
                WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"id", new List<string>{EnumSearchFunctions.BIGGER_THAN}},
                    // {"userId", new List<string>{EnumSearchFunctions.EQUALS}},
                }
            };
            var shopConfigs = await _sqlService.ListAsync("shop_config", modelConfig, queryConfig);

            var tokensToSend = new Dictionary<string, string>();

            foreach (var token in tokens)
            {
                string userId = token["userId"].ToString();
                if (!tokensToSend.ContainsKey(token["token"].ToString()))
                {
                    var language = "vn";
                    foreach (var cf in shopConfigs)
                    {
                        string cfUserId = cf["userId"].ToString();
                        if (cfUserId == userId)
                        {
                            language = cf["language"].ToString();
                            break;
                        }
                    }
                    tokensToSend.Add(token["token"].ToString(), language);
                }
            }

            if (tokensToSend.Any())
            {
                string title = "ISALE thông báo";
                string titleEn = "ISALE";
                string body = "ISale đã có phiên bản mới trên App Store/ Google Play, hãy truy cập ngay App Store/ Google Play để tải về bản mới nhất bạn nhé.";
                string bodyEn = "We have a new version on App Store/ Google Play, please access to App Store/ Google Play to get the latest version.";
                await SendMultiMessagesAsync(tokensToSend, title, body, titleEn, bodyEn, "request-version");
            }

            return "OK";
        }

        [HttpGet]
        [Route("RequestSurvey")]
        public async Task<ActionResult<string>> RequestSurvey(string key)
        {
            if (key != Key)
            {
                return NotFound();
            }
            var model = new Dictionary<string, object>{
                {"id", 0},
                // {"userId", "A14D904713A449ABB6420AEA2E32C7BB"},
            };
            var query = new QueryModelOnSearch()
            {
                WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"id", new List<string>{EnumSearchFunctions.BIGGER_THAN}},
                    // {"userId", new List<string>{EnumSearchFunctions.EQUALS}},
                }
            };
            var tokens = await _sqlService.ListAsync("fcmtoken", model, query);
            Dictionary<string, string> tokensToSend = new Dictionary<string, string>();

            if (tokens == null || !tokens.Any())
            {
                return "No tokens";
            }

            var modelSurvey = new Dictionary<string, object>{
                {"id", 0},
                {"subject", "Survey"},
                // {"userId", "F480982901E347B9BEABDAFBFBDE9080"},
            };
            var querySurvey = new QueryModelOnSearch()
            {
                WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"id", new List<string>{EnumSearchFunctions.BIGGER_THAN}},
                    {"subject", new List<string>{EnumSearchFunctions.EQUALS}},
                    // {"userId", new List<string>{EnumSearchFunctions.EQUALS}},
                }
            };

            var surveys = await _sqlService.ListAsync("ticket", modelSurvey, querySurvey);

            var modelConfig = new Dictionary<string, object>{
                {"id", 0},
            };
            var queryConfig = new QueryModelOnSearch()
            {
                WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"id", new List<string>{EnumSearchFunctions.BIGGER_THAN}},
                    // {"userId", new List<string>{EnumSearchFunctions.EQUALS}},
                }
            };
            var shopConfigs = await _sqlService.ListAsync("shop_config", modelConfig, queryConfig);

            foreach (var token in tokens)
            {
                string userId = token["userId"].ToString();
                var hasSurvey = false;
                if (surveys != null && surveys.Any())
                {
                    foreach (var survey in surveys)
                    {
                        string subUserId = survey["userId"].ToString();
                        if (subUserId == userId)
                        {
                            hasSurvey = true;
                            break;
                        }
                    }
                }
                if (hasSurvey)
                {
                    continue;
                }
                if (!tokensToSend.ContainsKey(token["token"].ToString()))
                {
                    var language = "vn";
                    foreach (var cf in shopConfigs)
                    {
                        string cfUserId = cf["userId"].ToString();
                        if (cfUserId == userId)
                        {
                            language = cf["language"].ToString();
                            break;
                        }
                    }
                    tokensToSend.Add(token["token"].ToString(), language);
                }
            }

            if (tokensToSend.Any())
            {
                string title = "ISALE thông báo";
                string titleEn = "ISALE";
                string body = "Chưa đến 1 phút, hãy giúp chúng tôi thực hiện một khảo sát, nhằm nâng cao hơn nữa chất lượng của ISALE và phục vụ bạn tốt hơn nữa nhé.";
                string bodyEn = "Just one minute, please help us to do a survey, to make the app better and fit your need with this app.";
                await SendMultiMessagesAsync(tokensToSend, title, body, titleEn, bodyEn, "survey");
            }

            return "OK";
        }

        [HttpGet]
        [Route("RequestChatgpt")]
        public async Task<ActionResult<string>> RequestChatgpt(string key)
        {
            if (key != Key)
            {
                return NotFound();
            }
            var model = new Dictionary<string, object>{
                {"id", 0},
                // {"userId", "A14D904713A449ABB6420AEA2E32C7BB"},
            };
            var query = new QueryModelOnSearch()
            {
                WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"id", new List<string>{EnumSearchFunctions.BIGGER_THAN}},
                    // {"userId", new List<string>{EnumSearchFunctions.EQUALS}},
                }
            };
            var tokens = await _sqlService.ListAsync("fcmtoken", model, query);
            Dictionary<string, string> tokensToSend = new Dictionary<string, string>();

            if (tokens == null || !tokens.Any())
            {
                return "No tokens";
            }

            var modelConfig = new Dictionary<string, object>{
                {"id", 0},
            };
            var queryConfig = new QueryModelOnSearch()
            {
                WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"id", new List<string>{EnumSearchFunctions.BIGGER_THAN}},
                    // {"userId", new List<string>{EnumSearchFunctions.EQUALS}},
                }
            };
            var shopConfigs = await _sqlService.ListAsync("shop_config", modelConfig, queryConfig);

            foreach (var token in tokens)
            {
                string userId = token["userId"].ToString();
                if (!tokensToSend.ContainsKey(token["token"].ToString()))
                {
                    var language = "vn";
                    foreach (var cf in shopConfigs)
                    {
                        string cfUserId = cf["userId"].ToString();
                        if (cfUserId == userId)
                        {
                            language = cf["language"].ToString();
                            break;
                        }
                    }
                    tokensToSend.Add(token["token"].ToString(), language);
                }
            }

            if (tokensToSend.Any())
            {
                string title = "ISALE thông báo";
                string titleEn = "ISALE";
                string body = "Bạn có muốn tích hợp ChatGPT, Chat fanpage, Zalo OA vào ISale hay không? Giúp chúng tôi biết được mong muốn này vào yêu cầu Tính năng mới nhé!";
                string bodyEn = "Do you want to integrate Chatgpt or Facebook Chat, Zalo OA into ISale? Help us to fill it to New feature input.";
                await SendMultiMessagesAsync(tokensToSend, title, body, titleEn, bodyEn, "survey");
            }

            return "OK";
        }

        [HttpGet]
        [Route("RequestRate")]
        public async Task<ActionResult<string>> RequestRate(string key)
        {
            if (key != Key)
            {
                return NotFound();
            }
            var model = new Dictionary<string, object>{
                {"id", 0},
                // {"userId", "F480982901E347B9BEABDAFBFBDE9080"},
            };
            var query = new QueryModelOnSearch()
            {
                WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"id", new List<string>{EnumSearchFunctions.BIGGER_THAN}},
                    // {"userId", new List<string>{EnumSearchFunctions.EQUALS}},
                }
            };
            var tokens = await _sqlService.ListAsync("fcmtoken", model, query);
            if (tokens == null || !tokens.Any())
            {
                return "No tokens";
            }

            var modelSub = new Dictionary<string, object>{
                {"id", 0},
                // {"userId", "F480982901E347B9BEABDAFBFBDE9080"},
            };
            var querySub = new QueryModelOnSearch()
            {
                WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"id", new List<string>{EnumSearchFunctions.BIGGER_THAN}},
                    // {"userId", new List<string>{EnumSearchFunctions.EQUALS}},
                }
            };
            var subs = await _sqlService.ListAsync("subscription", modelSub, querySub);

            var modelConfig = new Dictionary<string, object>{
                {"id", 0},
            };
            var queryConfig = new QueryModelOnSearch()
            {
                WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"id", new List<string>{EnumSearchFunctions.BIGGER_THAN}},
                    // {"userId", new List<string>{EnumSearchFunctions.EQUALS}},
                }
            };
            var shopConfigs = await _sqlService.ListAsync("shop_config", modelConfig, queryConfig);
            Dictionary<string, string> tokensToSend = new Dictionary<string, string>();

            foreach (var token in tokens)
            {
                string userId = token["userId"].ToString();
                var hasSub = false;
                if (subs != null && subs.Any())
                {
                    foreach (var sub in subs)
                    {
                        string subUserId = sub["userId"].ToString();
                        if (subUserId == userId)
                        {
                            hasSub = true;
                            break;
                        }
                    }
                }
                if (hasSub)
                {
                    continue;
                }
                if (!tokensToSend.ContainsKey(token["token"].ToString()))
                {
                    var language = "vn";
                    foreach (var cf in shopConfigs)
                    {
                        string cfUserId = cf["userId"].ToString();
                        if (cfUserId == userId)
                        {
                            language = cf["language"].ToString(); ;
                            break;
                        }
                    }
                    tokensToSend.Add(token["token"].ToString(), language);
                }
            }

            if (tokensToSend.Any())
            {
                string title = "ISALE thông báo";
                string body = "Chưa đến 1 phút, hãy giúp chúng tôi đánh giá ứng dụng bạn nhé, với đánh giá 5* bạn sẽ được lì xì ngay 1 tháng sử dụng gói chuyên nghiệp!";
                string titleEn = "ISALE";
                string bodyEn = "Just one minute, help us to rate this app, with a rate of 5* you will get a 1 Month Pro Plan!";
                await SendMultiMessagesAsync(tokensToSend, title, body, titleEn, bodyEn, "request-rate");
            }

            return "OK";
        }

        private Dictionary<string, string> BuildExceptionFunctions(string table)
        {
            Dictionary<string, string> exceptionFunctions = null;
            var tableUpper = table.ToUpper();
            if (tableUpper == "CONTACT")
            {
                exceptionFunctions = new Dictionary<string, string>();
                exceptionFunctions["LastActive"] = EnumExceptionFunctions.EMPTY_THEN_NOW;
                exceptionFunctions["StaffId"] = EnumExceptionFunctions.EMPTY_THEN_ZERO;
                exceptionFunctions["IsImportant"] = EnumExceptionFunctions.EMPTY_THEN_FALSE;
            }
            if (tableUpper == "NOTE")
            {
                exceptionFunctions = new Dictionary<string, string>();
                exceptionFunctions["createdAt"] = EnumExceptionFunctions.EMPTY_THEN_NOW;
                exceptionFunctions["modifiedAt"] = EnumExceptionFunctions.EMPTY_THEN_NOW;
            }
            return BuildExceptionFunctionsWithLower(exceptionFunctions);
        }

        private async Task<Dictionary<string, object>> GetCurrentPlan(string userId)
        {
            var model = new Dictionary<string, object>{
                {"userId", userId},
                {"subscriptionType", "PRO"},
            };
            var query = new QueryModelOnSearch()
            {
                WhereFieldQuerys = new Dictionary<string, List<string>> {
                    {"userId", new List<string>{EnumSearchFunctions.EQUALS}},
                    {"startDate", new List<string>{EnumSearchFunctions.SMALLER_THAN_TODAY}},
                    {"endDate", new List<string>{EnumSearchFunctions.BIGGER_THAN_TODAY}},
                }
            };
            var subscriptions = await _sqlService.ListAsync("subscription", model, query);
            if (subscriptions != null && subscriptions.Any())
            {
                return subscriptions.First();
            }
            return null;
        }

        private Dictionary<string, string> BuildExceptionFunctionsWithLower(Dictionary<string, string> exceptionFunctions)
        {
            if (exceptionFunctions == null)
            {
                return exceptionFunctions;
            }
            var newDic = new Dictionary<string, string>();
            var keys = exceptionFunctions.Keys.AsList();
            foreach (var key in keys)
            {
                newDic[key.ToLower()] = exceptionFunctions[key];
            }
            return newDic;
        }

        private async Task<string> SendMultiMessagesAsync(List<string> tokens, String title, string body, string page)
        {
            List<List<Message>> listOfMessages = new List<List<Message>>();
            List<Message> messages = new List<Message>();
            foreach (var token in tokens)
            {
                var message = new Message()
                {
                    Token = token,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Data = new Dictionary<string, string>(){
                            {"notification_foreground", "true"},
                            {"notification_title", title},
                            {"notification_body", body},
                            {"page", page},
                            {"id", "0"},
                        }
                };
                messages.Add(message);
            }
            int i = 1;
            List<Message> temp = null;
            foreach (var message in messages)
            {
                if (i == 1)
                {
                    temp = new List<Message>();
                    listOfMessages.Add(temp);
                }
                if (i <= 500)
                {
                    temp.Add(message);
                }
                if (i == 500)
                {
                    i = 1;
                    temp = null;
                }
                else
                {
                    i++;
                }
            }
            var messaging = FirebaseMessaging.DefaultInstance;
            try
            {
                foreach (var messagesToSend in listOfMessages)
                {
                    await messaging.SendAllAsync(messagesToSend);
                }
            }
            catch (System.Exception err)
            {
                return "NOT OK";
            }
            return "OK";
        }

        private async Task<string> SendMultiMessagesAsync(Dictionary<string, string> tokens, String title, string body, string titleEn, string bodyEn, string page)
        {
            List<List<Message>> listOfMessages = new List<List<Message>>();
            List<Message> messages = new List<Message>();
            foreach (var token in tokens.Keys)
            {
                var language = tokens[token];
                var message = new Message()
                {
                    Token = token,
                    Notification = new Notification
                    {
                        Title = language == "vn" ? title : titleEn,
                        Body = language == "vn" ? body : bodyEn,
                    },
                    Data = new Dictionary<string, string>(){
                            {"notification_foreground", "true"},
                            {"notification_title", language == "vn" ? title : titleEn},
                            {"notification_body", language == "vn" ? bodyEn : bodyEn},
                            {"page", page},
                            {"id", "0"},
                        }
                };
                messages.Add(message);
            }
            int i = 1;
            List<Message> temp = null;
            foreach (var message in messages)
            {
                if (i == 1)
                {
                    temp = new List<Message>();
                    listOfMessages.Add(temp);
                }
                if (i <= 500)
                {
                    temp.Add(message);
                }
                if (i == 500)
                {
                    i = 1;
                    temp = null;
                }
                else
                {
                    i++;
                }
            }
            var messaging = FirebaseMessaging.DefaultInstance;
            try
            {
                foreach (var messagesToSend in listOfMessages)
                {
                    await messaging.SendAllAsync(messagesToSend);
                }
            }
            catch (System.Exception err)
            {
                return "NOT OK";
            }
            return "OK";
        }

        private async Task<string> SendMessageAsync(string token, String title, string body, string page)
        {
            try
            {
                var message = new Message()
                {
                    Token = token,
                    Notification = new Notification
                    {
                        Title = title,
                        Body = body
                    },
                    Data = new Dictionary<string, string>(){
                {"notification_foreground", "true"},
                {"notification_title", title},
                {"notification_body", body},
                {"page", page},
                {"id", "0"},
                }
                };

                var messaging = FirebaseMessaging.DefaultInstance;
                return await messaging.SendAsync(message);
            }
            catch (System.Exception)
            {
                return "NOT OK";
            }
        }

        private async Task SendOrderMessageToShopOwnerAsync(String userId, string message = null, string titleInput = null)
        {
            var tokenModel = new Dictionary<string, object>();
            tokenModel["userId"] = userId;

            var shopConfigs = await _sqlService.ListAsync("shop_config", new Dictionary<string, object> { { "userId", userId } }, null);
            Dictionary<string, object> shopConfig = shopConfigs != null && shopConfigs.Any()
                ? shopConfigs.LastOrDefault()
                : null;
            string lang = "vn";
            if (shopConfig != null)
            {
                lang = shopConfig.ContainsKey("language") ? shopConfig["language"].ToString() : "vn";
            }
            string body = lang == "vn"
                ? "ISALE đã nâng cấp Gói Chuyên Nghiệp. Chú ý thử đăng xuất/đăng nhập lại ứng dụng, hoặc tắt và bật lại ứng dụng nếu ứng dụng chưa nhận gói mới nhé. Xin cảm ơn bạn đã tin dùng ISALE."
                : "ISALE has update your app to Pro Plan. Please to restart or logout/login app to get the new plan. Thank you for believe and using ISale!";
            if (!string.IsNullOrEmpty(message))
            {
                body = message;
            }
            string title = string.IsNullOrEmpty(titleInput) ? "Nâng cấp ISALE" : titleInput;

            var tokens = await _sqlService.ListAsync("fcmtoken", tokenModel, null);
            if (tokens != null && tokens.Any())
            {
                foreach (var token in tokens)
                {
                    var sendResult = await SendMessageAsync(token["token"].ToString(), title, body, "reload");
                }
            }
        }
    }
}
