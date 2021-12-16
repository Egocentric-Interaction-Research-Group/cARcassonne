using System;
using System.Linq;
using JetBrains.Annotations;
using QuikGraph;
using QuikGraph.Graphviz;
using QuikGraph.Graphviz.Dot;
using UnityEditorInternal.Profiling.Memory.Experimental;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UI;


namespace Carcassonne.State.Features
{
    // Type Aliases
    using CarcassonneEdge = TaggedUndirectedEdge<SubTile, ConnectionType>;
    using CarcassonneGraph = QuikGraph.UndirectedGraph<SubTile, TaggedUndirectedEdge<SubTile, ConnectionType>>;

    public enum ConnectionType
    {
        Tile,
        Board,
        Feature
    }
    public class SubTile : IComparable<SubTile>
        {
            public TileScript tile;
            public Vector2Int location;
            public TileScript.Geography geography;
            [CanBeNull] public MeepleScript meeple;

            /// <summary>
            /// Create a new SubTile
            /// </summary>
            /// <param name="tile">Reference to the TileScript</param>
            /// <param name="tilePosition">Position of the tile in board matrix space</param>
            /// <param name="direction">Direction of the SubTile space. Valid arguments are Vector2Int.[up|down|left|right|zero]</param>
            /// <param name="meeple">Reference to MeepleScript, if Meeple is present.</param>
            public SubTile(TileScript tile, Vector2Int tilePosition, Vector2Int direction, [CanBeNull] MeepleScript meeple = null)
            {
                this.tile = tile;
                this.location = tilePosition * 3 + Vector2Int.one + direction; // The centre of the tile is at the tile's position + [1,1] to leave room for the -1 movement.
                this.geography = tile.getGeographyAt(direction);
                // Debug.Log($"GEOGRAPHY 44: {tile} @ {direction} = {geography}");
                this.meeple = meeple;
            }

            public int CompareTo(SubTile other)
            {
                // Check to see if these are the same vertices
                if (location == other.location)
                {
                    if(tile == other.tile && geography == other.geography && meeple == other.meeple) return 0;
                
                    throw new ArgumentException(
                        $"Vertices have the same location ({location}), but have different tiles, geographies, or meeples." +
                        $"Only one SubTile may occupy a given location.");
                }
            
                // If they are different, sort by hash code
                //TODO Test this to make sure it works consistently.
                return (tile.GetHashCode() - other.tile.GetHashCode()) > 0 ? 1 : -1;
            }

        }

    public class BoardGraph : CarcassonneGraph
    {
        
        public RectInt Bounds => GetBounds();
        private RectInt GetBounds()
        {
            RectInt b = new RectInt();

            b.min = new Vector2Int(this.Vertices.Min(v => v.location.x), this.Vertices.Min(v => v.location.y));
            b.max = new Vector2Int(this.Vertices.Max(v => v.location.x), this.Vertices.Max(v => v.location.y));

            return b;
        }

        #region AddTiles

        /// <summary>
        /// Add two TileGraphs together. Uses the physical location of subtiles to find and link neighbours.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        public static BoardGraph operator +(BoardGraph a, BoardGraph b)
        {   
            var tg = new BoardGraph();
            tg.AddVerticesAndEdgeRange(a.Edges);
            tg.AddVerticesAndEdgeRange(b.Edges);
            
            // Search for adjacent vertices and add links
            foreach (var va in a.Vertices)
            {
                // Find all Subtiles that are adjacent to the vertex in graph a
                var toLink = b.Vertices.Where(vb => (va.location - vb.location).sqrMagnitude == 1);

                foreach (var subtile in toLink)
                {
                    Debug.Assert(va.geography == subtile.geography, $"Geographies ({va.geography}, {subtile.geography}) do not match.");

                    tg.AddEdge(EdgeBetween(va, subtile, ConnectionType.Board));

                    if (va.geography == TileScript.Geography.City || va.geography == TileScript.Geography.Road)
                    {
                        tg.AddEdge(EdgeBetween(va, subtile, ConnectionType.Feature));
                    }
                }
            }
            
            return tg;
        }

        #endregion


        /// <summary>
        /// Get a graph representation of the tile itself.
        /// </summary>
        /// <param name="tile"></param>
        /// <returns></returns>
        public static BoardGraph Get(TileScript tile,
            Vector2Int location)
        {
            return Get(tile, location, null);
        }
        
