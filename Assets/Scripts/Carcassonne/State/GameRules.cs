using UnityEngine;

namespace Carcassonne.State
{

    public struct GameRules
    {
        public const int BoardSize = 40;
        public static readonly RectInt BoardLimits = new RectInt(0, 0, BoardSize, BoardSize);

        public bool Abbots;
        public bool River;
        public bool Farmer;
        
    }
}