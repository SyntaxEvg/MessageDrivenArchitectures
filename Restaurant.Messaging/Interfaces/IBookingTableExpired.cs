namespace Restaurant.Messaging;

public interface IBookingTableExpired
{
    public Guid OrderId { get; }
    public Guid ClientId { get; }
}