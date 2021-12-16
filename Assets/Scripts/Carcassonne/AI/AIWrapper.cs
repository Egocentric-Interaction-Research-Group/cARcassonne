using Carcassonne.State;
using Carcassonne;
using UnityEngine;
using System;
using static Carcassonne.PointScript;

/*
 * The AIWrapper acts as a middle-man between the AIPlayer-class and the data it needs and actions it can perform. It separates the AI logic from the code implementation. Its specific purpose is to
 * allow the exact same AIPlayer-class to be used in the real environment and the training environment. This means the AIWrapper class will look different in both these project, as the code running
 * the game differs in the two implementations.
 */
namespace Assets.Scripts.Carcassonne.AI
{
    public class AIWrapper : InterfaceAIWrapper
    {
        public GameControllerScript gc;
        public GameState state; //Contains TileState, MeepleState, FeatureState, PlayerState and a GameLog.
        public PlayerScript player;
        public int totalTiles;

        #region "Interface methods"
        public AIWrapper()
        {
            gc = GameObject.Find("GameController").GetComponent<GameControllerScript>();
            state = gc.gameState;
            totalTiles = state.Tiles.Remaining.Count;
        }

        public bool IsAITurn()
        {
            return player.getID() == state.Players.Current.getID();
        }

        public void PickUpTile()
        {
            gc.PickupTileRPC();
        }

        public int GetCurrentTileId()
        {
            return state.Tiles.Current.id;
        }

        public Phase GetGamePhase()
        {
            return state.phase;
        }

        public int GetMeeplesLeft()
        {
            return player.AmountOfFreeMeeples();
        }

        public void EndTurn()
        {
            gc.EndTurnRPC();
        }

        public void DrawMeeple()
        {
            gc.meepleControllerScript.DrawMeepleRPC();
        }

        public void RotateTile()
        {
            gc.tileControllerScript.RotateTileRPC();
        }

        public void PlaceTile(int x, int z)
        {
            gc.iTileAimX = x;
            gc.iTileAimZ = z;
            gc.ConfirmPlacementRPC();
        }

        public void PlaceMeeple(Direction meepleDirection)
        {
            float meepleX = 0.000f;
            float meepleZ = 0.000f;
            
            //If clause only changes X if it is east or west.
            if (meepleDirection == Direction.EAST)
            {
                meepleX = 0.011f;
            }
            else if (meepleDirection == Direction.WEST)
            {
                meepleX = -0.011f;
            }
            
            //If clause only changes Z if it is north or south
            if (meepleDirection == Direction.NORTH)
            {
                meepleZ = 0.011f;
            }
            else if (meepleDirection == Direction.SOUTH)
            {
                meepleZ = -0.011f;
            }

            gc.meepleControllerScript.aiMeepleX = meepleX;
            gc.meepleControllerScript.aiMeepleZ = meepleZ;
            gc.ConfirmPlacementRPC();
        }

        public void FreeCurrentMeeple()
        {
            //This is only used as a workaround for a current bug, where a meeple cannot be properly placed on a tile (e.g. when someone occupies the road/city that it connects to)
            //but the game does not recognize this as a faulty placement either, and threfore does not return the meeple.
            gc.meepleControllerScript.FreeMeeple(state.Meeples.Current.gameObject, gc);
        }

        public int GetMaxMeeples()
        {
            return player.meeples.Count;
        }

        public int GetMaxTileId()
        {
            //This needs a better solution if expansions are added. This number has just been manually taken from the game scene.
            return 23;
        }
        public int GetMaxBoardSize()
        {
            return state.Tiles.Played.GetLength(0);
        }

        public float[,] GetPlacedTiles()
        {
            return null;
        }

        public TileScript[,] GetTiles()
        {
            return state.Tiles.Played;
        }

        public int GetNumberOfPlacedTiles()
        {
            return totalTiles - state.Tiles.Remaining.Count;
        }

        public int GetTotalTiles()
        {
            return totalTiles;
        }

        public void Reset()
        {
            //In the training environment, this resets the game stage entirely before the next training session. Serves no purpose here except to make the code function.
        }

        #endregion
    }
}
