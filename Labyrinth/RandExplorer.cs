using Labyrinth.Crawl;
using Labyrinth.Items;
using Labyrinth.Sys;
using Labyrinth.Tiles;

namespace Labyrinth
{
    public class RandExplorer(ICrawler crawler, IEnumRandomizer<RandExplorer.Actions> randomGenerator)
    {
        private readonly ICrawler _crawler = crawler;
        private readonly IEnumRandomizer<Actions> _randomGenerator = randomGenerator;

        public enum Actions
        {
            TurnLeft,
            Walk
        }

        public int GetOut(int n)
        {
            ArgumentOutOfRangeException.ThrowIfLessThanOrEqual(n, 0, "n must be strictly positive");
            MyInventory bag = new();

            for (; n > 0 && _crawler.FacingTile is not Outside; n--)
            {
                EventHandler<CrawlingEventArgs>? changeEvent;

                if (_crawler.FacingTile.IsTraversable
                    && _randomGenerator.Next() == Actions.Walk)
                {
                    CollectItems(_crawler.Walk(), bag);
                    changeEvent = PositionChanged;
                }
                else
                {
                    _crawler.Direction.TurnLeft();
                    changeEvent = DirectionChanged;
                }

                if (_crawler.FacingTile is Door door)
                {
                    TryUnlockDoor(bag, door);
                }

                changeEvent?.Invoke(this, new CrawlingEventArgs(_crawler));
            }
            return n;
        }

        public event EventHandler<CrawlingEventArgs>? PositionChanged;

        public event EventHandler<CrawlingEventArgs>? DirectionChanged;

        private static void CollectItems(Inventory source, MyInventory bag)
        {
            while (source.HasItems)
            {
                bag.MoveItemFrom(source);
            }
        }

        private static void TryUnlockDoor(MyInventory bag, Door door)
        {
            if (!door.IsLocked)
            {
                return;
            }

            var keyBuffer = new MyInventory();
            while (door.IsLocked)
            {
                var keyIndex = IndexOfKey(bag);
                if (keyIndex < 0)
                {
                    break;
                }

                keyBuffer.MoveItemFrom(bag, keyIndex);
                if (!door.Open(keyBuffer))
                {
                    bag.MoveItemFrom(keyBuffer);
                }
            }
        }

        private static int IndexOfKey(MyInventory bag)
        {
            var index = 0;

            foreach (var item in bag.Items)
            {
                if (item is Key)
                {
                    return index;
                }

                index++;
            }

            return -1;
        }
    }

}
