namespace Restaurant.Notification;

public sealed class Notifier
{
    public void Notify(Guid orderId, string message)
    {
        Console.WriteLine($"[Order: {orderId}] - Уважаемый клиент! {message}.");
    }
}