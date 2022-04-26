using System;
using System.Linq;
using Carcassonne.Controllers;
using Carcassonne.Models;
using Carcassonne.State;
using UnityEngine;

namespace Carcassonne.AI
{
    /// <summary>
    ///  The AIWrapper acts as a middle-man between the AIPlayer-class and the data it needs and actions it can perform. It separates the AI logic from the code implementation. Its specific purpose is to 
    ///  allow the exact same AIPlayer-class to be used in the real environment and the training environment. This means the AIWrapper class will look different in both these project, as the code running
    ///  the game differs in the two implementations.
    ///  Version 1.0
    /// </summary>
    public class AIWrapper : MonoBehaviour, InterfaceAIWrapper
    {
        public AIGameController aiController;
        public GameController controller;
        public MeepleController meepleController;
        public TileController tileController;
        
        public GameState state; //Contains TileState, MeepleState, FeatureState, PlayerState and a GameLog.
        public Player player;

        private void Start()
        {
            //TODO this won't work for multiple boards. try GetComponentInParent
            aiController = GetComponentInParent<AIGameController>();
            controller = GetComponentInParent<GameController>();
            meepleController = GetComponentInParent<MeepleController>();
            tileController = GetComponentInParent<TileController>();
            state = GetComponentInParent<GameState>();
            
            player = GetComponent<Player>();
            
            Debug.Assert(aiController != null);
            Debug.Assert(controller != null);
            Debug.Assert(meepleController != null);
            Debug.Assert(tileController != null);
            Debug.Assert(state != null);
            Debug.Assert(player != null);
        }

        #region Game Actions

        public bool PickUpTile()
        {
            return tileController.Draw();
        }
        
        public bool DrawMeeple()
        {
            return meepleController.Draw();
        }
        
        public void EndTurn()
        {
            controller.EndTurn();
        }
        
        public void Restart()
        {
            try
            {
                aiController.Restart();
            }
            catch (NullReferenceException e)
            {
                Debug.LogWarning("AIGameController not set. Could not invoke GameOver. If this is at the beginning of a new run, this isn't a problem.");
            }
        }

        #endregion
        
        #region Game Info

        public bool IsAITurn()
        {
            if (state == null)
            {
                Debug.Log("Not AI Turn: Null state");
                return false;
            }
            else if (state.Players == null)
            {
                Debug.Log("Not AI Turn: Null players");
                return false;
            }
            else if (state.Players.Current == null)
            {
                Debug.Log("Not AI Turn: Null current player");
                return false;
            }
            else if (player.id != state.Players.Current.id)
            {
                Debug.Log($"Not AI Turn: {player.id} != {state.Players.Current.id} (current)");
                return false;
            }

            return true;
            //return state != null && state.Players != null && state.Players.Current != null && player.id == state.Players.Current.id;
        }

        public Phase GetGamePhase()
        {
            return state.phase;
        }

        public int GetMaxBoardSize()
        {
            return GameRules.BoardSize;
        }

        // public int GetMinX()
        // {
        //     return state.Tiles.Limits.xMin;
        // }
        //
        // public int GetMaxX()
        // {
        //     return state.Tiles.Limits.xMax;
        // }
        //
        // public int GetMinZ()
        // {
        //     return state.Tiles.Limits.yMin;
        // }
        //
        // public int GetMaxZ()
        // {
        //     return state.Tiles.Limits.yMax;
        // }

        public float GetScore()
        {
            return (float)player.score;
        }

        public float GetScoreChange()
        {
            return (float)player.scoreChange;
        }

        public float GetUnscoredPointsChange()
        {
            return (float)player.unscoredPointsChange;
        }

        public float GetPotentialPointsChange()
        {
            return (float)player.potentialPointsChange;
        }
        
        public float GetOtherScoreChange()
        {
            return (float)state.Players.Others.Select(p => p.scoreChange).Sum();
        }

        public float GetOtherUnscoredPointsChange()
        {
            return (float)state.Players.Others.Select(p => p.unscoredPointsChange).Sum();
        }

        public float GetOtherPotentialPointsChange()
        {
            return (float)state.Players.Others.Select(p => p.potentialPointsChange).Sum();
        }

        #endregion
        
        #region Tile Actions

        public void RotateTile()
        {
            tileController.Rotate();
        }

        public bool PlaceTile(Vector2Int cell)
        {
            // var cell = new Vector2Int(x, z);
            return tileController.Place(cell);
        }
        
        #endregion

        #region Tile Info

        public int GetCurrentTileId()
        {
            if (state.Tiles.Current == null)
                return GetMaxTileId()+1;
            
            return state.Tiles.Current.ID;
        }
        
        public int GetCurrentTileRotations()
        {
            if (state.Tiles.Current == null)
                return 0;
            
            return state.Tiles.Current.Rotations;
        }
        
        public object[,] GetTiles()
        {
            return state.Tiles.Played;
        }

        public int GetNumberOfPlacedTiles()
        {
            return GetTotalTiles() - state.Tiles.Remaining.Count;
        }
        
        public int GetTotalTiles()
        {
            Debug.Assert(Tile.GetIDDistribution().Values.Sum() == 72, $"There should be 72 tiles in the stack. Found {Tile.GetIDDistribution().Values.Sum()}");
            return Tile.GetIDDistribution().Values.Sum();
        }
        
        public int GetMaxTileId()
        {
            //This needs a better solution if expansions are added. This number has just been manually taken from the game scene.
            return Tile.GetIDDistribution().Keys.Max();
        }

        public RectInt GetLimits()
        {
            return state.Tiles.Limits;
        }

        #endregion

        #region Meeple Actions

        public bool PlaceMeeple(Vector2Int meepleDirection)
        {
            Debug.Assert(state.Tiles.lastPlayedPosition != null, "State.Tiles.lastPlayedPosition should not be null, but it is.");
            // controller.PlaceMeeple(state.grid.TileToMeeple((Vector2Int)state.Tiles.lastPlayedPosition, meepleDirection));
            return meepleController.Place(state.grid.TileToMeeple((Vector2Int)state.Tiles.lastPlayedPosition,
                meepleDirection));
        }

        public void DiscardMeeple()
        {
            meepleController.Discard();
        }

        public void FreeCurrentMeeple()
        {
            //This is only used as a workaround for a current bug, where a meeple cannot be properly placed on a tile (e.g. when someone occupies the road/city that it connects to)
            //but the game does not recognize this as a faulty placement either, and threfore does not return the meeple.
            // meepleController.Free(state.Meeples.Current);
            meepleController.Discard();
        }

        #endregion

        #region Meeple

        public bool CanBePlaced()
        {
            return meepleController.CanBePlaced();
        }
        
        public int GetMeeplesLeft()
        {
            return state.Meeples.RemainingForPlayer(player).Count();
        }

        public int GetMaxMeeples()
        {
            return GameRules.MeeplesPerPlayer;
        }

        #endregion


    }
}
