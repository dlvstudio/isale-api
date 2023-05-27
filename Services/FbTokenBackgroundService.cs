using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;

public class FbTokenBackgroundService : BackgroundService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IChannelQueueService<FbUpdateToken> _queueTokenResponse;
    private readonly ISqlService _sqlService;
    private readonly ICacheService _cacheService;
    private readonly string _clientId = "***";
    private readonly string _clientSecret = "***";
    private readonly string _version = "v15.0";
    private readonly string _requestUrl = "https://graph.facebook.com/{0}/{1}?{2}";


    public FbTokenBackgroundService(
        IHttpClientFactory httpClientFactory,
        IChannelQueueService<FbUpdateToken> queueTokenResponse,
        ISqlService sqlService,
        ICacheService cacheService
    )
    {
        _httpClientFactory = httpClientFactory;
        _queueTokenResponse = queueTokenResponse;
        _sqlService = sqlService;
        _cacheService = cacheService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (await _queueTokenResponse.WaitToReadAsync(cancellationToken))
        {
            FbUpdateToken response = await _queueTokenResponse.ReadAsync(cancellationToken);
            try
            {
                if (string.IsNullOrEmpty(response.Token))
                {
                    continue;
                }
                var longLivedToken = await GetLonglivedUserAccessTokenAsync(response);
                if (longLivedToken == null)
                {
                    continue;
                }
                var tasks = new List<Task>();
                var fbTokens = await _sqlService.ListAsync("fbtoken",
                    new Dictionary<string, object>() {
                        { "userId", response.UserId },
                        { "fbUserId", response.FbUserId }
                    }, null);
                var fbToken = fbTokens != null && fbTokens.Any()
                    ? fbTokens.FirstOrDefault()
                    : null;
                if (fbToken == null)
                {
                    fbToken = new Dictionary<string, object>();
                    fbToken["id"] = 0;
                    fbToken["userId"] = response.UserId;
                    fbToken["fbUserId"] = response.FbUserId;
                }
                fbToken["accessToken"] = longLivedToken.AccessToken;
                tasks.Add(this._sqlService.SaveAsync(fbToken, "fbtoken", "Id", new List<string>() { "Id", "UserId" }, null));
                _cacheService.RemoveListEqualItem("fbtoken", response.UserId);
                var pageAccessResponse = await GetLonglivedPageAccessTokenAsync(response.FbUserId, longLivedToken.AccessToken);
                if (pageAccessResponse == null || pageAccessResponse.Data == null || !pageAccessResponse.Data.Any())
                {
                    return;
                }
                var pageTokens = pageAccessResponse.Data;
                var pageIds = new List<string>();
                foreach (var pageToken in pageTokens)
                {
                    pageIds.Add(pageToken.Id);
                }
                var fbPageTokens = await _sqlService.ListAsync("fbpage",
                    new Dictionary<string, object>() {
                        { "userId", response.UserId },
                        { "fbUserId", response.FbUserId },
                        { "pageIds", pageIds },
                    }, new QueryModelOnSearch()
                    {
                        WhereFieldQuerys = new Dictionary<string, List<string>>() {
                            {"pageId", new List<string> {EnumSearchFunctions.IN, "pageIds"}},
                            {"userId", new List<string> {EnumSearchFunctions.EQUALS, "userId"}},
                            {"fbUserId", new List<string> {EnumSearchFunctions.EQUALS, "fbUserId"}},
                        }
                    });
                foreach (var pageToken in pageTokens)
                {
                    var fbPageToken = fbPageTokens.FirstOrDefault(p => p.ContainsKey("userId") && p.ContainsKey("pageId") && p["pageId"].ToString() == pageToken.Id);
                    if (fbPageToken == null)
                    {
                        fbPageToken = new Dictionary<string, object>();
                        fbPageToken["id"] = 0;
                        fbPageToken["userId"] = response.UserId;
                        fbPageToken["pageId"] = pageToken.Id;
                        fbPageToken["name"] = pageToken.Name;
                        fbPageToken["isConnected"] = false;
                        fbPageToken["fbUserId"] = response.FbUserId;
                    }
                    fbPageToken["accessToken"] = pageToken.AccessToken;
                    tasks.Add(this._sqlService.SaveAsync(fbPageToken, "fbpage", "Id", new List<string>() { "Id", "UserId" }, null));
                }
                if (tasks.Any())
                {
                    _cacheService.RemoveListEqualItem("fbpage", response.UserId);
                    await Task.WhenAll(tasks.ToArray());
                }

            }
            catch (Exception e)
            {
            }
        }
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);
    }

    private async Task<FbTokenResponse> GetLonglivedUserAccessTokenAsync(FbUpdateToken token)
    {
        using (var httpClient = _httpClientFactory.CreateClient())
        {
            var httpRequestMessage = new HttpRequestMessage(
                HttpMethod.Get,
                string.Format(_requestUrl, _version, "oauth/access_token", "grant_type=fb_exchange_token&client_id=" + _clientId + "&client_secret=" + _clientSecret + "&fb_exchange_token=" + token.Token));

            var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                using (var contentStream =
                    await httpResponseMessage.Content.ReadAsStreamAsync())
                {
                    var serializer = new JsonSerializer();
                    using (var sr = new StreamReader(contentStream))
                    using (var jsonTextReader = new JsonTextReader(sr))
                    {
                        var tokenResponse = serializer.Deserialize<FbTokenResponse>(jsonTextReader);
                        return tokenResponse;
                    }
                }
            }
            return null;
        }
    }

    private async Task<FbPageTokenResponse> GetLonglivedPageAccessTokenAsync(string fbUserId, string accessToken)
    {
        using (var httpClient = _httpClientFactory.CreateClient())
        {
            var httpRequestMessage = new HttpRequestMessage(
                HttpMethod.Get,
                string.Format(_requestUrl, _version, fbUserId + "/accounts", "access_token=" + accessToken));

            var httpResponseMessage = await httpClient.SendAsync(httpRequestMessage);

            if (httpResponseMessage.IsSuccessStatusCode)
            {
                using (var contentStream =
                    await httpResponseMessage.Content.ReadAsStreamAsync())
                {
                    var serializer = new JsonSerializer();
                    using (var sr = new StreamReader(contentStream))
                    using (var jsonTextReader = new JsonTextReader(sr))
                    {
                        var tokenResponse = serializer.Deserialize<FbPageTokenResponse>(jsonTextReader);
                        return tokenResponse;
                    }
                }
            }
            return null;
        }
    }
}