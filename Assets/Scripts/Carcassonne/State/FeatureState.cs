using System;
using System.Collections.Generic;
using System.Linq;
using Carcassonne.State.Features;
using Carcassonne.Tiles;
using Carcassonne.Utilities;
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

        public IEnumerable<FeatureGraph> All => new List<FeatureGraph>().Concat(Cities).Concat(Roads).Concat(Cloisters);
        public IEnumerable<FeatureGraph> Complete => All.Where(f => f.Complete);
        public IEnumerable<FeatureGraph> Incomplete => All.Where(f => !f.Complete);


        public FeatureState(MeepleState meeples)
        {
            Meeples = meeples;
            
            Cities = new List<City>();
            Roads = new List<Road>();
            Cloisters = new List<Cloister>();
            Graph = new BoardGraph();
        }


        /// <summary>
        /// Gets the feature at the location on the baord. The location does not necessarily map to a vertex.
        /// It can be a corner or a centre piece of a tile that is not vertex-mapped.
        /// </summary>
        /// <param name="location">The subtile location</param>
        /// <returns></returns>
        public FeatureGraph GetFeatureAt(Vector2Int location)
        {
            (var position, var direction) = Coordinates.SubTileToBoard(location);
            
            Debug.Assert(location == Coordinates.TileToSubTile(position, direction), 
                $"Subtile Location {location} converted to Board coordinates is {position} and {direction}," +
                $"but this converts back to {Coordinates.TileToSubTile(position, direction)}");

            return GetFeatureAt(position, direction);
        }

        public FeatureGraph GetFeatureAt(Vector2Int position, Vector2Int direction)
        {
            var location = Coordinates.TileToSubTile(position, direction);
            var feature = All.SingleOrDefault(c =>
                c.Vertices.Count(v => v.location == location) == 1);

            if (feature != null)
            {
                return feature;
            }
            
            // Handle centre roads/cities and corner cities
            var subtileUp = Graph.Vertices.Single(t=> 
                t.location == Coordinates.TileToSubTile(position, direction + Vector2Int.up)); 
            var tile = subtileUp.tile;
            var geography = tile.getGeographyAt(direction);


            if (geography.HasCityOrRoad())
            {
                var newDirection = tile.Sides.First(kvp => kvp.Value == geography.Simple()).Key;
                var newLocation = Coordinates.TileToSubTile(position, newDirection);
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
            var subtileMeeples = Meeples.SubTilePlacement.Where(pm => GetFeatureAt(pm.Key).Complete);
            
            // Features for those Meeples
            var features = subtileMeeples.Select(pm => GetFeatureAt(pm.Key));
            
            return features.Distinct();
        }
    }
}