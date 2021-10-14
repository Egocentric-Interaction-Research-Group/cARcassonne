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

        public TileScript.Geography[,] Matrix => CalculateMatrix();

        private TileScript.Geography[,] CalculateMatrix()
        {
            // Find min and max indices for x and 
            // Are these things that TileScript could be tracking? BoardLimits? or something?
            int xmin = 0, xmax = 0, ymin = 0, ymax = 0; //FIXME this is a placeholder
            
            // Create a new matrix that is 3 * (xmax-xmin) x 3 * (ymax - ymin);
            var GeographyMatrix = new TileScript.Geography[3 * (xmax - xmin), 3 * (ymax - ymin)];

            //TODO Handle null tiles
            for (var i = 0; i < xmax - xmin; i++)
            {
                for (int j = 0; j < ymax - ymin; j++)
                {
                    for (int a = 0; a < 3; a++)
                    {
                        for (int b = 0; b < 3; b++)
                        {
                            GeographyMatrix[i * 3 + a, j * 3 + b] = Played[i+xmin,j+ymin].Matrix[a, b];
                        }
                    }
                }
            }
            
            return GeographyMatrix;
        }

        private void Awake()
        {
            Remaining = new List<TileScript>();
            Current = null;
        }
    }
}