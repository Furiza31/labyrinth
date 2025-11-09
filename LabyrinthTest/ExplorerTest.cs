using Labyrinth;
using Labyrinth.Crawl;
using Labyrinth.Sys;
using Moq;
using static Labyrinth.RandExplorer;

namespace LabyrinthTest;

public class ExplorerTest
{
    private class ExplorerEventsCatcher
    {
        public ExplorerEventsCatcher(RandExplorer explorer)
        {
            explorer.PositionChanged += (s, e) => CatchEvent(ref _positionChangedCount, e, _positionHistory);
            explorer.DirectionChanged += (s, e) => CatchEvent(ref _directionChangedCount, e, _directionHistory);
        }
        public int PositionChangedCount => _positionChangedCount;
        public int DirectionChangedCount => _directionChangedCount;

        public IReadOnlyList<(int X, int Y, Direction Dir)> PositionHistory => _positionHistory;
        public IReadOnlyList<(int X, int Y, Direction Dir)> DirectionHistory => _directionHistory;

        public (int X, int Y, Direction Dir)? LastArgs { get; private set; } = null;

        private void CatchEvent(
            ref int counter,
            CrawlingEventArgs e,
            List<(int X, int Y, Direction Dir)> history
        )
        {
            counter++;
            var directionSnapshot = CloneDirection(e.Direction);
            history.Add((e.X, e.Y, directionSnapshot));
            LastArgs = (e.X, e.Y, directionSnapshot);
        }
        private int _directionChangedCount = 0, _positionChangedCount = 0;
        private readonly List<(int X, int Y, Direction Dir)> _positionHistory = [];
        private readonly List<(int X, int Y, Direction Dir)> _directionHistory = [];

        private static Direction CloneDirection(Direction direction) =>
            direction switch
            {
                var dir when dir == Direction.North => Direction.North,
                var dir when dir == Direction.East => Direction.East,
                var dir when dir == Direction.South => Direction.South,
                var dir when dir == Direction.West => Direction.West,
                _ => throw new ArgumentOutOfRangeException(nameof(direction))
            };
    }

    private RandExplorer NewExplorerFor(
        string labyrinth,
        out ExplorerEventsCatcher events,
        params Actions[] actions
    )
    {
        var laby = new Labyrinth.Labyrinth(labyrinth);
        var mockRnd = new Mock<IEnumRandomizer<Actions>>();

        mockRnd.Setup(r => r.Next()).Returns(
            new Queue<Actions>(actions).Dequeue
        );
        var explorer = new RandExplorer(
            laby.NewCrawler(),
            mockRnd.Object
        );
        events = new ExplorerEventsCatcher(explorer);
        return explorer;
    }

    [Test]
    public void GetOutNegativeThrowsException()
    {
        var test = NewExplorerFor("""
            + +
            |x|
            +-+
            """,
            out var events
        );
        Assert.That(
            () => test.GetOut(-3),
            Throws.TypeOf<ArgumentOutOfRangeException>()
        );
        Assert.That(events.DirectionChangedCount, Is.EqualTo(0));
        Assert.That(events.PositionChangedCount, Is.EqualTo(0));
    }

    [Test]
    public void GetOutZeroThrowsException()
    {
        var test = NewExplorerFor("""
            + +
            |x|
            +-+
            """,
            out var events
        );
        Assert.That(
            () => test.GetOut(0),
            Throws.TypeOf<ArgumentOutOfRangeException>()
        );
        Assert.That(events.DirectionChangedCount, Is.EqualTo(0));
        Assert.That(events.PositionChangedCount, Is.EqualTo(0));
    }

    [Test]
    public void GetOutInHole()
    {
        var test = NewExplorerFor("""
            +-+
            |x/
            +-+
            |k|
            +-+
            """,
            out var events
        );
        var left = test.GetOut(10);

        Assert.That(left, Is.EqualTo(0));
        Assert.That(events.DirectionChangedCount, Is.EqualTo(10));
        Assert.That(events.PositionChangedCount, Is.EqualTo(0));
    }

    [Test]
    public void GetOutFacingOutsideAtStart()
    {
        var test = NewExplorerFor("""
            | x |
            |   |
            +---+
            """,
            out var events
        );
        var left = test.GetOut(10);

        Assert.That(left, Is.EqualTo(10));
        Assert.That(events.DirectionChangedCount, Is.EqualTo(0));
        Assert.That(events.PositionChangedCount, Is.EqualTo(0));
    }

    [Test]
    public void GetOutRotatingOnce()
    {
        var test = NewExplorerFor("""
            --+
              |
            x |
            --+
            """,
            out var events,
            Actions.TurnLeft
        );

        var left = test.GetOut(10);

        Assert.That(left, Is.EqualTo(9));
        Assert.That(events.DirectionChangedCount, Is.EqualTo(1));
        Assert.That(events.PositionChangedCount, Is.EqualTo(0));
        Assert.That(events.LastArgs, Is.EqualTo((0, 2, Direction.West)));
    }

