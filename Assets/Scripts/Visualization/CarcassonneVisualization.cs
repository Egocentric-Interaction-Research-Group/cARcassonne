//------------------------------------------------------------------------------------------------//
// Author:   Kasper Skott
// Created:  2021-10-22
// Modified: 2021-12-01
//------------------------------------------------------------------------------------------------//

using UnityEngine;
using System.Collections.Generic;
using Carcassonne.Models;
using Carcassonne.State;

// using Geography = Tile.Geography;
// using Direction = Carcassonne.Point.Direction;

namespace Carcassonne
{
    /// <summary>
    /// A method parameter struct that enforces a certain array size and handles erroneous input sizes to 
    /// <see cref="CarcassonneVisualization.UpdateMaterial(Tile[,], Vector2Int, IReadOnlyList{Meeple})"/>
    /// </summary>
    public readonly struct VisualizationInputTiles
    {
        public readonly Tile[,] tiles;
        
        public VisualizationInputTiles(Tile[,] input)
        {
            const int dims = CarcassonneVisualization.MAX_BOARD_DIMENSION;
            int inWidth    = input.GetLength(0);
            int inHeight   = input.GetLength(1);
            tiles          = new Tile[dims, dims];
            
            // Fill out internal array with input array, and pad with null if needed.
            bool noRow;
            for (int row = 0; row < dims; row++)
            {
                // Check whether the row exists in the input array.
                noRow = row >= inHeight;

                for (int col = 0; col < dims; col++)
                {
                    if (noRow || col >= inWidth) // Pad with null if out of bounds of the input array.
                        tiles[col, row] = null;
                    else
                        tiles[col, row] = input[col, row];
                }
            }
        }
    }

    /// <summary>
    /// This script is to be used in combination with the shader "CarcassoneVisualization".
    /// </summary>
    [RequireComponent(typeof(Renderer))]
    public class CarcassonneVisualization : MonoBehaviour
    {
        internal const int MAX_BOARD_DIMENSION = 31; // The maximum nbr of tiles in each axis. This is limited by the shader.

        internal static readonly Dictionary<Vector2Int, int> DIRECTION_MULTIPLIER = new Dictionary<Vector2Int, int>()
        {
            {Vector2Int.zero,  1},//tile.Center
            {Vector2Int.right, 10},//tile.East  
            {Vector2Int.up,    100},//tile.North 
            {Vector2Int.left,  1000},//tile.West  
            {Vector2Int.down,  10000},//tile.South 
        };

        private Material m_mat;
        public GameState state;

        /// <summary>
        /// Initializes the board with empty tile and meeple data.
        /// If initial data with the max size is not sent initially, the size of the
        /// first call to <see cref="UpdateMaterial(Tile[,], Vector2Int, IReadOnlyList{Meeple})"/> 
        /// will be set as the max size,
        /// and will not grow any larger.
        /// </summary>
        public void Init()
        {
            m_mat = GetComponent<Renderer>().material;

            const int boardSize = MAX_BOARD_DIMENSION * MAX_BOARD_DIMENSION;
            float[] tilesInit = new float[boardSize];
            for (int i = 0; i < boardSize; i++)
                tilesInit[i] = -1.0f;

            m_mat.SetFloatArray("_TileGeography", tilesInit);
            m_mat.SetFloatArray("_MeeplePlacement", tilesInit);
        }

        /// <summary>
        /// Use this method to send tile and meeple data to the shader.
        /// 
        /// If you're already sending a subsection of all tile data, consider
        /// using UpdateMaterial instead. However, you will have to know the
        /// current offset of your tiles into the original tile array of all tiles.
        /// </summary>
        /// <param name="allTiles">The entire board of tiles. May very well include null elements.</param>
        /// <param name="allMeeples">All meeples in the game, even if they're not placed.</param>
        public void VisualizeBoard()
        {
            TileState tiles = state.Tiles;
            MeepleState meeples = state.Meeples;
            Tile[,] allTiles = tiles.Played;
                
            // Get boundaries of the played tiles so as to only bother with placed tiles.
            // (Vector2Int size, Vector2Int offset) = GetPlayedTileBounds(state.Tiles);

            // Create a wrapper for the tiles, which checks and handles erroneous array sizes.
            VisualizationInputTiles inputTiles = new VisualizationInputTiles(allTiles);

            // Update the material instance to display only the bounds of played tiles,
            // and provide all meeples (free or not). meeples are automatically placed according
            // to the given offset.
            //
            // Note: the inputTiles parameter still expects the entire grid of tiles, but they
            // are culled off by the size and offset parameters.
            UpdateMaterial(inputTiles, state.Tiles.Limits.size, state.Tiles.Limits.position, meeples);
        }

