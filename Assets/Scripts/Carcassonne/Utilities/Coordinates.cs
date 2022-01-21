using UnityEngine;

namespace Carcassonne.Utilities
{
    public static class Coordinates
    {
        public static Vector2Int TileToSubTile(Vector2Int position, Vector2Int direction)
        {
            return position * 3 + direction + Vector2Int.one;
        }

        public static Vector2Int SubTileToTile(Vector2Int subTilePosition)
        {
            return subTilePosition / 3;
        }

        public static Vector2Int SubTileToDirection(Vector2Int subTilePosition)
        {
            (_, var direction) = SubTileToBoard(subTilePosition);

            return direction;
        }
        
        public static (Vector2Int position, Vector2Int direction) SubTileToBoard(Vector2Int subTilePosition)
        {
            var position = SubTileToTile(subTilePosition);
            var direction = (subTilePosition - 3 * position) - Vector2Int.one;

            Debug.Assert(new Vector2Int(3,3) / 3 == Vector2Int.one, "(3,3) / 3 should be (1,1)"); // Basic Test
            Debug.Assert(new Vector2Int(4,4) / 3 == Vector2Int.one, "(4,4) / 3 should be (1,1)"); // Remainder Test
            Debug.Assert(new Vector2Int(4,4) - 3 * (new Vector2Int(4,4) / 3) == Vector2Int.one, "(4,4) - 3 * (4,4)/ 3 should be (1,1)"); // Remainder Test
            Debug.Assert(new Vector2Int(5,5) - 3 * (new Vector2Int(5,5) / 3) == 2*Vector2Int.one, "(5,5) - 3 * (5,5)/ 3 should be (2,2)"); // Remainder Test

            return (position, direction);
        }
        
        
    }
}