using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;

namespace Carcassonne.State
{
    public class MeepleState
    {
        public Dictionary<PlayerScript.Player, int> Remaining => calculateRemainingMeeples();
        [CanBeNull] public MeepleScript Current => meeples.currentMeeple.GetComponent<MeepleScript>();
        public MeepleScript[,] Played; //TODO Implement

        private MeepleControllerScript meeples;
        private PlayerScript player;

        public MeepleState(GameControllerScript gameControllerScript)
        {
            this.meeples = gameControllerScript.meepleControllerScript;
            this.player = gameControllerScript.playerScript; 
        }

        private Dictionary<PlayerScript.Player, int> calculateRemainingMeeples()
        {
            return player.players.ToDictionary(p => p, p => calculateRemainingMeeplesForPlayer(p));
        }
        
        private int calculateRemainingMeeplesForPlayer(PlayerScript.Player player)
        {
            var playerMeeples = new List<GameObject>(player.meeples);
            return playerMeeples.Where(p => p.GetComponent<MeepleScript>().free).Count();
        }
    }
}