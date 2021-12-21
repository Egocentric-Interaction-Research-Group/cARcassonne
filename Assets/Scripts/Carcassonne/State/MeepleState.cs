using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Carcassonne.State
{
    public struct PlacedMeeple
    {
        public PlacedMeeple(MeepleScript meeple, Vector2Int direction)
        {
            Meeple = meeple;
            Direction = direction;
        }

        public MeepleScript Meeple { get; }
        public Vector2Int Direction { get; }
    }
    
    /// <summary>
    /// MeepleState hold all of the information about the position, availability, and ownership of meeples.
    /// Player meeple list derive from this information store.
    /// </summary>
    [CreateAssetMenu(fileName = "MeepleState", menuName = "States/MeepleState")]
    public class MeepleState : ScriptableObject, IGamePieceState<MeepleScript>
    {
        List<MeepleScript> IGamePieceState<MeepleScript>.Remaining => _remaining;
        
        /// <summary>
        /// The current Meeple being played.
        /// </summary>
        [CanBeNull] public MeepleScript Current { get; set; }

        public MeepleScript[,] Played { get; }
        public Vector2Int MatrixOrigin { get; }

        public Dictionary<Vector2Int, PlacedMeeple> Placement = new Dictionary<Vector2Int, PlacedMeeple>();
        public IEnumerable<MeepleScript> InPlay => Placement.Select(p => p.Value.Meeple);

        /// <summary>
        /// The set of all Meeples in the game.
        /// </summary>
        public List<MeepleScript> All = new List<MeepleScript>();
        
        /// <summary>
        /// The free meeples remaining for each player 
        /// </summary>
        // public Dictionary<PlayerScript, MeepleScript> Remaining => null;

        private List<MeepleScript> _remaining;

        private void OnEnable()
        {
            All.Clear();
            // GameObject[] meepleObjects = GameObject.FindGameObjectsWithTag("Meeple ");
            // All = from meeple in meepleObjects select meeple.GetComponent<MeepleScript>()).ToList();

            Current = null;
            
        }

        // private Dictionary<Player, int> CalculateRemainingMeeples()
        // {
        //     return Players.All.ToDictionary(p => p, CalculateRemainingMeeplesForPlayer);
        // }
        //
        // private int CalculateRemainingMeeplesForPlayer(Player player)
        // {
        //     var playerMeeples = new List<GameObject>(player.meeples);
        //     return playerMeeples.Count(p => p.GetComponent<MeepleScript>().free);
        // }

        public List<MeepleScript> MeeplesForPlayer(PlayerScript p)
        {
            return (from meeple in All where meeple.player == p select meeple).ToList();
        }

        [CanBeNull]
        public MeepleScript MeepleAt(Vector2Int xy)
        {
            throw new System.NotImplementedException();
        }
    }
}