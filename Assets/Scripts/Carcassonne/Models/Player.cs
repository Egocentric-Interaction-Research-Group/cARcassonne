using System.Collections.Generic;
using UnityEngine;

namespace Carcassonne.Models
{
    public class Player : MonoBehaviour
    {
        public int id;
        public string name;
        public bool isAI;
        public int score;
        
        private void Awake()
        {
            score = 0;
        }
    }
}