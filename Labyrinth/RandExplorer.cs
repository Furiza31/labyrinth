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
                    _crawler.Walk().SwapItems(bag);
                    changeEvent = PositionChanged;
                }
                else
                {
                    _crawler.Direction.TurnLeft();
                    changeEvent = DirectionChanged;
                }
                if (_crawler.FacingTile is Door door && door.IsLocked
                    && bag.HasItems && bag.ItemTypes.First() == typeof(Key))
                {
                    door.Open(bag);
                }
                changeEvent?.Invoke(this, new CrawlingEventArgs(_crawler));
            }
            return n;
        }

        public event EventHandler<CrawlingEventArgs>? PositionChanged;

        public event EventHandler<CrawlingEventArgs>? DirectionChanged;
    }

}
