using System.Threading.Channels;
using CorpNetMessenger.Domain.Interfaces.Services;

namespace CorpNetMessenger.Infrastructure.Services
{
    public class BoundedMessageQueue<T> : IMessageQueue<T>
    {
        private readonly Channel<T> _channel;

        public BoundedMessageQueue(int capacity)
        {
            var options = new BoundedChannelOptions(capacity)
            {
                SingleReader = true,
                SingleWriter = false,
                FullMode = BoundedChannelFullMode.Wait
            };
            _channel = Channel.CreateBounded<T>(options);
        }

        public int Count => 0;

        public async ValueTask<bool> EnqueueAsync(T item, CancellationToken cancellationToken = default)
        {
            return await _channel.Writer.WaitToWriteAsync(cancellationToken)
                .AsTask()
                .ContinueWith(t =>
                {
                    if (t.IsCompletedSuccessfully && _channel.Writer.TryWrite(item))
                        return true;
                    return false;
                }, cancellationToken);
        }

        public bool TryEnqueue(T item) => _channel.Writer.TryWrite(item);

        public ChannelReader<T> Reader => _channel.Reader;
    }
}