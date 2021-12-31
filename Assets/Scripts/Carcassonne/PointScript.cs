using System.Collections.Generic;
using System.Linq;
using Carcassonne.State;
using UnityEngine;

namespace Carcassonne
{
    public class PointScript : MonoBehaviour
    {
        public static Vector2Int North => Vector2Int.up;
        public static Vector2Int South => Vector2Int.down;
        public static Vector2Int West => Vector2Int.left;
        public static Vector2Int East => Vector2Int.right;
        public static Vector2Int Centre => Vector2Int.zero;
        public static Vector2Int Center => Vector2Int.zero;
        
        private bool broken;
        private int counter;
        private int finalScore;
        private Graph g;
        private readonly int nbrOfVertices = GameRules.BoardSize / 2;
        private int roadBlocks;
        private int vertexIterator;
        private bool[] visited;

        private void Start()
        {
            g = new Graph(nbrOfVertices);
        }

        // public bool testIfMeepleCantBePlaced(int Vindex, TileScript.Geography weight)
        // {
        //     roadBlocks = 0;
        //     broken = false;
        //     counter = 0;
        //     visited = new bool[GameRules.BoardSize / 2];
        //     dfs(Vindex, weight, false);
        //     return broken || roadBlocks == 2;
        // }
        //
        // public bool testIfMeepleCantBePlacedDirection(int Vindex, TileScript.Geography weight, Vector2Int direction)
        // {
        //     roadBlocks = 0;
        //     broken = false;
        //     counter = 0;
        //     visited = new bool[GameRules.BoardSize / 2];
        //     dfsDirection(Vindex, weight, direction, false);
        //     return broken || roadBlocks == 2;
        //     ;
        // }

        public int startDfs(int Vindex, Geography weight, bool GameEnd)
        {
            counter = 1;
            roadBlocks = 0;
            finalScore = 0;
            visited = new bool[GameRules.BoardSize / 2];
            Debug.Log("StartDFS");
            dfs(Vindex, weight, GameEnd);
            //Debug.Log(finalScore);
            if (weight == Geography.City) return counter;
            //Debug.Log("final score: " + finalScore);
            return finalScore;
        }

        /// <summary>
        ///     startDFS takes an index, a weight and a direction to calculate the number of points the finished set is worth.
        ///     Mainly used by tiles with a town as a centerpiece.
        ///     The direction starts the depth first search, but only in the specified direction.
        /// </summary>
        /// <param name="Vindex"></param>
        /// <param name="weight"></param>
        /// <param name="direction"></param>
        public int startDfsDirection(int Vindex, Geography weight, Vector2Int direction, bool GameEnd)
        {
            counter = 0;
            roadBlocks = 0;
            //if (weight == TileScript.geography.Road) roadBlocks = 1;
            finalScore = 0;
            visited = new bool[GameRules.BoardSize / 2]; //TODO Hardcoded

            dfsDirection(Vindex, weight, direction, GameEnd);
            //Debug.Log(finalScore);
            //Temporary fix
            //if (counter > 2)
            //{
            //    counter--;
            //}
            if (weight == Geography.City) return counter;
            //Debug.Log("final score: " + finalScore);
            return finalScore;
        }

