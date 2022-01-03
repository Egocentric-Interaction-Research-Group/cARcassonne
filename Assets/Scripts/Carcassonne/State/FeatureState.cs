using System;
using System.Collections.Generic;
using System.Linq;
using Carcassonne.State.Features;
using Photon.Pun;
using UnityEngine;

namespace Carcassonne.State
{
    [CreateAssetMenu(fileName = "FeatureState", menuName = "States/FeatureState")]
    public class FeatureState : ScriptableObject
    {
        public MeepleState Meeples;
        
        public BoardGraph Graph = new BoardGraph();

        public List<City> Cities = new List<City>();
        // public List<Road> roads;
        // public List<Chapel> chapels;

        public IEnumerable<FeatureGraph> All => new List<FeatureGraph>().Concat(Cities);

        private void Awake()
        {
            Cities = new List<City>();
            Graph = new BoardGraph();
        }

        /// <summary>
        /// 
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
            // Features in the middle of a tile are not NECESSARILY captured in the graph representation. Special processing required.
            if (direction == Vector2Int.zero)
            {
                // The subtile at the north side of the centre position in question
                SubTile subtileUp = Graph.Vertices.SingleOrDefault(t =>
                    t.location == Coordinates.TileToSubTile(position, direction + Vector2Int.up)); 
                TileScript tile = subtileUp.tile;
                
                // If it is a road or city, get the direction of a subtile from a connected edge of the same feature
                if (tile.Center.HasCityOrRoad())
                {
                    var geography = tile.Center.Simple();
                    // Get a direction that has the same geography (and is therefore connected to) the centre
                    direction = tile.Sides.First(kvp => kvp.Value == geography).Key;
                }
                else if (!tile.Center.IsFeature())
                {
                    return null;
                }
            }

            var city = Cities.SingleOrDefault(c =>
                c.Vertices.Count(v => v.location == Coordinates.TileToSubTile(position, direction)) == 1);
            // var road
            // var cloister

            return new FeatureGraph[] { city }.SingleOrDefault(f => f != null);
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