using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;

public class ChannelQueueService<T> : IChannelQueueService<T>
{
    private readonly Channel<T> _queue;

    public ChannelQueueService()
    {
        var opts = new BoundedChannelOptions(100) { FullMode = BoundedChannelFullMode.Wait };
        _queue = Channel.CreateBounded<T>(opts);
    }

    public async ValueTask WriteAsync(T fbTokenResponse)
    {
        await _queue.Writer.WriteAsync(fbTokenResponse);
    }

    public async ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken)
    {
        var result = await _queue.Reader.WaitToReadAsync(cancellationToken);
        return result;
    }

    public async ValueTask<T> ReadAsync(CancellationToken cancellationToken)
    {
        var result = await _queue.Reader.ReadAsync(cancellationToken);
        return result;
    }
}