using Carcassonne.State;

namespace Assets.Scripts.Carcassonne.AI
{
    interface InterfaceAIWrapper
    {
        //Further methods may be needed to handle the observation of placed tiles.
        public Phase GetGamePhase();

        public int GetMeeplesLeft();

        public void PickUpTile();

        public int GetCurrentTileId();
        
        public int GetBoardSize();

        public void EndTurn();

        public void DrawMeeple();

        public void RotateTile();

        public void PlaceTile(int x, int z);

        public void PlaceMeeple(float x, float z);

        public void FreeCurrentMeeple();

        public bool IsAITurn();
    }
}
