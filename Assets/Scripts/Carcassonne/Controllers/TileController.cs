using System;
using System.Collections.Generic;
using System.Linq;
using Carcassonne.State;
using UnityEngine;
using UnityEngine.Events;
using Carcassonne.Models;
using Carcassonne.State.Features;
using Random = UnityEngine.Random;

namespace Carcassonne.Controllers
{
    /// <summary>
    /// Maintains the state of a tile that is in play. Responsible for maintaining rotation and location of tile and
    /// setting the state when the tile is placed.
    /// </summary>
    public class TileController : GamePieceController<Tile>
    {
        // [SerializeField] internal StackScript stack;

        private GameState state => GetComponent<GameState>(); //null?
        private TileState tiles => state.Tiles;
        private Tile tile => state.Tiles.Current;

        /// <summary>
        /// Position of the current tile in board coordinates.
        /// </summary>
        // public Vector2Int position = new Vector2Int();
        // public int rotation = 0;
        // private bool _isstateNotNull;

        // Tile Spawn position has to be on a grid with the base tile.

        // #region Events
        //
        // public new UnityEvent<Tile> OnDraw = new UnityEvent<Tile>();
        // public new UnityEvent<Tile, int> OnRotate = new UnityEvent<Tile, int>();
        // public new UnityEvent<Tile> OnDiscard = new UnityEvent<Tile>();
        // public new UnityEvent<Tile, Vector2Int> OnPlace = new UnityEvent<Tile, Vector2Int>();
        //
        // #endregion
        
        #region UpdatedFunctions

        private void Awake()
        {
            Debug.Assert(state != null, "TileController: The state is null.");
            
            // Call Draw if a tile has to be discarded.
            OnDiscard.AddListener(delegate(Tile t) { Draw(); });
        }

        /// <summary>
        /// Draw a new tile.
        ///
        /// Pick a tile from the stack, set the current tile state, check if the tile is valid, set game state, 
        /// </summary>
        public override void Draw()
        {
            // TODO Should this check phase validity??? Probably, right?
            Debug.Log("Drawing new Tile.");
            // Debug.Assert(state.Tiles.Remaining.Count > 0, "TileController: The stack is empty.");
            
            // This can happen if Draw is called twice because of a Discard, for example.
            if (state.Tiles.Remaining.Count == 0){
                GetComponent<GameController>().GameOver();
                return;
            }

            // Get a new current tile
            state.Tiles.Current = state.Tiles.Remaining.Pop();
            Debug.Log($"Drew Tile id {tile.ID} ({tile})");
            
            if (!CanBePlaced())
            {
                Debug.Log($"Tile (ID: {tile.ID}) not possible to place: discarding and drawing a new one.");
                Discard();
                
                // Draw();
            }
            
            state.phase = Phase.TileDrawn;
            
            OnDraw.Invoke(tile);
        }

        public void PlaceFirst()
        {
            Debug.Assert(state != null, "TileController: The state is null.");
            
            var t = state.Tiles.Remaining.Pop();
            Debug.Log($"Starting tile: ({t.ID}) {t}");

            state.Tiles.Placement.Add(Vector2Int.zero, t);

            var bg = BoardGraph.FromTile(t, Vector2Int.zero, state.grid);
            Debug.Log($"Starting graph has {bg.VertexCount} vertices and {bg.EdgeCount} edges.");
            
            state.Features.Graph.Add(bg);

            Debug.Log($"First tile graph: {BoardGraph.FromTile(t, Vector2Int.zero, state.grid)}");
        }

        public override bool Place(Vector2Int cell)
        {
            if(state.phase != Phase.TileDrawn)
            {
                OnInvalidPlace.Invoke(tile, cell);
                return false;
            }
            
            if (!IsPlacementValid(cell))
            {
                OnInvalidPlace.Invoke(tile, cell);
                return false;
            }

            Debug.Log($"Placing tile {tile} at position {cell} with rotation {tile.Rotations}");
            
            state.Tiles.Placement.Add(cell, tile);
            state.Features.Graph.Add(BoardGraph.FromTile(tile, cell, state.grid));

            state.phase = Phase.TileDown;
            
            OnPlace.Invoke(tile, cell);
            return true;
        }

