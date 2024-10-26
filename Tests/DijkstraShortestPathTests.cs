using Semprg_DijkstraShortestPath;

namespace Tests;

public class DijkstraShortestPathTests
{
    private DijkstraGraph graph;
    private static string s_graphFilePath = Path.Combine(Environment.CurrentDirectory, "silnicni_vzdalenosti_mesta_reduced.csv");

    public DijkstraShortestPathTests()
    {
        graph = DijsktraAlg.ParseGraphDoubleEdged(s_graphFilePath);
    }

    [Theory]
    [InlineData("Praha", "Olomouc", 256)]
    [InlineData("Praha", "Ostrava", 350)]
    public void TestAlg(string from, string to, int expectedDistance)
    {
        var result = DijsktraAlg.FindShortestPath(from, to, graph);
        
        Assert.Equal(expectedDistance, result.TotalDistance);
    }
}