        private void dfsDirection(int Vindex, Geography weight, Vector2Int direction, bool GameEnd)
        {
            if (!visited[Vindex])
            {
                counter++;
                visited[Vindex] = true;

                var neighbours = g.getNeighbours(Vindex, weight, direction);

                if (weight == Geography.Road) Debug.Log(weight + " : " + neighbours.Count);

                for (var i = 0; i < neighbours.Count; i++)
                {
                    var tmp = g.getGraph()
                        .ElementAt(neighbours.ElementAt(i).endVertex); //Getting the tile that we are comming from
                    for (var j = 0; j < tmp.Count; j++)
                        if (tmp.ElementAt(j).endVertex == Vindex)
                            //Debug.Log("Meeple set on " + weight);
                            tmp.ElementAt(j).hasMeeple = true;
                    if (!neighbours.ElementAt(i).hasMeeple)
                        //Debug.Log("Meeple set on " + weight);
                        neighbours.ElementAt(i).hasMeeple = true;
                    else
                        broken = true;

                    //Does nothing right now.
                    if (neighbours.Count == 0)
                    {
                        placeVertex(vertexIterator++, new[] {Vindex}, new[] {weight}, Geography.Field,
                            new[] {Geography.Field}, new[] {direction});
                        neighbours = g.getNeighbours(Vindex, weight, direction);
                        tmp = g.getGraph()
                            .ElementAt(neighbours.ElementAt(0).endVertex); //Getting the tile that we are comming from
                        if (tmp.ElementAt(0).endVertex == Vindex)
                            //Debug.Log("Meeple set on " + weight);
                            tmp.ElementAt(0).hasMeeple = true;
                        RemoveVertex(vertexIterator);
                        vertexIterator--;
                        neighbours.RemoveFirst();
                    }
                    
                    //TODO Looks for roadblocks but won't find circular roads.
                    if (weight == Geography.Road)
                        if (neighbours.ElementAt(i).center == Geography.Village ||
                            neighbours.ElementAt(i).center == Geography.Cloister ||
                            neighbours.ElementAt(i).center == Geography.City)
                        {
                            //Debug.Log(neighbours.ElementAt(i).center);
                            roadBlocks++;
                            if (roadBlocks == 2)
                                finalScore = counter;
                            //Debug.Log(finalScore);
                            dfsDirection(neighbours.ElementAt(i).endVertex, weight, Graph.getReverseDirection(direction),
                                GameEnd);
                            //Debug.Log(roadBlocks);
                            //Debug.Log("RoadBlock hit");
                        }

                    if (neighbours.ElementAt(i).center == Geography.Village ||
                        neighbours.ElementAt(i).center == Geography.Field)
                        counter++;
                    else
                        dfs(neighbours.ElementAt(i).endVertex, weight, GameEnd);
                }
            }

            if (GameEnd) finalScore = counter;
        }

        public void RemoveVertex(int Vindex)
        {
            if (g.getGraph().ElementAt(Vindex) != null) g.getGraph().ElementAt(Vindex).Clear();
            for (var i = 0; i < g.getGraph().Count; i++)
            for (var j = 0; j < g.getGraph().ElementAt(i).Count; j++)
                if (g.getGraph().ElementAt(i).ElementAt(j).endVertex == Vindex)
                    g.getGraph().ElementAt(i).Remove(g.getGraph().ElementAt(i).ElementAt(j));
        }

        private void dfs(int Vindex, Geography weight, bool GameEnd)
        {
            if (!visited[Vindex])
            {
                if (weight == Geography.Road)
                {
                    counter++;
                    Debug.Log("Hit Road " + counter);
                }
                else if (weight == Geography.City)
                {
                    counter += 2;
                    Debug.Log("Hit Town " + counter);
                }

                visited[Vindex] = true;

                var neighbours = g.getNeighbours(Vindex, weight);
                for (var i = 0; i < neighbours.Count; i++)
                {
                    if (!neighbours.ElementAt(i).hasMeeple)
                    {
                        neighbours.ElementAt(i).hasMeeple = true;
                    }
                    else
                    {
                        if (weight == Geography.Road)
                        {
                            if (!visited[neighbours.ElementAt(i).endVertex]) broken = true;
                        }
                        else
                        {
                            broken = true;
                        }
                    }

                    if (weight == Geography.Road)
                        if (neighbours.ElementAt(i).center == Geography.Village ||
                            neighbours.ElementAt(i).center == Geography.Cloister ||
                            neighbours.ElementAt(i).center == Geography.City ||
                            neighbours.ElementAt(i).center == Geography.RoadStream)
                        {
                            roadBlocks++;
                            if (roadBlocks == 2) finalScore = counter;
                        }

                    if (neighbours.ElementAt(i).center == Geography.Village ||
                        neighbours.ElementAt(i).center == Geography.Field)
                        counter++;
                    else
                        dfs(neighbours.ElementAt(i).endVertex, weight, GameEnd);
                }
            }

            if (GameEnd) finalScore = counter;
        }

