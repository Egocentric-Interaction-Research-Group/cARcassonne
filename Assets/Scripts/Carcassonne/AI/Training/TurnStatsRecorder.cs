using System.Collections.Generic;
using Carcassonne.State;
using Unity.MLAgents;
using UnityEngine;

namespace Carcassonne.AI.Training
{
    public class TurnStatsRecorder : MonoBehaviour
    {
        public GameState state;
        public GameLog log;
        private StatsRecorder stats => Academy.Instance.StatsRecorder;

        public void OnTurnEnd(Turn t)
        {
            var statsDict = new Dictionary<string, float>();
            
            statsDict.Add($"Meeples/Remaining (P{t.player.id})", t.meeplesRemaining);
            statsDict.Add($"Points/Own Gain (P{t.player.id})", t.pointDifference[t.player].scoredPoints);
            statsDict.Add($"Points/Own Unscored Gain (P{t.player.id})", t.pointDifference[t.player].unscoredPoints);
            statsDict.Add($"Points/Own Potential Gain (P{t.player.id})", t.pointDifference[t.player].potentialPoints);

            foreach (var kvp in statsDict)
            {
                stats.Add(kvp.Key, kvp.Value, StatAggregationMethod.Histogram);
            }
        }
    }
}