using Carcassonne.State;
using Carcassonne;
using UnityEngine;

namespace Assets.Scripts.Carcassonne.AI
{
    public class AIWrapper : InterfaceAIWrapper
    {
        public GameControllerScript gc;
        public GameState gs; //Contains TileState, MeepleState, FeatureState, PlayerState and a GameLog.
        public PlayerScript player;

        #region "Interface methods"
        public AIWrapper()
        {
            gc = GameObject.Find("GameController").GetComponent<GameControllerScript>();
            gs = gc.gameState;
        }

        public bool IsAITurn()
        {
            return player.getID() == gs.Players.Current.getID();
        }

        public int GetBoardSize()
        {
            return gs.Tiles.Played.GetLength(0);
        }

        public void PickUpTile()
        {
            gc.PickupTileRPC();
        }

        public int GetCurrentTileId()
        {
            return gs.Tiles.Current.id;
        }

        public Phase GetGamePhase()
        {
            return gs.phase;
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

        public void PlaceMeeple(float x, float z)
        {
            gs.Meeples.Current.gameObject.transform.localPosition = gs.Tiles.Current.transform.localPosition + new Vector3(x, 0.86f, z);
            gc.meepleControllerScript.CurrentMeepleRayCast();
            gc.meepleControllerScript.AimMeeple(gc);
            gc.SetMeepleSnapPos();
            gc.ConfirmPlacementRPC();

            //The two rows below are just a workaround to get meeples to stay on top of the table and not have a seemingly random Y coordinate.
            gs.Meeples.Current.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY;
            gs.Meeples.Current.gameObject.transform.localPosition = new Vector3(gs.Meeples.Current.gameObject.transform.localPosition.x, 0.86f, gs.Meeples.Current.gameObject.transform.localPosition.z);

        }

        public void FreeCurrentMeeple()
        {
            gc.meepleControllerScript.FreeMeeple(gs.Meeples.Current.gameObject, gc);
        }

        #endregion

        //The methods below are only use for printing out information, used for test purposes.
        #region "Real game specific"
        public TileScript GetCurrentTile44()
        {
            return gs.Tiles.Current;
        }

        public MeepleScript GetCurrentMeeple333()
        {
            return gs.Meeples.Current;
        }
        #endregion
    }
}
