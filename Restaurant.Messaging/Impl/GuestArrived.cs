namespace Restaurant.Messaging;

public sealed class GuestArrived : IGuestArrived
{
    public GuestArrived(Guid orderId)
    {
        OrderId = orderId;
    }

    public Guid OrderId { get; }
}