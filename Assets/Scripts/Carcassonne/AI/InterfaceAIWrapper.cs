using Carcassonne.State;
using static Carcassonne.PointScript;

namespace Assets.Scripts.Carcassonne.AI
{
    interface InterfaceAIWrapper
    {
        //Further methods may be needed to handle the observation of placed tiles.

        public bool IsAITurn();

        public void PickUpTile();

        public int GetCurrentTileId();

        public Phase GetGamePhase();

        public int GetMeeplesLeft();

        public void EndTurn();

        public void DrawMeeple();

        public void RotateTile();

        public void PlaceTile(int x, int z);

        public void PlaceMeeple(Direction meepleDirection);

        public void FreeCurrentMeeple();

        public int GetMaxMeeples();

        public int GetMaxTileId();

        public int GetMaxBoardSize();

        public float[,] GetPlacedTiles();
    }
}
