using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Carcassonne
{

    /// <summary>
    /// Describes different phases of gameplay.
    /// </summary>
    public enum Phase
    {
        NewTurn,
        TileDrawn,
        TileDown,
        MeepleDrawn,
        MeepleDown,
        GameOver
    }

    public struct GameRules
    {
        public bool Abbots;
        public bool River;
        public bool Farmer;
    }

    public class TileState
    {
        public List<TileScript> Remaining => stack.remaining;
        [CanBeNull] public TileScript Current => stack.current;
        public TileScript[,] Played;

        private StackScript stack;

        public TileState(StackScript stack)
        {
            this.stack = stack;
        }
    }

    public class MeepleState
    {
        public Dictionary<PlayerScript.Player, int> Remaining;
        [CanBeNull] public MeepleScript Current;
        public MeepleScript[,] Played;
    }

    public class PlayerState
    {
        public List<PlayerScript.Player> Players;
        public PlayerScript.Player Current;
    }

    public class FeatureState
    {
        // public List<City> cities;
        // public List<Road> roads;
        // public List<Chapel> chapels;
    }

    [CreateAssetMenu(fileName = "GameState", menuName = "States/GameState")]
    [Serializable]
    public class GameState : ScriptableObject
    {
        public GameRules Rules;

        /// <summary>
        /// Describes what is happening currently in the game.
        /// </summary>
        public Phase phase;

        public TileState Tiles;
        public MeepleState Meeples;
        public FeatureState Features;
        public PlayerState Players;
        public GameLog Log;

        private void Awake()
        {
            Rules = new GameRules(); // Defaults all to false which is correct.
        }
    }
}