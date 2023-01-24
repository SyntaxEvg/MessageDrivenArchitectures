using System.Text.Json;
using Microsoft.Extensions.Logging;
using MassTransit.Audit;

namespace Restaurant.Notification;

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