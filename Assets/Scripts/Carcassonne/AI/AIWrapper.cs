using Carcassonne.State;
using Carcassonne;
using UnityEngine;
using System;
using static Carcassonne.PointScript;

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
            gc.pcRotate = true;
            gc.RotateTileRPC();
        }

        public void PlaceTile(int x, int z)
        {
            gc.iTileAimX = x;
            gc.iTileAimZ = z;
            gc.ConfirmPlacementRPC();
        }

        public void PlaceMeeple(Direction meepleDirection)
        {
            float meepleX = 0;
            float meepleZ = 0;
            if (meepleDirection == Direction.NORTH || meepleDirection == Direction.SOUTH || meepleDirection == Direction.CENTER)
            {
                meepleX = 0.000f;
            }
            else if (meepleDirection == Direction.EAST)
            {
                meepleX = 0.011f;
            }
            else if (meepleDirection == Direction.WEST)
            {
                meepleX = -0.011f;
            }

            if (meepleDirection == Direction.WEST || meepleDirection == Direction.EAST || meepleDirection == Direction.CENTER)
            {
                meepleZ = 0.000f;
            }
            else if (meepleDirection == Direction.NORTH)
            {
                meepleZ = 0.011f;
            }
            else if (meepleDirection == Direction.SOUTH)
            {
                meepleZ = -0.011f;
            }
            state.Meeples.Current.gameObject.transform.localPosition = state.Tiles.Current.transform.localPosition + new Vector3(meepleX, 0.86f, meepleZ);
            gc.meepleControllerScript.CurrentMeepleRayCast();
            gc.meepleControllerScript.AimMeeple(gc);
            gc.SetMeepleSnapPos();
            gc.ConfirmPlacementRPC();

            //The two rows below are just a workaround to get meeples to stay on top of the table and not have a seemingly random Y coordinate.
            state.Meeples.Current.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY;
            state.Meeples.Current.gameObject.transform.localPosition = new Vector3(state.Meeples.Current.gameObject.transform.localPosition.x, 0.86f, state.Meeples.Current.gameObject.transform.localPosition.z);

        }

        public void FreeCurrentMeeple()
        {
            gc.meepleControllerScript.FreeMeeple(state.Meeples.Current.gameObject, gc);
        }

        public int GetMaxMeeples()
        {
            return player.meeples.Count;
        }

        public int GetMaxTileId()
        {
            //This needs a better solution for expansions.
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
            //Reset everything and start a new game. Should this even exist in real game?
        }

        #endregion

        //The methods below are only use for printing out information, used for test purposes.
        #region "Real game specific"
        public TileScript GetCurrentTile()
        {
            return state.Tiles.Current;
        }

        public MeepleScript GetCurrentMeeple()
        {
            return state.Meeples.Current;
        }
        #endregion
    }
}
