using Carcassonne.State;
using static Carcassonne.PointScript;

namespace Assets.Scripts.Carcassonne.AI
{
    /// <summary>
    ///  This interfaced is used by the AIWrapper class, which acts as a middle-man between the AIPlayer-class and the data it needs and actions it can perform. 
    ///  It separates the AI logic from the code implementation. Its specific purpose is to allow the exact same AIPlayer-class to be used in the real environment 
    ///  and the training environment.This means the AIWrapper class will look different in both these project, as the code running the game differs in the two implementations.
    /// </summary>
    interface InterfaceAIWrapper
    {
        //Further methods may be needed to handle the observation of placed tiles.

        /// <summary>
        /// Checks if it is this AI-controlled players turn to make a move.
        /// </summary>
        /// <returns></returns>
        public bool IsAITurn();

        /// <summary>
        /// Picks up the next tile to be drawn, done at the beginning of a new turn.
        /// </summary>
        public void PickUpTile();

        /// <summary>
        /// Returns the ID of the current tile.
        /// </summary>
        /// <returns></returns>
        public int GetCurrentTileId();

        /// <summary>
        /// Returns the current game phase.
        /// </summary>
        /// <returns></returns>
        public Phase GetGamePhase();

        /// <summary>
        /// Gets the number of meeples left for the current player. Called upon by the AI agent during its turn.
        /// </summary>
        /// <returns></returns>
        public int GetMeeplesLeft();

        /// <summary>
        /// Ends the current AI agents turn.
        /// </summary>
        public void EndTurn();

        /// <summary>
        /// Draws a new meeple for placement.
        /// </summary>
        public void DrawMeeple();

        /// <summary>
        /// Rotates the current tile to be placed.
        /// </summary>
        public void RotateTile();

        /// <summary>
        /// Attempts to place the current tile in the position (x, z)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        public void PlaceTile(int x, int z);

        /// <summary>
        /// Places the meeple on the specified direction of tile placed in this turn.
        /// </summary>
        /// <param name="meepleDirection"></param> The direction of the tile to place the meeple on (North, South, West, East, or Center)
        public void PlaceMeeple(Direction meepleDirection);

        /// <summary>
        /// Frees the current meeple up. Utilized as a workaround for a bug where the meeple cannot be placed and cannot be returned by the AI.
        /// </summary>
        public void FreeCurrentMeeple();

        /// <summary>
        /// Gets the maximum meeples allowed in the game (for one player). Used for normalization
        /// </summary>
        /// <returns></returns>
        public int GetMaxMeeples();

        /// <summary>
        /// Get the highest tile-ID available in the current game settings, used for normalization.
        /// </summary>
        /// <returns></returns>
        public int GetMaxTileId();

        /// <summary>
        /// Gets the maximum size of the board.
        /// </summary>
        /// <returns></returns>
        public int GetMaxBoardSize();

        /// <summary>
        /// Gets the currently placed tiles on the board.
        /// </summary>
        /// <returns></returns>
        public float[,] GetPlacedTiles();
    }
}
