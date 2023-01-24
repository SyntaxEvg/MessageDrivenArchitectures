namespace Restaurant.Messaging;

public sealed class BookingRequested : IBookingRequested
{
    public BookingRequested(
        Guid orderId, 
        Guid clientId, 
        int numberOfSeats,
        DateTime creationDate,
        TimeSpan arriveVia, 
        Dish[]? preorder = null)
    {
        OrderId = orderId;
        ClientId = clientId;
        CreationDate = creationDate;
        ArriveVia = arriveVia;
        PreOrder = preorder;
    }

    public Guid OrderId { get; }
    public Guid ClientId { get; }
    public int NumberOfSeats { get; }
    public TimeSpan ArriveVia { get; }
    public Dish[]? PreOrder { get; }
    public DateTime CreationDate { get; }
}