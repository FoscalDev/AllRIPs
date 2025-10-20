using System.Threading.Channels;
using AllRIPs.INTERFACES;

namespace AllRIPs.SERVICES
{
    public class BackgroundTaskQueueService : IBackgroundTaskQueue
    {
        private readonly Channel<Func<CancellationToken, Task>> _queue;

        public BackgroundTaskQueueService()
        {
            // Capacidad ilimitada, puedes limitar si lo deseas
            _queue = Channel.CreateUnbounded<Func<CancellationToken, Task>>();
        }

        public void QueueBackgroundWorkItem(Func<CancellationToken, Task> workItem)
        {
            if (workItem == null) throw new ArgumentNullException(nameof(workItem));
            _queue.Writer.TryWrite(workItem);
        }

        public async Task<Func<CancellationToken, Task>> DequeueAsync(CancellationToken cancellationToken)
        {
            var workItem = await _queue.Reader.ReadAsync(cancellationToken);
            return workItem;
        }
    }
}
