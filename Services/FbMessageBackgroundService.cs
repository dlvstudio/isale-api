using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

public class FbMessageBackgroundService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IChannelQueueService<FbUpdateObject> _queueTokenResponse;
    private readonly ICategoryService _categoryService;
    private readonly IOrderRepository _orderRepository;
    private readonly ISqlService _sqlService;
    private readonly ICacheService _cacheService;
    private readonly string _version = "v15.0";
    private readonly string _requestUrl = "https://graph.facebook.com/{0}/{1}?{2}";
    private readonly List<string> phonePrefixes = new List<string>() {
        "08",
        "09",
        "03",
        "07",
        "05"
    };

    public FbMessageBackgroundService(
        IHttpClientFactory httpClientFactory,
        IChannelQueueService<FbUpdateObject> queueTokenResponse,
        ICategoryService categoryService,
        IOrderRepository orderRepository,
        ISqlService sqlService,
        ICacheService cacheService
    )
    {
        _httpClientFactory = httpClientFactory;
        _queueTokenResponse = queueTokenResponse;
        _categoryService = categoryService;
        _orderRepository = orderRepository;
        _sqlService = sqlService;
        _cacheService = cacheService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (await _queueTokenResponse.WaitToReadAsync(cancellationToken))
        {
            FbUpdateObject response = await _queueTokenResponse.ReadAsync(cancellationToken);
            try
            {
                var messageJson = new Dictionary<string, object>();
                messageJson["message"] = response.Json;
                await _sqlService.SaveAsync(messageJson, "fbwebhookmessage",
                    "Id",
                    new List<string>() { "Id" },
                null);
                var ids = new List<string>();
                var postIds = new List<string>();
                var fromUserIds = new List<string>();
                if (response.Entry == null || !response.Entry.Any())
                {
                    continue;
                }
                foreach (var entry in response.Entry)
                {
                    ids.Add(entry.Id);
                    if (entry.Messaging != null && entry.Messaging.Any())
                    {
                        foreach (var fbMessage in entry.Messaging)
                        {
                            if (fbMessage.Sender == null || fbMessage.Sender.Id == null)
                            {
                                continue;
                            }
                            fromUserIds.Add(fbMessage.Sender.Id);
                        }
                    }
                    if (entry.Changes != null && entry.Changes.Any())
                    {
                        foreach (var fbComment in entry.Changes)
                        {
                            if (fbComment.Value == null)
                            {
                                continue;
                            }
                            postIds.Add(fbComment.Value.PostId);
                        }
                    }
                }
                var pages = await _sqlService.ListAsync("fbpage",
                    new Dictionary<string, object>() { { "ids", ids } },
                    new QueryModelOnSearch()
                    {
                        WhereFieldQuerys = new Dictionary<string, List<string>>() {
                            {"pageid", new List<string> {EnumSearchFunctions.IN, "ids"}},
                        }
                    });
                if (pages == null || !pages.Any())
                {
                    continue;
                }
                var flows = await _sqlService.ListAsync("fbmessageflow",
                    new Dictionary<string, object>() { { "ids", ids }, { "fromUserIds", fromUserIds } },
                    new QueryModelOnSearch()
                    {
                        WhereFieldQuerys = new Dictionary<string, List<string>>() {
                            {"pageid", new List<string> {EnumSearchFunctions.IN, "ids"}},
                            {"fromUserId", new List<string> {EnumSearchFunctions.IN, "fromUserIds"}},
                        }
                    });
                var posts = await _sqlService.ListAsync("fbpost",
                    new Dictionary<string, object>() {
                        { "ids", ids },
                        { "postIds", postIds }
                    },
                    new QueryModelOnSearch()
                    {
                        WhereFieldQuerys = new Dictionary<string, List<string>>() {
                            {"pageId", new List<string> {EnumSearchFunctions.IN, "ids"}},
                            {"postId", new List<string> {EnumSearchFunctions.IN, "postIds"}},
                        }
                    });
                foreach (var entry in response.Entry)
                {
                    var page = pages.FirstOrDefault(p => p.ContainsKey("userId") && p.ContainsKey("pageId") && p["pageId"].ToString() == entry.Id);
                    if (page == null)
                    {
                        continue;
                    }
                    var userId = page["userId"].ToString();
                    var tasks = new List<Task>();
                    if (entry.Messaging != null && entry.Messaging.Any())
                    {
                        foreach (var message in entry.Messaging)
                        {
                            if (message.Message == null)
                            {
                                continue;
                            }
                            var fromUserId = message.Sender != null && message.Sender.Id != null
                                ? message.Sender.Id
                                : string.Empty;
                            if (fromUserId == string.Empty)
                            {
                                continue;
                            }
                            var fromUser = fromUserId != page["pageId"].ToString();
                            var fbMessFlow = flows.FirstOrDefault(f => f.ContainsKey("userId")
                                && f.ContainsKey("pageId")
                                && f.ContainsKey("fromUserId")
                                && f["pageId"].ToString() == entry.Id
                                && f["fromUserId"].ToString() == fromUserId
                            );
                            var messageText = message.Message != null ? message.Message.Text : string.Empty;
                            var messageId = message.Message != null ? message.Message.MessageId : string.Empty;
                            var messageTimestamp = FromMilisecond(message.Timestamp);
                            if (fbMessFlow == null)
                            {
                                fbMessFlow = new Dictionary<string, object>();
                                fbMessFlow["id"] = 0;
                                fbMessFlow["pageId"] = page["pageId"];
                                fbMessFlow["userId"] = userId;
                                fbMessFlow["pageName"] = page["name"];
                                fbMessFlow["fromUserName"] = message.Sender != null && message.Sender.Name != null
                                    ? message.Sender.Name
                                    : string.Empty;
                                fbMessFlow["fromUserId"] = fromUserId;

                            }
                            fbMessFlow["fromUser"] = fromUser;
                            fbMessFlow["lastMessage"] = messageText;
                            fbMessFlow["lastMessageId"] = messageId;
                            fbMessFlow["lastTimestamp"] = messageTimestamp;
                            fbMessFlow["isRead"] = !fromUser;
                            fbMessFlow["notified"] = !fromUser;
                            tasks.Add(this._sqlService.SaveAsync(fbMessFlow, "fbmessageflow", "Id", new List<string>() { "Id", "UserId" }, null));
                            var fromUserName = message.Sender != null && message.Sender.Name != null
                                ? message.Sender.Name
                                : string.Empty;
                            var fbMess = new Dictionary<string, object>();
                            fbMess["id"] = 0;
                            fbMess["userId"] = userId;
                            fbMess["message"] = messageText;
                            fbMess["messageId"] = messageId;
                            fbMess["timestamp"] = messageTimestamp;
                            fbMess["pageId"] = page["pageId"];
                            fbMess["pageName"] = page["name"];
                            fbMess["fromUserName"] = fromUserName;
                            fbMess["fromUserId"] = fromUserId;
                            fbMess["fromUser"] = fromUser;
                            int orderId = 0;
                            if (fromUser)
                            {
                                orderId = await AutoOrderForMessageAsync(page["accessToken"].ToString(), messageText, fromUserId, userId, page["pageId"].ToString(), fromUserName, page["name"].ToString());
                            }
                            fbMess["orderId"] = orderId;
                            tasks.Add(this._sqlService.SaveAsync(fbMess, "fbmessage", "Id", new List<string>() { "Id", "UserId" }, null));
                        }
                    }
                    if (entry.Changes != null && entry.Changes.Any())
                    {
                        foreach (var change in entry.Changes)
                        {
                            if (string.IsNullOrEmpty(change.Field) || change.Field != "feed")
                            {
                                continue;
                            }
                            if (change.Value == null || change.Value.Item != "comment" || change.Value.Verb != "add")
                            {
                                continue;
                            }
                            var fromUserId = change.Value.From != null && change.Value.From.Id != null
                                ? change.Value.From.Id
                                : string.Empty;
                            if (fromUserId == string.Empty)
                            {
                                continue;
                            }
                            var fromUser = fromUserId != page["pageId"].ToString();
                            if (!fromUser)
                            {
                                continue;
                            }
                            var fromUserName = change.Value.From != null && change.Value.From.Name != null
                                ? change.Value.From.Name
                                : string.Empty;
                            var fbPost = posts.FirstOrDefault(f => f.ContainsKey("userId")
                                && f.ContainsKey("pageId")
                                && f.ContainsKey("postId")
                                && f["pageId"].ToString() == entry.Id
                                && f["postId"].ToString() == change.Value.PostId
                            );
                            var commentText = change.Value != null ? change.Value.Message : string.Empty;
                            var commentId = change.Value != null ? change.Value.CommentId : string.Empty;
                            var postId = change.Value.PostId;
                            var commentTimestamp = FromSecond(change.Value.CreatedTime);
                            if (fbPost == null)
                            {
                                fbPost = new Dictionary<string, object>();
                                fbPost["id"] = 0;
                                fbPost["userId"] = userId;
                                fbPost["pageId"] = page["pageId"];
                                fbPost["postId"] = postId;
                                fbPost["pageName"] = page["name"];

                            }
                            fbPost["lastFromUserName"] = fromUserName;
                            fbPost["lastFromUserId"] = fromUserId;
                            fbPost["lastParentId"] = fromUserId;
                            fbPost["fromUser"] = fromUser;
                            fbPost["lastComment"] = commentText;
                            fbPost["lastCommentId"] = commentId;
                            fbPost["lastTimestamp"] = commentTimestamp;
                            fbPost["isRead"] = !fromUser;
                            fbPost["notified"] = !fromUser;
                            tasks.Add(this._sqlService.SaveAsync(fbPost, "fbpost", "Id", new List<string>() { "Id", "UserId" }, null));
                            var fbComment = new Dictionary<string, object>();
                            fbComment["id"] = 0;
                            fbComment["userId"] = userId;
                            fbComment["postId"] = change.Value.PostId;
                            fbComment["parentId"] = change.Value.ParentId;
                            fbComment["commentId"] = commentId;
                            fbComment["comment"] = commentText;
                            fbComment["pageId"] = page["pageId"];
                            fbComment["pageName"] = page["name"];
                            fbComment["timestamp"] = commentTimestamp;
                            fbComment["fromUserId"] = fromUserId;
                            fbComment["fromUserName"] = fromUserName;
                            fbComment["fromUser"] = fromUser;
                            int orderId = 0;
                            if (fromUser)
                            {
                                var isLiveVideo = fbPost.ContainsKey("liveVideoId") && fbPost["liveVideoId"] != null && !string.IsNullOrEmpty(fbPost["liveVideoId"].ToString());
                                await AutoCommentAsync(page["accessToken"].ToString(), commentId, userId, page["pageId"].ToString(), fromUserName, page["name"].ToString(), isLiveVideo);
                                orderId = await AutoOrderForCommentAsync(page["accessToken"].ToString(), commentText, commentId, userId, page["pageId"].ToString(), fromUserName, page["name"].ToString(), isLiveVideo);
                            }
                            fbComment["orderId"] = orderId;
                            tasks.Add(this._sqlService.SaveAsync(fbComment, "fbcomment", "Id", new List<string>() { "Id", "UserId" }, null));
                        }
                    }
                    if (tasks.Any())
                    {
                        await Task.WhenAll(tasks.ToArray());
                        _cacheService.RemoveListEqualItem("fbmessageflow", userId);
                        _cacheService.RemoveListEqualItem("fbmessage", userId);
                        _cacheService.RemoveListEqualItem("fbpost", userId);
                        _cacheService.RemoveListEqualItem("fbcomment", userId);
                    }
                }
            }
            catch (Exception e)
            {
            }
        }
    }

    private DateTime FromSecond(string second)
    {
        var value = Convert.ToInt64(second);
        DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return _epoch.AddSeconds(value);
    }

    private DateTime FromMilisecond(string milisecond)
    {
        var value = Convert.ToInt64(milisecond);
        DateTime _epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        return _epoch.AddMilliseconds(value);
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
    }

    private bool isPhoneNumber(string input)
    {
        if (input.Length < 10)
        {
            return false;
        }
        var hasPrefix = false;
        foreach (var pre in phonePrefixes)
        {
            if (input.StartsWith(pre))
            {
                hasPrefix = true;
                break;
            }
        }
        return hasPrefix;
    }

    private async Task<Dictionary<string, object>> isProductCode(string input, string userId)
    {
        if (input.Length < 5)
        {
            return null;
        }
        var model = new Dictionary<string, object>();
        model["code"] = input;
        model["userId"] = userId;
        var productObjs = await _sqlService.ListAsync("product", model, null);
        return productObjs != null && productObjs.Any() ? productObjs.FirstOrDefault() : null;
    }

    private async Task<int> AutoOrderForCommentAsync(string accessToken, string comment, string commentId, string userId, string pageId, string fromUserName, string pageName, bool isLiveVideo)
    {
        var autoConfigs = await _sqlService.ListAsync("fbautoorderconfig",
            new Dictionary<string, object>() {
                        { "userId", userId },
                        { "applyOnPostComment", true }
            }, null);
        if (autoConfigs == null || !autoConfigs.Any())
        {
            return 0;
        }
        var autoConfig = autoConfigs.FirstOrDefault(c => c.ContainsKey("pageId") && c["pageId"] != null && c["pageId"].ToString() == pageId);
        if (autoConfig == null)
        {
            autoConfig = autoConfigs.FirstOrDefault();
            if (autoConfig.ContainsKey("pageId") && autoConfig["pageId"] != null && !string.IsNullOrEmpty(autoConfig["pageId"].ToString()))
            {
                return 0;
            }
        }
        var applyOnPostComment = autoConfig.ContainsKey("applyOnPostComment") && autoConfig["applyOnPostComment"] != null && Convert.ToBoolean(autoConfig["applyOnPostComment"]);
        var applyOnLiveStream = autoConfig.ContainsKey("applyOnLiveStream") && autoConfig["applyOnLiveStream"] != null && Convert.ToBoolean(autoConfig["applyOnLiveStream"]);
        if ((!isLiveVideo && !applyOnPostComment) || (isLiveVideo && !applyOnLiveStream)) {
            return 0;
        }
        var commentTrim = comment.Replace(",", string.Empty);
        commentTrim = commentTrim.Replace(":", string.Empty);
        commentTrim = commentTrim.Replace(".", string.Empty);
        commentTrim = commentTrim.Replace("!", string.Empty);
        string phone = null;
        Dictionary<string, object> product = null;
        int quantity = 1;
        var arr = commentTrim.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
        if (arr.Count() == 3 && (
            autoConfig["comment"].ToString() == "(số điện thoại) (mã sản phẩm) (số lượng)"
            || autoConfig["comment"].ToString() == "(phone) (product code) (quantity)"
            ))
        {
            var first = arr.FirstOrDefault();
            if (!isPhoneNumber(first))
            {
                return 0;
            }
            phone = first;
            product = await isProductCode(arr[1], userId);
            if (product == null)
            {
                return 0;
            }
            int ret = 0;
            var isNumber = int.TryParse(arr[2], out ret);
            if (!isNumber)
            {
                return 0;
            }
            quantity = ret;
        }
        else if (arr.Count() == 2 && (
            autoConfig["comment"].ToString() == "(mã sản phẩm) (số lượng)"
            || autoConfig["comment"].ToString() == "(product code) (quantity)"
            || autoConfig["comment"].ToString() == "(số điện thoại) (mã sản phẩm)"
            || autoConfig["comment"].ToString() == "(phone) (product code)"
            ))
        {
            if (autoConfig["comment"].ToString() == "(mã sản phẩm) (số lượng)"
            || autoConfig["comment"].ToString() == "(product code) (quantity)")
            {
                product = await isProductCode(arr[0], userId);
                if (product == null)
                {
                    return 0;
                }
                int ret = 0;
                var isNumber = int.TryParse(arr[1], out ret);
                if (!isNumber)
                {
                    return 0;
                }
                quantity = ret;
            }
            else if (autoConfig["comment"].ToString() == "(số điện thoại) (mã sản phẩm)"
            || autoConfig["comment"].ToString() == "(phone) (product code)")
            {
                if (!isPhoneNumber(arr[0]))
                {
                    return 0;
                }
                phone = arr[0];
                product = await isProductCode(arr[1], userId);
                if (product == null)
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }
        else if (arr.Count() == 1 && (
            autoConfig["comment"].ToString() == "(mã sản phẩm)"
            || autoConfig["comment"].ToString() == "(product code)"
            ))
        {
            product = await isProductCode(arr[0], userId);
            if (product == null)
            {
                return 0;
            }
        } else {
            return 0;
        }
        Dictionary<string, object> contact = null;
        if (!string.IsNullOrEmpty(phone))
        {
            var contacts = await _sqlService.ListAsync("contact", new Dictionary<string, object>() {
                {"userId", userId},
                {"mobile", phone},
            }, null);
            contact = contacts != null && contacts.Any() ? contacts.FirstOrDefault() : null;
        }
        var order = await NewOrderAsync(product, comment, quantity, contact, userId, phone);
        if (autoConfig.ContainsKey("replyTemplate") && autoConfig["replyTemplate"] != null && !string.IsNullOrEmpty(autoConfig["replyTemplate"].ToString()))
        {
            var message = autoConfig["replyTemplate"].ToString();
            message = message.Replace("(ten)", fromUserName);
            message = message.Replace("(name)", fromUserName);
            message = message.Replace("(trang)", pageName);
            message = message.Replace("(page)", pageName);
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                var httpResponseMessage = await httpClient.PostAsJsonAsync(
                    string.Format(_requestUrl, _version, commentId + "/comments", "access_token=" + accessToken),
                    new
                    {
                        message = message
                    }
                );
                return order.Id;
                /* 
                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return order.Id;
                } */
            }
        }
        return order.Id;
    }

    private async Task<Order> NewOrderAsync(Dictionary<string, object> product, string comment, int quantity, Dictionary<string, object> contact, string userId, string phone)
    {
        var order = new Order()
        {
            UserId = userId,
            OrderCode = "FB-" + DateTime.Now.ToString("yyMMdd-HHmmss"),
            ContactId = contact != null && contact.ContainsKey("id") && contact["id"] != null ? Convert.ToInt32(contact["id"]) : 0,
            ContactPhone = contact != null && contact.ContainsKey("id") && contact["id"] != null ? null : phone,
        };
        var categories = await _categoryService.GetCategoriesToTrade(Convert.ToInt32(product["id"]), userId);
        var orderItem = new OrderItem()
        {
            Count = quantity,
            Price = product.ContainsKey("price") && product["price"] != null && !string.IsNullOrEmpty(product["price"].ToString())
                ? (decimal?)Convert.ToDecimal(product["price"])
                : null,
            CostPrice = product.ContainsKey("costPrice") && product["costPrice"] != null && !string.IsNullOrEmpty(product["costPrice"].ToString())
                ? (decimal?)Convert.ToDecimal(product["costPrice"])
                : null,
            ProductId = product.ContainsKey("id") && product["id"] != null && !string.IsNullOrEmpty(product["id"].ToString())
                ? Convert.ToInt32(product["id"])
                : 0,
            BasicUnit = null,
            UnitExchange = null,
            IsCombo = product.ContainsKey("isCombo") && product["isCombo"] != null
                ? Convert.ToBoolean(product["isCombo"])
                : false,
            ProductCode = product.ContainsKey("code") && product["code"] != null && !string.IsNullOrEmpty(product["code"].ToString())
                ? product["code"].ToString()
                : null,
            ProductName = product.ContainsKey("title") && product["title"] != null && !string.IsNullOrEmpty(product["title"].ToString())
                ? product["title"].ToString()
                : null,
            ProductAvatar = product.ContainsKey("avatarUrl") && product["avatarUrl"] != null && !string.IsNullOrEmpty(product["avatarUrl"].ToString())
                ? product["avatarUrl"].ToString()
                : null,
            Unit = product.ContainsKey("unit") && product["unit"] != null && !string.IsNullOrEmpty(product["unit"].ToString())
                ? product["unit"].ToString()
                : null,
            ShopPrice = null,
            Items = null,
            Materials = null,
            Categories = categories,
        };
        var net = orderItem.Price * orderItem.Count;
        order.Note = comment;
        order.Change = 0;
        order.Discount = 0;
        order.NetValue = net.HasValue ? net.Value : 0;
        order.Total = net.HasValue ? net.Value : 0;
        var items = new List<OrderItem>();
        items.Add(orderItem);
        order.Items = items;
        var serializerSettings = new JsonSerializerSettings();
        serializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        order.Status = 0;
        order.ItemsJson = JsonConvert.SerializeObject(order.Items, serializerSettings);
        var orderId = await _orderRepository.SaveOrder(order);
        if (orderId == 0)
        {
            return null; ;
        }
        order.Id = orderId;
        return order;
    }

    private async Task<int> AutoOrderForMessageAsync(string accessToken, string messageText, string fromUserId, string userId, string pageId, string fromUserName, string pageName)
    {
        var autoConfigs = await _sqlService.ListAsync("fbautoorderconfig",
            new Dictionary<string, object>() {
                        { "userId", userId },
                        { "isActive", true },
                        { "applyOnMessage", true }
            }, null);
        if (autoConfigs == null || !autoConfigs.Any())
        {
            return 0;
        }
        var autoConfig = autoConfigs.FirstOrDefault(c => c.ContainsKey("pageId") && c["pageId"] != null && c["pageId"].ToString() == pageId);
        if (autoConfig == null)
        {
            autoConfig = autoConfigs.FirstOrDefault();
            if (autoConfig.ContainsKey("pageId") && autoConfig["pageId"] != null && !string.IsNullOrEmpty(autoConfig["pageId"].ToString()))
            {
                return 0;
            }
        }
        var applyOnMesage = autoConfig.ContainsKey("applyOnMesage") && autoConfig["applyOnMesage"] != null && Convert.ToBoolean(autoConfig["applyOnMesage"]);
        if (!applyOnMesage) {
            return 0;
        }
        var commentTrim = messageText.Replace(",", string.Empty);
        commentTrim = commentTrim.Replace(":", string.Empty);
        commentTrim = commentTrim.Replace(".", string.Empty);
        commentTrim = commentTrim.Replace("!", string.Empty);
        string phone = null;
        Dictionary<string, object> product = null;
        int quantity = 1;
        var arr = commentTrim.Split(" ", StringSplitOptions.RemoveEmptyEntries).ToList();
        if (arr.Count() == 3 && (
            autoConfig["comment"].ToString() == "(số điện thoại) (mã sản phẩm) (số lượng)"
            || autoConfig["comment"].ToString() == "(phone) (product code) (quantity)"
            ))
        {
            var first = arr.FirstOrDefault();
            if (!isPhoneNumber(first))
            {
                return 0;
            }
            phone = first;
            product = await isProductCode(arr[1], userId);
            if (product == null)
            {
                return 0;
            }
            int ret = 0;
            var isNumber = int.TryParse(arr[2], out ret);
            if (!isNumber)
            {
                return 0;
            }
            quantity = ret;
        }
        else if (arr.Count() == 2 && (
            autoConfig["comment"].ToString() == "(mã sản phẩm) (số lượng)"
            || autoConfig["comment"].ToString() == "(product code) (quantity)"
            || autoConfig["comment"].ToString() == "(số điện thoại) (mã sản phẩm)"
            || autoConfig["comment"].ToString() == "(phone) (product code)"
            ))
        {
            if (autoConfig["comment"].ToString() == "(mã sản phẩm) (số lượng)"
            || autoConfig["comment"].ToString() == "(product code) (quantity)")
            {
                product = await isProductCode(arr[0], userId);
                if (product == null)
                {
                    return 0;
                }
                int ret = 0;
                var isNumber = int.TryParse(arr[1], out ret);
                if (!isNumber)
                {
                    return 0;
                }
                quantity = ret;
            }
            else if (autoConfig["comment"].ToString() == "(số điện thoại) (mã sản phẩm)"
            || autoConfig["comment"].ToString() == "(phone) (product code)")
            {
                if (!isPhoneNumber(arr[0]))
                {
                    return 0;
                }
                phone = arr[0];
                product = await isProductCode(arr[1], userId);
                if (product == null)
                {
                    return 0;
                }
            }
            else
            {
                return 0;
            }
        }
        else if (arr.Count() == 1 && (
            autoConfig["comment"].ToString() == "(mã sản phẩm)"
            || autoConfig["comment"].ToString() == "(product code)"
            ))
        {
            product = await isProductCode(arr[0], userId);
            if (product == null)
            {
                return 0;
            }
        }
        Dictionary<string, object> contact = null;
        if (!string.IsNullOrEmpty(phone))
        {
            var contacts = await _sqlService.ListAsync("contact", new Dictionary<string, object>() {
                {"userId", userId},
                {"mobile", phone},
            }, null);
            contact = contacts != null && contacts.Any() ? contacts.FirstOrDefault() : null;
        }
        var order = await NewOrderAsync(product, messageText, quantity, contact, userId, phone);
        if (order == null)
        {
            return 0;
        }
        if (autoConfig.ContainsKey("replyTemplate") && autoConfig["replyTemplate"] != null && !string.IsNullOrEmpty(autoConfig["replyTemplate"].ToString()))
        {
            var message = autoConfig["replyTemplate"].ToString();
            message = message.Replace("(ten)", fromUserName);
            message = message.Replace("(name)", fromUserName);
            message = message.Replace("(trang)", pageName);
            message = message.Replace("(page)", pageName);
            using (var httpClient = _httpClientFactory.CreateClient())
            {
                var httpResponseMessage = await httpClient.PostAsJsonAsync(
                    string.Format(_requestUrl, _version, pageId + "/messages", "access_token=" + accessToken),
                    new
                    {
                        message = new { text = messageText },
                        recipient = new { id = fromUserId }
                    }
                );

                if (httpResponseMessage.IsSuccessStatusCode)
                {
                    return order.Id;
                }
                return 0;
            }
        }
        return order.Id;
    }

    private async Task<bool> AutoCommentAsync(string accessToken, string commentId, string userId, string pageId, string fromUserName, string pageName, bool isLiveVideo)
    {
        var replyConfigs = await _sqlService.ListAsync("fbautoreplyconfig",
            new Dictionary<string, object>() {
                        { "userId", userId },
                        { "isActive", true }
            }, null);
        if (replyConfigs == null || !replyConfigs.Any())
        {
            return false;
        }
        var replyConfig = replyConfigs.FirstOrDefault(c => c.ContainsKey("pageId") && c["pageId"].ToString() == pageId);
        if (replyConfig == null)
        {
            replyConfig = replyConfigs.FirstOrDefault();
            if (replyConfig.ContainsKey("pageId") && !string.IsNullOrEmpty(replyConfig["pageId"].ToString()))
            {
                return false;
            }
        }
        var applyOnPostComment = replyConfig.ContainsKey("applyOnPostComment") && replyConfig["applyOnPostComment"] != null && Convert.ToBoolean(replyConfig["applyOnPostComment"]);
        var applyOnLiveStream = replyConfig.ContainsKey("applyOnLiveStream") && replyConfig["applyOnLiveStream"] != null && Convert.ToBoolean(replyConfig["applyOnLiveStream"]);
        if ((!isLiveVideo && !applyOnPostComment) || (isLiveVideo && !applyOnLiveStream)) {
            return false;
        }
        var message = replyConfig["comment"].ToString();
        message = message.Replace("(ten)", fromUserName);
        message = message.Replace("(name)", fromUserName);
        message = message.Replace("(trang)", pageName);
        message = message.Replace("(page)", pageName);
        using (var httpClient = _httpClientFactory.CreateClient())
        {
            var httpResponseMessage = await httpClient.PostAsJsonAsync(
                string.Format(_requestUrl, _version, commentId + "/comments", "access_token=" + accessToken),
                new
                {
                    message = message
                }
            );

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                return true;
            }
            return false;
        }
    }
}