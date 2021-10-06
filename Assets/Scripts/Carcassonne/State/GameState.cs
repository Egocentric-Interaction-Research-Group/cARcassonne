using System;
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
            Rules = new GameRules();
        }
    }
}