using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Carcassonne
{
    public class PointScript : MonoBehaviour
    {
        public enum Direction
        {
            NORTH,
            EAST,
            SOUTH,
            WEST,
            CENTER,
            SELF
        }

        private bool broken;
        private int counter;
        private int finalScore;
        private Graph g;
        private readonly int nbrOfVertices = 85;
        private int roadBlocks;
        private int vertexIterator;
        private bool[] visited;

        private void Start()
        {
            g = new Graph(nbrOfVertices);
        }

        public bool testIfMeepleCantBePlaced(int Vindex, TileScript.Geography weight)
        {
            roadBlocks = 0;
            broken = false;
            counter = 0;
            visited = new bool[85];
            dfs(Vindex, weight, false);
            return broken || roadBlocks == 2;
        }

        public bool testIfMeepleCantBePlacedDirection(int Vindex, TileScript.Geography weight, Direction direction)
        {
            roadBlocks = 0;
            broken = false;
            counter = 0;
            visited = new bool[85];
            dfsDirection(Vindex, weight, direction, false);
            return broken || roadBlocks == 2;
            ;
        }

        public int startDfs(int Vindex, TileScript.Geography weight, bool GameEnd)
        {
            counter = 1;
            roadBlocks = 0;
            finalScore = 0;
            visited = new bool[85];
            Debug.Log("StartDFS");
            dfs(Vindex, weight, GameEnd);
            //Debug.Log(finalScore);
            if (weight == TileScript.Geography.City) return counter;
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
        public int startDfsDirection(int Vindex, TileScript.Geography weight, Direction direction, bool GameEnd)
        {
            counter = 0;
            roadBlocks = 0;
            //if (weight == TileScript.geography.Road) roadBlocks = 1;
            finalScore = 0;
            visited = new bool[85];

            dfsDirection(Vindex, weight, direction, GameEnd);
            //Debug.Log(finalScore);
            //Temporary fix
            //if (counter > 2)
            //{
            //    counter--;
            //}
            if (weight == TileScript.Geography.City) return counter;
            //Debug.Log("final score: " + finalScore);
            return finalScore;
        }

        private void dfsDirection(int Vindex, TileScript.Geography weight, Direction direction, bool GameEnd)
        {
            if (!visited[Vindex])
            {
                counter++;
                visited[Vindex] = true;

                var neighbours = g.getNeighbours(Vindex, weight, direction);

                if (weight == TileScript.Geography.Road) Debug.Log(weight + " : " + neighbours.Count);

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
                        placeVertex(vertexIterator++, new[] {Vindex}, new[] {weight}, TileScript.Geography.Field,
                            new[] {TileScript.Geography.Field}, new[] {direction});
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

                    if (weight == TileScript.Geography.Road)
                        if (neighbours.ElementAt(i).center == TileScript.Geography.Village ||
                            neighbours.ElementAt(i).center == TileScript.Geography.Cloister ||
                            neighbours.ElementAt(i).center == TileScript.Geography.City)
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

                    if (neighbours.ElementAt(i).center == TileScript.Geography.Village ||
                        neighbours.ElementAt(i).center == TileScript.Geography.Field)
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

        private void dfs(int Vindex, TileScript.Geography weight, bool GameEnd)
        {
            if (!visited[Vindex])
            {
                if (weight == TileScript.Geography.Road)
                {
                    counter++;
                    Debug.Log("Hit Road " + counter);
                }
                else if (weight == TileScript.Geography.City)
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
                        if (weight == TileScript.Geography.Road)
                        {
                            if (!visited[neighbours.ElementAt(i).endVertex]) broken = true;
                        }
                        else
                        {
                            broken = true;
                        }
                    }

                    if (weight == TileScript.Geography.Road)
                        if (neighbours.ElementAt(i).center == TileScript.Geography.Village ||
                            neighbours.ElementAt(i).center == TileScript.Geography.Cloister ||
                            neighbours.ElementAt(i).center == TileScript.Geography.City ||
                            neighbours.ElementAt(i).center == TileScript.Geography.RoadStream)
                        {
                            roadBlocks++;
                            if (roadBlocks == 2) finalScore = counter;
                        }

                    if (neighbours.ElementAt(i).center == TileScript.Geography.Village ||
                        neighbours.ElementAt(i).center == TileScript.Geography.Field)
                        counter++;
                    else
                        dfs(neighbours.ElementAt(i).endVertex, weight, GameEnd);
                }
            }

            if (GameEnd) finalScore = counter;
        }

        public void placeVertex(int Vindex, int[] Vindexes, TileScript.Geography[] weights,
            TileScript.Geography startCenter, TileScript.Geography[] endCenters, Direction[] directions)
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

            public static Direction getReverseDirection(Direction direction)
            {
                Direction res;
                switch (direction)
                {
                    case Direction.EAST:
                        res = Direction.WEST;
                        break;
                    case Direction.WEST:
                        res = Direction.EAST;
                        break;
                    case Direction.NORTH:
                        res = Direction.SOUTH;
                        break;
                    case Direction.SOUTH:
                        res = Direction.NORTH;
                        break;
                    default:
                        res = Direction.NORTH;
                        break;
                }

                return res;
            }

            public void addEdge(int startVertex, int endVertex, TileScript.Geography weight,
                TileScript.Geography startCenter, TileScript.Geography endCenter, Direction direction)
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


            public LinkedList<Edge> getNeighbours(int Vindex, TileScript.Geography weight)
            {
                var neighbours = new LinkedList<Edge>();
                if (weight == TileScript.Geography.Road)
                    for (var i = 0; i < graph.ElementAt(Vindex).Count; i++)
                        if (graph.ElementAt(Vindex).ElementAt(i).weight == weight)
                            neighbours.AddLast(graph.ElementAt(Vindex).ElementAt(i));
                if (weight == TileScript.Geography.City)
                    for (var i = 0; i < graph.ElementAt(Vindex).Count; i++)
                        if (graph.ElementAt(Vindex).ElementAt(i).weight == weight)
                            neighbours.AddLast(graph.ElementAt(Vindex).ElementAt(i));
                return neighbours;
            }

            public LinkedList<Edge> getNeighbours(int Vindex, TileScript.Geography weight, Direction direction)
            {
                var neighbours = new LinkedList<Edge>();
                if (weight == TileScript.Geography.Road || weight == TileScript.Geography.City)
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
            public TileScript.Geography center;
            public Direction direction;
            public int endVertex;
            public bool hasMeeple;
            public TileScript.Geography weight;

            public Edge(int endVertex, TileScript.Geography weight, TileScript.Geography center, Direction direction)
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