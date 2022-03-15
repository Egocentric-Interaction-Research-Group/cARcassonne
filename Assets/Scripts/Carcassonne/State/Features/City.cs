using System;
using System.Collections.Generic;
using System.Linq;
using Carcassonne.Models;
using UnityEngine;
using QuikGraph;

namespace Carcassonne.State.Features
{
    using CarcassonneEdge = TaggedUndirectedEdge<SubTile, ConnectionType>;
    using BoardGraphFilter = Func<IEnumerable<TaggedUndirectedEdge<SubTile, ConnectionType>>, IEnumerable<TaggedUndirectedEdge<SubTile, ConnectionType>>>;
    
    public class City : FeatureGraph
    {
        public int Shields => Vertices.Select(v=> v.tile).Distinct().Count(t=> t.Shield);

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
        
        public override int Points => getPoints();

        private int getPoints()
        {
            int points = Segments + Shields;
            
            return Complete ? points * 2 : points;
        }
        
        public override int PotentialPoints => getPotentialPoints();

        private int getPotentialPoints()
        {
            int points = Segments + Shields;

            return points * 2;
        }

    }
}