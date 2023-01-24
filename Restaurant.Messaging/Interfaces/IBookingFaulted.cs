namespace Restaurant.Messaging;

public interface IBookingFaulted
{
    public Guid OrderId { get; }
    public Guid TableId { get; }
}