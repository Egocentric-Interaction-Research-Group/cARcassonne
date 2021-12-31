using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using QuikGraph;
using QuikGraph.Algorithms.ConnectedComponents;

namespace Carcassonne.State.Features
{
    using CarcassonneEdge = TaggedUndirectedEdge<SubTile, ConnectionType>;
    using BoardGraphFilter = Func<IEnumerable<TaggedUndirectedEdge<SubTile, ConnectionType>>, IEnumerable<TaggedUndirectedEdge<SubTile, ConnectionType>>>;
    
    public class City : CarcassonneGraph, IFeature
    {
        public int Segments => FeatureEdges.Count() - IntraTileFeatureConnections;
        public int OpenSides => ComputeOpenSides();

        public int Shields => Vertices.Select(v=> v.tile).Distinct().Count(t=> t.Shield);
        public bool Complete => OpenSides == 0;
        // public bool Completable => IsCompletable();

        /// <summary>
        /// Get the bounding box for the city in the SubTile Coordinate system.
        /// </summary>
        // public RectInt Bounds => CalculateBounds();

        // public bool Contains(Vector2Int xy) => Vertices.Any(v => v.location == xy);

        private int IntraTileFeatureConnections =>
            Edges.Count(e => e.Tag == ConnectionType.Feature && e.Source.tile == e.Target.tile);

        private IEnumerable<CarcassonneEdge> FeatureEdges => Edges.Where(e => e.Tag == ConnectionType.Feature);
        private IEnumerable<CarcassonneEdge> BoardEdges => Edges.Where(e => e.Tag == ConnectionType.Board);

        /// <summary>
        /// Compute the number of open sides as the number of vertices that do not have Board Edges attached to them.
        /// </summary>
        /// <returns></returns>
        private int ComputeOpenSides()
        {
            var sourceBoardVertices = BoardEdges.Select(e => e.Source);
            var targetBoardVertices = BoardEdges.Select(e => e.Target);

            var boardVertices = Enumerable.Concat(sourceBoardVertices, targetBoardVertices);

            return boardVertices.Distinct().Count();
        }

        // private bool IsCompletable()
        // {
        //     if (Complete) return true;
        //     throw new System.NotImplementedException();
        // }

        // private Dictionary<PlayerScript, int> CountMeeplesForPlayers()
        // {
        //     var meeplesForPlayer = new Dictionary<PlayerScript, int>();
        //
        //     foreach (var position in positions.Vertices.Where(v => v.meeple))
        //     {
        //         var player = position.meeple.player;
        //         if (!meeplesForPlayer.ContainsKey(player))
        //         {
        //             meeplesForPlayer.Add(player, 0);
        //         }
        //
        //         meeplesForPlayer[player] += 1;
        //     }
        //
        //     return meeplesForPlayer;
        // }

        // public void Add(Vector2Int xy, TileScript t)
        // {
        //     Add(xy, t, null);
        // }
        //
        // public void Add(Vector2Int xy, TileScript t, [CanBeNull] MeepleScript m)
        // {
        //     var position = new CarcassonneVertex(xy, t, m);
        //     
        //     // Add the vertex
        //     positions.AddVertex(position);
        //
        //     // var clockwiseTiles = new[] { Vector2Int.up, Vector2Int.right, Vector2Int.down, Vector2Int.left };
        //     // for (var i = 0; i < clockwiseTiles.Length; i++) // For each side
        //     // {
        //     //     // Check if (the side is a city) and (there is a tile on that side)
        //     //     if( (t.Sides[i] == TileScript.Geography.City) && (positions.Vertices.Any(p => p.location == (position.location + clockwiseTiles[i]))) )
        //     //     {
        //     //         UndirectedEdge<CarcassonneVertex> e = new UndirectedEdge<CarcassonneVertex>(position, positions.Vertices.Single(p => p.location == (position.location + clockwiseTiles[i])));
        //     //         positions.AddEdge(e);
        //     //     }
        //     // }
        //
        //     foreach (var side in t.Sides)
        //     {
        //         var dir = side.Key;
        //         var geo = side.Value;
        //
        //         if (geo == TileScript.Geography.City &&
        //             positions.Vertices.Any(p => p.location == (position.location + dir)))
        //         {
        //             UndirectedEdge<CarcassonneVertex> e = City.EdgeBetween(position,
        //                 positions.Vertices.Single(p => p.location == (position.location + dir)));
        //             positions.AddEdge(e);
        //         }
        //     }
        // }

        // private RectInt CalculateBounds()
        // {
        //     RectInt b = new RectInt();
        //
        //     foreach (var subTile in Vertices)
        //     {
        //         if (b.size == Vector2Int.zero)
        //         {
        //             b.position = subTile.location;
        //             b.size = Vector2Int.one;
        //         }
        //
        //         if (subTile.location.x <  b.xMin){ b.xMin = subTile.location.x;}
        //         if (subTile.location.x >= b.xMax){ b.xMax = subTile.location.x + 1;}
        //         if (subTile.location.y <  b.yMin){ b.yMin = subTile.location.y;}
        //         if (subTile.location.y >= b.yMax){ b.yMax = subTile.location.y + 1;}
        //     }
        //     
        //     return b;
        // }
        
        public static BoardGraphFilter CityFilter = edges =>
            edges.Where(e =>
            {
                var featureEdge = e.Tag == ConnectionType.Feature;
                var cityVertices = e.Source.geography == Geography.City &&
                                   e.Target.geography == Geography.City;
                return featureEdge && cityVertices;
            });

        public static List<City> FromBoardGraph(BoardGraph bg)
        {
            var cities = new List<City>();
            
            FilteredConnectedComponentsAlgorithm<SubTile, CarcassonneEdge> connectedComponents =
                new FilteredConnectedComponentsAlgorithm<SubTile, CarcassonneEdge>(bg, CityFilter);
            
            // This should still contain non-city nodes, they should just be isolated.
            connectedComponents.Compute();
            // Filter non-City subtiles here.
            var components = connectedComponents.Components.Where(kvp => kvp.Key.geography == Geography.City);
            
            var cityVertices = components.ToLookup(p => p.Value, p=>p.Key);

            foreach (var cityVertexGroup in cityVertices)
            {
                var city = City.CityFromVertexList(bg, cityVertices[cityVertexGroup.Key]);
                cities.Add(city);
            }

            return cities;
        }
    
        public static City CityFromVertexList(BoardGraph bg, IEnumerable<SubTile> vertices)
        {
            City city = new City();
            
            Debug.Assert(vertices.All(v=> v.geography == Geography.City));

            city.AddVertexRange(vertices);

            // Edges that are between two vertices that are part of this city and are Feature/Board edges.
            var tileEdges = bg.Edges.Where(e => e.Tag != ConnectionType.Tile &&
                                city.Vertices.Contains(e.Source) && city.Vertices.Contains(e.Target));
            city.AddEdgeRange(tileEdges);

            return city;
        }
    }
}