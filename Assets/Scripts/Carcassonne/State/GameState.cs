using System;
using Carcassonne.Utilities;
using UnityEngine;

namespace Carcassonne.State
{
    [CreateAssetMenu(fileName = "GameState", menuName = "States/GameState")]
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

        public GameState()
        {
            Reset();
        }

        private void Awake()
        {
            Reset();
        }

        private void Reset()
        {
            Debug.Log("Resetting Game State...");
            
            Rules = new GameRules();
            Tiles = new TileState();
            Meeples = new MeepleState();
            Features = new FeatureState(Meeples);
            Players = new PlayerState();
        }

        private void OnEnable()
        {
            Reset();
        }
    }
}