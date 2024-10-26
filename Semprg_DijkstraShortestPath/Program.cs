var graphFilePath = Path.Combine(Environment.CurrentDirectory, "silnicni_vzdalenosti_mesta_reduced.csv");

// Praha -> Hradec -> Olomouc (256)
// Praha -> Hradec -> Jihlava -> Ostrava (350)
var path = FindShortestPath(
    "Praha",
    "Olomouc",
    () => ParseGraphDoubleEdged(graphFilePath));

PrettyPrintPath(path.Nodes, path.TotalDistance);

(DijkstraNode[] Nodes, float TotalDistance) FindShortestPath(string FromName, string ToName, Func<DijkstraEdge[]> GraphBuilder)
{
    // Input is a set of nodes with edges
    // Edges have a price - the distance between nodes

    // Find the shortest path between two nodes using Dijkstra's algorithm
    //  Visit a node:
    //  - Mark it as visited
    //  - Visit all neighbors:
    //      - Set their value to the minimum of their current value and the value of the current node + the edge value (distance from node to other node)
    //      - Mark from whom the neighbor was visited (the current node)
    //  Start by visiting the first node
    //  Then visit the smallest unvisited node
    //  Repeat until END is reached
    //  The value of the END node is the shortest path
    //  The path can be reconstructed by following the "from" pointers

    // The function should ensure a clean graph each time
    // (which might be useful, if it is edited, so here it is useles... :D)
    var graph = GraphBuilder();

    // Nodes that have been visited
    var visitedNodes = new HashSet<DijkstraNode>();

    // Metadata about the nodes
    var nodeMetadata = new Dictionary<DijkstraNode, DijkstraVisitedMetadata>(graph.Length);

    // Visit first node (it is visited from itself and has total distance 0)
    var startNode = new DijkstraNode(FromName);
    var endNode = new DijkstraNode(ToName);
    nodeMetadata[startNode] = new DijkstraVisitedMetadata(startNode, 0);

    var smallestValueDijkstraNode = startNode;
    
    while(true)
    {
        var didReachEnd = DijkstraVisitNeighbors(smallestValueDijkstraNode);
        if(didReachEnd)
            break;
        
        smallestValueDijkstraNode = nodeMetadata
            .Where(n => !visitedNodes.Contains(n.Key))
            .MinBy(n => n.Value.TotalDistance)
            .Key;
    }

    var totalDistance = nodeMetadata[endNode].TotalDistance;
    var path = ReconsturctPath(endNode);

    return (path, totalDistance);

    // Visit all nodes
    // Returns true if reached the end node, false otherwise
    bool DijkstraVisitNeighbors(DijkstraNode FromNode)
    {
        visitedNodes.Add(FromNode);

        var unvisitedNeighborEdges = graph
            .Where(edge => edge.From.Equals(FromNode) && !visitedNodes.Contains(edge.To));
        
        foreach (var neighborEdge in unvisitedNeighborEdges)
        {
            // Calculate distance for neighbor
            var doesHaveMetadata = nodeMetadata.TryGetValue(neighborEdge.To, out var metadata);
            
            var distanceFromNodeToNeighbor = nodeMetadata[FromNode].TotalDistance + neighborEdge.Value;

            if(doesHaveMetadata)
            {
                // Already visited from someone, might've been a shorter distance
                
                if(distanceFromNodeToNeighbor < metadata.TotalDistance)
                {
                    nodeMetadata[neighborEdge.To] = new DijkstraVisitedMetadata(
                        ReachedFrom: FromNode,
                        TotalDistance: distanceFromNodeToNeighbor
                    );
                }
            } 
            else
            {
                nodeMetadata[neighborEdge.To] = new DijkstraVisitedMetadata(
                    ReachedFrom: FromNode,
                    TotalDistance: distanceFromNodeToNeighbor
                );
            }


            if(neighborEdge.To.Equals(endNode))
            {
                return true;
            }

            //// Set the smallest value node (if viable)
            //if(totalDistance < nodeMetadata[smallestValueDijkstraNode].TotalDistance)
            //{
            //    smallestValueDijkstraNode = neighborEdge.To;
            //}
        }

        return false;
    }

    // Reconstruct the path
    DijkstraNode[] ReconsturctPath(DijkstraNode from)
    {
        var path = new List<DijkstraNode>();
        var current = from;

        while(true)
        {
            path.Add(current);
            current = nodeMetadata[current].ReachedFrom;

            // If node was reached from itself, it is the start node
            if (current == nodeMetadata[current].ReachedFrom)
                break;
        }

        path.Reverse();
        return path.ToArray();
    }
}

DijkstraEdge[] ParseGraphOneWay(string csvPath)
{
    // From, To, Distance
    // Skip first line

    var lines = File.ReadAllLines(csvPath);
    var edges = new DijkstraEdge[lines.Length - 1];

    for (var i = 1; i < lines.Length; i++)
    {
        var parts = lines[i].Split(',');
        edges[i - 1] = new DijkstraEdge(
            new DijkstraNode(parts[0]),
            new DijkstraNode(parts[1]),
            float.Parse(parts[2]));
    }

    return edges;
}

DijkstraEdge[] ParseGraphDoubleEdged(string csvPath)
{
    // From, To, Distance
    // Skip first line

    var lines = File.ReadAllLines(csvPath);
    var edges = new List<DijkstraEdge>((lines.Length-1) * 2);
    var ei = 0;
    var li = 1;
    while(true)
    {
        var parts = lines[li].Split(',');
        var from = new DijkstraNode(parts[0]);
        var to = new DijkstraNode(parts[1]);
        var distance = float.Parse(parts[2]);

        edges.Add(new DijkstraEdge(from, to, distance));
        edges.Add(new DijkstraEdge(to, from, distance));

        ei+=2;
        li++;
        if(li > lines.Length - 1)
            break;
    }


    return edges.ToArray();
}  

void PrettyPrintPath(DijkstraNode[] path, float totalDistance)
{
    // n1 -> n2 -> n3
    Console.WriteLine($"Total distance: {totalDistance}");
    Console.WriteLine("Path:");
    for (int i = 0; i < path.Length; i++)
    {
        DijkstraNode node = path[i];
        Console.Write(node.Name);
        if (i < path.Length - 1)
        {
            Console.Write(" -> ");
        }
    }
}

readonly record struct DijkstraNode(string Name);
readonly record struct DijkstraEdge(DijkstraNode From, DijkstraNode To, float Value);
readonly record struct DijkstraVisitedMetadata(DijkstraNode ReachedFrom, float TotalDistance);
