using System;
using System.Collections.Generic;
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

        public int scoreChange => score - previousScore;
        
        private void Awake()
        {
            score = 0;
        }

        public void OnNewTurn()
        {
            previousScore = score;
        }
    }
}