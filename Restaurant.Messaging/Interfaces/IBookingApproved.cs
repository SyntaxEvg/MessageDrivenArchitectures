namespace Restaurant.Messaging;

public interface IBookingApproved
{
    public Guid OrderId { get; }
    public Guid ClientId { get; }
}