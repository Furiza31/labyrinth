using Labyrinth.Crawl;
using Labyrinth.Items;
using Labyrinth.Navigation;
using Labyrinth.Tiles;
using NSubstitute;

namespace LabyrinthTest.Navigation
{
    [TestFixture(Description = "Tests for Explorator")]
    public class ExploratorTest
    {
        private static ICrawler MakeCrawler(Tile startTile, Direction? direction = null)
        {
            ICrawler crawler = Substitute.For<ICrawler>();
            crawler.FacingTile.Returns(_ => startTile);
            crawler.Direction.Returns(direction ?? Direction.North);
            return crawler;
        }

        private static IMovementStrategy StrategyReturning(MoveAction[] actions)
        {
            IMovementStrategy strategy = Substitute.For<IMovementStrategy>();
            int i = 0;
            strategy.NextAction(Arg.Any<ICrawler>())
                 .Returns(_ => actions[Math.Min(i++, actions.Length - 1)]);
            return strategy;
        }

        [Test]
        public void ExploratorThrowsWhenCrawlerIsNull()
        {
            IMovementStrategy strategy = Substitute.For<IMovementStrategy>();

            Assert.Throws<ArgumentNullException>(() => new Explorator(null!, strategy));
        }

        [Test]
        public void ExploratorThrowsWhenStrategyIsNull()
        {
            ICrawler crawler = Substitute.For<ICrawler>();

            Assert.Throws<ArgumentNullException>(() => new Explorator(crawler, null!));
        }

        [Test]
        public void GetOutThrowsWhenMaxStepsNegative()
        {
            ICrawler crawler = MakeCrawler(Wall.Singleton);
            IMovementStrategy strategy = Substitute.For<IMovementStrategy>();
            IExplorator explorator = new Explorator(crawler, strategy);

            Assert.Throws<ArgumentOutOfRangeException>(() => explorator.GetOut(-1));
        }

        [Test]
        public void GetOutReturnsTrueIfAlreadyFacingOutside()
        {
            Tile outside = Outside.Singleton;
            ICrawler crawler = MakeCrawler(outside);
            IMovementStrategy strategy = Substitute.For<IMovementStrategy>();
            IExplorator explorator = new Explorator(crawler, strategy);

            Assert.That(explorator.GetOut(10), Is.True);
            strategy.DidNotReceiveWithAnyArgs().NextAction(crawler);
        }

        [Test]
        public void GetOutStopsAndReturnsTrueWhenOutsideEncounteredDuringLoop()
        {
            Tile wall = Wall.Singleton;
            Tile outside = Outside.Singleton;
            ICrawler crawler = Substitute.For<ICrawler>();
            crawler.Direction.Returns(Direction.North);
            crawler.FacingTile.Returns(_ => wall, _ => outside, _ => outside);
            crawler.Walk().Returns(new MyInventory());
            IMovementStrategy strategy = StrategyReturning([MoveAction.Walk, MoveAction.Walk, MoveAction.Walk]);
            IExplorator explorator = new Explorator(crawler, strategy);

            Assert.That(explorator.GetOut(3), Is.True);
            strategy.DidNotReceiveWithAnyArgs().NextAction(default!);
            crawler.DidNotReceive().Walk();
        }


        [Test]
        public void GetOutReturnsFalseWhenNoExitWithinMaxSteps()
        {
            ICrawler crawler = MakeCrawler(Wall.Singleton);
            crawler.Walk().Returns(new MyInventory());
            IMovementStrategy strategy = StrategyReturning([MoveAction.Walk, MoveAction.TurnLeft, MoveAction.TurnRight]);
            IExplorator explorator = new Explorator(crawler, strategy);

            Assert.That(explorator.GetOut(3), Is.False);
        }

        [Test]
        public void WalkAssignsInventoryFromCrawlerWalk()
        {
            ICrawler crawler = MakeCrawler(Wall.Singleton);
            Inventory noItem = new MyInventory(null);
            Inventory someItem = new MyInventory(Substitute.For<ICollectable>());
            crawler.Walk().Returns(noItem, someItem);
            IMovementStrategy strategy = StrategyReturning([MoveAction.Walk, MoveAction.Walk]);
            IExplorator explorator = new Explorator(crawler, strategy);

            Assert.That(explorator.GetOut(2), Is.False);
            Received.InOrder(() =>
            {
                strategy.NextAction(crawler);
                crawler.Walk();
                strategy.NextAction(crawler);
                crawler.Walk();
            });
        }

        [Test]
        public void TurnLeftCallsDirectionTurnLeft()
        {
            Tile wall = Wall.Singleton;
            Direction direction = Direction.North;
            ICrawler crawler = Substitute.For<ICrawler>();
            crawler.FacingTile.Returns(_ => wall);
            crawler.Direction.Returns(direction);
            IMovementStrategy strategy = StrategyReturning([MoveAction.TurnLeft]);
            IExplorator explorator = new Explorator(crawler, strategy);

            Assert.That(explorator.GetOut(1), Is.False);
            crawler.DidNotReceive().Walk();
        }

        [Test]
        public void TurnRightCallsDirectionTurnRight()
        {
            Tile wall = Wall.Singleton;
            Direction direction = Direction.North;
            ICrawler crawler = Substitute.For<ICrawler>();
            crawler.FacingTile.Returns(_ => wall);
            crawler.Direction.Returns(direction);
            IMovementStrategy strategy = StrategyReturning([MoveAction.TurnRight]);
            IExplorator explorator = new Explorator(crawler, strategy);

            Assert.That(explorator.GetOut(1), Is.False);
            crawler.DidNotReceive().Walk();
        }

