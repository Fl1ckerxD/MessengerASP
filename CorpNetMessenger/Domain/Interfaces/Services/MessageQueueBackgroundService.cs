using CorpNetMessenger.Domain.Entities;
using CorpNetMessenger.Domain.Interfaces.Repositories;
using CorpNetMessenger.Infrastructure.Services;

namespace CorpNetMessenger.Domain.Interfaces.Services
{
    public class MessageQueueBackgroundService<T> : BackgroundService
    {
        private readonly BoundedMessageQueue<T> _messageQueue;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MessageQueueBackgroundService<T>> _logger;
        private readonly int _maxBatchSize;
        private readonly TimeSpan _maxWaitTime;

        public MessageQueueBackgroundService(BoundedMessageQueue<T> messageQueue, IServiceScopeFactory scopeFactory,
        ILogger<MessageQueueBackgroundService<T>> logger, int maxBatchSize = 50, TimeSpan? maxWait = null)
        {
            _messageQueue = messageQueue;
            _scopeFactory = scopeFactory;
            _logger = logger;
            _maxBatchSize = maxBatchSize;
            _maxWaitTime = maxWait ?? TimeSpan.FromSeconds(200);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var reader = _messageQueue.Reader;
            var batch = new List<T>(_maxBatchSize);

            try
            {
                while (!stoppingToken.IsCancellationRequested)
                {
                    // Ждём первый элемент (блокирующе, пока есть данные)
                    if (!await reader.WaitToReadAsync(stoppingToken))
                        break;

                    batch.Clear();

                    // Добавляем первый элемент (блокирующее чтение)
                    if (reader.TryRead(out var first))
                        batch.Add(first);

                    var batchStart = DateTime.UtcNow;

                    // Собираем до maxBatchSize или пока не выйдет таймаут
                    while (batch.Count < _maxBatchSize)
                    {
                        if (reader.TryRead(out var item))
                        {
                            batch.Add(item);
                            continue;
                        }

                        var remaining = _maxWaitTime - (DateTime.UtcNow - batchStart);
                        if (remaining <= TimeSpan.Zero)
                            break;

                        // Ждём либо появление, либо таймаут (через Task.WhenAny)
                        var waitTask = reader.WaitToReadAsync(stoppingToken).AsTask();
                        var delayTask = Task.Delay(remaining, stoppingToken);
                        var completed = await Task.WhenAny(waitTask, delayTask);
                        if (completed == delayTask)
                            break;
                        // если waitTask завершился — цикл продолжится и TryRead попытается взять элемент
                    }

                    if (batch.Count == 0) continue;

                    // Обработка батча в scope (новый DbContext)
                    using var scope = _scopeFactory.CreateScope();
                    var uow = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
                    // Можно реализовать retry с backoff
                    try
                    {
                        // Преобразование batch в сущности и сохранение в БД
                        foreach (var item in batch)
                        {
                            if (item is not Message message)
                                throw new InvalidOperationException("MessageQueueBackgroundService поддерживает только Message");
                            if (message == null)
                                throw new ArgumentNullException(nameof(item), "Сообщение не может быть null");
                            await uow.Messages.AddAsync(message);
                        }
                        await uow.SaveAsync();
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Ошибка при записи батча сообщений (count={Count})", batch.Count);
                        // TODO: retry / poison handling / persist batch to disk
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Неожиданная ошибка сервис очереди");
            }
        }
    }
}