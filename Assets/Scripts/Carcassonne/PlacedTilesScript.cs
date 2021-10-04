using UnityEngine;

namespace Carcassonne
{
    /// <summary>
    /// Class encapsulating information about tiles that have been played on the board.
    /// </summary>
    public class PlacedTilesScript : MonoBehaviour
    {
        public Vector3 BasePosition;
    

        private GameObject[,] placedTiles;

        private void Start()
        {
        }

        public void InstansiatePlacedTilesArray()
        {
            placedTiles = new GameObject[170, 170];
        }

        public void PlaceTile(int x, int z, GameObject tile)
        {
            placedTiles[x, z] = tile;
        }

        public void removeTile(int x, int z)
        {
            placedTiles[x, z] = null;
        }

        public GameObject getPlacedTiles(int x, int z)
        {
            return placedTiles[x, z];
        }


        public int GetLength(int dimension)
        {
            return placedTiles.GetLength(dimension);
        }

        public bool HasNeighbor(int x, int z)
        {
            if (x + 1 < placedTiles.GetLength(0))
                if (placedTiles[x + 1, z] != null)
                    return true;
            if (x - 1 >= 0)
                if (placedTiles[x - 1, z] != null)
                    return true;
            if (z + 1 < placedTiles.GetLength(1))
                if (placedTiles[x, z + 1] != null)
                    return true;
            if (z - 1 >= 0)
                if (placedTiles[x, z - 1] != null)
                    return true;
            return false;
        }

        public bool MatchGeographyOrNull(int x, int y, PointScript.Direction dir, TileScript.Geography geography)
        {
            if (placedTiles[x, y] == null)
                return true;
            if (placedTiles[x, y].GetComponent<TileScript>().getGeographyAt(dir) == geography)
                return true;
            return false;
        }

        public bool CityTileHasCityCenter(int x, int y)
        {
            return placedTiles[x, y].GetComponent<TileScript>().getCenter() == TileScript.Geography.City ||
                   placedTiles[x, y].GetComponent<TileScript>().getCenter() == TileScript.Geography.CityRoad;
        }

        public bool CityTileHasGrassOrStreamCenter(int x, int y)
        {
            return placedTiles[x, y].GetComponent<TileScript>().getCenter() == TileScript.Geography.Grass ||
                   placedTiles[x, y].GetComponent<TileScript>().getCenter() == TileScript.Geography.Stream;
        }

        //Hämtar grannarna till en specifik tile
        public int[] GetNeighbors(int x, int y)
        {
            var Neighbors = new int[4];
            var itt = 0;


            if (placedTiles[x + 1, y] != null)
            {
                Neighbors[itt] = placedTiles[x + 1, y].GetComponent<TileScript>().vIndex;
                itt++;
            }

            if (placedTiles[x - 1, y] != null)
            {
                Neighbors[itt] = placedTiles[x - 1, y].GetComponent<TileScript>().vIndex;
                itt++;
            }

            if (placedTiles[x, y + 1] != null)
            {
                Neighbors[itt] = placedTiles[x, y + 1].GetComponent<TileScript>().vIndex;
                itt++;
            }

            if (placedTiles[x, y - 1] != null) Neighbors[itt] = placedTiles[x, y - 1].GetComponent<TileScript>().vIndex;
            return Neighbors;
        }

        public TileScript.Geography[] getWeights(int x, int y)
        {
            var weights = new TileScript.Geography[4];
            var itt = 0;
            if (placedTiles[x + 1, y] != null)
            {
                weights[itt] = placedTiles[x + 1, y].GetComponent<TileScript>().West;
                itt++;
            }

            if (placedTiles[x - 1, y] != null)
            {
                weights[itt] = placedTiles[x - 1, y].GetComponent<TileScript>().East;
                itt++;
            }

            if (placedTiles[x, y + 1] != null)
            {
                weights[itt] = placedTiles[x, y + 1].GetComponent<TileScript>().South;
                itt++;
            }

            if (placedTiles[x, y - 1] != null) weights[itt] = placedTiles[x, y - 1].GetComponent<TileScript>().North;
            return weights;
        }