    [Test]
    public void GetOutRotatingTwice()
    {
        var test = NewExplorerFor("""
            +---+
            |   |
            | x |
            """,
            out var events,
            Actions.TurnLeft,
            Actions.TurnLeft
        );

        var left = test.GetOut(10);

        Assert.That(left, Is.EqualTo(8));
        Assert.That(events.DirectionChangedCount, Is.EqualTo(2));
        Assert.That(events.LastArgs, Is.EqualTo((2, 2, Direction.South)));
    }

    [Test]
    public void GetOutWalkingOnce()
    {
        var test = NewExplorerFor("""
            --+
             x|
            --+
            """,
            out var events,
            // auto turn left
            Actions.Walk
        );

        var left = test.GetOut(10);

        Assert.That(left, Is.EqualTo(8));
        Assert.That(events.PositionChangedCount, Is.EqualTo(1));
        Assert.That(events.DirectionChangedCount, Is.EqualTo(1));
        Assert.That(events.LastArgs, Is.EqualTo((0, 1, Direction.West)));
    }

    [Test]
    public void GetOutWalkingExactMoves()
    {
        var test = NewExplorerFor("""
            ---+
              x|
            ---+
            """,
            out var events,
            // auto turn left
            Actions.Walk,
            Actions.Walk
        );

        var left = test.GetOut(3);

        Assert.That(left, Is.EqualTo(0));
        Assert.That(events.DirectionChangedCount, Is.EqualTo(1));
        Assert.That(events.PositionChangedCount, Is.EqualTo(2));
        Assert.That(events.LastArgs, Is.EqualTo((0, 1, Direction.West)));
    }

    [Test]
    public void GetOutWithMultipleMoves()
    {
        var test = NewExplorerFor("""
            +---+
               k|
            + -/+
            |  x|
            | --+
            """,
            out var events,
            // auto turn left
            Actions.Walk,
            Actions.Walk,
            // auto turn left
            Actions.TurnLeft,
            Actions.TurnLeft,
            Actions.Walk,
            Actions.Walk,
            // auto turn left
            Actions.Walk
        );

        var left = test.GetOut(15);

        Assert.That(left, Is.EqualTo(5));
        Assert.That(events.DirectionChangedCount, Is.EqualTo(5));
        Assert.That(events.PositionChangedCount, Is.EqualTo(5));
        Assert.That(events.LastArgs, Is.EqualTo((0, 1, Direction.West)));
    }

    [Test]
    public void GetOutPassingADoor()
    {
        var test = NewExplorerFor("""
            +-/-+
            | k |
            | x |
            +---+
            """,
            out var events,
            Actions.Walk,
            Actions.Walk
        );
        var left = test.GetOut(10);

        Assert.That(left, Is.EqualTo(8));
        Assert.That(events.DirectionChangedCount, Is.EqualTo(0));
        Assert.That(events.PositionChangedCount, Is.EqualTo(2));
        Assert.That(events.LastArgs, Is.EqualTo((2, 0, Direction.North)));
    }

    [Test]
    public void GetOutPassingTwoDoors()
    {
        var test = NewExplorerFor("""
            +--+
            |kx|
            +/-+
            | k/
            +--+
            """,
            out var events,
            // auto turn left
            Actions.Walk, // key
                          // auto turn left
            Actions.Walk, // door
            Actions.Walk,
            // auto turn left
            Actions.Walk, // key
            Actions.Walk  // door
        );
        var left = test.GetOut(10);

        Assert.That(left, Is.EqualTo(2));
        Assert.That(events.DirectionChangedCount, Is.EqualTo(3));
        Assert.That(events.PositionChangedCount, Is.EqualTo(5));
        Assert.That(events.LastArgs, Is.EqualTo((3, 3, Direction.East)));
    }

    [Test]
    public void GetOutPassingTwoKeysBeforeDoors()
    {
        var test = NewExplorerFor("""
            +--+
            |kx/
            | k/
            +--+
            """,
            out var events,
            Actions.Walk,// key
            Actions.Walk,
            Actions.Walk,// second key before doors
            Actions.Walk // door
        );
        var left = test.GetOut(10);

        Assert.That(left, Is.EqualTo(3));
        Assert.That(events.DirectionChangedCount, Is.EqualTo(3));
        Assert.That(events.PositionChangedCount, Is.EqualTo(4));
        Assert.That(events.LastArgs, Is.EqualTo((3, 2, Direction.East)));
    }

    [Test]
    public void GetOutCollectsAndTriesMultipleKeys()
    {
        var test = NewExplorerFor("""
            +------+
            |x k  /|
            |  |   |
            |  k  /|
            +------+
            """,
            out var events,
            Actions.Walk,
            Actions.Walk,
            Actions.Walk,
            Actions.Walk,
            Actions.TurnLeft,
            Actions.Walk,
            Actions.Walk,
            Actions.TurnLeft,
            Actions.Walk,
            Actions.Walk,
            Actions.TurnLeft,
            Actions.Walk,
            Actions.Walk,
            Actions.Walk,
            Actions.Walk,
            Actions.Walk
        );

        var left = test.GetOut(25);

        Assert.That(left, Is.EqualTo(0));
        Assert.That(events.PositionChangedCount, Is.EqualTo(13));
        Assert.That(events.DirectionChangedCount, Is.EqualTo(12));
        Assert.That(events.PositionHistory[events.PositionHistory.Count - 1], Is.EqualTo((6, 1, Direction.East)));
    }
}
