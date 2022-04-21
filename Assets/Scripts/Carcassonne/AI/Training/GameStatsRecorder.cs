using System.Collections.Generic;
using System.Linq;
using Carcassonne.State;
using Carcassonne.Utilities;
using Unity.MLAgents;
using UnityEngine;

namespace Carcassonne.AI.Training
{
    public class GameStatsRecorder : MonoBehaviour
    {
        public GameState state;
        public GameLog log;
        private StatsRecorder stats => Academy.Instance.StatsRecorder;

        private IEnumerable<Turn> P0Turns => log.Turns.Where(t => t.player.id == 0);
        private IEnumerable<Turn> P1Turns => log.Turns.Where(t => t.player.id == 1);

        // Point statistics
        private int CompletedCities => state.Features.Cities.Count(c => c.Complete);
        private int CompletedRoads => state.Features.Roads.Count(r => r.Complete);
        private int CompletedCloisters => state.Features.Cloisters.Count(c => c.Complete);
        private float AvgCompletedCityPoints => CompletedCities > 0 ? (float)state.Features.Cities.Where(c => c.Complete).Average(c => c.Points) : 0f;
        private float AvgCompletedRoadPoints => CompletedRoads > 0 ? (float)state.Features.Roads.Where(r => r.Complete).Average(r => r.Points) : 0f;
        private int TotalCityPoints => state.Features.Cities.Sum(c => c.Points);
        private int TotalRoadPoints => state.Features.Roads.Sum(r => r.Points);
        private int TotalCloisterPoints => state.Features.Cloisters.Sum(c => c.Points);

        private int TilesDiscarded => state.Tiles.Discarded.Count;
        
        //TODO Sequence contains no elements. Insert GameOver delay (like Turn delay coroutine)
        private float AvgMeeplesRemainingPerTurn => (float)P0Turns.Average(t => t.meeplesRemaining);
        // private float AvgMeepleTurnsOnFeature;
        
        private float AvgPointGainPerOwnTile => (float)P0Turns.Average(t => t.pointDifference[t.player].scoredPoints);
        private float AvgPointGainPerOtherTile => (float)P0Turns.Average(t => t.pointDifference.Where(kvp => kvp.Key != t.player).
            Average(pair => pair.Value.scoredPoints));
        private float AvgUnscoredPointGainPerOwnTile => (float)P0Turns.Average(t => t.pointDifference[t.player].unscoredPoints);
        private float AvgUnscoredPointGainPerOtherTile => (float)P0Turns.Average(t => t.pointDifference.Where(kvp => kvp.Key != t.player).
            Average(pair => pair.Value.unscoredPoints));
        private float AvgPotentialPointGainPerOwnTile => (float)P0Turns.Average(t => t.pointDifference[t.player].potentialPoints);
        private float AvgPotentialPointGainPerOtherTile => (float)P0Turns.Average(t => t.pointDifference.Where(kvp => kvp.Key != t.player).
            Average(pair => pair.Value.potentialPoints));

        private float P0Score => (float)state.Players.All.Single(p => p.id == 0).FinalScore;
        private float P1Score => (float)state.Players.All.Single(p => p.id == 1).FinalScore;
        private float Winner => P1Score > P0Score ? 1.0f : 0.0f;
        
        public void OnGameOver()
        {
            stats.Add("Features/Completed Cities", CompletedCities);
            stats.Add("Features/Completed Roads", CompletedRoads);
            stats.Add("Features/Completed Cloisters", CompletedCloisters);
            
            stats.Add("Points/Points per Completed City (Avg)", AvgCompletedCityPoints);
            stats.Add("Points/Points per Completed Road (Avg)", AvgCompletedRoadPoints);
            
            stats.Add("Points/Total City Points", TotalCityPoints);
            stats.Add("Points/Total Road Points", TotalRoadPoints);
            stats.Add("Points/Total Cloister Points", TotalCloisterPoints);
            
            stats.Add("Tiles/Discarded Tiles", TilesDiscarded);
            
            stats.Add("Meeples/Meeples Remaining per Turn (Avg)", AvgMeeplesRemainingPerTurn);
            
            stats.Add("Player/Point gain per Own Turn", AvgPointGainPerOwnTile);
            stats.Add("Player/Unscored point gain per Own Turn", AvgUnscoredPointGainPerOwnTile);
            stats.Add("Player/Potential point gain per Own Turn", AvgPotentialPointGainPerOwnTile);
            
            stats.Add("Opponent/Point gain per Opponent Turn", AvgPointGainPerOtherTile);
            stats.Add("Opponent/Unscored point gain per Opponent Turn", AvgUnscoredPointGainPerOtherTile);
            stats.Add("Opponent/Potential point gain per Opponent Turn", AvgPotentialPointGainPerOtherTile);
            
            stats.Add("Players/0", P0Score);
            stats.Add("Players/1", P1Score);
            stats.Add("Players/Winner", Winner);
        }
    }
}