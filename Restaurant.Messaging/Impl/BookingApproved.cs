namespace Restaurant.Messaging;

public sealed class BookingApproved : IBookingApproved
{
    public BookingApproved(Guid orderId, Guid clientId)
    {
        ClientId = clientId;
        OrderId = orderId;
    }

    public Guid OrderId { get; }
    public Guid ClientId { get; }
}