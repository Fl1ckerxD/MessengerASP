namespace CorpNetMessenger.Domain.Interfaces.Services
{
    public interface IMessageQueue<T>
    {
        ValueTask<bool> EnqueueAsync(T item, CancellationToken cancellationToken = default);
        bool TryEnqueue(T item);
        int Count { get; }
    }
}