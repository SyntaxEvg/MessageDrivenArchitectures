using System.Collections.Concurrent;

namespace Restaurant.Booking;

public sealed class Restaurant
{
    private readonly ConcurrentDictionary<Guid, Table> _tables;
    private readonly TimeSpan _syncOperationDelay = TimeSpan.FromSeconds(5);
    private readonly object _lock = new();

    public Restaurant()
    {
        _tables = GetTables(10);
    }

    /// <summary>
    /// The table booking.
    /// </summary>
    /// <param name="numberOfSeats"></param>
    /// <returns> 
    /// Returns a table id, if restaurant contains table with current <paramref name = "numberOfSeats"/>, otherwise null.
    /// </returns>
    public Guid? BookTable(int numberOfSeats)
    {
        Table? table = null;

        lock (_lock)
        {
            table = _tables.FirstOrDefault(p =>
                                          p.Value.SeatsCount >= numberOfSeats &&
                                          p.Value.State == TableState.Free).Value;

            if (table is null)
            {
                return null;
            }

            table.SetState(TableState.Booked);

            return table.Id;
        }
    }
    /// <summary>
    /// The table booking asynchronously.
    /// </summary>
    /// <param name="numberOfSeats"></param>
    /// <returns> 
    /// Returns a table id, if restaurant contains table with current <paramref name = "numberOfSeats"/>, otherwise null.
    /// </returns>
    public async Task<Guid?> BookTableAsync(int numberOfSeats)
    {
        return await Task.Run<Guid?>(() =>
        {
            lock (_lock)
            {
                var table = _tables.FirstOrDefault(p =>
                                                   p.Value.SeatsCount >= numberOfSeats &&
                                                   p.Value.State == TableState.Free).Value;

                if (table is null)
                {
                    return null;
                }

                table.SetState(TableState.Booked);

                return table.Id;
            }
        });
    }
    /// <summary>
    /// The table unbooking.
    /// </summary>
    /// <param name="id">Table id.</param>
    /// <returns>
    /// Returns true if unbooking operation completed succesfully, if table with current <paramref name="id"/>
    /// is free, returns false, otherwise null (table with current <paramref name="id"/> doesn't exists).
    /// </returns>
    public bool? UnbookTable(Guid tableId)
    {
        Table? table = null;

        lock (_lock)
        {
            table = _tables.FirstOrDefault(pair => pair.Key == tableId).Value;

            Task.Delay(_syncOperationDelay).Wait();

            if (table is null)
            {
                return null;
            }
            else if (table.State == TableState.Free)
            {
                return false;
            }

            table?.SetState(TableState.Free);

            return true;
        }
    }
    /// <summary>
    /// The table unbooking asynchronously.
    /// </summary>
    /// <param name="id">Table id.</param>
    /// <returns>
    /// Returns true if unbooking operation completed succesfully, if table with current <paramref name="id"/>
    /// is free, returns false, otherwise null (table with current <paramref name="id"/> doesn't exists).
    /// </returns>
    public async Task<bool?> UnbookTableAsync(Guid tableId)
    {
        return await Task.Run<bool?>(() =>
        {
            lock (_lock)
            {
                var table = _tables.FirstOrDefault(pair => pair.Key == tableId).Value;

                if (table is null)
                {
                    return null;
                }
                else if (table.State == TableState.Free)
                {
                    return false;
                }

                table?.SetState(TableState.Free);

                return true;
            }
        });
    }
    public async Task UnbookTables()
    {
        await Task.Run(() =>
        {
            lock (_lock)
            {
                foreach (var table in _tables.Values)
                {
                    if (table.State == TableState.Booked)
                    {
                        table.SetState(TableState.Free);
                    }
                }
            }
        });
    }
    private ConcurrentDictionary<Guid, Table> GetTables(int count)
    {
        var tables = new ConcurrentDictionary<Guid, Table>();

        for (int i = 0; i < count; i++)
        {
            var id = Guid.NewGuid();
            tables.TryAdd(id, new Table (id, (int)NumberOfSeats.Max));
        }

        return tables;
    }
}