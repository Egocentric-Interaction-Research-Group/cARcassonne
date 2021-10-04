using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Carcassonne
{
    public struct GameRules
    {
        public bool abbots;
        public bool river;
        public bool farmer;
    }

    public class TileState
    {
        public Stack<TileScript> remaining;
        [CanBeNull] public TileScript current;
        public TileScript[,] played;
    }

    public class MeepleState
    {
        public Dictionary<PlayerScript.Player, int> remaining;
        [CanBeNull] public MeepleScript current;
        public MeepleScript[,] played;
    }

    public class PlayerState
    {
        public List<PlayerScript.Player> players;
        public PlayerScript.Player current;
    }

    public class FeatureState
    {
        // public List<City> cities;
        // public List<Road> roads;
        // public List<Chapel> chapels;
    }
    
    [CreateAssetMenu(fileName="GameState", menuName="States/GameState")]
    [Serializable]
    public class GameState : ScriptableObject
    {
        public GameRules rules;
        
        /// <summary>
        /// Describes what is happening currently in the game.
        /// </summary>
        public GameControllerScript.Phases phase;
        public TileState tiles;
        public MeepleState meeples;
        public FeatureState features;
        public PlayerState players;
        public GameLog log;

        private void Awake()
        {
            rules = new GameRules(); // Defaults all to false which is correct.
            
            
        }
    }
}