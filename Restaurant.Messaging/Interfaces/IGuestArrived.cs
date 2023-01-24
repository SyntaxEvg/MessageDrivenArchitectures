namespace Restaurant.Messaging;

public interface IGuestArrived
{
    public Guid OrderId { get; }
}