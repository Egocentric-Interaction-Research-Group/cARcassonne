using System;
using UnityEngine;

namespace Carcassonne.Models
{
    public class Player : MonoBehaviour, IComparable<Player>
    {
        
        /// <summary>
        /// A unique identifier for the player
        /// </summary>
        public int id;
        
        /// <summary>
        /// A unique, human-readable name for the player
        /// </summary>
        public string username;
        
        /// <summary>
        /// Player's current score as it appears on the Carcassonne scorecard.
        /// Includes only points gained from completed features.
        /// </summary>
        public int score;
        
        /// <summary>
        /// Player's score at the previous turn.
        /// Used to calculate the point gain at every turn.
        /// Note that this is updated every turn (regardless of player) so that it reflects the points after the previous tile.
        /// This will always be \<= @score.
        /// </summary>
        public int previousScore = 0;
        
        /// <summary>
        /// Points that the player would gain *if* the game were over.
        /// This reflects the points of features (@Carcassonne.State.Feature.City, @Carcassonne.State.Feature.Road, and
        /// @Carcassonne.State.Feature.Cloister elements) that the player currently controls (or shares control of).
        /// These points are not guaranteed for the player (they can lose control of the feature) and they don't reflect
        /// the increase in city points that are gained when a player completes a city before the end of the game.
        /// </summary>
        public int unscoredPoints;
        
        /// <summary>
        /// Player's unscored points at the end of the previous turn.
        /// This differs from @previousScore in that it **can** be < @unscoredPoints if a feature is completed or
        /// control is lost over the feature in a turn. 
        /// </summary>
        public int previousUnscoredPoints = 0;
        public int potentialPoints;
        public int previousPotentialPoints = 0; 
        
        /// <summary>
        /// Player's score if the game were over.
        /// </summary>
        /// <remarks>The final score is the sum of the current @Carcassonne.Models.Player.score and the remaining @unscoredPoints.</remarks>
        public int FinalScore => score + unscoredPoints;
        
        public int scoreChange => score - previousScore;
        public int unscoredPointsChange => unscoredPoints - previousUnscoredPoints;
        public int potentialPointsChange => potentialPoints - previousPotentialPoints;
        
        private void Awake()
        {
            score = 0;
        }

        public void UpdateScores()
        {
            Debug.Log($"EOT New Turn (P{id}). Setting previous points. Score: {score}, Unscored Points: {unscoredPoints}, Potential Points: {potentialPoints} " +
                      $"prev: {previousScore}, prevUnscore: {previousUnscoredPoints}, prevPot: {previousPotentialPoints}, " +
                      $"dScore: {scoreChange}, dUnscore: {unscoredPointsChange}, dPot: {potentialPointsChange}");
            previousScore = score;
            previousUnscoredPoints = unscoredPoints;
            previousPotentialPoints = potentialPoints;
        }

        public int CompareTo(Player other)
        {
            if (ReferenceEquals(this, other)) return 0;
            if (ReferenceEquals(null, other)) return 1;
            return id.CompareTo(other.id);
        }
    }
}