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
        public GameObject[] tileArray;

        /// <summary>
        ///     The next tile
        /// </summary>
        private int nextTile;

        /// <summary>
        ///     An array of ID's.
        /// </summary>
        private int[] tiles;


        /// <summary>
        ///     Shuffles the array of tiles.
        /// </summary>
        /// <returns></returns>
        private void Shuffle(int[] randomIndex)
        {
            //System.Random rand = new System.Random();

            for (var i = tileArray.Length - 2; i > 0; i--)
            {
                //int randomIndex = rand.Next(0, i + 1);
                var temp = tileArray[i];
                tileArray[i] = tileArray[randomIndex[i]];
                tileArray[randomIndex[i]] = temp;
            }

            // return this.tiles;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public GameObject Pop()
        {
            var tile = tileArray[nextTile];


            nextTile--;

            return tile;
        }

        public int GetTileCount()
        {
            return nextTile;
        }

        /// <summary>
        /// </summary>
        /// <returns></returns>
        public StackScript createStackScript()
        {
            //setAll();
            return this;
        }

        public void PopulateTileArray()
        {
            //randomIndex = new int[84];
            tileArray = GameObject.FindGameObjectsWithTag("Tile");
            nextTile = tileArray.Length - 1;

            if (PhotonNetwork.IsMasterClient)
            {
                var tmpRandomIndexArray = new int[84];
                var rand = new Random();
                for (var i = tileArray.Length - 2; i > 0; i--) tmpRandomIndexArray[i] = rand.Next(0, i + 1);
                photonView.RPC("GetRandomIndexRPC", RpcTarget.All, tmpRandomIndexArray);
            }

            if (randomIndexArray != null) Shuffle(randomIndexArray);
        }

        [PunRPC]
        public void GetRandomIndexRPC(int[] random)
        {
            randomIndexArray = random;
        }
    }
}