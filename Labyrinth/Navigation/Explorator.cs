using System.Runtime.CompilerServices;
using System.Threading.Channels;
using Labyrinth.Crawl;
using Labyrinth.Items;
using Labyrinth.Tiles;

namespace Labyrinth.Navigation;

public class Explorator(ICrawler crawler, IMovementStrategy strategy) : IExplorator
{
    private readonly ICrawler _crawler = crawler ?? throw new ArgumentNullException(nameof(crawler));
    private readonly IMovementStrategy _strategy = strategy ?? throw new ArgumentNullException(nameof(strategy));
    private Inventory? _inventory;

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
                            if (door.IsOpened && _inventory?.HasItem == true)
                            {
                                _ = door.Open(_inventory);
                            }
                        }
                        _inventory = _crawler.Walk();
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
}