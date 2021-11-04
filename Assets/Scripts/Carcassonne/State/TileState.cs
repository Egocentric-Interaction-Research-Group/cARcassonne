using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Carcassonne.State
{
    public class Limits
    {
        public Limits()
        {
            this.min = new Vector2Int(int.MaxValue, int.MaxValue);
            this.max = new Vector2Int(int.MinValue, int.MinValue);
        }

        public Vector2Int min;
        public Vector2Int max;
    }
    
    [CreateAssetMenu(fileName = "TileState", menuName = "States/TileState")]
    public class TileState : ScriptableObject
    {
        public List<TileScript> Remaining;
        [CanBeNull] public TileScript Current;
        public TileScript[,] Played;

        public TileScript.Geography?[,] Matrix => CalculateMatrix();

        private TileScript.Geography?[,] CalculateMatrix()
        {
            // Find min and max indices for x and 
            // Are these things that TileScript could be tracking? BoardLimits? or something?
            Limits l = CalculateLimits();
            int xmin = l.min.x, xmax = l.max.x, ymin = l.min.y, ymax = l.max.y; //FIXME this is a placeholder
            
            Debug.Log($"Found Limits. Tiles found from ({xmin},{ymin}) to ({xmax},{ymax})");
            
            // Create a new matrix that is 3 * (xmax-xmin) x 3 * (ymax - ymin);
            var GeographyMatrix = new TileScript.Geography?[3 * (xmax - xmin), 3 * (ymax - ymin)];
            
            for (var i = 0; i < xmax - xmin; i++)
            {
                for (int j = 0; j < ymax - ymin; j++)
                {
                    for (int a = 0; a < 3; a++)
                    {
                        for (int b = 0; b < 3; b++)
                        {
                            if (Played[i + xmin, j + ymin] is null)
                            {
                                GeographyMatrix[i * 3 + a, j * 3 + b] = null;
                            }
                            else
                            {
                                GeographyMatrix[i * 3 + a, j * 3 + b] = Played[i + xmin, j + ymin].Matrix[a, b];
                            }
                        }
                    }
                }
            }
            
            return GeographyMatrix;
        }

        private Limits CalculateLimits()
        {
            Limits l = new Limits();
            for (var i = 0; i < Played.GetLength(0); i++)
            {
                for (var j = 0; j < Played.GetLength(1); j++)
                {
                    if (Played[i, j] != null)
                    {
                        if (i < l.min.x)
                            l.min.x = i;
                        if (i >= l.max.x)
                            l.max.x = i+1;
                        if (j < l.min.y)
                            l.min.y = j;
                        if (j >= l.max.y)
                            l.max.y = j+1;
                    }
                }
            }

            return l;
        }

        private void Awake()
        {
            Remaining = new List<TileScript>();
            Current = null;
        }

        public override string ToString()
        {
            var s = ""; 
            for (var j = Matrix.GetLength((1)) - 1; j >= 0; j--) // Prints from the top-left, so y coordinate needs to start from the top.
            {
                for (var i = 0; i < Matrix.GetLength(0); i++)
                {
                    s += $" | {Matrix[i,j], 8}";
                }
                s += $" |\n";
            }
            return s;
        }
    }
}