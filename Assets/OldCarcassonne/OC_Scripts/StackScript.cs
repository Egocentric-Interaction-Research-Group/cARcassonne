using Photon.Pun;
using UnityEditor;
using UnityEngine;

/// <summary>
/// The Stack of tiles.
/// </summary>
public class StackScript : MonoBehaviourPun
{

    /// <summary>
    /// A reference to the prefab Tile, to be used later.
    /// </summary>
    public GameObject tile;

    public Transform basePositionTransform;

    public int[] randomIndexArray;

    public int test;

    /// <summary>
    /// The array of tiles
    /// </summary>
    public GameObject[] tileArray;
    /// <summary>
    /// An array of ID's.
    /// </summary>
    int[] tiles;
    /// <summary>
    /// The next tile
    /// </summary>
    int nextTile;


    /// <summary>
    /// 
    /// </summary>
    /// <param name="array"></param>
    /// 

    void Update()
    {

    }
    public void setArray(UnityEngine.GameObject[] array)
    {
        this.tileArray = array;
        this.nextTile = array.Length;

        if (array != null)
        {
            setAll();
        }
    }

    /// <summary>
    /// Creates a new tile by giving it an ID. The ID corresponds to a category of tiles with distinct attributes that only it is aware of.
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public UnityEngine.GameObject createTiles(int id)
    {

        GameObject res = Instantiate(tile, new Vector3(2.0f, 0.0f, 0.0f), Quaternion.identity);
        res.GetComponent<TileScript>().AssignAttributes(id + 1);

        return res;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="tiles"></param>
    /// <returns></returns>
    /// 
    public int[] generateIDs(int[] tiles)
    {
        tiles = new int[84];
        int counter = 0;
        int[] array = new int[33];

        array[0] = 4; array[1] = 2; array[2] = 8; array[3] = 9; array[4] = 4;
        array[5] = 1; array[6] = 5; array[7] = 4; array[8] = 3; array[9] = 3;
        array[10] = 3; array[11] = 1; array[12] = 3; array[13] = 3; array[14] = 2;
        array[15] = 3; array[16] = 2; array[17] = 2; array[18] = 2; array[19] = 3;
        array[20] = 1; array[21] = 1; array[22] = 2; array[23] = 1; array[24] = 2;
        array[25] = 2; array[26] = 2; array[27] = 1; array[28] = 1; array[29] = 1;
        array[30] = 1; array[31] = 0; array[32] = 1;

        for (int i = 0; i < array.Length; i++)
        {
            for (int j = 0; j < array[i]; j++)
            {
                tiles[counter] = i;
                counter++;
            }
        }

        return tiles;

    }

    /// <summary>
    /// Creates all of the tiles.
    /// </summary>
    public void setAll()
    {

        tileArray = new GameObject[85];
        this.tiles = generateIDs(tiles);
        nextTile = tileArray.Length - 1;

        //this.tiles = shuffle();


        for (int j = 0; j < tiles.Length; j++)
        {
            GameObject res = InstatiateTiles(tiles[j], 1f, 0.2f * j, 0.0f);

            tileArray[j] = res;
        }

        tileArray[tileArray.Length - 1] = InstatiateTiles(7, basePositionTransform.position.x, basePositionTransform.position.y, basePositionTransform.position.z);




        for (int j = 0; j < tileArray.Length; j++)
        {
            tileArray[j].transform.parent = tileArray[tileArray.Length - 1].transform;
        }

    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="x"></param>
    /// <param name="y"></param>
    /// <param name="z"></param>
    /// <returns></returns>

    public GameObject InstatiateTiles(int id, float x, float y, float z)
    {
        //GameObject res = PhotonNetwork.Instantiate(tile.name, new Vector3(x, y, z), Quaternion.identity);
        GameObject res = Instantiate(tile, new Vector3(x, y, z), Quaternion.identity);
        res.GetComponent<TileScript>().AssignAttributes(id + 1);
        //res.GetComponentInChildren<MeshRenderer>().enabled = false;

        return res;
    }

    /// <summary>
    /// Shuffles the array of tiles.
    /// </summary>
    /// <returns></returns>
    /// 
    private void Shuffle(int[] randomIndex)
    {
        //System.Random rand = new System.Random();

        for (int i = tileArray.Length - 2; i > 0; i--)
        {
            //int randomIndex = rand.Next(0, i + 1);
            GameObject temp = tileArray[i];
            tileArray[i] = tileArray[randomIndex[i]];
            tileArray[randomIndex[i]] = temp;
        }

        // return this.tiles;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="gameObj"></param>
    public void Push(GameObject gameObj)
    {
        tileArray[nextTile + 1] = gameObj;
        nextTile++;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// 
    public GameObject Pop()
    {

        GameObject tile = tileArray[nextTile];

       
        nextTile--;

        return tile;
    }

    public int GetTileCount()
    {
        return nextTile;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <returns></returns>
    /// 
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
            int[] tmpRandomIndexArray = new int[84];
            System.Random rand = new System.Random();
            for (int i = tileArray.Length - 2; i > 0; i--)
            {

                tmpRandomIndexArray[i] = rand.Next(0, i + 1);

            }
            this.photonView.RPC("GetRandomIndexRPC", RpcTarget.All, (object)tmpRandomIndexArray);
        }

        if(randomIndexArray != null)
        {
            Shuffle(randomIndexArray);
        }

    }

    [PunRPC]
    public void GetRandomIndexRPC(int[] random)
    {
        randomIndexArray = random;
    }

}
