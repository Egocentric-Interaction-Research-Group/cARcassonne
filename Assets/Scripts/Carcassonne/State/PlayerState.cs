using System.Collections.Generic;
using Carcassonne.Meeples;
using Carcassonne.Players;
using UnityEngine;

namespace Carcassonne.State
{
    public class PlayerState
    {
        public List<PlayerScript> All = new List<PlayerScript>();
        public PlayerScript Current;
        
        // Derived Properties
        public List<MeepleScript> Meeples => new List<MeepleScript>();

        public PlayerState()
        {
            All.Clear();
            Current = null;
        }
    }
}