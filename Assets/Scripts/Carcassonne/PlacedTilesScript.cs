using System;
using System.Collections.Generic;
using System.Linq;
using Carcassonne.State;
using Carcassonne.State.Features;
using UnityEngine;

namespace Carcassonne
{
    /// <summary>
    /// Class encapsulating information about tiles that have been played on the board.
    /// </summary>
    public class PlacedTilesScript : MonoBehaviour
    {
        public Vector3 BasePosition;

        public TileState tiles;
        public FeatureState features;

        private void Start()
        {
        }

        public void InstansiatePlacedTilesArray()
        {
            tiles.Played = new TileScript[170, 170];
        }

        public void PlaceTile(int x, int z, GameObject tile)
        {
            var ts = tile.GetComponent<TileScript>();
            var pos = new Vector2Int(x, z);
            // Update the Tile State
            tiles.Played[x, z] = ts;
            tiles.lastPlayedPosition = new Vector2(x, z);
            
            // Update Feature States
            
            // Cities
            var cities = new List<City>();
            foreach (var side in ts.Sides)
            {
                var dir = pos + side.Key;
                var geo = side.Value;
                
                // If there's a city linked to an existing tile, add this tile to that city.
                if (geo == TileScript.Geography.City && tiles.Played[dir.x, dir.y] != null)
                {
                    try
                    {
                        var c = features.Cities.Single(c => c.Contains(dir));
                        if (!c.Contains(pos)) // As long as the current tile has not already been added to the city.
                        {
                            c.Add(pos, ts);
                            cities.Add(c);
                        }
                    }
                    catch (ArgumentNullException e) {}
                }
            }

            // If the tile has linked multiple cities.
            if (cities.Count > 1)
            {
                var c = new City();
                // Add all of the newly connected cities together, remove them from the list, and add the newly created city.
                foreach (var city in cities)
                {
                    c += city;
                    features.Cities.Remove(city);
                }
                features.Cities.Add(c);
            } else if (cities.Count == 0 && ts.Sides.Any(s => s.Value == TileScript.Geography.City))
            { // If there are city parts to the tile, but no cities have been linked, create a new city
                var c = new City();
                c.Add(pos, ts);
                features.Cities.Add(c);
            }
        }

        //FIXME This should be changable to a TileScript return type
        public GameObject GetPlacedTile(int x, int z)
        {
            var t = tiles.Played[x, z];
            if (t is null)
            {
                return null;
            }

            return t.gameObject;
        }

        /// <summary>
        /// Checks whether there are any tiles adjacent to the given position.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        /// <returns></returns>
        public bool AnyAdjacentTiles(int x, int z)
        {
            if (x + 1 < tiles.Played.GetLength(0))
                if (tiles.Played[x + 1, z] != null)
                    return true;
            if (x - 1 >= 0)
                if (tiles.Played[x - 1, z] != null)
                    return true;
            if (z + 1 < tiles.Played.GetLength(1))
                if (tiles.Played[x, z + 1] != null)
                    return true;
            if (z - 1 >= 0)
                if (tiles.Played[x, z - 1] != null)
                    return true;
            return false;
        }

        private bool PositionIsInBounds(Vector2Int p)
        {
            return p.x >= 0 && p.x < tiles.Played.GetLength(0) &&
                   p.y >= 0 && p.y < tiles.Played.GetLength(1);
        }

        /// <summary>
        /// Tests whether a board position disqualifies a tile with a particular geography facing that position.
        /// Checks that the position has no tile OR a tile matching a particular geography in the given direction.
        /// Also returns true if the position is off the board (out of bounds).
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="dir"></param>
        /// <param name="geography"></param>
        /// <returns></returns>
        public bool DirectionIsEmptyOrMatchesGeography(int x, int y, Vector2Int dir, TileScript.Geography geography)
        {
            if (!PositionIsInBounds(new Vector2Int(x, y)))
                return true;
            
            if (tiles.Played[x, y] == null)
                return true;
            
            if (tiles.Played[x, y].getGeographyAt(dir) == geography)
                return true;
            
            return false;
        }

        public bool CityTileHasCityCenter(int x, int y)
        {
            return tiles.Played[x, y].getCenter() == TileScript.Geography.City ||
                   tiles.Played[x, y].getCenter() == TileScript.Geography.CityRoad;
        }

        public bool CityTileHasGrassOrStreamCenter(int x, int y)
        {
            return tiles.Played[x, y].getCenter() == TileScript.Geography.Field ||
                   tiles.Played[x, y].getCenter() == TileScript.Geography.Stream;
        }

        //Hämtar grannarna till en specifik tile
        public int[] GetNeighbors(int x, int y)
        {
            var Neighbors = new int[4];
            var itt = 0;


            if (tiles.Played[x + 1, y] != null)
            {
                Neighbors[itt] = tiles.Played[x + 1, y].vIndex;
                itt++;
            }

            if (tiles.Played[x - 1, y] != null)
            {
                Neighbors[itt] = tiles.Played[x - 1, y].vIndex;
                itt++;
            }

            if (tiles.Played[x, y + 1] != null)
            {
                Neighbors[itt] = tiles.Played[x, y + 1].vIndex;
                itt++;
            }

            if (tiles.Played[x, y - 1] != null) Neighbors[itt] = tiles.Played[x, y - 1].vIndex;
            return Neighbors;
        }

