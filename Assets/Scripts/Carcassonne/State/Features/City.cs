using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using QuikGraph;

namespace Carcassonne.State.Features
{
    public class CarcassonneVertex : IComparable<CarcassonneVertex>
    {
        public Vector2Int location;
        public TileScript tile; // What if we allow null tiles where there is an open edge?
        [CanBeNull] public MeepleScript meeple;

        public CarcassonneVertex(Vector2Int location, TileScript tile, [CanBeNull] MeepleScript meeple = null)
        {
            this.location = location;
            this.tile = tile;
            this.meeple = meeple;
        }

        public int CompareTo(CarcassonneVertex other)
        {
            // Check to see if these are the same vertices
            if (location == other.location)
            {
                if(tile == other.tile && meeple == other.meeple) return 0;
                
                throw new ArgumentException(
                    $"Vertices have the same location ({location}), but are the same tile/meeple. Different tiles/meeples cannot be at the same location.",
                    tile == other.tile ? "CarcassonneVertex.meeple" : "CarcassonneVertex.tile");
            }
            
            // If they are different, sort by distance from origin
            return (location.magnitude - other.location.magnitude) > 0 ? 1 : -1;
        }
    }
    
    public class City : IFeature
    {
        // Does it make more sense just to have one graph for the whole board and have parts disconnected from each other?
        public UndirectedGraph<CarcassonneVertex, UndirectedEdge<CarcassonneVertex>> positions =
            new UndirectedGraph<CarcassonneVertex, UndirectedEdge<CarcassonneVertex>>();

        public int Segments => positions.VertexCount;
        public int OpenSides => Complete ? 0 : ComputeOpenSides();
        public int Shields => positions.Vertices.Count(v => v.tile.Shield);
        public bool Complete => complete;
        // public bool Completable => IsCompletable();
        public Dictionary<PlayerScript, int> Meeples => CountMeeplesForPlayers();
        
        public RectInt BoundingBox => CalculateBounds();

        public bool Contains(Vector2Int xy) => positions.Vertices.Any(v => v.location == xy);

        private bool complete = false;

        private int ComputeOpenSides()
        {
            var openSides = 0;

            foreach (var position in positions.Vertices)
            {
                // Calculate the number of sides of the tile that are city
                var citySides = position.tile.Sides.Count(s => s.Value == TileScript.Geography.City);
                // Subtract the number of connected sides from the number of city sides and add to overall open side count.
                openSides += citySides - positions.AdjacentDegree(position);
            }
            
            if (openSides == 0)
            {
                complete = true;
            }

            return openSides;
        }

        // private bool IsCompletable()
        // {
        //     if (Complete) return true;
        //     throw new System.NotImplementedException();
        // }

        private Dictionary<PlayerScript, int> CountMeeplesForPlayers()
        {
            var meeplesForPlayer = new Dictionary<PlayerScript, int>();

            foreach (var position in positions.Vertices.Where(v => v.meeple))
            {
                var player = position.meeple.player;
                if (!meeplesForPlayer.ContainsKey(player))
                {
                    meeplesForPlayer.Add(player, 0);
                }

                meeplesForPlayer[player] += 1;
            }

            return meeplesForPlayer;
        }

        public void Add(Vector2Int xy, TileScript t)
        {
            Add(xy, t, null);
        }

        public void Add(Vector2Int xy, TileScript t, [CanBeNull] MeepleScript m)
        {
            var position = new CarcassonneVertex(xy, t, m);
            
            // Add the vertex
            positions.AddVertex(position);

            // var clockwiseTiles = new[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
            // for (var i = 0; i < clockwiseTiles.Length; i++) // For each side
            // {
            //     // Check if (the side is a city) and (there is a tile on that side)
            //     if( (t.Sides[i] == TileScript.Geography.City) && (positions.Vertices.Any(p => p.location == (position.location + clockwiseTiles[i]))) )
            //     {
            //         UndirectedEdge<CarcassonneVertex> e = new UndirectedEdge<CarcassonneVertex>(position, positions.Vertices.Single(p => p.location == (position.location + clockwiseTiles[i])));
            //         positions.AddEdge(e);
            //     }
            // }

            foreach (var side in t.Sides)
            {
                var dir = side.Key;
                var geo = side.Value;

                if (geo == TileScript.Geography.City &&
                    positions.Vertices.Any(p => p.location == (position.location + dir)))
                {
                    UndirectedEdge<CarcassonneVertex> e = City.EdgeBetween(position,
                        positions.Vertices.Single(p => p.location == (position.location + dir)));
                    positions.AddEdge(e);
                }
            }
        }

        private RectInt CalculateBounds()
        {
            RectInt b = new RectInt();

            foreach (var position in positions.Vertices)
            {
                if (b.size == Vector2Int.zero)
                {
                    b.position = position.location;
                    b.size = Vector2Int.one;
                }

                if (position.location.x <  b.xMin){ b.xMin = position.location.x;}
                if (position.location.x >= b.xMax){ b.xMax = position.location.x + 1;}
                if (position.location.y <  b.yMin){ b.yMin = position.location.y;}
                if (position.location.y >= b.yMax){ b.yMax = position.location.y + 1;}
            }
            
            return b;
        }

        /// <summary>
        /// Overload the plus operator to allow merge of two cities.
        /// The two cities should already contain a common CarcassonneVertex when they are merged.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        /// <example>
        /// City a = new City();
        /// City b = new City();
        /// // Add tiles (and meeples) to cities.
        /// a += b;
        /// </example>
        public static City operator +(City a, City b)
        {   
            var c = new City();
            c.positions.AddVerticesAndEdgeRange(a.positions.Edges);
            c.positions.AddVerticesAndEdgeRange(b.positions.Edges);
            
            // update Complete status
            c.ComputeOpenSides();
            
            return c;
        }

        /// <summary>
        /// Automates the creation of undirected edges between two verticies.
        /// Undirected edges need to go from a lesser vertex to a greater one (via CompareTo), so this sorts the
        /// vertices and returns a valid edge. 
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        private static UndirectedEdge<CarcassonneVertex> EdgeBetween(CarcassonneVertex a, CarcassonneVertex b)
        {
            if (a.CompareTo(b) > 0)
            {
                return new UndirectedEdge<CarcassonneVertex>(b, a);
            }

            return new UndirectedEdge<CarcassonneVertex>(a, b);
        }
    }
}