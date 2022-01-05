using System.Collections.Generic;
using Carcassonne.State;
using UnityEngine;

namespace Carcassonne
{
    /// <summary>
    /// Represents the entire moves of a single player during one round of play. 
    /// </summary>
    /// <remarks>Stores the player, their tile placement, and their meeple placement (if any) for a given turn
    /// </remarks>
    [System.Serializable]
    public struct Turn
    {
        public PlayerScript Player;
        public TileScript Tile;
        public Vector2 Location;
        public Vector2Int? MeeplePlacement; // The `?` means that it can be null, if no Meeple was placed.
        
        public bool MeeplePlayed => MeeplePlacement != null;
    }
    
    /// <summary>
    /// A log of the <see cref="Turn">Turns</see> for a game.
    /// </summary>
    /// <remarks>Turns are stored in a <see cref="Stack{T}"/> which is built as the game progresses.</remarks>
    [System.Serializable]
    [CreateAssetMenu(fileName = "GameLog", menuName = "GameLog")]
    public class GameLog : ScriptableObject
    {
        public Stack<Turn> Turns = new Stack<Turn>();
        public GameState state;

        public void LogTurn()
        {
            var t = new Turn
            {
                Player = state.Players.Current,
                Tile = state.Tiles.Current,
                Location = state.Tiles.lastPlayedPosition,
                MeeplePlacement = state.Meeples.Current?.direction
            };

            Turns.Push(t);
            
            Debug.Log($"Turn {Turns.Count}: Player {t.Player.name} | Tile ID {t.Tile.id}, Rotation ({t.Tile.rotation}), Position: {t.Location.x},{t.Location.y} | Meeple: {t.MeeplePlacement}");
        }
        
        private void OnEnable()
        {
            Turns.Clear();
        }
    }
}