        public override void Discard()
        {
            //TODO: Is this right?
            // Destroy(tile);
            state.Tiles.Discarded.Add(tile);
            tile.gameObject.SetActive(false);
                
            OnDiscard.Invoke(tile);
        }

        /// <summary>
        /// Rotates the current tile by 90 degrees, r times.
        /// </summary>
        public void Rotate(int r = 1)
        {
            tile.Rotate(r);
            
            OnRotate.Invoke(tile, tile.Rotations);
        }
        
        public void RotateTo(int r)
        {
            Debug.Log($"Rotating to {r}");
            tile.RotateTo(r);
            
            OnRotate.Invoke(tile, tile.Rotations);
        }
        
        public override bool IsPlacementValid(Vector2Int cell)
        {
            var tile = state.Tiles.Current;
            
            // Check that there is no tile in that position
            if (CellIsOccupied(cell))
            {
                Debug.Log("Invalid placement: Occupied cell");
                return false;
            }
            
            // Check that there is a matching neighbour
            bool hasNeigbour = false;
            foreach (var side in tile.Sides)
            {
                var dir = side.Key; // The direction (up/down/left/right) to check
                var geo = side.Value; // The geographic feature in that direction on the base tile
                var neighbour = dir + cell;
                
                // Tracks whether there is at least one neighbour
                // var neighbourIsInBounds = PositionIsInBounds(neighbour); // If neighbour is not in bounds, don't change hasNeighbour.
                // if (!hasNeigbour && neighbourIsInBounds) hasNeigbour = tiles.Played[cell.x + dir.x, cell.y + dir.y] != null;
                hasNeigbour ^= tiles.Placement.ContainsKey(neighbour);

                // Check whether a direction is empty or matches the geography of the tile
                if (!DirectionIsEmptyOrMatchesGeography(neighbour, -dir, geo))
                {
                    Debug.Log($"Invalid placement: Non-matching geography at {dir}");
                    return false;
                }
            }
            
            // The sides are all empty or matches. Return whether there is a neighbour.
            if (!hasNeigbour)
            {
                Debug.Log("Invalid placement: No neighbours");
            }
            
            return hasNeigbour;
        }

        public bool CellIsOccupied(Vector2Int cell) => tiles.Placement.ContainsKey(cell);

        public override bool CanBePlaced()
        {
            // Log the cells that have been visited
            HashSet<Vector2Int> visitedTiles = new HashSet<Vector2Int>();
            
            // Check the cells adjacent to each placed tile
            foreach (var kvp in state.Tiles.Placement)
            {
                var c = kvp.Key;
                var t = kvp.Value;
                foreach (var side in Tile.Directions) // Every neighbouring cell to a placed tile
                {
                    var neighbour = c + side;
                    if (!visitedTiles.Contains(neighbour))
                    {
                        for (var rotation = 0; rotation < 4; rotation++)
                        {
                            if (IsPlacementValid(neighbour))
                            {
                                Debug.Log($"Found a valid position at ({neighbour.x},{neighbour.y}) with rotation {rotation}.");
                            
                                // Randomly rotate tile to not bias positioning
                                // tile.Rotate(Random.Range(0,4));
                                tile.RotateTo(0); //TODO Switch this once there is a way of syncing. 
                            
                                return true;
                            }
                        
                            tile.Rotate();
                        }
                        visitedTiles.Add(neighbour);
                    }
                }
            }
            
            // TODO: Re-implement with a list of open cells adjacent placed tiles.
            // for (var cell = new Vector2Int(); cell.x < tiles.Played.GetLength(0); cell.x++)
            // {
            //     for (cell.y = 0; cell.y < tiles.Played.GetLength(1); cell.y++)
            //     {
            //         for (var rotation = 0; rotation < 4; rotation++)
            //         {
            //             if (IsPlacementValid(cell))
            //             {
            //                 // tileControllerScript.ResetTileRotation();
            //                 Debug.Log($"Found a valid position at ({cell.x},{cell.y}) with rotation {rotation}.");
            //                 
            //                 // Randomly rotate tile to not bias positioning
            //                 tile.Rotate(Random.Range(0,4));
            //                 
            //                 return true;
            //             }
            //             
            //             tile.Rotate();
            //         }
            //     }
            // }

            Debug.LogWarning($"Tile ID {tile.ID} cannot be placed.");
            return false;
        }

