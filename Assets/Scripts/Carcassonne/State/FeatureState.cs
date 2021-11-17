using System.Collections.Generic;
using System.Linq;
using Carcassonne.State.Features;
using UnityEngine;

namespace Carcassonne.State
{
    [CreateAssetMenu(fileName = "FeatureState", menuName = "States/FeatureState")]
    public class FeatureState : ScriptableObject
    {
        public List<City> Cities = new List<City>();
        // public List<Road> roads;
        // public List<Chapel> chapels;
        
        private void Awake()
        {
            Cities = new List<City>();
        }
    }
}