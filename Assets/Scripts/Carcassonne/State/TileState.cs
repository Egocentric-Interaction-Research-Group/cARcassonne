using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace Carcassonne.State
{
    // public class Limits
    // {
    //     public Limits()
    //     {
    //         this.min = new Vector2Int(int.MaxValue, int.MaxValue);
    //         this.max = new Vector2Int(int.MinValue, int.MinValue);
    //     }
    //
    //     public Vector2Int min;
    //     public Vector2Int max;
    // }
    
    [CreateAssetMenu(fileName = "TileState", menuName = "States/TileState")]
    public class TileState : ScriptableObject
    {
        public List<TileScript> Remaining;
        [CanBeNull] public TileScript Current;
        public TileScript[,] Played;
        
        public Vector2 lastPlayedPosition;

        public TileScript.Geography?[,] Matrix => CalculateMatrix();

        private TileScript.Geography?[,] CalculateMatrix()
        {
            // Find min and max indices for x and 
            // Are these things that TileScript could be tracking? BoardLimits? or something?
            //var l = CalculateLimits();
            RectInt l = new RectInt();
            l.xMin = 65;
            l.xMax = 105;
            l.yMin = 65;
            l.yMax = 105;
            int xmin = l.xMin, xmax = l.xMax, ymin = l.yMin, ymax = l.yMax;
            
            //Debug.Log($"Found Limits. Tiles found from ({xmin},{ymin}) to ({xmax},{ymax})");
            
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

        private RectInt CalculateLimits()
        {
            RectInt lim = new RectInt();
            for (var i = 0; i < Played.GetLength(0); i++)
            {
                for (var j = 0; j < Played.GetLength(1); j++)
                {
                    if (Played[i, j] != null)
                    {
                        if (lim.size == Vector2Int.zero)
                        {
                            lim = new RectInt(i, j, 1, 1);
                        } else
                        {
                            if (i <  lim.xMin){ lim.xMin = i;}
                            if (i >= lim.xMax){ lim.xMax = i + 1;}
                            if (j <  lim.yMin){ lim.yMin = j;}
                            if (j >= lim.yMax){ lim.yMax = j + 1;}
                        }
                    }
                }
            }

            return lim;
        }

        private void Awake()
        {
            Remaining = new List<TileScript>();
            Current = null;
        }

        public override string ToString()
        {
            var s = "";


            for (int i = 0 ; i < Matrix.GetLength(0) ; i++)
            {
                for (int j = 0; j < Matrix.GetLength(1); j++)
                {
                    if (j == Matrix.GetLength(0) - 1)
                    {
                        if (Matrix[i, j] == null)
                        {
                            s += $"{"0",0}";
                        }
                        else
                            s += $"{Matrix[i, j],0}";
                    }
                    else
                    {
                        if (Matrix[i, j] == null)
                        {
                            s += $"{"0,",0}";
                        }
                        else
                            s += $"{Matrix[i, j],0},";
                    }
                }
                s += $"\n";

            }

            /*
            for (int j = Matrix.GetLength(1) - 1; j >= 0; j--) // Prints from the top-left, so y coordinate needs to start from the top.
            {
                for (int i = 0; i < Matrix.GetLength(0); i++)
                {
                    if (i == Matrix.GetLength(0) - 1)
                    {
                        if (Matrix[i, j] == null)
                        {
                            s += $"{"0",0}";
                        }
                        else
                            s += $"{Matrix[i, j],0}";
                    }
                    else
                    {
                        if (Matrix[i, j] == null)
                        {
                            s += $"{"0,",0}";
                        }
                        else
                            s += $"{Matrix[i, j],0},";
                    }
                }
                s += $"\n";

            }
            */
            return s;
        }
    }
}