using System.Collections.Generic;
using System.Linq;
using Carcassonne.Models;
using Carcassonne.Players;
using Carcassonne.State;
using Carcassonne.Tiles;
using UnityEngine;

namespace Carcassonne.Utilities
{
    /// <summary>
    /// Represents the entire moves of a single player during one round of play. 
    /// </summary>
    /// <remarks>Stores the player, their tile placement, and their meeple placement (if any) for a given turn
    /// </remarks>
    [System.Serializable]
    public struct Turn
    {
        public Player Player;
        public Tile Tile;
        public Vector2Int Cell;
        public Vector2Int? MeeplePlacement; // The position in Meeple space
        
        public bool MeeplePlayed => MeeplePlacement != null;
    }
    
    /// <summary>
    /// A log of the <see cref="Turn">Turns</see> for a game.
    /// </summary>
    /// <remarks>Turns are stored in a <see cref="Stack{T}"/> which is built as the game progresses.</remarks>
    [System.Serializable]
    // [CreateAssetMenu(fileName = "GameLog", menuName = "GameLog")]
    public class GameLog : MonoBehaviour
    {
        public Stack<Turn> Turns = new Stack<Turn>();
        public GameState state;
        public GridMapper grid => GetComponent<GridMapper>();

        public void LogTurn()
        {
            var position = state.Tiles.Placement.Single(kvp => kvp.Value == state.Tiles.Current).Key;
            var t = new Turn
            {
                Player = state.Players.Current,
                Tile = state.Tiles.Current,
                Cell = position,
                MeeplePlacement = state.Meeples.Placement.Keys.SingleOrDefault(mCell => grid.MeepleToTile(mCell) == position)
            };

            Turns.Push(t);
            
            Debug.Log($"Turn {Turns.Count}: Player {t.Player.name} | Tile ID {t.Tile.ID}, Rotation ({t.Tile.Rotations}), Position: {t.Cell.x},{t.Cell.y} | Meeple: {t.MeeplePlacement}");
        }
        
        private void OnEnable()
        {
            Turns.Clear();
        }
    }
}