        // /// <summary>
        // /// Gets the boundaries of existing tiles in the given array.
        // /// The offset is the left-most and upper-most existing tiles.
        // /// The size how many tiles the boundary spans in each direction
        // /// from that offset.
        // /// </summary>
        // /// <param name="tiles">The tile state.</param>
        // /// <returns>size (width, height), and offset (x, y).</returns>
        // public static (Vector2Int, Vector2Int) GetPlayedTileBounds(TileState tiles)
        // {
        //     return (tiles.Limits.size, tiles.Limits.position);
        // }
        // public static (Vector2Int, Vector2Int) GetPlayedTileBounds(Tile[,] allTiles)
        // {
        //     Vector2Int dims = new Vector2Int(allTiles.GetLength(0), allTiles.GetLength(1));
        //
        //     int minRow = int.MaxValue;
        //     int minCol = int.MaxValue;
        //     int maxRow = int.MinValue;
        //     int maxCol = int.MinValue;
        //     for (int row = 0; row < dims.y; row++)
        //     {
        //         for (int col = 0; col < dims.x; col++)
        //         {
        //             if (allTiles[col, row] == null) // Valid tile
        //                 continue;
        //
        //             if (minRow == int.MaxValue) // Has not yet found upper-most
        //                 minRow = row;
        //
        //             if (col < minCol)
        //                 minCol = col;
        //
        //             if (row >= maxRow)
        //                 maxRow = row + 1;
        //
        //             if (col >= maxCol)
        //                 maxCol = col + 1;
        //         }
        //     }
        //
        //     Vector2Int size = new Vector2Int(maxCol - minCol, maxRow - minRow);
        //     Vector2Int offset = new Vector2Int(minCol, minRow);
        //     return (size, offset);
        // }

        /// <summary>
        /// Gets a subsection of the specified 2-dimensional array.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="arr2d">The array to slice from.</param>
        /// <param name="size">The size of the subsection.</param>
        /// <param name="offset">Where in arr2d to begin slicing.</param>
        /// <returns>A new 2D array of the specified size.</returns>
        // public static T[,] Get2DSubSection<T>(T[,] arr2d, Vector2Int size, Vector2Int offset)
        // {
        //     T[,] section = new T[size.x, size.y];
        //     for (int y = 0; y < size.y; y++)
        //         for (int x = 0; x < size.x; x++)
        //             section[x, y] = arr2d[x + offset.x, y + offset.y];
        //
        //     return section;
        // }

