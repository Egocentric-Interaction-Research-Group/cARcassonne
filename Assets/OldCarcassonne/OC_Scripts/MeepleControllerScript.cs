using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.UI;
using Photon.Pun;
using UnityEngine;

public class MeepleControllerScript : MonoBehaviourPun
{
    
    [SerializeField]
    internal GameControllerScript gameControllerScript;
    [HideInInspector] public List<MeepleScript> MeeplesInCity;
    internal float fMeepleAimX; //TODO Make Private
    internal float fMeepleAimZ; //TODO Make Private

    public MeepleControllerScript(GameControllerScript gameControllerScript)
    {
        this.gameControllerScript = gameControllerScript;
    }

    public void DrawMeepleRPC()
    {
        if (PhotonNetwork.LocalPlayer.NickName == (gameControllerScript.currentPlayer.getID() + 1).ToString())
            this.photonView.RPC("DrawMeeple",
                RpcTarget.All);
    }
    
    public ParticleSystem drawMeepleEffect;
    [HideInInspector] public GameObject currentMeeple;
    [HideInInspector] public GameObject meepleMesh;
    [HideInInspector] public GameObject MeeplePrefab;
    public GameObject meepleSpawnPosition;
    internal int iMeepleAimX;
    internal int iMeepleAimZ;
    public TileScript.geography meepleGeography;
    public RaycastHit meepleHitTileDirection;

    internal void CurrentMeepleRayCast() //TODO Should be private
    {
        RaycastHit hit;
        var layerMask = 1 << 8;

        Physics.Raycast(currentMeeple.transform.position, currentMeeple.transform.TransformDirection(Vector3.down),
            out hit, Mathf.Infinity, layerMask);

        var local = gameControllerScript.table.transform.InverseTransformPoint(hit.point);


        fMeepleAimX = local.x;
        fMeepleAimZ = local.z;


        if (fMeepleAimX - gameControllerScript.stackScript.basePositionTransform.localPosition.x > 0)
        {
            iMeepleAimX = (int) ((fMeepleAimX - gameControllerScript.stackScript.basePositionTransform.localPosition.x) * gameControllerScript.scale + 1f) / 2 +
                          85;
        }
        else
        {
            iMeepleAimX = (int) ((fMeepleAimX - gameControllerScript.stackScript.basePositionTransform.localPosition.x) * gameControllerScript.scale - 1f) / 2 +
                          85;
        }

        if (fMeepleAimZ - gameControllerScript.stackScript.basePositionTransform.localPosition.z > 0)
        {
            iMeepleAimZ = (int) ((fMeepleAimZ - gameControllerScript.stackScript.basePositionTransform.localPosition.z) * gameControllerScript.scale + 1f) / 2 +
                          85;
        }
        else
        {
            iMeepleAimZ = (int) ((fMeepleAimZ - gameControllerScript.stackScript.basePositionTransform.localPosition.z) * gameControllerScript.scale - 1f) / 2 +
                          85;
        }
    }
    
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public MeepleScript FindMeeple(int x, int y, TileScript.geography geography, GameControllerScript gameControllerScript)
    {
        MeepleScript res = null;

        foreach (var p in gameControllerScript.PlayerScript.players)
        foreach (var m in p.meeples)
        {
            var tmp = m.GetComponent<MeepleScript>();

            if (tmp.geography == geography && tmp.x == x && tmp.z == y) return tmp;
        }

        return res;
    }

    public void AimMeeple(GameControllerScript gameControllerScript)
    {
        try
        {
            if (gameControllerScript.PlacedTiles.getPlacedTiles(this.iMeepleAimX, this.iMeepleAimZ) == gameControllerScript.TileControllerScript.currentTile)
            {
                var tile = gameControllerScript.PlacedTiles.getPlacedTiles(this.iMeepleAimX, this.iMeepleAimZ);
                var tileScript = tile.GetComponent<TileScript>();

                var layerMask = 1 << 9;
                Physics.Raycast(this.currentMeeple.transform.position, this.currentMeeple.transform.TransformDirection(Vector3.down), out this.meepleHitTileDirection,
                    Mathf.Infinity, layerMask);

                this.meepleGeography = TileScript.geography.Grass;
                gameControllerScript.Direction = PointScript.Direction.CENTER;

                if (this.meepleHitTileDirection.collider != null)
                {
                    if (this.meepleHitTileDirection.collider.name == "East")
                    {
                        gameControllerScript.Direction = PointScript.Direction.EAST;
                        this.meepleGeography = tileScript.East;
                    }
                    else if (this.meepleHitTileDirection.collider.name == "West")
                    {
                        gameControllerScript.Direction = PointScript.Direction.WEST;
                        this.meepleGeography = tileScript.West;
                    }
                    else if (this.meepleHitTileDirection.collider.name == "North")
                    {
                        gameControllerScript.Direction = PointScript.Direction.NORTH;
                        this.meepleGeography = tileScript.North;
                    }
                    else if (this.meepleHitTileDirection.collider.name == "South")
                    {
                        gameControllerScript.Direction = PointScript.Direction.SOUTH;
                        this.meepleGeography = tileScript.South;
                    }
                    else if (this.meepleHitTileDirection.collider.name == "Center")
                    {
                        gameControllerScript.Direction = PointScript.Direction.CENTER;
                        this.meepleGeography = tileScript.getCenter();
                    }

                    gameControllerScript.SnapPosition = this.meepleHitTileDirection.collider.transform.position;

                    if (this.meepleGeography == TileScript.geography.City || this.meepleGeography == TileScript.geography.Road || this.meepleGeography == TileScript.geography.Cloister)
                    {
                        gameControllerScript.ChangeConfirmButtonApperance(true);
                        gameControllerScript.CanConfirm = true;
                    }
                }
                else
                {
                    gameControllerScript.SnapPosition = this.currentMeeple.transform.position;
                    gameControllerScript.ChangeConfirmButtonApperance(false);
                    gameControllerScript.CanConfirm = false;
                }
            }
            else
            {
                gameControllerScript.SnapPosition = this.currentMeeple.transform.position;
                this.meepleGeography = TileScript.geography.Grass;
                gameControllerScript.ChangeConfirmButtonApperance(false);
                gameControllerScript.CanConfirm = false;
            }
        }
        catch (IndexOutOfRangeException e)
        {
            Debug.Log(e);
            gameControllerScript.ErrorOutput = e.ToString();
        }
    }

