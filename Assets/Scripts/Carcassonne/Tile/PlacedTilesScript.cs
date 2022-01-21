using System;
using Carcassonne.Controller;
using Carcassonne.State;
using Carcassonne.State.Features;
using UnityEngine;

namespace Carcassonne.Tile
{
    /// <summary>
    /// Class encapsulating information about tiles that have been played on the board.
    /// </summary>
    public class PlacedTilesScript : MonoBehaviour
    {
        public Vector3 BasePosition;

        public TileState tiles;
        public FeatureState features;

        public int Count => _count;
        private int _count;

        private void Start()
        {
            _count = 0;
        }

        [Obsolete("This function is obsolete. Just here for a consistency check now.", false)]
        public void PlacedTilesArrayIsEmptyCheck()
        {
            foreach (var t in tiles.Played)
            {
                Debug.Assert( t == null, $"All members of the array tiles.Played should be null at the beginning of the game, but {t} was found.");
            }
        }

        public void PlaceTile(int x, int z, GameObject tile)
        {
            var ts = tile.GetComponent<TileScript>();
            var pos = new Vector2Int(x, z);
            // Update the Tile State
            tiles.Placement.Add(pos, ts);
            tiles.lastPlayedPosition = pos;
            
            // Update Feature States
            features.Graph.Add(BoardGraph.FromTile(ts, pos) );

            _count++;
            
            Debug.Log($"{features.Graph}");
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
        public bool DirectionIsEmptyOrMatchesGeography(int x, int y, Vector2Int dir, Geography geography)
        {
            if (!PositionIsInBounds(new Vector2Int(x, y)))
                return true;
            
            if (tiles.Played[x, y] == null)
                return true;
            
            if (tiles.Played[x, y].getGeographyAt(dir) == geography)
                return true;
            
            return false;
        }
        
        // public bool CityTileHasGrassOrStreamCenter(int x, int y)
        // {
        //     return tiles.Played[x, y].getCenter() == Geography.Field ||
        //            tiles.Played[x, y].getCenter() == Geography.Stream;
        // }
        //
        // //Hämtar grannarna till en specifik tile
        // public int[] GetNeighbors(int x, int y)
        // {
        //     var Neighbors = new int[4];
        //     var itt = 0;
        //
        //
        //     if (tiles.Played[x + 1, y] != null)
        //     {
        //         Neighbors[itt] = tiles.Played[x + 1, y].vIndex;
        //         itt++;
        //     }
        //
        //     if (tiles.Played[x - 1, y] != null)
        //     {
        //         Neighbors[itt] = tiles.Played[x - 1, y].vIndex;
        //         itt++;
        //     }
        //
        //     if (tiles.Played[x, y + 1] != null)
        //     {
        //         Neighbors[itt] = tiles.Played[x, y + 1].vIndex;
        //         itt++;
        //     }
        //
        //     if (tiles.Played[x, y - 1] != null) Neighbors[itt] = tiles.Played[x, y - 1].vIndex;
        //     return Neighbors;
        // }
        //
        // public Geography[] getWeights(int x, int y)
        // {
        //     var weights = new Geography[4];
        //     var itt = 0;
        //     if (tiles.Played[x + 1, y] != null)
        //     {
        //         weights[itt] = tiles.Played[x + 1, y].West;
        //         itt++;
        //     }
        //
        //     if (tiles.Played[x - 1, y] != null)
        //     {
        //         weights[itt] = tiles.Played[x - 1, y].East;
        //         itt++;
        //     }
        //
        //     if (tiles.Played[x, y + 1] != null)
        //     {
        //         weights[itt] = tiles.Played[x, y + 1].South;
        //         itt++;
        //     }
        //
        //     if (tiles.Played[x, y - 1] != null) weights[itt] = tiles.Played[x, y - 1].North;
        //     return weights;
        // }
        //
        // public Geography[] getCenters(int x, int y)
        // {
        //     var centers = new Geography[4];
        //     var itt = 0;
        //     if (tiles.Played[x + 1, y] != null)
        //     {
        //         centers[itt] = tiles.Played[x + 1, y].getCenter();
        //         itt++;
        //     }
        //
        //     if (tiles.Played[x - 1, y] != null)
        //     {
        //         centers[itt] = tiles.Played[x - 1, y].getCenter();
        //         itt++;
        //     }
        //
        //     if (tiles.Played[x, y + 1] != null)
        //     {
        //         centers[itt] = tiles.Played[x, y + 1].getCenter();
        //         itt++;
        //     }
        //
        //     if (tiles.Played[x, y - 1] != null) centers[itt] = tiles.Played[x, y - 1].getCenter();
        //     return centers;
        // }
        //
        // public Vector2Int[] getDirections(int x, int y)
        // {
        //     var directions = new Vector2Int[4];
        //     var itt = 0;
        //     if (tiles.Played[x + 1, y] != null)
        //     {
        //         directions[itt] = Vector2Int.right;
        //         itt++;
        //     }
        //
        //     if (tiles.Played[x - 1, y] != null)
        //     {
        //         directions[itt] = Vector2Int.left;
        //         itt++;
        //     }
        //
        //     if (tiles.Played[x, y + 1] != null)
        //     {
        //         directions[itt] = Vector2Int.up;
        //         itt++;
        //     }
        //
        //     if (tiles.Played[x, y - 1] != null) directions[itt] = Vector2Int.down;
        //     return directions;
        // }
        //
        // public int CheckSurroundedCloister(int x, int z, bool endTurn)
        // {
        //     var pts = 1;
        //     if (tiles.Played[x - 1, z - 1] != null) pts++;
        //     if (tiles.Played[x - 1, z] != null) pts++;
        //     if (tiles.Played[x - 1, z + 1] != null) pts++;
        //     if (tiles.Played[x, z - 1] != null) pts++;
        //     if (tiles.Played[x, z + 1] != null) pts++;
        //     if (tiles.Played[x + 1, z - 1] != null) pts++;
        //     if (tiles.Played[x + 1, z] != null) pts++;
        //     if (tiles.Played[x + 1, z + 1] != null) pts++;
        //     if (pts == 9 || endTurn)
        //         return pts;
        //     return 0;
        // }

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