        public TileScript.Geography[] getWeights(int x, int y)
        {
            var weights = new TileScript.Geography[4];
            var itt = 0;
            if (tiles.Played[x + 1, y] != null)
            {
                weights[itt] = tiles.Played[x + 1, y].West;
                itt++;
            }

            if (tiles.Played[x - 1, y] != null)
            {
                weights[itt] = tiles.Played[x - 1, y].East;
                itt++;
            }

            if (tiles.Played[x, y + 1] != null)
            {
                weights[itt] = tiles.Played[x, y + 1].South;
                itt++;
            }

            if (tiles.Played[x, y - 1] != null) weights[itt] = tiles.Played[x, y - 1].North;
            return weights;
        }

        public TileScript.Geography[] getCenters(int x, int y)
        {
            var centers = new TileScript.Geography[4];
            var itt = 0;
            if (tiles.Played[x + 1, y] != null)
            {
                centers[itt] = tiles.Played[x + 1, y].getCenter();
                itt++;
            }

            if (tiles.Played[x - 1, y] != null)
            {
                centers[itt] = tiles.Played[x - 1, y].getCenter();
                itt++;
            }

            if (tiles.Played[x, y + 1] != null)
            {
                centers[itt] = tiles.Played[x, y + 1].getCenter();
                itt++;
            }

            if (tiles.Played[x, y - 1] != null) centers[itt] = tiles.Played[x, y - 1].getCenter();
            return centers;
        }

        public PointScript.Direction[] getDirections(int x, int y)
        {
            var directions = new PointScript.Direction[4];
            var itt = 0;
            if (tiles.Played[x + 1, y] != null)
            {
                directions[itt] = PointScript.Direction.EAST;
                itt++;
            }

            if (tiles.Played[x - 1, y] != null)
            {
                directions[itt] = PointScript.Direction.WEST;
                itt++;
            }

            if (tiles.Played[x, y + 1] != null)
            {
                directions[itt] = PointScript.Direction.NORTH;
                itt++;
            }

            if (tiles.Played[x, y - 1] != null) directions[itt] = PointScript.Direction.SOUTH;
            return directions;
        }

        public int CheckSurroundedCloister(int x, int z, bool endTurn)
        {
            var pts = 1;
            if (tiles.Played[x - 1, z - 1] != null) pts++;
            if (tiles.Played[x - 1, z] != null) pts++;
            if (tiles.Played[x - 1, z + 1] != null) pts++;
            if (tiles.Played[x, z - 1] != null) pts++;
            if (tiles.Played[x, z + 1] != null) pts++;
            if (tiles.Played[x + 1, z - 1] != null) pts++;
            if (tiles.Played[x + 1, z] != null) pts++;
            if (tiles.Played[x + 1, z + 1] != null) pts++;
            if (pts == 9 || endTurn)
                return pts;
            return 0;
        }

        public bool TilePlacementIsValid(GameObject tile, int x, int z)
        {
            return TilePlacementIsValid(tile.GetComponent<TileScript>(), x, z);
        }
        
        public bool TilePlacementIsValid(TileScript tile, int x, int z)
        {
            var r = new Vector2Int(x, z);
            
            // Check that there is no tile in that position
            if (tiles.Played[x, z] != null) return false;
            
            // Check that there is a matching neighbour
            bool hasNeigbour = false;
            foreach (var side in tile.Sides)
            {
                var dir = side.Key; // The direction (up/down/left/right) to check
                var geo = side.Value; // The geographic feature in that direction on the base tile
                var neighbour = dir + r;
                
                // Tracks whether there is at least one neighbour
                var neighbourIsInBounds = PositionIsInBounds(neighbour); // If neighbour is not in bounds, don't change hasNeighbour.
                if (!hasNeigbour && neighbourIsInBounds) hasNeigbour = tiles.Played[r.x + dir.x, r.y + dir.y] != null;

                // Check whether a direction is empty or matches the geography of the tile
                if (!DirectionIsEmptyOrMatchesGeography(neighbour.x, neighbour.y, -dir, geo)) return false;
            }
            
            // The sides are all empty or matches. Return whether there is a neighbour.
            return hasNeigbour;
        }

        public bool TileCanBePlaced(TileScript tile, GameControllerScript gameControllerScript)
        {
            for (var x = 0; x < tiles.Played.GetLength(0); x++)
            {
                for (var y = 0; y < tiles.Played.GetLength(1); y++)
                {
                    for (var k = 0; k < 4; k++)
                    {
                        if (TilePlacementIsValid(tile, x, y))
                        {
                            gameControllerScript.tileControllerScript.ResetTileRotation();
                            Debug.Log($"Found a valid position for tile {tile} (ID: {tile.id}) at ({x},{y}) with rotation {k}.");
                            return true;
                        }
                        
                        gameControllerScript.tileControllerScript.RotateTile();
                    }
                }
            }

            // Reset rotation to default because tile has been rotated in testing.
            gameControllerScript.tileControllerScript.ResetTileRotation();
            return false;
        }
    }
}