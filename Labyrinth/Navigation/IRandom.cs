namespace Labyrinth.Navigation;

public interface IRandom
{
    /// <summary>
    /// Generates a random integer between min (inclusive) and max (exclusive).
    /// </summary>
    /// <param name="min">The inclusive lower bound of the random number returned.</param>
    /// <param name="max">The exclusive upper bound of the random number returned.</param>
    /// <returns>A random integer between min and max.</returns>
    int Next(int min, int max);
}