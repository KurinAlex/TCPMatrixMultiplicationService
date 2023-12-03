using Utility;

namespace Server;

internal record ThreadContainer(MultiplicationData Data)
{
    public IList<(int, double)> Results { get; } = new List<(int, double)>();
}