    public void FreeMeeple(GameObject meeple, GameControllerScript gameControllerScript)
    {
        meeple.GetComponent<MeepleScript>().free = true;
        meeple.transform.position = new Vector3(20, 20, 20);
        meeple.GetComponentInChildren<Rigidbody>().useGravity = false;
        meeple.GetComponentInChildren<BoxCollider>().enabled = false;
        meeple.GetComponentInChildren<MeshRenderer>().enabled = false;
        gameControllerScript.state = GameControllerScript.GameStates.TileDown;
    }

    [PunRPC]
    public void DrawMeeple()
    {
        if (gameControllerScript.state == GameControllerScript.GameStates.TileDown)
        {
            foreach (var meeple in gameControllerScript.PlayerScript1.players[gameControllerScript.currentPlayer.getID()].meeples)
                if (meeple.GetComponent<MeepleScript>().free)
                {
                    meeple.GetComponentInChildren<Rigidbody>().useGravity = true;
                    meeple.GetComponentInChildren<BoxCollider>().enabled = true;
                    meeple.GetComponentInChildren<MeshRenderer>().enabled = true;
                    meeple.GetComponentInChildren<ObjectManipulator>().enabled = true;
                    meeple.transform.position = this.meepleSpawnPosition.transform.position;
                    meeple.transform.parent = gameControllerScript.table.transform;

                    this.currentMeeple = meeple;
                    this.currentMeeple.transform.rotation = Quaternion.identity;

                    gameControllerScript.UpdateDecisionButtons(true, false, this.currentMeeple);
                    gameControllerScript.state = GameControllerScript.GameStates.MeepleDrawn;
                    break;
                }
        }
        else
        {
            var deniedSound = gameObject.GetComponent<AudioSource>();
            deniedSound.Play();
        }
    }

    public MeepleScript FindMeeple(int x, int y, TileScript.geography geography, PointScript.Direction direction, GameControllerScript gameControllerScript)
    {
        MeepleScript res = null;

        foreach (var p in gameControllerScript.PlayerScript2.players)
        foreach (var m in p.meeples)
        {
            var tmp = m.GetComponent<MeepleScript>();

            if (tmp.geography == geography && tmp.x == x && tmp.z == y && tmp.direction == direction) return tmp;
        }

        return res;
    }

    public void PlaceMeeple(GameObject meeple, int xs, int zs, PointScript.Direction direction,
        TileScript.geography meepleGeography, GameControllerScript gameControllerScript)
    {
        var currentTileScript = gameControllerScript.TileControllerScript1.currentTile.GetComponent<TileScript>();
        var currentCenter = currentTileScript.getCenter();
        bool res;
        if (currentCenter == TileScript.geography.Village || currentCenter == TileScript.geography.Grass ||
            currentCenter == TileScript.geography.Cloister && direction != PointScript.Direction.CENTER)
            res = GetComponent<PointScript>()
                .testIfMeepleCantBePlacedDirection(currentTileScript.vIndex, meepleGeography, direction);
        else if (currentCenter == TileScript.geography.Cloister && direction == PointScript.Direction.CENTER)
            res = false;
        else
            res = GetComponent<PointScript>().testIfMeepleCantBePlaced(currentTileScript.vIndex, meepleGeography);

        if (meepleGeography == TileScript.geography.City)
        {
            if (currentCenter == TileScript.geography.City)
                res = gameControllerScript.CityIsFinished(xs, zs) || res;
            else
                res = gameControllerScript.CityIsFinishedDirection(xs, zs, direction) || res;
        }

        if (!currentTileScript.checkIfOcupied(direction) && !res)
        {
            meeple.GetComponentInChildren<Rigidbody>().useGravity = false;
            meeple.GetComponentInChildren<BoxCollider>().enabled = false;
            meeple.GetComponent<ObjectManipulator>().enabled = false;

            currentTileScript.occupy(direction);
            if (meepleGeography == TileScript.geography.Cityroad) meepleGeography = TileScript.geography.City;

            meeple.GetComponent<MeepleScript>().assignAttributes(xs, zs, direction, meepleGeography);
            meeple.GetComponent<MeepleScript>().free = false;


            gameControllerScript.state = GameControllerScript.GameStates.MeepleDown;
        }
    }
}