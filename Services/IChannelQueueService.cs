using System.Threading;
using System.Threading.Tasks;

public interface IChannelQueueService<T>
{
    ValueTask WriteAsync(T item);
    ValueTask<bool> WaitToReadAsync(CancellationToken cancellationToken);
    ValueTask<T> ReadAsync(CancellationToken cancellationToken);
}