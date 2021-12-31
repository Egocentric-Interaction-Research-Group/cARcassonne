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
        public BoardGraph Graph = new BoardGraph();
        
        public List<City> Cities = new List<City>();
        // public List<Road> roads;
        // public List<Chapel> chapels;
        
        private void Awake()
        {
            Cities = new List<City>();
            Graph = new BoardGraph();
        }

        public CarcassonneGraph GetFeatureAt(Vector2Int position, Vector2Int direction)
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

            return new CarcassonneGraph[] { city }.SingleOrDefault(f => f != null);
        }
    }
}