using Carcassonne.State;
using UnityEngine;

namespace Carcassonne.AI
{
    /// <summary>
    ///  This interfaced is used by the AIWrapper class, which acts as a middle-man between the AIPlayer-class and the data it needs and actions it can perform. 
    ///  It separates the AI logic from the code implementation. Its specific purpose is to allow the exact same AIPlayer-class to be used in the real environment 
    ///  and the training environment.This means the AIWrapper class will look different in both these project, as the code running the game differs in the two implementations.
    ///  Version 1.0
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
        public bool PickUpTile();

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
        /// Ends the current AI agents turn.
        /// </summary>
        public void EndTurn();

        /// <summary>
        /// Draws a new meeple for placement.
        /// </summary>
        public bool DrawMeeple();

        /// <summary>
        /// Rotates the current tile to be placed.
        /// </summary>
        public void RotateTile();

        /// <summary>
        /// Attempts to place the current tile in the position (x, z)
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        public bool PlaceTile(Vector2Int cell);

        /// <summary>
        /// Places the meeple on the specified direction of tile placed in this turn.
        /// </summary>
        /// <param name="meepleDirection"></param> The direction of the tile to place the meeple on (North, South, West, East, or Center)
        public bool PlaceMeeple(Vector2Int meepleDirection);

        /// <summary>
        /// Frees the current meeple up. Utilized as a workaround for a bug where the meeple cannot be placed and cannot be returned by the AI.
        /// </summary>
        public void FreeCurrentMeeple();

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
        /// Used for observations. The object should be a representation of a tile.
        /// </summary>
        /// <returns></returns>
        public object[,] GetTiles();

        /// <summary>
        /// Returns the number of tiles that have been placed this game.
        /// </summary>
        /// <returns></returns>
        public int GetNumberOfPlacedTiles();

        /// <summary>
        /// Returns the total amount of tiles in the game (played and unplayed)
        /// </summary>
        /// <returns></returns>
        public int GetTotalTiles();

        /// <summary>
        /// Returns the amount of meeples left for the AI agent.
        /// </summary>
        /// <returns></returns>
        public int GetMeeplesLeft();

        /// <summary>
        /// Returns the maximum number of meeples for the AI agent, used for normalization.
        /// </summary>
        /// <returns></returns>
        public int GetMaxMeeples();

        /// <summary>
        /// Resets the game for another session. May not be useful in real implementation, but needed in training environment.
        /// </summary>
        public void Restart();
        
        // /// <summary>
        // /// Returns the minimum allowed X coordinate for the AI agent to move in to before it is forced to restart from the base tile.
        // /// </summary>
        // /// <returns></returns>
        // public int GetMinX();
        //
        // /// <summary>
        // /// Returns the maximum allowed X coordinate for the AI agent to move in to before it is forced to restart from the base tile.
        // /// </summary>
        // /// <returns></returns>
        // public int GetMaxX();
        //
        // /// <summary>
        // /// Returns the minimum allowed Z coordinate for the AI agent to move in to before it is forced to restart from the base tile.
        // /// </summary>
        // /// <returns></returns>
        // public int GetMinZ();
        //
        // /// <summary>
        // /// Returns the maximum allowed Z coordinate for the AI agent to move in to before it is forced to restart from the base tile.
        // /// </summary>
        // /// <returns></returns>
        // public int GetMaxZ();

        /// <summary>
        /// Gets the current score of the AI agent player.
        /// </summary>
        /// <returns></returns>
        public float GetScore();

        /// <summary>
        /// Gets the score change this round for the AI agent.
        /// </summary>
        /// <returns></returns>
        public float GetScoreChange();

    }
}
