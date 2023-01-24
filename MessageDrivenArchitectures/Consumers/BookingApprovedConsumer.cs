using MassTransit;
using Restaurant.Messaging;

namespace Restaurant.Booking.Consumers;

public sealed class BookingApprovedConsumer : IConsumer<IBookingApproved>
{
    public async Task Consume(ConsumeContext<IBookingApproved> context)
    {
        await context.Publish<IGuestArrived>(new GuestArrived(context.Message.OrderId));
    }
}