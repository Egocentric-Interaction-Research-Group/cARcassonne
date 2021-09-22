using Photon.Pun;
using UnityEngine;
using Random = System.Random;

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
    /// </summary>
    /// <param name="tiles"></param>
    /// <returns></returns>
    public int[] generateIDs(int[] tiles)
    {
        tiles = new int[84];
        var counter = 0;
        var array = new int[33];

        array[0] = 4;
        array[1] = 2;
        array[2] = 8;
        array[3] = 9;
        array[4] = 4;
        array[5] = 1;
        array[6] = 5;
        array[7] = 4;
        array[8] = 3;
        array[9] = 3;
        array[10] = 3;
        array[11] = 1;
        array[12] = 3;
        array[13] = 3;
        array[14] = 2;
        array[15] = 3;
        array[16] = 2;
        array[17] = 2;
        array[18] = 2;
        array[19] = 3;
        array[20] = 1;
        array[21] = 1;
        array[22] = 2;
        array[23] = 1;
        array[24] = 2;
        array[25] = 2;
        array[26] = 2;
        array[27] = 1;
        array[28] = 1;
        array[29] = 1;
        array[30] = 1;
        array[31] = 0;
        array[32] = 1;

        for (var i = 0; i < array.Length; i++)
        for (var j = 0; j < array[i]; j++)
        {
            tiles[counter] = i;
            counter++;
        }

        return tiles;
    }

    /// <summary>
    ///     Creates all of the tiles.
    /// </summary>
    public void setAll()
    {
        tileArray = new GameObject[85];
        tiles = generateIDs(tiles);
        nextTile = tileArray.Length - 1;

        //this.tiles = shuffle();


        for (var j = 0; j < tiles.Length; j++)
        {
            var res = InstatiateTiles(tiles[j], 1f, 0.2f * j, 0.0f);

            tileArray[j] = res;
        }

        tileArray[tileArray.Length - 1] = InstatiateTiles(7, basePositionTransform.position.x,
            basePositionTransform.position.y, basePositionTransform.position.z);


        for (var j = 0; j < tileArray.Length; j++)
            tileArray[j].transform.parent = tileArray[tileArray.Length - 1].transform;
    }

    /// <summary>
    /// </summary>
    /// <param name="id"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>
    public GameObject InstatiateTiles(int id, float x, float y, float z)
    {
        //GameObject res = PhotonNetwork.Instantiate(tile.name, new Vector3(x, y, z), Quaternion.identity);
        var res = Instantiate(tile, new Vector3(x, y, z), Quaternion.identity);
        res.GetComponent<TileScript>().AssignAttributes(id + 1);
        //res.GetComponentInChildren<MeshRenderer>().enabled = false;

        return res;
    }

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