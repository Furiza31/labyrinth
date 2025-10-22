namespace Labyrinth.Build;

/// <summary>
/// Event arguments for labyrinth start event.
/// </summary>
public class StartEventArgs(int x, int y) : EventArgs
{
    /// <summary>
    /// Crawler starting position X coordinate.
    /// </summary>
    public int X { get; init; } = x;

    /// <summary>
    /// Crawler starting position Y coordinate.
    /// </summary>
    public int Y { get; init; } = y;
}