namespace Restaurant.Messaging;

public class TableBooked : ITableBooked
{
    public TableBooked(Guid orderId, 
                       Guid clientId, 
                       Guid tableId,
                       DateTime creationDate)
    {
        OrderId = orderId;
        ClientId = clientId;
        TableId = tableId;
        CreationDate = creationDate;
    }

    public Guid OrderId { get; }
    public Guid ClientId { get; }
    public Guid TableId { get; }
    public DateTime CreationDate { get; }
}