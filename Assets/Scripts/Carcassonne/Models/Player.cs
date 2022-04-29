using UnityEngine;

namespace Carcassonne.Models
{
    public class Player : MonoBehaviour
    {
        public int id;
        public string username;
        public bool isAI;
        public int score;
        public int previousScore = 0;
        public int unscoredPoints;
        public int previousUnscoredPoints = 0;
        public int potentialPoints;
        public int previousPotentialPoints = 0; 
        
        public int FinalScore => score + unscoredPoints;
        
        public int scoreChange => score - previousScore;
        public int unscoredPointsChange => unscoredPoints - previousUnscoredPoints;
        public int potentialPointsChange => potentialPoints - previousPotentialPoints;
        
        private void Awake()
        {
            score = 0;
        }

        public void OnNewTurn()
        {
            Debug.Log($"EOT New Turn (P{id}). Setting previous points. Score: {score}, Unscored Points: {unscoredPoints}, Potential Points: {potentialPoints} " +
                      $"prev: {previousScore}, prevUnscore: {previousUnscoredPoints}, prevPot: {previousPotentialPoints}, " +
                      $"dScore: {scoreChange}, dUnscore: {unscoredPointsChange}, dPot: {potentialPointsChange}");
            previousScore = score;
            previousUnscoredPoints = unscoredPoints;
            previousPotentialPoints = potentialPoints;
        }
    }
}