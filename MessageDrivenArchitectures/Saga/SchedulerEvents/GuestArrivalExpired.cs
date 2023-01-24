using Restaurant.Messaging;

namespace Restaurant.Booking;

public sealed class GuestArrivalExpired : IGuestArrivalExpired
{
    private readonly BookingState _instance;

    public GuestArrivalExpired(BookingState instance)
    {
        _instance= instance;
    }

    public Guid OrderId => _instance.OrderId;
}