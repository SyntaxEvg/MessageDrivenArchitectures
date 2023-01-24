namespace Restaurant.Booking;

internal sealed class Table
{
    public Table(Guid id, int seatsCount)
    {
        Id = id;
        State = TableState.Free;
        SeatsCount = seatsCount;
    }

    public Guid Id { get; }
    public TableState State { get; private set; }
    public int SeatsCount { get; }

    public void SetState(TableState state) => State = state;

}