        // private bool PositionIsInBounds(Vector2Int p)
        // {
        //     var inBounds = p.x >= 0 && p.x < tiles.Played.GetLength(0) &&
        //            p.y >= 0 && p.y < tiles.Played.GetLength(1);
        //
        //     if (!inBounds)
        //     {
        //         Debug.LogWarning($"Position {p} is not in bounds ({tiles.Played.GetLength(0)}x{tiles.Played.GetLength(1)}).");
        //     }
        //     
        //     return inBounds;
        // }
        
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
        internal bool DirectionIsEmptyOrMatchesGeography(Vector2Int cell, Vector2Int dir, Geography geography)
        {
            if (!tiles.Placement.ContainsKey(cell))
            {
                // Debug.Log($"No tile at {cell}");
                return true;
            }
            
            if (tiles.Placement[cell].GetGeographyAt(dir) == geography)
            {
                // Debug.Log($"Tile at {cell} matches {geography} in the direction {dir}");
                return true;
            }
            
            return false;
        }

        #endregion

        // /// <summary>
        // /// Perform a rotation of a tile, if in the correct phase. Always sets tile to the closest 90 degree angle greater than now.
        // /// </summary>
        // public void RotateTile()
        // {
        //     //TODO Why are we checking the phase anyways? I added NewTurn because this was causing the check for valid new piece to fail.
        //     if (gameControllerScript.state.phase == Phase.TileDrawn || gameControllerScript.state.phase == Phase.NewTurn)
        //     {
        //         tiles.Current.Rotate();
        //         
        //         tiles.Current.gameObject.transform.Rotate(0.0f, 90.0f, 0.0f, Space.Self);
        //     }
        //     else
        //     {
        //         Debug.LogWarning($"Tile not rotated because call came in {gameControllerScript.state.phase} and rotation is only valid during TileDrawn and NewTurn.");
        //     }
        // }

        /// <summary>
        /// Reset tile rotation internal state. WARNING: This only deals with the internal state. It does not rotate the tile in the view.
        /// </summary>
        // public void ResetTileRotation()
        // {
        //     tiles.Current.Rotate(0);
        // }

        // #region Photon
        // /// <summary>
        // /// Called on Tile:Manipulation Started (set in Unity Inspector)
        // /// </summary>
        // public void ChangeCurrentTileOwnership()
        // {
        //     if (tiles.Current.gameObject.GetComponent<PhotonView>().Owner.NickName != (gameControllerScript.currentPlayer.id + 1).ToString())
        //         tiles.Current.transferTileOwnership(gameControllerScript.currentPlayer.id);
        // }
        //
        // #endregion
        
        // #region UI
        //
        // [PunRPC]
        // public void RotateDegrees()
        // {
        //     var angles = tiles.Current.gameObject.transform.localEulerAngles;
        //     var rotation = GetRotationFromAngle(angles.y);
        //     
        //     // Set the snap angle and snap the piece
        //     angles.y = rotation * 90;
        //     tiles.Current.gameObject.transform.localEulerAngles = angles;
        //     
        //     // Set the internal model
        //     tiles.Current.Rotate(rotation);
        //     
        //     Debug.Log($"Tile {tiles.Current} transformed to {angles.y} (Rotation {rotation}).");
        // }
        //
        // /// <summary>
        // /// Calculates a rotation number (0-3) from an Euler angle in the y axis.
        // /// </summary>
        // /// <param name="angle"></param>
        // /// <returns></returns>
        // private int GetRotationFromAngle(float angle)
        // {
        //     int rotate = (int)(angle) / 90;
        //     if (angle % 90 > 45)
        //         rotate += 1;
        //     return rotate % 4;
        // }
        //
        // #endregion
    }
}