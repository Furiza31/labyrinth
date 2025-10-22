namespace Labyrinth.Crawl;

public class CrawlingEventArgs(int x, int y, Direction direction) : EventArgs
{
    /// <summary>
    /// Crawler current position X coordinate.
    /// </summary>
    public int X { get; init; } = x;

    /// <summary>
    /// Crawler current position Y coordinate.
    /// </summary>
    public int Y { get; init; } = y;

    /// <summary>
    /// Crawler current facing direction.
    /// </summary>
    public Direction Direction { get; init; } = direction;
}
