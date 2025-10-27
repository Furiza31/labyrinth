using System.Collections;
using Labyrinth.Crawl;
using Labyrinth.Items;
using Labyrinth.Tiles;

namespace Labyrinth.Navigation;

public class Explorator(ICrawler crawler, IMovementStrategy strategy) : IExplorator
{
    private readonly ICrawler _crawler = crawler ?? throw new ArgumentNullException(nameof(crawler));
    private readonly IMovementStrategy _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
    private Inventory? _inventory;
    private readonly ArrayList _bag = new();

    public bool GetOut(int n)
    {
        if (n < 0) throw new ArgumentOutOfRangeException(nameof(n), "n must be non-negative.");

        if (_crawler.FacingTile is Outside) return true;

        for (int i = 0; i < n; i++)
        {
            if (_crawler.FacingTile is Outside) return true;
            try
            {
                switch (_strategy.NextAction(_crawler))
                {
                    case MoveAction.Walk:
                        if (_crawler.FacingTile is Door door)
                        {
                            var keySlot = FindKeyInventory();
                            if (door.IsLocked && keySlot is not null)
                            {
                                if (door.Open(keySlot) && !keySlot.HasItem)
                                {
                                    _bag.Remove(keySlot);
                                }
                            }
                        }
                        _inventory = _crawler.Walk();
                        if (_inventory.HasItem)
                        {
                            var slot = new MyInventory();
                            slot.MoveItemFrom(_inventory);
                            _bag.Add(slot);
                        }
                        break;
                    case MoveAction.TurnLeft: _crawler.TurnLeft(); break;
                    case MoveAction.TurnRight: _crawler.TurnRight(); break;
                    default: throw new NotSupportedException("Action inconnue");
                }
            }
            catch (InvalidOperationException)
            {
                // Ignore invalid moves
            }
        }
        return false;
    }

    private Inventory? FindKeyInventory()
    {
        foreach (var item in _bag)
        {
            if (item is Inventory inventory && inventory.HasItem && inventory.ItemType == typeof(Key))
            {
                return inventory;
            }
        }
        return null;
    }
}
