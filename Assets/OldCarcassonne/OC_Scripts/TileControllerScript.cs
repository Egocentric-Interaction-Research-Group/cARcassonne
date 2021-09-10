using Photon.Pun;
using UnityEngine;

public class TileControllerScript : MonoBehaviourPun
{
    private GameControllerScript gameControllerScript;
    public Vector3 currentTileEulersOnManip;
    public ParticleSystem drawTileEffect;
    [HideInInspector] public GameObject currentTile;
    [HideInInspector] public GameObject baseTile;
    [HideInInspector] public Vector3 tileSnapPosition;
    [HideInInspector] public GameObject tileMesh;
    public GameObject drawTile;
    public GameObject tileSpawnPosition;
    public float fTileAimX;
    public float fTileAimZ;

    public TileControllerScript(GameControllerScript gameControllerScript)
    {
        this.gameControllerScript = gameControllerScript;
    }

    public void ChangeCurrentTileOwnership()
    {
        if (currentTile.GetComponent<PhotonView>().Owner.NickName != (gameControllerScript.currentPlayer.getID() + 1).ToString())
            currentTile.GetComponent<TileScript>().transferTileOwnership(gameControllerScript.currentPlayer.getID());
    }
}