        /// <summary>
        /// Updates the material to display the given section of tiles.
        /// </summary>
        /// <param name="inputTiles">A wrapped 2d array of the entire grid of tiles.</param>
        /// <param name="displaySize">The 2-dimensional nbr of tiles to be displayed.
        ///     Specifies how much of inputTiles to display.</param>
        /// <param name="displayOffset">The tile-space offset of the tiles to be displayed
        ///     into the entire tile array.</param>
        /// <param name="allMeeples">A list of all meeples.</param>
        public void UpdateMaterial(
            VisualizationInputTiles inputTiles,
            Vector2Int displaySize,
            Vector2Int displayOffset,
            // IReadOnlyList<Meeple> allMeeples)
            MeepleState meeples)
        {
            // Tile[,] allTiles = inputTiles.tiles;
            // Vector2Int updateDim = new Vector2Int(
            //     allTiles.GetLength(0), allTiles.GetLength(1));
            
            // Size adjustment
            displaySize += 3*Vector2Int.one;
            displayOffset -= Vector2Int.one;

            // Make sure to always display 1:1 column-row ratio.
            if (displaySize.x < displaySize.y)
                displaySize.x = displaySize.y;
            else
                displaySize.y = displaySize.x;

            // Send the display size to the shader. This determines how many tiles of the inputted
            // tiles are shown on the visualization board.
            m_mat.SetInt("_DisplayColumns", displaySize.x);
            m_mat.SetInt("_DisplayRows", displaySize.y);

            // Send the offsets to the shader.
            // m_mat.SetInt("_ColumnOffset", displayOffset.x);
            // m_mat.SetInt("_RowOffset", displayOffset.y);

            // var playerMeeples = CreateMeepleDictionary(displaySize, displayOffset, meeples);

            // Prepare two arrays to send to the shader. One array contains the geographies of each,
            // while the other contains the player id associated with each direction of each tile
            // (this indicates that the player of the given id has placed a meeple there).
            // All geography data is encoded into a single float, as is the meeple/player id data
            // (one float for geograpies, one float for meeples).
            // Note: Needs to send *float* arrays to the shader because shaders seem to use floats
            //   internally anyway, and there is no interface for sending int arrays.
            float[] tileArray   = new float[MAX_BOARD_DIMENSION * MAX_BOARD_DIMENSION];
            float[] meeplesArray = new float[MAX_BOARD_DIMENSION * MAX_BOARD_DIMENSION];
            RectInt bbox = new RectInt(0, 0, MAX_BOARD_DIMENSION, MAX_BOARD_DIMENSION);
            
            for (int i = 0; i < tileArray.Length; i++) // Fill arrays with default values
            {
                tileArray[i] = -1.0f;
                meeplesArray[i] = 0.0f;
            }

            foreach ( var kvp in state.Tiles.Placement)
            {
                var cell = kvp.Key - displayOffset;
                var tile = kvp.Value;

                if (!bbox.Contains(cell))
                {
                    Debug.LogWarning($"Tile cell {cell} is outside of the bounding box {bbox}");
                    continue;
                }

                var idx = cell.x + cell.y * MAX_BOARD_DIMENSION;
                
                float tileGeography = (float)tile.Center;
                tileGeography      += (float)tile.East  * 10;
                tileGeography      += (float)tile.North * 100;
                tileGeography      += (float)tile.West  * 1000;
                tileGeography      += (float)tile.South * 10000;
                
                // Store in the arrays that will be sent to the shader.
                tileArray[idx]   = tileGeography;
            }
            
            foreach ( var kvp in state.Meeples.Placement)
            {
                var meepleCell = kvp.Key;
                var meeple = kvp.Value;
                (var cell, var direction) = state.grid.MeepleToTileDirection(meepleCell);
                cell -= displayOffset;
                
                if (!bbox.Contains(cell))
                {
                    Debug.LogWarning($"Meeple cell {cell} is outside of the bounding box {bbox}");
                    continue;
                }

                var idx = cell.x + cell.y * MAX_BOARD_DIMENSION;
                
                // playerMeeples = {(x,y): [0, 0, 2, 0, 0]} -> [0, 0, 200, 0, 0] -> 200
                // Set tileMeeple value. If meeple belongs to player 1, 10; player 2, 100; player 3, 1000; etc.
                // float tileMeeple = (float)Math.Pow(10, meeple.player.id);
                float tileMeeple = (meeple.player.id + 1) * DIRECTION_MULTIPLIER[direction];

                // Store in the arrays that will be sent to the shader.
                meeplesArray[idx] = tileMeeple;
            }
            // for (int row = 0; row < MAX_BOARD_DIMENSION; row++)
            // {
            //     for (int col = 0; col < MAX_BOARD_DIMENSION; col++)
            //     {
            //         int idx = col + row * MAX_BOARD_DIMENSION;
            //
            //         // Handle if the inputted size is exceeded. This should not happen, btw.
            //         if (col >= updateDim.x || row >= updateDim.y)
            //         {
            //             tileArray[idx] = -1.0f;
            //             meeplesArray[idx] = 0.0f;
            //             continue;
            //         }
            //
            //         // Only set values for existing tiles.
            //         if (allTiles.GetValue(col, row) is Tile t)
            //         {
            //             // Combine all 5 tile geographies into a single float.
            //             float tileGeography = (float)t.Center;
            //             tileGeography      += (float)t.East  * 10;
            //             tileGeography      += (float)t.North * 100;
            //             tileGeography      += (float)t.West  * 1000;
            //             tileGeography      += (float)t.South * 10000;
            //
            //             int[] playersIds;
            //             float tileMeeple;
            //             if (!playerMeeples.TryGetValue((col, row), out playersIds))
            //             {
            //                 tileMeeple = 0; // Default to no meeple in any direction.
            //             }
            //             else
            //             {
            //                 // Combine all meeple placement on this tile into a single float.
            //                 tileMeeple  = playersIds[0];
            //                 tileMeeple += playersIds[1] * 10;
            //                 tileMeeple += playersIds[2] * 100;
            //                 tileMeeple += playersIds[3] * 1000;
            //                 tileMeeple += playersIds[4] * 10000;
            //             }
            //
            //             // Store in the arrays that will be sent to the shader.
            //             tileArray[idx]   = tileGeography;
            //             meeplesArray[idx] = tileMeeple;
            //         }
            //         else // Invalid or non-existent Tile.
            //         {
            //             tileArray[idx] = -1.0f;
            //             meeplesArray[idx] = 0.0f;
            //         }
            //     }
            // }

            m_mat.SetFloatArray("_TileGeography", tileArray);
            m_mat.SetFloatArray("_MeeplePlacement", meeplesArray);
        }

