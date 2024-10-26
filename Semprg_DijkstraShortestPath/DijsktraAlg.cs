namespace Semprg_DijkstraShortestPath;

public static class DijsktraAlg
{
    public static (DijkstraNode[] Nodes, float TotalDistance) FindShortestPath(
        string fromName,
        string toName,
        DijkstraGraph graph)
    {
        // Input is a set of nodes with edges
        // Edges have a price - the distance between nodes

        // Find the shortest path between two nodes using Dijkstra's algorithm
        //  Visit a node:
        //  - Mark it as visited
        //  - Calculate distance to all neighbors:
        //      - Set their value to the minimum of their current value and the value of the current node + the edge value (distance from node to other node)
        //      - Mark from whom the neighbor was visited (the current node)
        //  Start by visiting the first node
        //  Then visit the smallest unvisited node
        //  Repeat until all nodes are visited
        //  The value of the END node is the shortest path
        //  The path can be reconstructed by following the "from" pointers

        // Nodes that have been visited
        var visitedNodes = new HashSet<DijkstraNode>();
        var nextNodePriorityQueue = new PriorityQueue<DijkstraNode, float>();
        
        // Metadata about the nodes
        var nodeMetadata = new Dictionary<DijkstraNode, DijkstraVisitedMetadata>(graph.TotalNodeCount);

        // Visit first node (it is visited from itself and has total distance 0)
        var startNode = new DijkstraNode(fromName);
        var endNode = new DijkstraNode(toName);
        nodeMetadata[startNode] = new DijkstraVisitedMetadata(startNode, 0);
        nextNodePriorityQueue.Enqueue(startNode, 0f);
        
        // var smallestValueDijkstraNode = startNode;
        
        while(true)
        {
            var smallestPriorityNode = nextNodePriorityQueue.Dequeue();
            
            DijkstraVisitNode(smallestPriorityNode);

            if (visitedNodes.Count == graph.TotalNodeCount)
                break; // All nodes have been visited
        }

        var totalDistance = nodeMetadata[endNode].TotalDistance;
        var path = ReconstructPath(endNode);

        return (path, totalDistance);

        void DijkstraVisitNode(DijkstraNode visitedNode)
        {
            visitedNodes.Add(visitedNode);

            var unvisitedNeighborEdges = graph.Edges
                .Where(edge => edge.From.Equals(visitedNode) && !visitedNodes.Contains(edge.To));
            
            foreach (var neighborEdge in unvisitedNeighborEdges)
            {
                // Calculate distance for neighbor
                var doesNeighborHaveMetadata = nodeMetadata.TryGetValue(neighborEdge.To, out var neighborMetadata);
                var distanceFromNodeToNeighbor = nodeMetadata[visitedNode].TotalDistance + neighborEdge.Value;

                if(!doesNeighborHaveMetadata 
                   || distanceFromNodeToNeighbor < neighborMetadata.TotalDistance) 
                {
                    // Either Calculating neighbor for the first time ||
                    // Distance from neighbor is smaller than previous distance
                    
                    // Update distance (and whom it was visited from)
                    nodeMetadata[neighborEdge.To] = new DijkstraVisitedMetadata(
                        ReachedFrom: visitedNode,
                        TotalDistance: distanceFromNodeToNeighbor
                    );
                    
                    // Add/Requeue in the priority queue
                    nextNodePriorityQueue.Enqueue(neighborEdge.To, distanceFromNodeToNeighbor);
                } 
            }
        }

        // Reconstruct the path
        DijkstraNode[] ReconstructPath(DijkstraNode from)
        {
            var reconstructedPath = new List<DijkstraNode>();
            var current = from;

            while (!IsNodeStartNode(current))
            {
                reconstructedPath.Add(current);
                current = nodeMetadata[current].ReachedFrom;
            } 
            
            reconstructedPath.Add(current); // Add the start node at the end
            reconstructedPath.Reverse();

            return reconstructedPath.ToArray();
        }

        bool IsNodeStartNode(DijkstraNode node) // If node was reached from itself, it is the start node
            => node == nodeMetadata[node].ReachedFrom;
    }

    // public static DijkstraEdge[] ParseGraphOneWay(string csvPath)
    // {
    //     // From, To, Distance
    //     // Skip first line
    //
    //     var lines = File.ReadAllLines(csvPath);
    //     var edges = new DijkstraEdge[lines.Length - 1];
    //
    //     for (var i = 1; i < lines.Length; i++)
    //     {
    //         var parts = lines[i].Split(',');
    //         edges[i - 1] = new DijkstraEdge(
    //             new DijkstraNode(parts[0]),
    //             new DijkstraNode(parts[1]),
    //             float.Parse(parts[2]));
    //     }
    //
    //     return edges;
    // }

    public static DijkstraGraph ParseGraphDoubleEdged(string csvPath)
    {
        // From, To, Distance
        // Skip first line

        var lines = File.ReadAllLines(csvPath);
        var edges = new List<DijkstraEdge>((lines.Length-1) * 2);
        var li = 1;
        while(true)
        {
            var parts = lines[li].Split(',');
            var from = new DijkstraNode(parts[0]);
            var to = new DijkstraNode(parts[1]);
            var distance = float.Parse(parts[2]);

            edges.Add(new DijkstraEdge(from, to, distance));
            edges.Add(new DijkstraEdge(to, from, distance));

            li++;
            if(li > lines.Length - 1)
                break;
        }

        var totalNodes = edges
            .Select(e => e.From)
            .DistinctBy(n => n.Name)
            .Count();
        
        return new (edges.ToArray(), totalNodes);
    }  

    public static void PrettyPrintPath(DijkstraNode[] path, float totalDistance)
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
}