namespace Restaurant.Messaging;

public interface IBookingExpired
{
    public Guid OrderId { get; }
}