        public TileScript.Geography[] getCenters(int x, int y)
        {
            var centers = new TileScript.Geography[4];
            var itt = 0;
            if (placedTiles[x + 1, y] != null)
            {
                centers[itt] = placedTiles[x + 1, y].GetComponent<TileScript>().getCenter();
                itt++;
            }

            if (placedTiles[x - 1, y] != null)
            {
                centers[itt] = placedTiles[x - 1, y].GetComponent<TileScript>().getCenter();
                itt++;
            }

            if (placedTiles[x, y + 1] != null)
            {
                centers[itt] = placedTiles[x, y + 1].GetComponent<TileScript>().getCenter();
                itt++;
            }

            if (placedTiles[x, y - 1] != null) centers[itt] = placedTiles[x, y - 1].GetComponent<TileScript>().getCenter();
            return centers;
        }

        public PointScript.Direction[] getDirections(int x, int y)
        {
            var directions = new PointScript.Direction[4];
            var itt = 0;
            if (placedTiles[x + 1, y] != null)
            {
                directions[itt] = PointScript.Direction.EAST;
                itt++;
            }

            if (placedTiles[x - 1, y] != null)
            {
                directions[itt] = PointScript.Direction.WEST;
                itt++;
            }

            if (placedTiles[x, y + 1] != null)
            {
                directions[itt] = PointScript.Direction.NORTH;
                itt++;
            }

            if (placedTiles[x, y - 1] != null) directions[itt] = PointScript.Direction.SOUTH;
            return directions;
        }

        public int CheckSurroundedCloister(int x, int z, bool endTurn)
        {
            var pts = 1;
            if (placedTiles[x - 1, z - 1] != null) pts++;
            if (placedTiles[x - 1, z] != null) pts++;
            if (placedTiles[x - 1, z + 1] != null) pts++;
            if (placedTiles[x, z - 1] != null) pts++;
            if (placedTiles[x, z + 1] != null) pts++;
            if (placedTiles[x + 1, z - 1] != null) pts++;
            if (placedTiles[x + 1, z] != null) pts++;
            if (placedTiles[x + 1, z + 1] != null) pts++;
            if (pts == 9 || endTurn)
                return pts;
            return 0;
        }

        public bool CheckNeighborsIfTileCanBePlaced(GameObject tile, int x, int y)
        {
            var script = tile.GetComponent<TileScript>();
            var isNotAlone2 = false;

            if (placedTiles[x - 1, y] != null)
            {
                isNotAlone2 = true;
                if (script.West == placedTiles[x - 1, y].GetComponent<TileScript>().East) return false;
            }

            if (placedTiles[x + 1, y] != null)
            {
                isNotAlone2 = true;
                if (script.East == placedTiles[x + 1, y].GetComponent<TileScript>().West) return false;
            }

            if (placedTiles[x, y - 1] != null)
            {
                isNotAlone2 = true;
                if (script.South == placedTiles[x, y - 1].GetComponent<TileScript>().North) return false;
            }

            if (placedTiles[x, y + 1] != null)
            {
                isNotAlone2 = true;
                if (script.North == placedTiles[x, y + 1].GetComponent<TileScript>().South) return false;
            }

            return isNotAlone2;
        }

        //Kontrollerar att tilen får placeras på angivna koordinater
        public bool TilePlacementIsValid(GameObject tile, int x, int z)
        {
            var script = tile.GetComponent<TileScript>();
            var isNotAlone = false;

            //Debug.Log(placedTiles[x - 1, z]);

            if (placedTiles[x - 1, z] != null)
            {
                isNotAlone = true;
                if (script.West != placedTiles[x - 1, z].GetComponent<TileScript>().East) return false;
            }

            if (placedTiles[x + 1, z] != null)
            {
                isNotAlone = true;
                if (script.East != placedTiles[x + 1, z].GetComponent<TileScript>().West) return false;
            }

            if (placedTiles[x, z - 1] != null)
            {
                isNotAlone = true;
                if (script.South != placedTiles[x, z - 1].GetComponent<TileScript>().North) return false;
            }

            if (placedTiles[x, z + 1] != null)
            {
                isNotAlone = true;
                if (script.North != placedTiles[x, z + 1].GetComponent<TileScript>().South) return false;
            }

            if (placedTiles[x, z] != null) return false;
            return isNotAlone;
        }
    }
}