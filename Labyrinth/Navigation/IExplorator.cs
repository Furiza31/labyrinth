namespace Labyrinth.Navigation;

public interface IExplorator
{
    /// <summary>
    /// Explores the labyrinth until an exit is found or the maximum number of steps is reached.
    /// </summary>
    /// <param name="maxSteps">The maximum number of steps to take during exploration.</param>
    /// <returns>True if an exit was found; otherwise, false.</returns>
    bool GetOut(int maxSteps);
}