using System.Collections.Generic;
using System.Linq;
using Carcassonne.State.Features;
using Photon.Pun;
using UnityEngine;

namespace Carcassonne.State
{
    [CreateAssetMenu(fileName = "FeatureState", menuName = "States/FeatureState")]
    public class FeatureState : ScriptableObject
    {
        public BoardGraph Graph = new BoardGraph();
        
        public List<City> Cities = new List<City>();
        // public List<Road> roads;
        // public List<Chapel> chapels;
        
        private void Awake()
        {
            Cities = new List<City>();
            Graph = new BoardGraph();
        }

        public IFeature GetFeatureAt(Vector2Int position, Vector2Int direction)
        {
            throw new System.NotImplementedException();
        }
    }
}