namespace Restaurant.Messaging;

public interface IGuestArrivalExpired
{
    public Guid OrderId { get; }
}