        private Dictionary<(int, int), int[]> CreateMeepleDictionary(Vector2Int displaySize, Vector2Int displayOffset,
            MeepleState meeples)
        {
            // Create a dictionary with key (col, row) and value array (playerId+1 for each direction).
            // This maps a certain location with the player that occupies it.
            Dictionary<(int, int), int[]> playerMeeples = new Dictionary<(int, int), int[]>();

            foreach (var kvp in meeples.Placement)
            {
                var meepleCell = kvp.Key;
                var meeple = kvp.Value;

                (var cell, var direction) = state.grid.MeepleToTileDirection(meepleCell);
                cell -= displayOffset;
                
                Debug.Assert(meeple != null, "Meeple is null");
                Debug.Assert(meeple.player != null, "Meeple player is null");;
                
                int[] playersAtDirections = new int[5];
                if (direction == Vector2Int.zero) playersAtDirections[0] = meeple.player.id + 1;
                else if (direction == Vector2Int.right) playersAtDirections[1] = meeple.player.id + 1;
                else if (direction == Vector2Int.up) playersAtDirections[2] = meeple.player.id + 1;
                else if (direction == Vector2Int.left) playersAtDirections[3] = meeple.player.id + 1;
                else if (direction == Vector2Int.down) playersAtDirections[4] = meeple.player.id + 1;

                playerMeeples[(cell.x, cell.y)] = playersAtDirections;
            }

            return playerMeeples;
        }

        // private Dictionary<(int, int), int[]> CreateMeepleDictionary(Vector2Int displaySize, Vector2Int displayOffset, IReadOnlyList<Meeple> allMeeples)
        // {
        //     // Create a dictionary with key (col, row) and value array (playerId+1 for each direction).
        //     // This maps a certain location with the player that occupies it.
        //     Dictionary<(int, int), int[]> playerMeeples = new Dictionary<(int, int), int[]>();
        //     foreach (Meeple m in allMeeples)
        //     {
        //         if (m.free)
        //             continue;
        //
        //         // Convert meeple x and z into coordinates local to the displayOffset.
        //         // Example: meeple.x = 2; displayOffset.x = 2; meepleLocalX = 0.
        //         int meepleLocalX = m.x - displayOffset.x;
        //         int meepleLocalY = m.z - displayOffset.y;
        //
        //         (int, int) absoluteLocation = (m.x, m.z); // Location on the entire grid of tiles.
        //
        //         // Cull meeples that are outside the dimensions shown.
        //         if (meepleLocalX < 0 || meepleLocalX >= displaySize.x ||
        //             meepleLocalY < 0 || meepleLocalY >= displaySize.y)
        //             continue;
        //
        //
        //         int[] playersAtDirections;
        //         if (!playerMeeples.TryGetValue(absoluteLocation, out playersAtDirections))
        //         {
        //             playersAtDirections = new int[5];
        //             for (int i = 0; i < 5; i++)
        //                 playersAtDirections[i] = 0;
        //         }
        //
        //         if (m.direction == Direction.CENTER) playersAtDirections[0] = m.playerId + 1;
        //         else if (m.direction == Direction.EAST) playersAtDirections[1] = m.playerId + 1;
        //         else if (m.direction == Direction.NORTH) playersAtDirections[2] = m.playerId + 1;
        //         else if (m.direction == Direction.WEST) playersAtDirections[3] = m.playerId + 1;
        //         else if (m.direction == Direction.SOUTH) playersAtDirections[4] = m.playerId + 1;
        //
        //         playerMeeples[absoluteLocation] = playersAtDirections;
        //     }
        //
        //     return playerMeeples;
        // }

