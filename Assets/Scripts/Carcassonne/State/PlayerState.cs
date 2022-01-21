using System.Collections.Generic;
using Carcassonne.Meeple;
using Carcassonne.Player;
using UnityEngine;

namespace Carcassonne.State
{
    [CreateAssetMenu(fileName = "PlayerState", menuName = "States/PlayerState")]
    public class PlayerState : ScriptableObject
    {
        public List<PlayerScript> All = new List<PlayerScript>();
        public PlayerScript Current;
        
        // Derived Properties
        public List<MeepleScript> Meeples => new List<MeepleScript>();
        
        private void OnEnable()
        {
            All.Clear();
            Current = null;
        }
    }
}