namespace Restaurant.Messaging;

public interface IBookingRequested
{
    public Guid OrderId { get; }
    public Guid ClientId { get; }
    public int NumberOfSeats { get; }
    public TimeSpan ArriveVia { get; }
    public Dish[]? PreOrder { get; }
    public DateTime CreationDate { get; }
}