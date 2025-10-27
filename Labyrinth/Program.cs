using Labyrinth.Crawl;
using Labyrinth.Navigation;

const string asciiMap = """
    +--+--------+
    |  /        |
    |  +--+--+  |
    |     |k    |
    +--+  |  +--+
       |k  x    |
    +  +-------/|
    |           |
    +-----------+
    """;

var labyrinth = new Labyrinth.Labyrinth(asciiMap);
var crawler = labyrinth.NewCrawler();
var explorator = new Explorator(crawler, new RandomMovementStrategy(new SystemRandom()));

const int refreshDelayMs = 50;

var rows = labyrinth
    .ToString()
    .Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);

var currentPosition = (X: crawler.X, Y: crawler.Y);

Console.CursorVisible = false;

try
{
    Console.Clear();

    foreach (var row in rows)
    {
        Console.WriteLine(row);
    }

    static char DirectionToChar(Direction direction) =>
        direction == Direction.North ? '^' :
        direction == Direction.East ? '>' :
        direction == Direction.South ? 'v' : '<';

    void RestoreTile(int x, int y)
    {
        if (y < 0 || y >= rows.Length) return;
        var tileRow = rows[y];
        if (x < 0 || x >= tileRow.Length) return;

        Console.SetCursorPosition(x, y);
        Console.Write(tileRow[x]);
    }

    void DrawExplorer(CrawlingEventArgs args)
    {
        Console.SetCursorPosition(args.X, args.Y);
        Console.Write(DirectionToChar(args.Direction));

        currentPosition = (args.X, args.Y);
        Thread.Sleep(refreshDelayMs);
    }

    DrawExplorer(new CrawlingEventArgs(crawler.X, crawler.Y, crawler.Direction));

    crawler.OnPositionChanged += (_, args) =>
    {
        RestoreTile(currentPosition.X, currentPosition.Y);
        DrawExplorer(args);
    };

    crawler.OnDirectionChanged += (_, args) => DrawExplorer(args);

    explorator.GetOut(100000);

    Console.SetCursorPosition(0, rows.Length + 1);
    Console.WriteLine("Exploration terminée.");
}
finally
{
    Console.CursorVisible = true;
}
