using System;
using Carcassonne.Models;
using UnityEngine;

namespace Carcassonne.State
{
    // [CreateAssetMenu(fileName = "GameState", menuName = "States/GameState")]
    public class GameState : MonoBehaviour
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

        public GridMapper grid;

        // private void Awake()
        // {
        //     Reset();
        // }

        public void Reset()
        {
            Debug.Log("Resetting Game State...");
            
            Rules = new GameRules();
            Tiles = new TileState();
            Meeples = new MeepleState();
            Features = new FeatureState(Meeples, grid);
            Players = new PlayerState();
        }

        // private void OnEnable()
        // {
        //     Reset();
        // }
    }
}