namespace Restaurant.Messaging;

public interface ITableBooked
{
    public Guid OrderId { get; }
    public Guid ClientId { get; }
    public Guid TableId { get; }
    public DateTime CreationDate { get; }
}