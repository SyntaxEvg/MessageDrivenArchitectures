using MassTransit;
using Restaurant.Messaging;

namespace Restaurant.Booking.Consumers;

public sealed class BookingFaultedConsumer : IConsumer<IBookingFaulted>
{
    private readonly Restaurant _restaurant;

    public BookingFaultedConsumer(Restaurant restaurant)
    {
        _restaurant = restaurant;
    }

    public async Task Consume(ConsumeContext<IBookingFaulted> context)
    {
        await _restaurant.UnbookTableAsync(context.Message.TableId);
    }
}