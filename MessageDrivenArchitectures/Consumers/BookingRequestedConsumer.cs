using MassTransit;
using Restaurant.Messaging;

namespace Restaurant.Booking.Consumers;

public sealed class BookingRequestedConsumer : IConsumer<IBookingRequested>
{
    private readonly Restaurant _restaurant;
    private readonly ILogger _logger;

    public BookingRequestedConsumer(Restaurant restaurant,
                                    ILogger<BookingRequestedConsumer> logger)
    {
        ArgumentNullException.ThrowIfNull(restaurant, nameof(restaurant));
        ArgumentNullException.ThrowIfNull(logger, nameof(logger));

        _restaurant = restaurant;
        _logger = logger;
    }   

    public async Task Consume(ConsumeContext<IBookingRequested> context)
    {
        var bookedTableId = await _restaurant.BookTableAsync(context.Message.NumberOfSeats);

        if (bookedTableId is not null)
        {
            await context.Publish<ITableBooked>(new TableBooked(context.Message.OrderId,
                                                                context.Message.ClientId,
                                                                (Guid)bookedTableId,
                                                                context.Message.CreationDate));
        }

        string answer = bookedTableId is null
            ? "no suitable table."
            : $"table picked up.";

        _logger.LogInformation("[Order: {oredrId}] - " + answer, context.Message.OrderId);
    }
}