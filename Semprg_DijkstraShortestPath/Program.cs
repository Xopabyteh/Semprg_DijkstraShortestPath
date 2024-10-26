using Semprg_DijkstraShortestPath;

var graphFilePath = Path.Combine(Environment.CurrentDirectory, "silnicni_vzdalenosti_mesta_reduced.csv");
var graph = DijsktraAlg.ParseGraphDoubleEdged(graphFilePath);

// Expected results:
// Praha -> Hradec -> Olomouc (256)
// Praha -> Hradec -> Jihlava -> Ostrava (350)
var path = DijsktraAlg.FindShortestPath(
    "Praha",
    "Olomouc",
    graph);

DijsktraAlg.PrettyPrintPath(path.Nodes, path.TotalDistance);