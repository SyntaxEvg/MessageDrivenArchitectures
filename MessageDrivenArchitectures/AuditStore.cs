using MassTransit.Audit;
using System.Text.Json;

namespace Restaurant.Booking;

internal sealed class AuditStore : IMessageAuditStore
{
    private readonly ILogger _logger;

    public AuditStore(ILogger<AuditStore> logger)
    {
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));
        _logger = logger;
    }

    public Task StoreMessage<T>(T message, MessageAuditMetadata metadata) where T : class
    {
        _logger.LogInformation(JsonSerializer.Serialize(message) + "\n" + JsonSerializer.Serialize(metadata));
        return Task.CompletedTask;
    }
}