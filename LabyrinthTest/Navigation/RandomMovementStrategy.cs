using Labyrinth.Crawl;
using Labyrinth.Navigation;
using NSubstitute;

namespace LabyrinthTest.Navigation;

[TestFixture(Description = "Tests for RandomMovementStrategy")]
public class RandomMovementStrategyTests
{
    private class MyRandom(int[] values) : IRandom
    {
        private readonly int[] _values = values;
        private int _index = 0;
        public int Next(int minValue, int maxValue)
        {
            return _values[_index++ % _values.Length];
        }
    }

    [Test]
    public void WalkTurnLeftTurnRight()
    {
        ICrawler crawler = Substitute.For<ICrawler>();
        MyRandom random = new([0, 1, 2, 0, 2, 1]);
        IMovementStrategy strategy = new RandomMovementStrategy(random);

        Assert.That(strategy.NextAction(crawler), Is.EqualTo(MoveAction.Walk));
        Assert.That(strategy.NextAction(crawler), Is.EqualTo(MoveAction.TurnLeft));
        Assert.That(strategy.NextAction(crawler), Is.EqualTo(MoveAction.TurnRight));
        Assert.That(strategy.NextAction(crawler), Is.EqualTo(MoveAction.Walk));
        Assert.That(strategy.NextAction(crawler), Is.EqualTo(MoveAction.TurnRight));
        Assert.That(strategy.NextAction(crawler), Is.EqualTo(MoveAction.TurnLeft));
    }

    [Test]
    public void NullRandomThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new RandomMovementStrategy(null!));
    }
}