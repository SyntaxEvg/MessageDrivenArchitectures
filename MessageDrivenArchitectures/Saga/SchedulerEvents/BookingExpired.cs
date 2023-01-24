using Restaurant.Messaging;

namespace Restaurant.Booking;

public sealed class BookingExpired : IBookingExpired
{
    private readonly BookingState _instance;

    public BookingExpired(BookingState instance)
    {
        _instance = instance;
    }

    public Guid OrderId => _instance.OrderId;
}