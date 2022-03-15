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
        private TileController tileController => GetComponent<TileController>();

        public UnityEvent OnGameStart = new UnityEvent();
        public UnityEvent OnTurnEnd = new UnityEvent();
        public UnityEvent OnTurnStart = new UnityEvent();
        public UnityEvent<FeatureGraph> OnFeatureCompleted = new UnityEvent<FeatureGraph>();
        public UnityEvent OnGameOver = new UnityEvent();
        public UnityEvent OnScoreChanged = new UnityEvent();

        public List<UnityEventBase> Events => new List<UnityEventBase>()
        {
            OnGameStart,
            OnTurnEnd,
            OnTurnStart,
            OnFeatureCompleted,
            OnGameOver,
            OnScoreChanged
        };

        public List<UnityEventBase> AllEvents => Events.Concat(tileController.Events).Concat(meepleController.Events).ToList();

        #region ConvenienceProperties

        public Player player => state.Players.Current;

        public RectInt bounds => throw new NotImplementedException();

        public int Turn { get; private set; }

        #endregion

        /// <summary>
        /// Starts a new game of Carcassonne.
        /// </summary>
        public void NewGame(IList<Player> players, IList<Meeple> meeples, Stack<Tile> tiles)
        {
            // Clear the board
            state.Reset();
            
            // Reset turns
            Turn = 0;

            // Shuffle the deck
            // CreateAndShuffleDeck();
            state.Tiles.Remaining = tiles;
            Debug.Log($"Tiles loaded. {state.Tiles.Remaining.Count} remaining.");
            
            // Set up players
            state.Players.All = players;
            
            // Setup meeples
            state.Meeples.All = meeples;
            
            OnGameStart.Invoke();

            NewTurn();
        }

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
                var scores = ScoreFeatures(features);
                UpdateScores(scores);
                FreeMeeplesInFeatures(features);
                
                //Update potential scores
                var incompleteFeatures = state.Features.IncompleteWithMeeples.ToList();
                var unscoredPoints = ScoreFeatures(incompleteFeatures);
                var potentialPoints = ScoreFeatures(incompleteFeatures, potential: true);

                foreach (var player in unscoredPoints.Keys)
                {
                    player.unscoredPoints = unscoredPoints[player];
                    player.potentialPoints = potentialPoints[player];
                }

                foreach (var feature in features)
                {
                    OnFeatureCompleted.Invoke(feature);
                }
                
                OnTurnEnd.Invoke();

                // PrintTurnToLogWindow();

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

        // private void PrintTurnToLogWindow()
        // {
        //     var _dims0 = state.Tiles.Matrix.GetLength(0);
        //     var _dims1 = state.Tiles.Matrix.GetLength(1);
        //     var _length = state.Tiles.Matrix.Length;
        //     var _origin = state.Tiles.MatrixOrigin;
        //     // var _tiles = state.Tiles;
        //     var _cityBounds = state.Features.Cities;//[0]; //TODO Something is broken here.
        //     
        //     Debug.Log($"Board Matrix Dims: {_dims0}" +
        //               $"x{_dims1}" +
        //               $" ({_length})\n" +
        //               $"Board Matrix Origin: {_origin}\n" +
        //               // $"Board Matrix:\n{_tiles}\n" +
        //               $"City Bounds: {_cityBounds}");
        // }

        public void NewTurn()
        {
            state.Players.Next();

            state.phase = Phase.NewTurn;
            state.Tiles.Current = null;
            state.Meeples.Current = null;

            Turn += 1;

            OnTurnStart.Invoke();
        }

        public void GameOver()
        {
            Debug.Log("Game Over.");
            var features = state.Features.Incomplete;
            var scores = ScoreFeatures(features);
            UpdateScores(scores);
            FreeMeeplesInFeatures(features);

            state.phase = Phase.GameOver;

            OnGameOver.Invoke();
        }

        /// <summary>
        /// Calculates scores, assigns points to players, and frees meeples from features.
        /// Should be called with a list of newly completed features after each turn OR at the end of the game for all
        /// incomplete features.
        /// </summary>
        /// <param name="features"></param>
        /// <param name="potential"></param>
        internal IDictionary<Player, int> ScoreFeatures(IEnumerable<FeatureGraph> features, bool potential=false)
        {
            Dictionary<Player, int> scores = state.Players.All.ToDictionary(p => p, p=> 0);
            
            foreach (var f in features)
            {
                var meeples = this.state.Meeples.InFeature(f).ToList();

                var playerMeeples = meeples.GroupBy(m => m.player);
                var playerMeepleCount = playerMeeples.ToDictionary(g => g.Key, g => g.Count());

                // Select all players with the number of meeples in the feature equal to the top number of meeples.
                var scoringPlayers = playerMeepleCount.Where(kvp => kvp.Value == playerMeepleCount.Values.Max())
                    .Select((kvp => kvp.Key));

                // Calculate points for those that are finished
                foreach (var p in scoringPlayers)
                {
                    if (potential)
                    {
                        scores[p] += f.PotentialPoints;
                    }
                    else
                    {
                        scores[p] += f.Points;
                    }
                }
            }
            
            return scores;
        }

        public void UpdateScores(IDictionary<Player, int> scoringPlayers)
        {
            // Calculate points for those that are finished
            foreach (var kvp in scoringPlayers)
            {
                var player = kvp.Key;
                var score = kvp.Value;
                player.score += score;
            }
        
            if (scoringPlayers.Any())
            {
                OnScoreChanged.Invoke();
            }
        }
        
        public void FreeMeeplesInFeatures(IEnumerable<FeatureGraph> features)
        {
            foreach (var f in features)
            {
                var meeples = this.state.Meeples.InFeature(f).ToList();
        
                // Free meeples
                foreach (var m in meeples)
                {
                    meepleController.Free(m);
                }
            }
        }
    }
}