        [Test]
        public void WalkOnDoorWithOpenedDoorAndInventoryWithItemCallsOpen()
        {
            Door door = new Door();
            MyInventory pocket = new MyInventory();
            ICrawler crawler = Substitute.For<ICrawler>();
            crawler.Direction.Returns(Direction.North);
            crawler.FacingTile.Returns(_ => Wall.Singleton, _ => Wall.Singleton, _ => door, _ => door);
            crawler.Walk().Returns(pocket, new MyInventory(null));
            IMovementStrategy strategy = StrategyReturning([MoveAction.Walk, MoveAction.Walk]);
            IExplorator explorator = new Explorator(crawler, strategy);

            Assert.That(explorator.GetOut(2), Is.False);
            Assert.That(door.IsOpened, Is.True);
            Assert.That(pocket.HasItem, Is.False);
        }

        [Test]
        public void WalkOnDoorWhenInventoryAlreadyHeldAndDoorOpenedCallsOpen()
        {
            Door door = new Door();
            MyInventory pocket = new MyInventory();
            ICrawler crawler = Substitute.For<ICrawler>();
            crawler.Direction.Returns(Direction.North);
            crawler.FacingTile.Returns(_ => Wall.Singleton, _ => door, _ => door);
            crawler.Walk().Returns(pocket);
            IMovementStrategy strategy = StrategyReturning([MoveAction.Walk]);
            IExplorator explorator = new Explorator(crawler, strategy);

            Assert.That(explorator.GetOut(1), Is.False);
            Assert.That(door.IsOpened, Is.True);
            Assert.That(pocket.HasItem, Is.False);
        }


        [Test]
        public void WalkOnDoorWhenDoorClosedDoesNotCallOpen()
        {
            Door door = new Door();
            MyInventory dump = new MyInventory();
            door.LockAndTakeKey(dump);
            ICrawler crawler = Substitute.For<ICrawler>();
            crawler.Direction.Returns(Direction.North);
            crawler.FacingTile.Returns(_ => door);
            crawler.Walk().Returns(new MyInventory(null));
            IMovementStrategy strategy = StrategyReturning([MoveAction.Walk]);
            IExplorator explorator = new Explorator(crawler, strategy);

            Assert.That(explorator.GetOut(1), Is.False);
            Assert.That(door.IsLocked, Is.True);
        }


        [Test]
        public void WalkOnDoorWhenNoInventoryDoesNotCallOpen()
        {
            Door openedDoor = new Door();
            ICrawler crawler = Substitute.For<ICrawler>();
            crawler.Direction.Returns(Direction.North);
            crawler.FacingTile.Returns(_ => openedDoor);
            MyInventory empty = new MyInventory(null);
            crawler.Walk().Returns(empty);
            IMovementStrategy strategy = StrategyReturning([MoveAction.Walk]);
            IExplorator explorator = new Explorator(crawler, strategy);

            Assert.That(explorator.GetOut(1), Is.False);
            Assert.That(openedDoor.IsOpened, Is.True);
        }

        [Test]
        public void NextActionInvalidOperationIsIgnoredAndLoopContinues()
        {
            Tile tile = Wall.Singleton;
            ICrawler crawler = MakeCrawler(tile);
            crawler.Walk().Returns(new MyInventory());
            IMovementStrategy strat = Substitute.For<IMovementStrategy>();
            strat.NextAction(Arg.Any<ICrawler>())
                 .Returns(_ => { throw new InvalidOperationException("nope"); },
                           _ => MoveAction.Walk);
            IExplorator explorator = new Explorator(crawler, strat);

            Assert.That(explorator.GetOut(2), Is.False);
            strat.Received(2).NextAction(crawler);
            crawler.Received(1).Walk();
        }

        [Test]
        public void NextActionOtherExceptionPropagates()
        {
            Tile tile = Wall.Singleton;
            ICrawler crawler = MakeCrawler(tile);
            IMovementStrategy strat = Substitute.For<IMovementStrategy>();
            strat.NextAction(Arg.Any<ICrawler>())
                 .Returns(_ => { throw new ApplicationException("boom"); });
            IExplorator explorator = new Explorator(crawler, strat);

            Assert.Throws<ApplicationException>(() => explorator.GetOut(1));
        }

        [Test]
        public void WalkInvalidOperationIsIgnored()
        {
            Tile tile = Wall.Singleton;
            ICrawler crawler = MakeCrawler(tile);
            crawler.Walk().Returns(_ => { throw new InvalidOperationException(); });
            IMovementStrategy strategy = StrategyReturning([MoveAction.Walk, MoveAction.TurnLeft]);
            IExplorator explorator = new Explorator(crawler, strategy);

            Assert.That(explorator.GetOut(2), Is.False);
            crawler.Received(1).Walk();
        }

        [Test]
        public void StrategyIsCalledAtMostNTimesAndStopsWhenOutside()
        {
            Tile tile = Wall.Singleton;
            Outside outside = Outside.Singleton;
            ICrawler crawler = Substitute.For<ICrawler>();
            crawler.Direction.Returns(Direction.North);
            crawler.FacingTile.Returns(_ => tile, _ => tile, _ => outside, _ => outside);
            crawler.Walk().Returns(new MyInventory());
            IMovementStrategy strategy = StrategyReturning([MoveAction.Walk, MoveAction.Walk, MoveAction.Walk, MoveAction.Walk]);
            IExplorator explorator = new Explorator(crawler, strategy);

            Assert.That(explorator.GetOut(10), Is.True);
            strategy.Received(1).NextAction(crawler);
        }
    }
}