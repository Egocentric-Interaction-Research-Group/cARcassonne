using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Carcassonne.State.Features
{
    public class City : IFeature
    {
        public Dictionary<Vector2Int, TileScript> segments; // TODO Rename
        public Dictionary<Vector2Int, MeepleScript> meeples; // TODO Rename

        public int Segments => segments.Count;
        public int OpenSides => ComputeOpenSides();
        public int Shields => segments.Values.Count(t => t.Shield);
        public bool Complete => complete;
        public bool Completable => IsCompletable();
        public Dictionary<PlayerScript, int> Meeples => CountMeeplesForPlayers();

        private bool complete = false;

        private int ComputeOpenSides()
        {
            var openSides = 0;

            //TODO Calculate open sides here.
            foreach (KeyValuePair<Vector2Int,TileScript> segment in segments)
            {
                
            }

            if (openSides == 0)
            {
                complete = true;
            }
            
            throw new System.NotImplementedException();
        }

        private bool IsCompletable()
        {
            if (Complete) return true;
            throw new System.NotImplementedException();
        }

        private Dictionary<PlayerScript, int> CountMeeplesForPlayers()
        {
            throw new System.NotImplementedException();
        }

        public void Add(Vector2Int xy, TileScript t)
        {
            segments.Add(xy, t);
        }

        public void Add(Vector2Int xy, TileScript t, MeepleScript m)
        {
            segments.Add(xy, t);
            meeples.Add(xy, m);
        }
        
        /// <summary>
        /// Overload the plus operator to allow merge of two cities.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        /// <example>
        /// City a = new City();
        /// City b = new City();
        /// // Add tiles (and meeples) to cities.
        /// a += b;
        /// </example>
        public static City operator +(City a, City b)
        {   
            // Merge Tile Lists
            b.segments.ToList().ForEach(pair => a.segments[pair.Key] = pair.Value);
            
            //Merge meeple lists
            b.meeples.ToList().ForEach(pair => a.meeples[pair.Key] = pair.Value);
            
            return a;
        }
    }
}