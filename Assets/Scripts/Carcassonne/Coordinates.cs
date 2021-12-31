using UnityEngine;

namespace Carcassonne
{
    public static class Coordinates
    {
        public static Vector2Int TileToSubTile(Vector2Int position, Vector2Int direction)
        {
            return position * 3 + direction + Vector2Int.one;
        }

        public static Vector2Int SubTileToTile(Vector2Int subTilePosition)
        {
            throw new System.NotImplementedException(); 
        }

        public static Vector2Int SubTileToDirection(Vector2Int subTilePosition)
        {
            throw new System.NotImplementedException();
        }
        
        public static (Vector2Int position, Vector2Int direction) SubTileToBoard(Vector2Int subTilePosition)
        {
            throw new System.NotImplementedException();
        }
        
        
    }
}