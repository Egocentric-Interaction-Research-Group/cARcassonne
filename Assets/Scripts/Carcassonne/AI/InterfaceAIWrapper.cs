using Carcassonne.State;

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

        public void PlaceMeeple(float x, float z);

        public void FreeCurrentMeeple();

        public int GetMaxMeeples();

        public int GetMaxTileId();

        public int GetMaxBoardSize();

        public float[,] GetPlacedTiles();
    }
}