        #if TESTING // Can probably be deleted.
        //---- FOR TESTING -----------------------------------------------------------------------//

        /// <summary>
        /// Updates the material with test data.
        /// This is just for testing things quickly without a real data set.
        /// </summary>
        private void UpdateWithTestData()
        {
            // Fills a new Tile with the given Geography.
            Func<Geography, Tile>
            CreateTile = (geo) =>
            {
                Tile tile = new Tile();
                tile.East = geo;
                tile.North = geo;
                tile.West = geo;
                tile.South = geo;
                tile.Center = geo;
                return tile;
            };

            int showColumns = 31;
            int showRows = 31;

            // Create and fill every tile with grass by default.
            Tile[,] tiles = new Tile[showColumns, showRows];
            for (int row = 0; row < showRows; row++)
                for (int col = 0; col < showColumns; col++)
                    tiles[col, row] = CreateTile(Geography.Field);

            tiles[1, 0] = null;
            tiles[2, 0] = null;
            tiles[3, 0] = null;
            tiles[4, 0] = null;
            tiles[4, 3] = null;
            tiles[4, 4] = null;
            tiles[0, 4] = null;
            tiles[3, 4] = null;
            tiles[0, 3] = null;

            tiles[0, 0].Center = Geography.Road;
            tiles[0, 0].East = Geography.Road;
            tiles[0, 0].South = Geography.Road;

            tiles[0, 1].North = Geography.Road;
            tiles[0, 1].Center = Geography.Village;
            tiles[0, 1].South = Geography.Road;

            tiles[0, 2].West = Geography.City;
            tiles[0, 2].North = Geography.Road;
            tiles[0, 2].Center = Geography.Road;
            tiles[0, 2].East = Geography.Road;

            tiles[1, 2].West = Geography.Road;
            tiles[1, 2].Center = Geography.Road;
            tiles[1, 2].East = Geography.Road;

            tiles[2, 1].North = Geography.Road;
            tiles[2, 1].Center = Geography.Road;
            tiles[2, 1].South = Geography.Road;

            tiles[2, 2].West = Geography.Road;
            tiles[2, 2].North = Geography.Road;
            tiles[2, 2].South = Geography.Road;
            tiles[2, 2].East = Geography.Road;
            tiles[2, 2].Center = Geography.Village;

            tiles[2, 3].North = Geography.Road;
            tiles[2, 3].Center = Geography.Road;
            tiles[2, 3].South = Geography.Road;

            tiles[2, 4].North = Geography.Road;
            tiles[2, 4].Center = Geography.Village;

            tiles[3, 1].Center = Geography.Cloister;

            tiles[3, 2].West = Geography.Road;
            tiles[3, 2].Center = Geography.City;
            tiles[3, 2].East = Geography.City;

            tiles[4, 2].West = Geography.City;
            tiles[4, 2].Center = Geography.City;
            tiles[4, 2].South = Geography.City;
            tiles[4, 2].East = Geography.City;

            tiles[15, 15].Center = Geography.City;
            tiles[15, 15].South = Geography.City;
            tiles[15, 15].East = Geography.City;

            List<Meeple> meeples = new List<Meeple>(); // Should actually use something like MeepleState.All.
            meeples.Add(new Meeple());
            meeples[0].assignAttributes(0, 1, Direction.CENTER, Geography.Field);
            meeples[0].free = false;
            meeples[0].playerId = 0;

            meeples.Add(new Meeple());
            meeples[1].assignAttributes(1, 1, Direction.EAST, Geography.City);
            meeples[1].free = false;
            meeples[1].playerId = 1;

            meeples.Add(new Meeple());
            meeples[2].assignAttributes(2, 1, Direction.NORTH, Geography.City);
            meeples[2].free = false;
            meeples[2].playerId = 2;

            meeples.Add(new Meeple());
            meeples[3].assignAttributes(3, 1, Direction.WEST, Geography.City);
            meeples[3].free = false;
            meeples[3].playerId = 3;

            meeples.Add(new Meeple());
            meeples[4].assignAttributes(4, 1, Direction.SOUTH, Geography.City);
            meeples[4].free = false;
            meeples[4].playerId = 4;

            VisualizationInputTiles inputTiles = new VisualizationInputTiles(tiles);

            UpdateMaterial(inputTiles, 
                new Vector2Int(5, 5), 
                new Vector2Int(0, 0), 
                meeples);
        }
        #endif
    }
}