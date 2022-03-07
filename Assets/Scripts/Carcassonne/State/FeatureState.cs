using System;
using System.Collections.Generic;
using System.Linq;
using Carcassonne.Models;
using Carcassonne.State.Features;
using UnityEngine;

namespace Carcassonne.State
{
    public class FeatureState
    {
        public MeepleState Meeples;
        
        public BoardGraph Graph;
        public List<City> Cities;
        public List<Road> Roads;
        public List<Cloister> Cloisters;

        public GridMapper grid;

        public IEnumerable<FeatureGraph> All => new List<FeatureGraph>().Concat(Cities).Concat(Roads).Concat(Cloisters);
        public IEnumerable<FeatureGraph> Complete => All.Where(f => f.Complete);
        public IEnumerable<FeatureGraph> Incomplete => All.Where(f => !f.Complete);


        public FeatureState(MeepleState meeples, GridMapper grid)
        {
            Meeples = meeples;
            
            Cities = new List<City>();
            Roads = new List<Road>();
            Cloisters = new List<Cloister>();
            Graph = new BoardGraph();
            
            Graph.Changed += UpdateFeatures;
            
            this.grid = grid;
        }


        /// <summary>
        /// Gets the feature at the location on the baord. The location does not necessarily map to a vertex.
        /// It can be a corner or a centre piece of a tile that is not vertex-mapped.
        /// </summary>
        /// <param name="location">The subtile location</param>
        /// <returns></returns>
        public FeatureGraph GetFeatureAt(Vector2Int location)
        {
            (var position, var direction) = grid.MeepleToTileDirection(location);
            
            Debug.Assert(location == grid.TileToMeeple(position, direction), 
                $"Subtile Location {location} converted to Board coordinates is {position} and {direction}," +
                $"but this converts back to {grid.TileToMeeple(position, direction)}");

            return GetFeatureAt(position, direction);
        }

        public FeatureGraph GetFeatureAt(Vector2Int position, Vector2Int direction)
        {
            var location = grid.TileToMeeple(position, direction);
            var feature = All.SingleOrDefault(c =>
                c.Vertices.Count(v => v.location == location) == 1);

            if (feature != null)
            {
                return feature;
            }
            
            // Handle centre roads/cities and corner cities
            var subtileUp = Graph.Vertices.Single(t=> 
                t.location == grid.TileToMeeple(position, direction + Vector2Int.up)); 
            var tile = subtileUp.tile;
            var geography = tile.GetGeographyAt(direction);

            if (geography.HasCityOrRoad())
            {
                var newDirection = tile.Sides.First(kvp => kvp.Value == geography.Simple()).Key;
                var newLocation = grid.TileToMeeple(position, newDirection);
                feature = All.SingleOrDefault(c =>
                    c.Vertices.Count(v => v.location == newLocation) == 1);
            }

            return feature;
        }

        /// <summary>
        /// An enumerable list of cities that are complete, but still have meeples registered on them. These
        /// have not been processed for points and need to be 
        /// </summary>
        public IEnumerable<FeatureGraph> CompleteWithMeeples => GetCompleteWithMeeples();

        private IEnumerable<FeatureGraph> GetCompleteWithMeeples()
        {
            // Subtile placement dictionary of meeples in complete features
            var subtileMeeples = Meeples.Placement.Where(pm => GetFeatureAt(pm.Key).Complete);
            
            // Features for those Meeples
            var features = subtileMeeples.Select(pm => GetFeatureAt(pm.Key));
            
            return features.Distinct();
        }
        
        public void UpdateFeatures(object sender, BoardChangedEventArgs args)
        {
            BoardGraph graph = args.graph;
            Cities = City.FromBoardGraph(graph);
            Roads = Road.FromBoardGraph(graph);
            Cloisters = Cloister.FromBoardGraph(graph);

            string debugString = "Cities: \n\n";
            foreach (var city in Cities)
            {
                debugString += city.ToString();
                debugString += "\n";
                debugString += $"Segments: {city.Segments}, Open Sides: {city.OpenSides}, Complete: {city.Complete}";
                debugString += "\n\n";
            }
            Debug.Log(debugString);
            
            debugString = "Roads: \n\n";
            foreach (var road in Roads)
            {
                debugString += road.ToString();
                debugString += "\n";
                debugString += $"Segments: {road.Segments}, Open Sides: {road.OpenSides}, Complete: {road.Complete}";
                debugString += "\n\n";
            }
            Debug.Log(debugString);
            
            debugString = "Cloisters: \n\n";
            foreach (var cloister in Cloisters)
            {
                debugString += cloister.ToString();
                debugString += "\n";
                debugString += $"Segments: {cloister.Segments}, Open Sides: {cloister.OpenSides}, Complete: {cloister.Complete}";
                debugString += "\n\n";
            }
            Debug.Log(debugString);
        }
    }
}