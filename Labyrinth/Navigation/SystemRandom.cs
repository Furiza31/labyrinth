namespace Labyrinth.Navigation;

public class SystemRandom() : IRandom
{
    private readonly Random _random = new Random();

    public int Next(int min, int max) => _random.Next(min, max);
}