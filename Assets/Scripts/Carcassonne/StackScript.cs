using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Photon.Pun;
using UnityEngine;
using Random = System.Random;

// TODO: Why not use an actual Stack object? Or two lists?

// Game State requires a count of remaining tiles, and a list of remaining tiles. Possible search by criteria?

namespace Carcassonne
{
    /// <summary>
    ///     The Stack of tiles.
    /// </summary>
    public class StackScript : MonoBehaviourPun
    {
        /// <summary>
        ///     A reference to the prefab Tile, to be used later.
        /// </summary>
        public GameObject tile;

        public Transform basePositionTransform;

        public int[] randomIndexArray;

        /// <summary>
        ///     The array of tiles
        /// </summary>
        public List<GameObject> tileArray;

        public GameObject firstTile;
        public List<TileScript> remaining;
        [CanBeNull] public TileScript current;
        public TileScript[,] played;
        
        /// <summary>
        /// </summary>
        /// <returns></returns>
        public GameObject Pop()
        {
            var rand = new Random();
            var idx = rand.Next(remaining.Count);
            
            photonView.RPC("PopRPC", RpcTarget.All, idx);
            
            return current.gameObject;
        }
        
        [PunRPC]
        public void PopRPC(int idx)
        {
            current = remaining[idx];
            remaining.Remove(current);
        }

        public bool isEmpty()
        {
            return remaining.Count == 0;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public StackScript createStackScript()
        {
            //setAll();
            return this;
        }
        
        /// <summary>
        /// Populates the array of tiles. Finds all game objects tagged tile. The Master client sets 
        /// </summary>
        public void PopulateTileArray()
        {
            //randomIndex = new int[84];
            tileArray = new List<GameObject>(GameObject.FindGameObjectsWithTag("Tile"));
            // Filter out tiles not in set. TODO: This should reference the game rules and pick relevant sets.
            tileArray = tileArray.Where(t => t.GetComponent<TileScript>().tileSet == TileScript.TileSet.Base && t != firstTile ).ToList();
            
            foreach (var t in tileArray)
            {
                remaining.Add(t.GetComponent<TileScript>());
            }
            
            Debug.Log($"Tile array is populated. {remaining.Count} items remain in the stack.");

        }

    }
}