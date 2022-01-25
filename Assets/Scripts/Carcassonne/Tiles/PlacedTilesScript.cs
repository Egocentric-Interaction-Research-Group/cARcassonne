using System;
using Carcassonne.Controllers;
using Carcassonne.State;
using Carcassonne.State.Features;
using UnityEngine;

namespace Carcassonne.Tiles
{
    /// <summary>
    /// Class encapsulating information about tiles that have been played on the board.
    /// </summary>
    public class PlacedTilesScript : MonoBehaviour
    {
        public GameState state;
        public TileState tiles => state.Tiles;
        public FeatureState features => state.Features;

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