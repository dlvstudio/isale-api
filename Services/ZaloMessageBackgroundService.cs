using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

public class ZaloMessageBackgroundService : BackgroundService
{
    private readonly IChannelQueueService<HookObject> _queueTokenResponse;
    private readonly ISqlService _sqlService;

    public ZaloMessageBackgroundService(
        IChannelQueueService<HookObject> queueTokenResponse,
        ISqlService sqlService
    )
    {
        _queueTokenResponse = queueTokenResponse;
        _sqlService = sqlService;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (await _queueTokenResponse.WaitToReadAsync(cancellationToken))
        {
            HookObject response = await _queueTokenResponse.ReadAsync(cancellationToken);
            try
            {
                var messageJson = new Dictionary<string, object>();
                messageJson["message"] = response.Json;
                await _sqlService.SaveAsync(messageJson, "zalowebhookmessage",
                    "Id",
                    new List<string>() { "Id" },
                null);
            }
            catch (Exception e)
            {
            }
        }
    }
}