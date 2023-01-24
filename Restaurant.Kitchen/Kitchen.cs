using System.Collections.Concurrent;
using Restaurant.Messaging;

namespace Restaurant.Kitchen;

public sealed class Kitchen
{
    private readonly IReadOnlyList<Dish> _dishes = new List<Dish>()
    {
        Dish.Pizza, Dish.Lasagna, Dish.Pasta, Dish.Chicken
    };
    private readonly ConcurrentDictionary<Dish, bool> _stoplist = new()
    {
        [Dish.Pizza] = false,
        [Dish.Lasagna] = false,
        [Dish.Chicken] = false,
        [Dish.Pasta] = false,
    };
    private readonly object _lock = new();

    public bool CheckKitchenReady(params Dish[]? dishes) => dishes is null ? true : CheckStoplist(dishes);
   
    private bool CheckStoplist(Dish dish)
    {
        _stoplist.TryGetValue(dish, out bool hasInList);

        return hasInList;
    }
  
    private bool CheckStoplist(params Dish[] dishes)
    {
        bool hasInList = false;

        for (int i = 0; i < dishes.Length; i++)
        {
            _stoplist.TryGetValue(dishes[i], out hasInList);

            if (hasInList)
            {
                break;
            }
        }

        return hasInList;
    }
    private void TryChangeStoplistRandom(int chance)
    {
        var rnd = new Random();

        if (rnd.Next(100) < chance)
        {
            var dishToAdd = (Dish)rnd.Next(_dishes.Count);
            _stoplist.TryUpdate(dishToAdd, true, false);
        }
    }
    private void ResetStoplist()
    {
        lock (_lock)
        {
            foreach (var pair in _stoplist)
            {
                if (pair.Value)
                {
                    _stoplist[pair.Key] = false;
                }
            }
        }
    }
}