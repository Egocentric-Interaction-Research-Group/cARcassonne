using UnityEngine;

namespace Carcassonne.Models
{
    public class GridMapper : MonoBehaviour
    {
        public Grid tile;
        public Grid meeple;

        public Vector2Int TileToMeeple(Vector2Int cell)
        {
            var cell3D = To3D(cell);
            var meepleCell = meeple.WorldToCell(tile.CellToWorld(cell3D));
            return To2D(meepleCell);
        }
        
        public Vector2Int TileToMeeple(Vector2Int cell, Vector2Int direction)
        {
            var meepleCell = TileToMeeple(cell);
            return meepleCell + direction + Vector2Int.one;
        }

        public Vector2Int MeepleToTile(Vector2Int cell)
        {
            var cell3D = To3D(cell);
            var tileCell = tile.WorldToCell(meeple.CellToWorld(cell3D));
            return To2D(tileCell);
        }
        
        public (Vector2Int cell, Vector2Int direction) MeepleToTileDirection(Vector2Int cell)
        {
            var tile = MeepleToTile(cell);
            var direction = MeepleToDirection(cell);

            return (tile, direction);
        }
        
        /// <summary>
        /// Direction is in the four cardinal directions (Up, Down, Left, Right) and their variations (Centre, Left/Up, etc.) 
        /// </summary>
        /// <param name="cell"></param>
        /// <returns></returns>
        public Vector2Int MeepleToDirection(Vector2Int cell)
        {
            var cell3D = To3D(cell);
            var meepleCellCentreWorld = meeple.GetCellCenterWorld(cell3D);
            var cellCenterLocal = tile.WorldToLocal(meepleCellCentreWorld);
            var tileInterp = tile.LocalToCellInterpolated(cellCenterLocal);
            var tileInterpFraction = tileInterp - Vector3Int.FloorToInt(tileInterp);

            var direction = (tileInterpFraction - (Vector3.one * 0.5f)) * 3.0f; // Expands values from 0.5 to 1
            // Debug.Log($"Cell {cell} -> 3D {cell3D} -> CellCentreWorld {meepleCellCentreWorld} -> CellCentreLocal {cellCenterLocal} -> interp {tileInterp} -> frac {tileInterpFraction} -> dir {direction} -> {To2D(Vector3Int.RoundToInt(direction))}");

            return To2D(Vector3Int.RoundToInt(direction));
        }
        
        private Vector3Int To3D(Vector2Int cell)
        {
            // return new Vector3Int(cell.x, 0, cell.y);
            return (Vector3Int)cell;
        }
        
        private Vector2Int To2D(Vector3Int cell)
        {
            return (Vector2Int)cell;
        }

        private void Start()
        {
            TestGridMapper();
        }

        private void TestGridMapper()
        {
            Debug.Assert((Vector2Int)(new Vector3Int(1, 2, 3)) == new Vector2Int(1,2), "Cast doesn't work like I think it does.");
            
            // TileToMeeple
            Debug.Assert(TileToMeeple(Vector2Int.zero) == Vector2Int.zero, "(0,0) T != (0,0) M");
            Debug.Assert(TileToMeeple(Vector2Int.one) == 3 * Vector2Int.one, "(1,1) T == (3,3) M");
            Debug.Assert(TileToMeeple(-Vector2Int.one) == -3 * Vector2Int.one, "(-1,-1) T == (-3,-3) M");
            
            // MeepleToTile
            Debug.Assert(MeepleToTile(Vector2Int.zero) == Vector2Int.zero, "(0,0) M != (0,0) T");
            Debug.Assert(MeepleToTile(Vector2Int.one) == Vector2Int.zero, "(1,1) M != (0,0) T");
            Debug.Assert(MeepleToTile(2*Vector2Int.one) == Vector2Int.zero, "(2,2) M != (0,0) T");
            Debug.Assert(MeepleToTile(3*Vector2Int.one) == Vector2Int.one, "(3,3) M != (1,1) T");
            Debug.Assert(MeepleToTile(-Vector2Int.one) == -Vector2Int.one, "(-1,-1) M != (-1,-1) T");
            Debug.Assert(MeepleToTile(-3*Vector2Int.one) == -Vector2Int.one, "(-3,-3) M != (-1,-1) T");
            
            // MeepleToDirection
            Debug.Assert(MeepleToDirection(Vector2Int.zero) == -Vector2Int.one, "(0,0) M != (-1,-1) D"); //
            Debug.Assert(MeepleToDirection(Vector2Int.one) == Vector2Int.zero, "(1,1) M != (0,0) D");
            Debug.Assert(MeepleToDirection(2*Vector2Int.one) == Vector2Int.one, "(2,2) M != (1,1) D"); //
            Debug.Assert(MeepleToDirection(-Vector2Int.one) == Vector2Int.one, "(-1,-1) M != (1,1) D"); //
            Debug.Assert(MeepleToDirection(3*Vector2Int.one) == -Vector2Int.one, "(3,3) M != (-1,-1) D"); //
            Debug.Assert(MeepleToDirection(7*Vector2Int.one) == Vector2Int.zero, "(7,7) M != (0,0) D");
            Debug.Assert(MeepleToDirection(new Vector2Int(2,4)) == Vector2Int.right, "(2,4) M != (1,0) D"); //
        }
    }
}