using MassTransit;
using Microsoft.Extensions.Logging;
using Restaurant.Messaging;

namespace Restaurant.Kitchen.Consumers;

public sealed class KitchenBookingRequestedConsumer : IConsumer<IBookingRequested>
{
    private readonly Kitchen _kitchen;
    private readonly ILogger _logger;

    public KitchenBookingRequestedConsumer(Kitchen kitchen, 
                                           ILogger<KitchenBookingRequestedConsumer> logger)
    {
        ArgumentNullException.ThrowIfNull(kitchen, nameof(kitchen));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _kitchen = kitchen;
        _logger = logger;
    }

    public Task Consume(ConsumeContext<IBookingRequested> context)
    {
        if (!_kitchen.CheckKitchenReady(context.Message.PreOrder))
        {
            _logger.LogInformation("[Order: {orderId}] - cancellation, by stop list.", context.Message.OrderId);
        }
        else
        {
            context.Publish<IKitchenReady>(new KitchenReady(context.Message.OrderId));
            _logger.LogInformation("[Order: {orderId}] - kitchen ready.", context.Message.OrderId);
        }

        return context.ConsumeCompleted;
    }
}