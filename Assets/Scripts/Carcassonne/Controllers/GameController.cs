using System;
using System.Collections.Generic;
using System.Linq;
using Carcassonne.Models;
using Carcassonne.State;
using Carcassonne.State.Features;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

namespace Carcassonne.Controllers
{
    [RequireComponent(typeof(GameState), typeof(MeepleController))]
    public class GameController : MonoBehaviour
    {
        // private GameControllerScript _gameControllerScript;
        //
        // public GameController(GameControllerScript gameControllerScript)
        // {
        //     _gameControllerScript = gameControllerScript;
        // }

        /// <summary>
        /// Stores the full state of the game for processing.
        /// </summary>
        public GameState state => GetComponent<GameState>();
        private MeepleController meepleController => GetComponent<MeepleController>();

        public UnityEvent OnGameStart = new UnityEvent();
        public UnityEvent OnTurnEnd = new UnityEvent();
        public UnityEvent OnTurnStart = new UnityEvent();
        public UnityEvent<FeatureGraph> OnFeatureCompleted = new UnityEvent<FeatureGraph>();
        public UnityEvent OnGameOver = new UnityEvent();
        public UnityEvent OnScoreChanged = new UnityEvent();

        #region ConvenienceProperties

        public Player player => state.Players.Current;

        public RectInt bounds => throw new NotImplementedException();

        #endregion

        /// <summary>
        /// Starts a new game of Carcassonne.
        /// </summary>
        public void NewGame(int nPlayers)
        {
            // Clear the board
            state.Reset();
            
            // Shuffle the deck
            // CreateAndShuffleDeck();
            
            // Set up players
            throw new NotImplementedException();
            
            // Setup meeples
            
            OnGameStart.Invoke();
        }
        
        /// <summary>
        /// Starts a new game of Carcassonne.
        /// </summary>
        public void NewGame(IList<Player> players, IList<Meeple> meeples, Stack<Tile> tiles)
        {
            // Clear the board
            state.Reset();
            
            // Shuffle the deck
            // CreateAndShuffleDeck();
            state.Tiles.Remaining = tiles;
            Debug.Log($"Tiles loaded. {state.Tiles.Remaining.Count} remaining.");
            
            // Set up players
            state.Players.All = players;
            
            // Setup meeples
            state.Meeples.All = meeples;
            
            // Place first tile
            
            OnGameStart.Invoke();

            NewTurn();
        }

        // private void CreateAndShuffleDeck()
        // {
        //     List<Tile> tiles = new List<Tile>();
        //     foreach (var kvp in state.Rules.GetTileIDCounts())
        //     {
        //         var id = kvp.Key;
        //         var count = kvp.Value;
        //         Debug.Log($"Creating {count} tiles with ID {id}. {state.Tiles.Remaining.Count} tiles in the deck.");
        //         
        //         // Create representations of all of the tiles
        //         for (int i = 0; i < count; i++)
        //         {
        //             tiles.Add(Tile.CreateTile(id));
        //         }
        //
        //         // Shuffle and add to the remaining tiles deck
        //         while (tiles.Count > 0)
        //         {
        //             var idx = Random.Range(0, tiles.Count);
        //             state.Tiles.Remaining.Push(tiles[idx]);
        //             tiles.RemoveAt(idx);
        //         }
        //         
        //         // Push the starting tile
        //         state.Tiles.Remaining.Push(Tile.CreateTile(state.Rules.GetStartingTileID()));
        //     }
        // }

        /// <summary>
        /// End the current players turn. Calculate any points acquired by placement of tile and/or meeple and move
        /// from phase TileDown or MeepleDown to either NewTurn or if there are no more tiles that can be drawn, end the game through
        /// GameOver()
        /// </summary>
        public bool EndTurn()
        {
            if (state.phase == Phase.TileDown || state.phase == Phase.MeepleDown)
            {
                //TODO: Do this by event instead.
                // Log.LogTurn();

                // Check finished features
                var features = state.Features.CompleteWithMeeples.ToList();
                ScoreFeatures(features);

                foreach (var feature in features)
                {
                    OnFeatureCompleted.Invoke(feature);
                }
                
                OnTurnEnd.Invoke();

                LogTurn();

                if (state.Tiles.Remaining.Count == 0)
                {
                    GameOver();
                    return true;
                }
                
                NewTurn();

                return true;

            }
            
            // Couldn't end turn.
            return false;
        }

        private void LogTurn()
        {
            var _dims0 = state.Tiles.Matrix.GetLength(0);
            var _dims1 = state.Tiles.Matrix.GetLength(1);
            var _length = state.Tiles.Matrix.Length;
            var _origin = state.Tiles.MatrixOrigin;
            // var _tiles = state.Tiles;
            var _cityBounds = state.Features.Cities;//[0]; //TODO Something is broken here.
            
            Debug.Log($"Board Matrix Dims: {_dims0}" +
                      $"x{_dims1}" +
                      $" ({_length})\n" +
                      $"Board Matrix Origin: {_origin}\n" +
                      // $"Board Matrix:\n{_tiles}\n" +
                      $"City Bounds: {_cityBounds}");
        }

        public void NewTurn()
        {
            state.Players.Next();

            state.phase = Phase.NewTurn;
            state.Tiles.Current = null;
            state.Meeples.Current = null;

            OnTurnStart.Invoke();
        }

        public void GameOver()
        {
            Debug.Log("Game Over.");
            var features = state.Features.Incomplete;
            ScoreFeatures(features);

            state.phase = Phase.GameOver;

            OnGameOver.Invoke();
        }

        /// <summary>
        /// Calculates scores, assigns points to players, and frees meeples from features.
        /// Should be called with a list of newly completed features after each turn OR at the end of the game for all
        /// incomplete features.
        /// </summary>
        /// <param name="features"></param>
        public void ScoreFeatures(IEnumerable<FeatureGraph> features)
        {
                foreach (var f in features)
                {
                    var meeples = this.state.Meeples.InFeature(f).ToList();

                    var playerMeeples = meeples.GroupBy(m => m.player);
                    var playerMeepleCount = playerMeeples.ToDictionary(g => g.Key, g => g.Count());

                    var scoringPlayers = playerMeepleCount.Where(kvp => kvp.Value == playerMeepleCount.Values.Max())
                        .Select((kvp => kvp.Key));

                    // Calculate points for those that are finished
                    foreach (var p in scoringPlayers)
                    {
                        p.score += f.Points;
                    }

                    if (scoringPlayers.Count() > 0)
                    {
                        OnScoreChanged.Invoke();
                    }
                    
                    // Free meeples
                    foreach (var m in meeples)
                    {
                        Debug.LogWarning("Rethink this. GameController should not reference MeepleController");
                        meepleController.Free(m);
                    }
                }
        }
    }
}