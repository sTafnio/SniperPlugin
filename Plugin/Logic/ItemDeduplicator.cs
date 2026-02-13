using System.Collections.Generic;
using System.Linq;

namespace SniperPlugin.Logic;

public class ItemDeduplicator
{
    private readonly Queue<string> _items = new(75);
    private const int MaxCacheSize = 75;

    public bool IsDuplicate(string itemId)
    {
        lock (_items)
        {
            if (_items.Contains(itemId)) return true;

            _items.Enqueue(itemId);

            if (_items.Count > MaxCacheSize)
            {
                _items.Dequeue();
            }

            return false;
        }
    }
}
