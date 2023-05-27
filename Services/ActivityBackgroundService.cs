using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

public class ActivityBackgroundService : BackgroundService
{
    private readonly IChannelQueueService<UserActivity> _queueTokenResponse;
    private readonly IActivityService _activityService;


    public ActivityBackgroundService(
        IActivityService activityService,
        IChannelQueueService<UserActivity> queueTokenResponse
    )
    {
        _activityService = activityService;
        _queueTokenResponse = queueTokenResponse;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        while (await _queueTokenResponse.WaitToReadAsync(cancellationToken))
        {
            UserActivity response = await _queueTokenResponse.ReadAsync(cancellationToken);
            try
            {
                await _activityService.Log(response.UserId, response.Feature, response.Action, response.Note, response.Session);
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
}