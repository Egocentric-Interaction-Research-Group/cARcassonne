using System;
using System.Collections.Generic;
using UnityEngine;

namespace Carcassonne.State
{

    public struct GameRules
    {
        private static readonly Dictionary<int, int> BaseTileCounts = new Dictionary<int, int>()
        {
            {1, 4},
            {2, 2},
            {3, 8},
            {4, 9},
            {5, 4},
            {6, 1},
            {7, 5},
            {8, 3},
            {9, 3},
            {10, 3},
            {11, 3},
            {12, 1},
            {13, 3},
            {14, 3},
            {15, 2},
            {16, 3},
            {17, 2},
            {18, 2},
            {19, 2},
            {20, 3},
            {21, 1},
            {22, 1},
            {23, 2},
            {24, 1},
        };

        private static readonly int BaseTileID = 8;
        public const int MeeplesPerPlayer = 7;

        public const int BoardSize = 40;
        public static readonly RectInt BoardLimits = new RectInt(0, 0, BoardSize, BoardSize);

        public bool Abbots;
        public bool River;
        public bool Farmer;

        public IDictionary<int, int> GetTileIDCounts()
        {
            if (River)
            {
                throw new NotImplementedException("River tiles are not implemented.");
            }

            return BaseTileCounts;
        }
        public int GetStartingTileID()
        {
            if (River)
            {
                throw new NotImplementedException("River tiles are not implemented.");
            }

            return BaseTileID;
        }
        
    }
}