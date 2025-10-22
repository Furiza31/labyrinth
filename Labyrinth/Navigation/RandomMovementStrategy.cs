using Labyrinth.Crawl;

namespace Labyrinth.Navigation;

public class RandomMovementStrategy(IRandom random) : IMovementStrategy
{
    private readonly IRandom _random = random ?? throw new ArgumentNullException(nameof(random));

    public MoveAction NextAction(ICrawler crawler)
    {
        var actionIndex = _random.Next(0, 3);
        return actionIndex switch
        {
            0 => MoveAction.Walk,
            1 => MoveAction.TurnLeft,
            2 => MoveAction.TurnRight,
            _ => throw new NotSupportedException("Action inconnue")
        };
    }
}