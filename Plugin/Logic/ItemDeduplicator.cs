using System.Collections.Generic;

namespace SniperPlugin.Logic;

public class ItemDeduplicator
{
    private readonly Queue<string> _queue = new(75);
    private readonly HashSet<string> _set = new(75);
    private const int MaxCacheSize = 75;

    public bool IsDuplicate(string itemId)
    {
        lock (_queue)
        {
            if (_set.Contains(itemId)) return true;

            _queue.Enqueue(itemId);
            _set.Add(itemId);

            if (_queue.Count > MaxCacheSize)
            {
                _set.Remove(_queue.Dequeue());
            }

            return false;
        }
    }
}
