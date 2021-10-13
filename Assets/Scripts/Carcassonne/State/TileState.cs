using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Carcassonne.State
{
    [CreateAssetMenu(fileName = "TileState", menuName = "States/TileState")]
    public class TileState : ScriptableObject
    {
        public List<TileScript> Remaining;
        [CanBeNull] public TileScript Current;
        public TileScript[,] Played;

        private void Awake()
        {
            Remaining = new List<TileScript>();
            Current = null;
        }
    }
}