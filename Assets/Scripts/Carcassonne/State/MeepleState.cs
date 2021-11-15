using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Carcassonne.State
{
    /// <summary>
    /// MeepleState hold all of the information about the position, availability, and ownership of meeples.
    /// Player meeple list derive from this information store.
    /// </summary>
    [CreateAssetMenu(fileName = "MeepleState", menuName = "States/MeepleState")]
    public class MeepleState : ScriptableObject
    {
        public List<MeepleScript> All = new List<MeepleScript>();
        public Dictionary<PlayerScript, MeepleScript> Free => null;
        [CanBeNull] public MeepleScript Current;
        // public MeepleScript[,] Played; //TODO Implement

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