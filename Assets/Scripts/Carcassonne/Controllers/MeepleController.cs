using System.Collections.Generic;
using System.Linq;
using Carcassonne.Models;
using Carcassonne.State;
using UnityEngine;
using UnityEngine.Events;

namespace Carcassonne.Controllers
{
    public class MeepleController : GamePieceController<Meeple>
    {
        private GameState state => GetComponent<GameState>();
        private Meeple meeple => state.Meeples.Current;

        public GridMapper grid => GetComponent<GridMapper>();
        
        // #region Events
        //
        // public new UnityEvent<Meeple> OnDraw = new UnityEvent<Meeple>();
        // public new UnityEvent OnInvalidDraw = new UnityEvent();
        // public new UnityEvent<Meeple> OnDiscard = new UnityEvent<Meeple>();
        // public new UnityEvent<Meeple, Vector2Int> OnPlace = new UnityEvent<Meeple, Vector2Int>();
        // public new UnityEvent<Meeple, Vector2Int> OnInvalidPlace = new UnityEvent<Meeple, Vector2Int>();
        // public new UnityEvent<Meeple> OnFree = new UnityEvent<Meeple>();
        //
        // #endregion

        #region Convenience Properties
        private MeepleState meeples => state.Meeples;
        #endregion
        
        public void Free(Meeple meeple)
        {
            // If this is a meeple that has already been played on a tile (as opposed to one that is being placed).
            if(state.Meeples.InPlay.Contains(meeple)){
                var placements = state.Meeples.Placement;
                var position = placements.Single(kvp => kvp.Value == meeple).Key;
                Debug.Log($"Meeple at position {position} has been freed.");
                placements.Remove(position);
            }
            
            OnFree.Invoke(meeple);
        }

        public override bool IsPlacementValid(Vector2Int cell)
        {
            Debug.Log($"Checking feature at {cell}");
            var position = grid.MeepleToTile(cell);
            
            // If the placement is not on the current tile, it is invalid
            if (!state.Tiles.Placement.ContainsKey(position) || state.Tiles.Placement[position] != state.Tiles.Current)
            {
                Debug.Log($"Invalid: Tile at {position} does not exist ({!state.Tiles.Placement.ContainsKey(position)}) or is not the current tile.");
                return false;
            }
            
            var direction = grid.MeepleToDirection(cell);
            // If it is a corner, it is invalid.
            if (direction.magnitude > 1.0)
            {
                Debug.Log($"Invalid: Direction magnitude {direction.magnitude} means this is a corner subtile.");
                return false;
            }

            // If this side cannot hold a meeple, it is invalid.
            if (!state.Tiles.Current.GetGeographyAt(direction).IsFeature())
            {
                Debug.Log($"Invalid: Geography ({state.Tiles.Current.GetGeographyAt(direction)}) is not a feature.");
                return false;
            }

            var feature = state.Features.GetFeatureAt(position, direction);
            
            // Placement is invalid if not on a type of feature that can have a meeple
            if (feature == null)
            {
                Debug.Log($"Invalid: No feature at {position}.");
                return false;
            }
            
            // Placement is invalid if feature already has meeple
            if (meeples.InFeature(feature).Any())
            {
                Debug.Log($"Invalid: Feature at {position} has meeple.");
                return false;
            }

            // Nothing makes it invalid, so return true
            return true;
        }

        public override bool Place(Vector2Int cell)
        {
            // Test if Meeple placement is valid
            if (!IsPlacementValid(cell))
            {
                OnInvalidPlace.Invoke(meeple, cell);
                return false;
            }

            // var position = grid.MeepleToTile(cell);
            // var direction = grid.MeepleToDirection(cell);
            
            // Place meeple
            state.Meeples.Placement.Add(cell, meeple); //position, new PlacedMeeple(meeple, direction));

            // Move game to next phase
            state.phase = Phase.MeepleDown;
            
            OnPlace.Invoke(meeple, cell);
            return true;
        }

        public override void Draw()
        {
            Debug.Log("Drawing new Meeple.");
            // Can't draw if a meeple is in play or a player has none left.
            if (RemainingForCurrentPlayer.Count() < 1 || state.Meeples.Current != null)
            {
                OnInvalidDraw.Invoke();
            }
            
            // Get a new current tile
            state.Meeples.Current = RemainingForCurrentPlayer.First();
            
            state.phase = Phase.MeepleDrawn;
            
            OnDraw.Invoke(meeple);
        }

        public override void Discard()
        {
            OnDiscard.Invoke(meeple);
            
            meeples.Current = null;
        }

        public override bool CanBePlaced()
        {
            var tile = state.Tiles.Current;
            var position = state.Tiles.lastPlayedPosition;

            if (tile == null || position == null) return false;

            // Check each direction to see if the meeple can be placed.
            foreach (var direction in tile.Geographies.Keys)
            {
                if (IsPlacementValid(grid.TileToMeeple((Vector2Int)position, direction)))
                    return true;
            }
            
            // If not, return false.
            return false;
        }

        /// <summary>
        /// Meeples that have not been played for the current player
        /// </summary>
        private IEnumerable<Meeple> RemainingForCurrentPlayer =>
            state.Meeples.Remaining.Where(meeple => meeple.player == state.Players.Current);
    }
}