        public static BoardGraph Get(TileScript tile, Vector2Int location, [CanBeNull] MeepleScript meeple)
        {
            BoardGraph g = new BoardGraph();
    
            foreach (var side in tile.Sides)
            {
                // Add connection to neighbouring subtile
                var direction = side.Key;
                var geography = side.Value;
                // Debug.Log($"GEOGRAPHY 140: {tile} @ {direction} = {geography}");
                g = AddAndConnectSubTile(tile, location, meeple, direction, g, geography);
            }
            
            // Add a centre vertex IF it is a cloister
            if (tile.Center == TileScript.Geography.Cloister)
            {
                var direction = Vector2Int.zero;
                g = AddAndConnectSubTile(tile, location, meeple, direction, g, null);
                
                // Remove connections between NORTH/SOUTH and EAST/WEST. They are redundant now that the centre is connected.
                var edgesToRemove =
                    g.Edges.Where(e => (e.Source.location - e.Target.location).sqrMagnitude == 4);
                g.RemoveEdges(edgesToRemove);
            }

            return g;
        }

        private static BoardGraph AddAndConnectSubTile(TileScript tile, Vector2Int location, MeepleScript meeple,
            Vector2Int direction, BoardGraph g, TileScript.Geography? geography)
        {
            Debug.Assert( (TileScript.Geography.City & TileScript.Geography.CityRoad) == TileScript.Geography.City );
            Debug.Assert( (TileScript.Geography.Road & TileScript.Geography.CityRoad) == TileScript.Geography.Road );
            
            SubTile st = new SubTile(tile, location, direction);
            if (meeple != null && meeple.GetDirection() == direction) st.meeple = meeple;

            g.AddVertex(st);
            foreach (var v in g.Vertices.Where(v => v != st))
            {
                // Fully connect tile verticies
                g.AddEdge(EdgeBetween(v, st, ConnectionType.Tile));

                // If the vertices are of the same type (City or Road) AND they are connected by the centre, add a connection
                if (geography!= null && (geography == TileScript.Geography.City || geography == TileScript.Geography.Road)
                                     && geography == v.geography && (geography & tile.Center) == geography)
                {
                    g.AddEdge(EdgeBetween(v, st, ConnectionType.Feature));
                }
            }

            return g;
        }

        /// <summary>
        /// Automates the creation of undirected edges between two verticies.
        /// Undirected edges need to go from a lesser vertex to a greater one (via CompareTo), so this sorts the
        /// vertices and returns a valid edge. 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static CarcassonneEdge EdgeBetween(SubTile a, SubTile b, ConnectionType t)
        {
            if (a.CompareTo(b) > 0)
            {
                CarcassonneEdge e = new CarcassonneEdge(b, a, t);
                return new CarcassonneEdge(b, a, t);
            }

            return new CarcassonneEdge(a, b, t);
        }

        public override string ToString()
        {
            return this.ToGraphviz(algorithm =>
            {
                algorithm.FormatVertex += (sender, args) =>
                {
                    var p = args.Vertex.location - Bounds.min;
                    args.VertexFormat.Label = $"{args.Vertex.geography}";
                    args.VertexFormat.Position = new GraphvizPoint(p.x, p.y);
                    switch (args.Vertex.geography)
                    {
                        case TileScript.Geography.City:
                            args.VertexFormat.StrokeColor = GraphvizColor.Red;
                            break;
                        case TileScript.Geography.Road:
                            args.VertexFormat.StrokeColor = GraphvizColor.Black;
                            break;
                        case TileScript.Geography.Field:
                            args.VertexFormat.StrokeColor = GraphvizColor.Green;
                            break;
                        case TileScript.Geography.Cloister:
                            args.VertexFormat.StrokeColor = GraphvizColor.Blue;
                            break;
                    }
                };
                algorithm.FormatEdge += (sender, args) =>
                {
                    switch (args.Edge.Tag)
                    {
                        case ConnectionType.Board:
                            args.EdgeFormat.StrokeColor = GraphvizColor.Black;
                            break;
                        case ConnectionType.Feature:
                            args.EdgeFormat.StrokeColor = GraphvizColor.DeepPink;
                            break;
                        case ConnectionType.Tile:
                            args.EdgeFormat.StrokeColor = GraphvizColor.Aqua;
                            break;
                    }
                };
            });
        }
    }
}