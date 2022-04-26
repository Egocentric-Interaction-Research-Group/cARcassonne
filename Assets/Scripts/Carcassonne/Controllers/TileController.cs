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
        private GameController controller => GetComponent<GameController>(); //null?
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
        public override bool Draw()
        {
            // TODO Should this check phase validity??? Probably, right?
            Debug.Log("Drawing new Tile.");
            // Debug.Assert(state.Tiles.Remaining.Count > 0, "TileController: The stack is empty.");
            
            // This can happen if Draw is called twice because of a Discard, for example.
            if (state.Tiles.Remaining.Count == 0){
                GetComponent<GameController>().GameOver();
                return false;
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
            
            return true;
        }

        public void PlaceFirst()
        {
            Debug.Assert(state != null, "TileController: The state is null.");
            
            var t = state.Tiles.Remaining.Pop();
            Debug.Log($"Starting tile: ({t.ID}) {t}");

            state.Tiles.Placement.Add(Vector2Int.zero, t);

            var bg = BoardGraph.FromTile(t, Vector2Int.zero, state.grid);
            Debug.Log($"Starting graph has {bg.VertexCount} vertices and {bg.EdgeCount} edges.");;
            bg.SetTurn(0);
            
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
            var bg = BoardGraph.FromTile(tile, cell, state.grid);
            bg.SetPlayer(state.Players.Current);
            bg.SetTurn(controller.Turn);
            state.Features.Graph.Add(bg);

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
            
            Debug.LogWarning($"Tile ID {tile.ID} cannot be placed.");
            return false;
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

    }
}