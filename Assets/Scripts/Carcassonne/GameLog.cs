using System.Collections.Generic;

namespace Carcassonne
{
    public struct Turn
    {
        public PlayerScript.Player player;
        public TileScript tile;
        public int[] location;
        public PointScript.Direction meeplePlacement;
    }
    
    public class GameLog
    {
        public LinkedList<Turn> turns;
    }
}