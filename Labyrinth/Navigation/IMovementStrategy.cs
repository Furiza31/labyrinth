using Labyrinth.Crawl;

namespace Labyrinth.Navigation;

public interface IMovementStrategy
{
    /// <summary>
    /// Determines the next movement action for the given crawler.
    /// </summary>
    /// <param name="crawler">The crawler for which to determine the next action.</param>
    /// <returns>The next movement action.</returns>
    MoveAction NextAction(ICrawler crawler);
}