        public void placeVertex(int Vindex, int[] Vindexes, Geography[] weights,
            Geography startCenter, Geography[] endCenters, Vector2Int[] directions)
        {
            vertexIterator = Vindex;
            for (var i = 0; i < Vindexes.Length; i++)
                if (Vindexes[i] != 0)
                    g.addEdge(Vindex, Vindexes[i], weights[i], startCenter, endCenters[i], directions[i]);
        }

        public class Graph
        {
            private readonly LinkedList<LinkedList<Edge>> graph;

            public Graph(int nbrOfVertices)
            {
                graph = new LinkedList<LinkedList<Edge>>();
                for (var i = 0; i < nbrOfVertices; i++) graph.AddLast(new LinkedList<Edge>());
            }

            public static Vector2Int getReverseDirection(Vector2Int direction)
            {
                return -direction;
            }

            public void addEdge(int startVertex, int endVertex, Geography weight,
                Geography startCenter, Geography endCenter, Vector2Int direction)
            {
                graph.ElementAt(startVertex)
                    .AddLast(new Edge(endVertex, weight, endCenter, getReverseDirection(direction)));
                graph.ElementAt(endVertex).AddLast(new Edge(startVertex, weight, startCenter, direction));
            }

            public override string ToString()
            {
                var result = "";
                for (var i = 0; i < graph.Count; i++)
                {
                    result += i + ": " + "\n";
                    foreach (var e in graph.ElementAt(i)) result += i + " --> " + e + "\n";
                }

                return result;
            }


            public LinkedList<Edge> getNeighbours(int Vindex, Geography weight)
            {
                var neighbours = new LinkedList<Edge>();
                if (weight == Geography.Road)
                    for (var i = 0; i < graph.ElementAt(Vindex).Count; i++)
                        if (graph.ElementAt(Vindex).ElementAt(i).weight == weight)
                            neighbours.AddLast(graph.ElementAt(Vindex).ElementAt(i));
                if (weight == Geography.City)
                    for (var i = 0; i < graph.ElementAt(Vindex).Count; i++)
                        if (graph.ElementAt(Vindex).ElementAt(i).weight == weight)
                            neighbours.AddLast(graph.ElementAt(Vindex).ElementAt(i));
                return neighbours;
            }

            public LinkedList<Edge> getNeighbours(int Vindex, Geography weight, Vector2Int direction)
            {
                var neighbours = new LinkedList<Edge>();
                if (weight == Geography.Road || weight == Geography.City)
                    for (var i = 0; i < graph.ElementAt(Vindex).Count; i++)
                        if (graph.ElementAt(Vindex).ElementAt(i).weight == weight &&
                            graph.ElementAt(Vindex).ElementAt(i).direction == getReverseDirection(direction))
                        {
                            neighbours.AddLast(graph.ElementAt(Vindex).ElementAt(i));
                        }

                return neighbours;
            }

            public LinkedList<LinkedList<Edge>> getGraph()
            {
                return graph;
            }
        }

        public class Edge
        {
            public Geography center;
            public Vector2Int direction;
            public int endVertex;
            public bool hasMeeple;
            public Geography weight;

            public Edge(int endVertex, Geography weight, Geography center, Vector2Int direction)
            {
                hasMeeple = false;
                this.endVertex = endVertex;
                this.weight = weight;
                this.center = center;
                this.direction = direction;
            }

            public override string ToString()
            {
                return "(" + endVertex + ")";
            }
        }
    }
}