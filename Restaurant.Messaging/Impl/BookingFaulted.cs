namespace Restaurant.Messaging;

public sealed class BookingFaulted : IBookingFaulted
{
    public BookingFaulted(Guid orderId, Guid tableId)
    {
        OrderId = orderId;
        TableId = tableId;
    }

    public Guid OrderId { get; }
    public Guid TableId { get; }
}