using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class MeepleControllerScript : MonoBehaviourPun
{
    [SerializeField]
    internal GameControllerScript gameControllerScript;
    [HideInInspector] public List<MeepleScript> MeeplesInCity;
    public float fMeepleAimX; //TODO Make Private
    public float fMeepleAimZ; //TODO Make Private

    public MeepleControllerScript(GameControllerScript gameControllerScript)
    {
        this.gameControllerScript = gameControllerScript;
    }

    public void DrawMeepleRPC()
    {
        if (PhotonNetwork.LocalPlayer.NickName == (gameControllerScript.currentPlayer.getID() + 1).ToString()) gameControllerScript.photonView.RPC("DrawMeeple", RpcTarget.All);
    }
}

public class GameControllerScript : MonoBehaviourPun
{
    //Add Meeple Down state functionality
    public enum GameStates
    {
        NewTurn,
        TileDrawn,
        TileDown,
        MeepleDrawn,
        MeepleDown,
        GameOver
    }

    public Vector3 currentTileEulersOnManip;
    public bool gravity;
    public bool startGame, pcRotate, isManipulating;

    public Material[] playerMaterials;
    public Material[] buttonMaterials;
    public GameObject[] playerHuds;
    public GameObject endButtonBackplate, confirmButtonBackplate;
    public GameObject meepleInButton;
    public ParticleSystem bellSparkleEffect, drawTileEffect, drawMeepleEffect, smokeEffect;

    public float scale;

    [HideInInspector] public GameObject currentTile, baseTile, table, currentMeeple;

    [HideInInspector] public StackScript stackScript;

    [HideInInspector] public Vector3 tileSnapPosition;


    [HideInInspector] public GameObject tileMesh;

    [HideInInspector] public GameObject meepleMesh;

    [HideInInspector] public GameObject MeeplePrefab;

    [HideInInspector] public GameObject playerHUD;

    public GameObject confirmButton, rotateButton;
    public Sprite crossIcon, checkIcon;

    public RectTransform mPanelGameOver;

    public GameObject drawTile;

    public GameObject tileSpawnPosition, meepleSpawnPosition;

    public GameStates state;

    //private int xs, zs;

    private float aimX = 0, aimZ = 0;

    private bool canConfirm;

    private bool cityIsFinished;

    [HideInInspector] public PlayerScript.Player currentPlayer;

    private GameObject decisionButtons;

    private PointScript.Direction direction;

    private string errorOutput = "";

    private int firstTurnCounter;

    private float fTileAimX;
    private float fTileAimZ;

    private int iMeepleAimX, iMeepleAimZ;

    private bool isPunEnabled;
    //float xOffset, zOffset, yOffset;

    private int iTileAimX, iTileAimZ;

    private PlayerScript.Player lastPlayer;

    private TileScript.geography meepleGeography;

    private RaycastHit meepleHitTileDirection;

    private int NewTileRotation;

    //The points of each player where each index represents a player (index+1).
    // public int[] points;
    //The matrix of tiles (separated by 2.0f in all 2D directions)
    //private GameObject[,] placedTiles;
    private PlacedTilesScript placedTiles;
    private Color32 playerColor;

    //Number of players
    private int players;

    private PlayerScript playerScript;

    private bool renderCurrentTile = false;

    private Vector3 snapPosition;

    //public ErrorPlaneScript ErrorPlane;


    private int tempX;
    private int tempY;

    private TurnScript turnScript;

    private int VertexItterator;

    private bool[,] visited;

    public bool IsPunEnabled
    {
        set => isPunEnabled = value;
    }

    [SerializeField]
    internal MeepleControllerScript meepleControllerScript;

    private void Start()
    {
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (currentTile != null)
        {
            CurrentTileRaycastPosition();

            if (placedTiles.TilePlacementIsValid(currentTile, iTileAimX, iTileAimZ))
                ChangeConfirmButtonApperance(true);
            else
                ChangeConfirmButtonApperance(false);

            snapPosition = new Vector3
            (stackScript.basePositionTransform.localPosition.x + (iTileAimX - 85) * 0.033f,
                currentTile.transform.localPosition.y,
                stackScript.basePositionTransform.localPosition.z + (iTileAimZ - 85) * 0.033f);
        }

        if (startGame)
        {
            NewGame();
            startGame = false;
        }

        if (Input.GetKeyDown(KeyCode.P)) EndTurn();
        if (Input.GetKeyDown(KeyCode.R) && PhotonNetwork.LocalPlayer.NickName == (currentPlayer.getID() + 1).ToString())
            if (!isManipulating)
            {
                pcRotate = true;
                RotateTileRPC();
            }

        if (Input.GetKeyDown(KeyCode.J)) FreeMeeple(currentMeeple); //FIXME: Throws error when no meeple assigned!
        if (Input.GetKeyDown(KeyCode.B)) GameOver();

        switch (state)
        {
            case GameStates.NewTurn:
                bellSparkleEffect.Stop();
                drawMeepleEffect.Stop();

                if (firstTurnCounter != 0) drawTileEffect.Play();

                endButtonBackplate.GetComponent<MeshRenderer>().material = buttonMaterials[0];
                drawTile.GetComponent<BoxCollider>().enabled = true;


                break;
            case GameStates.TileDrawn:
                //drawTile.GetComponent<BoxCollider>().enabled = false;
                drawTileEffect.Stop();

                break;
            case GameStates.TileDown:

                if (firstTurnCounter != 0) drawMeepleEffect.Play();
                currentTile.transform.localPosition = new Vector3
                (stackScript.basePositionTransform.localPosition.x + (iTileAimX - 85) * 0.033f, 0.5900002f,
                    stackScript.basePositionTransform.localPosition.z + (iTileAimZ - 85) * 0.033f);
                endButtonBackplate.GetComponent<MeshRenderer>().material = buttonMaterials[1];

                break;
            case GameStates.MeepleDrawn:

                //confirmButton.SetActive(true);
                //confirmButton.transform.position = new Vector3(currentMeeple.transform.position.x + 0.05f, currentMeeple.transform.position.y + 0.05f, currentMeeple.transform.position.z + 0.07f);
                ////confirmButton.transform.up = table.transform.forward;
                drawMeepleEffect.Stop();
                CurrentMeepleRayCast();
                AimMeeple();

                break;
            case GameStates.MeepleDown:
                //currentMeeple.transform.position = snapPosition;
                endButtonBackplate.GetComponent<MeshRenderer>().material = buttonMaterials[2];

                bellSparkleEffect.Play();

                break;
            case GameStates.GameOver:
                break;
        }
    }


    //Startar nytt spel
    public void NewGame()
    {
        int players = PhotonNetwork.CurrentRoom.PlayerCount;
        firstTurnCounter = players;
        placedTiles = GetComponent<PlacedTilesScript>();
        placedTiles.InstansiatePlacedTilesArray();

        table = GameObject.Find("Table");
        decisionButtons = GameObject.Find("DecisionButtons");

        stackScript = GetComponent<StackScript>().createStackScript();

        stackScript.PopulateTileArray();

        turnScript = GetComponent<TurnScript>();
        playerScript = GetComponent<PlayerScript>();

        BaseTileCreation();

        for (var i = 0; i < players; i++)
        {
            playerScript.CreatePlayer(i, "player " + i, playerMaterials[i], GameObject.Find("User" + (i + 1)));
            playerHuds[i].SetActive(true);
            playerHuds[i].GetComponentInChildren<TextMeshPro>().text = "Score: 0";
            playerScript.players[i].meeples = GameObject.FindGameObjectsWithTag("Meeple " + i);
            foreach (var meeple in playerScript.players[i].meeples)
            {
                meeple.GetComponent<MeepleScript>().createByPlayer(playerScript.players[i]);
                meeple.GetComponent<MeepleScript>().SetMeepleOwner();
            }
        }

        if (PhotonNetwork.IsMasterClient)
            playerHuds[0].transform.GetChild(3).gameObject.GetComponent<TextMeshPro>().text = "Player 1    (You)";
        else
            playerHuds[1].transform.GetChild(3).gameObject.GetComponent<TextMeshPro>().text = "Player 2    (You)";

        NewTileRotation = 0;
        VertexItterator = 1;

        PlaceTile(currentTile, 85, 85, true);

        currentPlayer = playerScript.players[0];

        Debug.Log("Denna spelarese namn: " + PhotonNetwork.LocalPlayer.NickName);
        Debug.Log("Current " + (currentPlayer.getID() + 1));

        playerHuds[0].GetComponentInChildren<MeshRenderer>().material = playerMaterials[0];
        meepleInButton.GetComponent<MeshRenderer>().material = buttonMaterials[3];

        state = GameStates.NewTurn;
    }

    private void BaseTileCreation()
    {
        currentTile = stackScript.Pop();
        currentTile.name = "BaseTile";
        currentTile.transform.parent = table.transform;
        currentTile.GetComponent<ObjectManipulator>().enabled = false;
        currentTile.GetComponent<NearInteractionGrabbable>().enabled = false;
    }


    private MeepleScript FindMeeple(int x, int y, TileScript.geography geography)
    {
        MeepleScript res = null;

        foreach (var p in playerScript.players)
        foreach (var m in p.meeples)
        {
            var tmp = m.GetComponent<MeepleScript>();

            if (tmp.geography == geography && tmp.x == x && tmp.z == y) return tmp;
        }

        return res;
    }

    private MeepleScript FindMeeple(int x, int y, TileScript.geography geography, PointScript.Direction direction)
    {
        MeepleScript res = null;

        foreach (var p in playerScript.players)
        foreach (var m in p.meeples)
        {
            var tmp = m.GetComponent<MeepleScript>();

            if (tmp.geography == geography && tmp.x == x && tmp.z == y && tmp.direction == direction) return tmp;
        }

        return res;
    }

    public bool CityIsFinishedDirection(int x, int y, PointScript.Direction direction)
    {
        meepleControllerScript.MeeplesInCity = new List<MeepleScript>();
        meepleControllerScript.MeeplesInCity.Add(FindMeeple(x, y, TileScript.geography.City, direction));

        cityIsFinished = true;
        visited = new bool[170, 170];
        RecursiveCityIsFinishedDirection(x, y, direction);
        Debug.Log(
            "DIRECTION__________________________CITY IS FINISHED EFTER DIRECTION REKURSIV: ___________________________" +
            cityIsFinished + " X: " + x + " Z: " + y + " MEEPLEINCITY: " + FindMeeple(x, y, TileScript.geography.City));
        return cityIsFinished;
    }

    //Test City checker
    public bool CityIsFinished(int x, int y)
    {
        meepleControllerScript.MeeplesInCity = new List<MeepleScript>();
        meepleControllerScript.MeeplesInCity.Add(FindMeeple(x, y, TileScript.geography.City));


        cityIsFinished = true;
        visited = new bool[170, 170];
        RecursiveCityIsFinished(x, y);
        Debug.Log("__________________________CITY IS FINISHED EFTER REKURSIV: ___________________________" +
                  cityIsFinished + " X: " + x + " Z: " + y + " MEEPLEINCITY: " +
                  FindMeeple(x, y, TileScript.geography.City));

        return cityIsFinished;
    }

    public void RecursiveCityIsFinishedDirection(int x, int y, PointScript.Direction direction)
    {
        visited[x, y] = true;
        if (direction == PointScript.Direction.NORTH)
            if (placedTiles.getPlacedTiles(x, y).GetComponent<TileScript>().North == TileScript.geography.City)
            {
                if (placedTiles.getPlacedTiles(x, y + 1) != null)
                {
                    if (!visited[x, y + 1]) RecursiveCityIsFinished(x, y + 1);
                }
                else
                {
                    cityIsFinished = false;
                }
            }

        if (direction == PointScript.Direction.EAST)
            if (placedTiles.getPlacedTiles(x, y).GetComponent<TileScript>().East == TileScript.geography.City)
            {
                if (placedTiles.getPlacedTiles(x + 1, y) != null)
                {
                    if (!visited[x + 1, y]) RecursiveCityIsFinished(x + 1, y);
                }
                else
                {
                    cityIsFinished = false;
                }
            }

        if (direction == PointScript.Direction.SOUTH)
            if (placedTiles.getPlacedTiles(x, y).GetComponent<TileScript>().South == TileScript.geography.City)
            {
                if (placedTiles.getPlacedTiles(x, y - 1) != null)
                {
                    if (!visited[x, y - 1]) RecursiveCityIsFinished(x, y - 1);
                }
                else
                {
                    cityIsFinished = false;
                }
            }

        if (direction == PointScript.Direction.WEST)
            if (placedTiles.getPlacedTiles(x, y).GetComponent<TileScript>().West == TileScript.geography.City)
            {
                if (placedTiles.getPlacedTiles(x - 1, y) != null)
                {
                    if (!visited[x - 1, y]) RecursiveCityIsFinished(x - 1, y);
                }
                else
                {
                    cityIsFinished = false;
                }
            }
    }

    public bool TileCanBePlaced(TileScript script)
    {
        for (var i = 0; i < placedTiles.GetLength(0); i++)
        for (var j = 0; j < placedTiles.GetLength(1); j++)
            if (placedTiles.HasNeighbor(i, j) && placedTiles.getPlacedTiles(i, j) == null)
                for (var k = 0; k < 4; k++)
                {
                    if (placedTiles.MatchGeographyOrNull(i - 1, j, PointScript.Direction.EAST, script.West))
                        if (placedTiles.MatchGeographyOrNull(i + 1, j, PointScript.Direction.WEST, script.East))
                            if (placedTiles.MatchGeographyOrNull(i, j - 1, PointScript.Direction.NORTH, script.South))
                                if (placedTiles.MatchGeographyOrNull(i, j + 1, PointScript.Direction.SOUTH,
                                    script.North))
                                {
                                    ResetTileRotation();
                                    return true;
                                }

                    RotateTile();
                }

        ResetTileRotation();
        return false;
    }

    public void RecursiveCityIsFinished(int x, int y)
    {
        visited[x, y] = true;


        if (placedTiles.getPlacedTiles(x, y) != null)
        {
            if (placedTiles.getPlacedTiles(x, y).GetComponent<TileScript>().North == TileScript.geography.City)
                if (!placedTiles.CityTileHasGrassOrStreamCenter(x, y))
                {
                    if (placedTiles.getPlacedTiles(x, y + 1) != null)

                    {
                        if (!visited[x, y + 1]) RecursiveCityIsFinished(x, y + 1);
                    }
                    else
                    {
                        cityIsFinished = false;
                    }
                }

            if (placedTiles.getPlacedTiles(x, y).GetComponent<TileScript>().East == TileScript.geography.City)
                if (!placedTiles.CityTileHasGrassOrStreamCenter(x, y))
                {
                    if (placedTiles.getPlacedTiles(x + 1, y) != null)
                    {
                        if (!visited[x + 1, y]) RecursiveCityIsFinished(x + 1, y);
                    }
                    else
                    {
                        cityIsFinished = false;
                    }
                }

            if (placedTiles.getPlacedTiles(x, y).GetComponent<TileScript>().South == TileScript.geography.City)
                if (!placedTiles.CityTileHasGrassOrStreamCenter(x, y))
                {
                    if (placedTiles.getPlacedTiles(x, y - 1) != null)
                    {
                        if (!visited[x, y - 1]) RecursiveCityIsFinished(x, y - 1);
                    }
                    else
                    {
                        cityIsFinished = false;
                    }
                }

            if (placedTiles.getPlacedTiles(x, y).GetComponent<TileScript>().West == TileScript.geography.City)
                if (!placedTiles.CityTileHasGrassOrStreamCenter(x, y))
                {
                    if (placedTiles.getPlacedTiles(x - 1, y) != null)
                    {
                        if (!visited[x - 1, y]) RecursiveCityIsFinished(x - 1, y);
                    }
                    else
                    {
                        cityIsFinished = false;
                    }
                }
        }
    }

    [PunRPC]
    private void CurrentTileRaycastPosition()
    {
        RaycastHit hit;
        var layerMask = 1 << 8;

        Physics.Raycast(currentTile.transform.position, currentTile.transform.TransformDirection(Vector3.down), out hit,
            Mathf.Infinity, layerMask);


        var local = table.transform.InverseTransformPoint(hit.point);

        fTileAimX = local.x;
        fTileAimZ = local.z;


        if (fTileAimX - stackScript.basePositionTransform.localPosition.x > 0)
        {
            iTileAimX = (int) ((fTileAimX - stackScript.basePositionTransform.localPosition.x) * scale + 1f) / 2 + 85;

            var testX = ((fTileAimX - stackScript.basePositionTransform.localPosition.x) * 10f + 1f) / 2f + 85f;
            //Debug.Log("Float X: " + Math.Round(testX));
        }
        else
        {
            iTileAimX = (int) ((fTileAimX - stackScript.basePositionTransform.localPosition.x) * scale - 1f) / 2 + 85;
            var testX = ((fTileAimX - stackScript.basePositionTransform.localPosition.x) * 10f - 1f) / 2f + 85f;
            //Debug.Log("Float X: " + Math.Round(testX));
        }

        if (fTileAimZ - stackScript.basePositionTransform.localPosition.z > 0)
        {
            iTileAimZ = (int) ((fTileAimZ - stackScript.basePositionTransform.localPosition.z) * scale + 1f) / 2 + 85;

            var testZ = ((fTileAimZ - stackScript.basePositionTransform.localPosition.z) * 10f + 1f) / 2f + 85f;
            //Debug.Log("Float Z: " + Math.Round(testZ));
        }
        else
        {
            iTileAimZ = (int) ((fTileAimZ - stackScript.basePositionTransform.localPosition.z) * scale - 1f) / 2 + 85;

            var testZ = ((fTileAimZ - stackScript.basePositionTransform.localPosition.z) * 10f - 1f) / 2f + 85f;
            //Debug.Log("Float Z: " + Math.Round(testZ));
        }
    }

    public void PlaceMeeple(GameObject meeple, int xs, int zs, PointScript.Direction direction,
        TileScript.geography meepleGeography)
    {
        var currentTileScript = currentTile.GetComponent<TileScript>();
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
                res = CityIsFinished(xs, zs) || res;
            else
                res = CityIsFinishedDirection(xs, zs, direction) || res;
        }

        if (!currentTileScript.checkIfOcupied(direction) && !res)
        {
            meeple.GetComponentInChildren<Rigidbody>().useGravity = false;
            meeple.GetComponentInChildren<BoxCollider>().enabled = false;
            meeple.GetComponent<ObjectManipulator>().enabled = false;

            var geography = currentTileScript.getGeographyAt(direction);
            currentTileScript.occupy(direction);
            if (meepleGeography == TileScript.geography.Cityroad) meepleGeography = TileScript.geography.City;

            meeple.GetComponent<MeepleScript>().assignAttributes(xs, zs, direction, meepleGeography);
            meeple.GetComponent<MeepleScript>().free = false;


            state = GameStates.MeepleDown;
        }
    }

    //Metod för att placera en tile på brädan
    public void PlaceTile(GameObject tile, int x, int z, bool firstTile)
    {
        tempX = x;
        tempY = z;
        tile.GetComponent<TileScript>().vIndex = VertexItterator;

        GetComponent<PointScript>().placeVertex(VertexItterator, placedTiles.GetNeighbors(tempX, tempY),
            placedTiles.getWeights(tempX, tempY), currentTile.GetComponent<TileScript>().getCenter(),
            placedTiles.getCenters(tempX, tempY), placedTiles.getDirections(tempX, tempY));

        VertexItterator++;

        tile.GetComponent<BoxCollider>().enabled = false;
        tile.GetComponent<Rigidbody>().useGravity = false;
        tile.GetComponent<ObjectManipulator>().enabled = false;
        tile.GetComponent<Rigidbody>().isKinematic = true;

        if (!firstTile)
        {
            placedTiles.PlaceTile(x, z, tile);


            currentTile.transform.localPosition = snapPosition;
        }
        else
        {
            placedTiles.PlaceTile(x, z, tile);
            tile.transform.localPosition = stackScript.basePositionTransform.localPosition;
        }


        calculatePoints(false, false);
    }


    public void PickupTileRPC()
    {
        if (PhotonNetwork.LocalPlayer.NickName == (currentPlayer.getID() + 1).ToString())
            photonView.RPC("PickupTile", RpcTarget.All);
    }

    //Metod för att plocka upp en ny tile
    [PunRPC]
    public void PickupTile()
    {
        if (state == GameStates.NewTurn)
        {
            currentTile = stackScript.Pop();
            UpdateDecisionButtons(true, true, currentTile);
            ActivateCurrentTile();
            if (!TileCanBePlaced(currentTile.GetComponent<TileScript>()))
            {
                Debug.Log("Tile not possible to place: discarding and drawing a new one. " + "Tile id: " +
                          currentTile.GetComponent<TileScript>().id);
                Destroy(currentTile);
                PickupTile();
            }
            else
            {
                ResetTileRotation();
                state = GameStates.TileDrawn;
            }
        }
        else
        {
            var deniedSound = gameObject.GetComponent<AudioSource>();
            deniedSound.Play();
        }
    }

    private void ActivateCurrentTile()
    {
        currentTile.GetComponentInChildren<MeshRenderer>().enabled = true;
        currentTile.GetComponentInChildren<Collider>().enabled = true;
        currentTile.GetComponentInChildren<Rigidbody>().useGravity = true;
        currentTile.transform.parent = table.transform;
        currentTile.transform.rotation = table.transform.rotation;
        currentTile.transform.position = tileSpawnPosition.transform.position;
        smokeEffect.Play();
    }

    public void ChangeCurrentTileOwnership()
    {
        if (currentTile.GetComponent<PhotonView>().Owner.NickName != (currentPlayer.getID() + 1).ToString())
            currentTile.GetComponent<TileScript>().transferTileOwnership(currentPlayer.getID());
    }

    public void ConfirmPlacementRPC()
    {
        if (PhotonNetwork.LocalPlayer.NickName == (currentPlayer.getID() + 1).ToString())
            photonView.RPC("ConfirmPlacement", RpcTarget.All);
    }

    [PunRPC]
    public void ConfirmPlacement()
    {
        CurrentTileRaycastPosition();
        if (state == GameStates.TileDrawn)
        {
            if (placedTiles.TilePlacementIsValid(currentTile, iTileAimX, iTileAimZ))
            {
                PlaceTile(currentTile, iTileAimX, iTileAimZ, false);

                confirmButton.SetActive(false);
                //rotateButton.SetActive(false);
                state = GameStates.TileDown;
            }
            else if (!placedTiles.TilePlacementIsValid(currentTile, iTileAimX, iTileAimZ))
            {
                Debug.Log("Tile cant be placed");
            }
        }
        else if (state == GameStates.MeepleDrawn)
        {
            if (currentMeeple != null)
            {
                if (canConfirm)
                {
                    if (meepleGeography == TileScript.geography.City ||
                        meepleGeography == TileScript.geography.Cloister ||
                        meepleGeography == TileScript.geography.Road)
                        PlaceMeeple(currentMeeple, iMeepleAimX, iMeepleAimZ, direction, meepleGeography);
                }
                else
                {
                    FreeMeeple(currentMeeple);
                }
            }
        }
    }


    //Funktion för undo
    public void UndoAction()
    {
        if (state == GameStates.TileDown || state == GameStates.MeepleDrawn)
        {
            placedTiles.removeTile(tempX, tempY);
            currentTile.GetComponentInChildren<MeshRenderer>().enabled = false;
            state = GameStates.TileDrawn;

            VertexItterator--;
            GetComponent<PointScript>().RemoveVertex(VertexItterator);
        }
    }


    public void EndTurnRPC()
    {
        if (PhotonNetwork.LocalPlayer.NickName == (currentPlayer.getID() + 1).ToString())
        {
            photonView.RPC("EndTurn", RpcTarget.All);
            photonView.RPC("DebugStuff", RpcTarget.All);
        }
    }

    //Avslutar nuvarande spelares runda
    [PunRPC]
    public void EndTurn()
    {
        if (state == GameStates.TileDown || state == GameStates.MeepleDown)
        {
            calculatePoints(true, false);
            NewTileRotation = 0;
            if (stackScript.GetTileCount() == -1)
            {
                GameOver();
            }
            else
            {
                if (playerScript.players.Count > 1)
                {
                    if (currentPlayer == playerScript.players[0])
                        currentPlayer = playerScript.players[1];
                    else
                        currentPlayer = playerScript.players[0];
                }


                Debug.Log("CurrentPlayer = " + currentPlayer.getID());
                ChangePlayerHud();

                if (firstTurnCounter != 0) firstTurnCounter -= 1;

                state = GameStates.NewTurn;
            }
        }
        else
        {
            var deniedAudio = gameObject.GetComponent<AudioSource>();
            deniedAudio.Play();
        }
    }

    private void ChangePlayerHud()
    {
        for (var i = 0; i < playerScript.players.Count; i++)
            playerHuds[i].GetComponentInChildren<TextMeshPro>().text =
                "Score: " + playerScript.players[i].GetPlayerScore();


        if (currentPlayer.getID() == 0)
        {
            playerHuds[0].GetComponentInChildren<MeshRenderer>().material = playerMaterials[0];
            meepleInButton.GetComponent<MeshRenderer>().material = buttonMaterials[3];

            playerHuds[1].GetComponentInChildren<MeshRenderer>().material = playerMaterials[5];
            playerHuds[2].GetComponentInChildren<MeshRenderer>().material = playerMaterials[6];
            playerHuds[3].GetComponentInChildren<MeshRenderer>().material = playerMaterials[7];
        }
        else if (currentPlayer.getID() == 1)
        {
            playerHuds[1].GetComponentInChildren<MeshRenderer>().material = playerMaterials[1];
            meepleInButton.GetComponent<MeshRenderer>().material = buttonMaterials[4];

            playerHuds[0].GetComponentInChildren<MeshRenderer>().material = playerMaterials[4];
            playerHuds[2].GetComponentInChildren<MeshRenderer>().material = playerMaterials[6];
            playerHuds[3].GetComponentInChildren<MeshRenderer>().material = playerMaterials[7];
        }
        //else if (currentPlayer == 2)
        //{
        //    playerHuds[2].GetComponentInChildren<MeshRenderer>().material = playerMaterials[2];
        //    meepleInButton.GetComponent<MeshRenderer>().material = playerMaterials[2];

        //    playerHuds[0].GetComponentInChildren<MeshRenderer>().material = playerMaterials[4];
        //    playerHuds[1].GetComponentInChildren<MeshRenderer>().material = playerMaterials[5];
        //    playerHuds[3].GetComponentInChildren<MeshRenderer>().material = playerMaterials[7];
        //}
        //else if (currentPlayer == 3)
        //{
        //    playerHuds[3].GetComponentInChildren<MeshRenderer>().material = playerMaterials[3];
        //    meepleInButton.GetComponent<MeshRenderer>().material = playerMaterials[3];

        //    playerHuds[0].GetComponentInChildren<MeshRenderer>().material = playerMaterials[4];
        //    playerHuds[1].GetComponentInChildren<MeshRenderer>().material = playerMaterials[5];
        //    playerHuds[2].GetComponentInChildren<MeshRenderer>().material = playerMaterials[6];
        //}
    }

    public void calculatePoints(bool RealCheck, bool GameEnd)
    {
        foreach (var p in playerScript.GetPlayers())
            for (var j = 0; j < p.meeples.Length; j++)
            {
                var meeple = p.meeples[j].GetComponent<MeepleScript>();
                if (!meeple.free)
                {
                    var tileID = placedTiles.getPlacedTiles(meeple.x, meeple.z).GetComponent<TileScript>().id;
                    var finalscore = 0;
                    if (meeple.geography == TileScript.geography.City)
                    {
                        //CITY DIRECTION
                        if (placedTiles.getPlacedTiles(meeple.x, meeple.z).GetComponent<TileScript>().getCenter() ==
                            TileScript.geography.Stream ||
                            placedTiles.getPlacedTiles(meeple.x, meeple.z).GetComponent<TileScript>().getCenter() ==
                            TileScript.geography.Grass ||
                            placedTiles.getPlacedTiles(meeple.x, meeple.z).GetComponent<TileScript>().getCenter() ==
                            TileScript.geography.Road ||
                            placedTiles.getPlacedTiles(meeple.x, meeple.z).GetComponent<TileScript>().getCenter() ==
                            TileScript.geography.Village)
                        {
                            if (CityIsFinishedDirection(meeple.x, meeple.z, meeple.direction))
                            {
                                Debug.Log("CITY IS FINISHED END");

                                finalscore = GetComponent<PointScript>()
                                    .startDfsDirection(
                                        placedTiles.getPlacedTiles(meeple.x, meeple.z).GetComponent<TileScript>()
                                            .vIndex, meeple.geography, meeple.direction, GameEnd);
                            }

                            //else
                            //{
                            //    GetComponent<PointScript>().startDfsDirection(placedTiles.getPlacedTiles(meeple.x, meeple.z).
                            //        GetComponent<TileScript>().vIndex, meeple.geography, meeple.direction, GameEnd);
                            //}
                            if (GameEnd)
                            {
                                Debug.Log("GAME END");
                                finalscore = GetComponent<PointScript>()
                                    .startDfsDirection(
                                        placedTiles.getPlacedTiles(meeple.x, meeple.z).GetComponent<TileScript>()
                                            .vIndex, meeple.geography, meeple.direction, GameEnd);
                            }
                        }
                        else
                        {
                            //CITY NO DIRECTION
                            if (CityIsFinished(meeple.x, meeple.z))
                                finalscore = GetComponent<PointScript>()
                                    .startDfs(
                                        placedTiles.getPlacedTiles(meeple.x, meeple.z).GetComponent<TileScript>()
                                            .vIndex, meeple.geography, GameEnd);
                            if (GameEnd)
                            {
                                Debug.Log("GAME END I ELSE");
                                finalscore = GetComponent<PointScript>()
                                    .startDfsDirection(
                                        placedTiles.getPlacedTiles(meeple.x, meeple.z).GetComponent<TileScript>()
                                            .vIndex, meeple.geography, meeple.direction, GameEnd);
                            }
                        }
                    }
                    else
                    {
                        ///ROAD
                        if (placedTiles.getPlacedTiles(meeple.x, meeple.z).GetComponent<TileScript>().getCenter() ==
                            TileScript.geography.Village ||
                            placedTiles.getPlacedTiles(meeple.x, meeple.z).GetComponent<TileScript>().getCenter() ==
                            TileScript.geography.Grass)
                        {
                            finalscore = GetComponent<PointScript>().startDfsDirection(placedTiles
                                .getPlacedTiles(meeple.x, meeple.z)
                                .GetComponent<TileScript>().vIndex, meeple.geography, meeple.direction, GameEnd);
                            if (GameEnd)
                                finalscore--;
                        }
                        else
                        {
                            finalscore = GetComponent<PointScript>()
                                .startDfs(
                                    placedTiles.getPlacedTiles(meeple.x, meeple.z).GetComponent<TileScript>().vIndex,
                                    meeple.geography, GameEnd);
                            if (GameEnd)
                                finalscore--;
                        }

                        //CLOISTER
                        if (placedTiles.getPlacedTiles(meeple.x, meeple.z).GetComponent<TileScript>().getCenter() ==
                            TileScript.geography.Cloister &&
                            meeple.direction == PointScript.Direction.CENTER)
                            finalscore = placedTiles.CheckSurroundedCloister(meeple.x, meeple.z, GameEnd);
                    }

                    if (finalscore > 0 && RealCheck)
                    {
                        Debug.Log(currentPlayer.getID() + " recieved " + finalscore + " points. MEEPLEGEO: " +
                                  meepleGeography);
                        meeple.playerScriptPlayer.SetPlayerScore(
                            meeple.playerScriptPlayer.GetPlayerScore() + finalscore);

                        meeple.free = true;
                        meeple.transform.position = new Vector3(20, 20, 20);
                        meeple.GetComponentInChildren<Rigidbody>().useGravity = false;
                        meeple.GetComponentInChildren<BoxCollider>().enabled = false;
                        meeple.GetComponentInChildren<MeshRenderer>().enabled = false;
                    }
                }
            }
    }

    public void FreeMeeple(GameObject meeple)
    {
        meeple.GetComponent<MeepleScript>().free = true;
        meeple.transform.position = new Vector3(20, 20, 20);
        meeple.GetComponentInChildren<Rigidbody>().useGravity = false;
        meeple.GetComponentInChildren<BoxCollider>().enabled = false;
        meeple.GetComponentInChildren<MeshRenderer>().enabled = false;
        state = GameStates.TileDown;
    }

    [PunRPC]
    public void DrawMeeple()
    {
        if (state == GameStates.TileDown)
        {
            foreach (var meeple in playerScript.players[currentPlayer.getID()].meeples)
                if (meeple.GetComponent<MeepleScript>().free)
                {
                    meeple.GetComponentInChildren<Rigidbody>().useGravity = true;
                    meeple.GetComponentInChildren<BoxCollider>().enabled = true;
                    meeple.GetComponentInChildren<MeshRenderer>().enabled = true;
                    meeple.GetComponentInChildren<ObjectManipulator>().enabled = true;
                    meeple.transform.position = meepleSpawnPosition.transform.position;
                    meeple.transform.parent = table.transform;

                    currentMeeple = meeple;
                    currentMeeple.transform.rotation = Quaternion.identity;

                    UpdateDecisionButtons(true, false, currentMeeple);
                    state = GameStates.MeepleDrawn;
                    break;
                }
        }
        else
        {
            var deniedSound = gameObject.GetComponent<AudioSource>();
            deniedSound.Play();
        }
    }

    public void ToggleBoundsOnOff()
    {
        table.GetComponent<BoundsControl>().enabled ^= true;
        table.GetComponent<ObjectManipulator>().enabled ^= true;
    }

    public void RotateTileRPC()
    {
        if (PhotonNetwork.LocalPlayer.NickName == (currentPlayer.getID() + 1).ToString())
            photonView.RPC("RotateTile", RpcTarget.All);
    }


    [PunRPC]
    public void RotateTile()
    {
        if (state == GameStates.TileDrawn)
        {
            NewTileRotation++;
            if (NewTileRotation > 3) NewTileRotation = 0;
            currentTile.GetComponent<TileScript>().Rotate();

            if (pcRotate) currentTile.transform.Rotate(0.0f, 90.0f, 0.0f, Space.Self);
        }
    }

    public void ResetTileRotation()
    {
        NewTileRotation = 0;
        currentTile.GetComponent<TileScript>().rotation = 0;
    }

    private void GameOver()
    {
        calculatePoints(true, true);
        ChangePlayerHud();
        state = GameStates.GameOver;
    }

    [PunRPC]
    public void DebugStuff()
    {
        //GameObject text = GameObject.Find("DebugText");
        //GameObject text1 = GameObject.Find("DebugText (1)");
        //if (currentTile != null)
        //{
        //    if (PhotonNetwork.CurrentRoom.PlayerCount > 1)
        //    {

        //        text.GetComponent<TextMeshPro>().text = "Blåa Meeples: " + playerScript.players[0].GetFreeMeeples() + " Gröna Meeples: " + playerScript.players[1].GetFreeMeeples();
        //        text1.GetComponent<TextMeshPro>().text = "Antal tiles kvar: " + stackScript.GetTileCount();
        //    }
        //    else
        //    {
        //        text.GetComponent<TextMeshPro>().text = "Blåa Meeples: " + playerScript.players[0].GetFreeMeeples();
        //        text1.GetComponent<TextMeshPro>().text = "Antal tiles kvar: " + stackScript.GetTileCount() + 1;
        //    }


        //}
    }

    public void SetCurrentTileSnapPosition()
    {
        currentTile.transform.localPosition = snapPosition;
    }


    public void RotateDegreesRPC()
    {
        photonView.RPC("RotateDegrees", RpcTarget.All);
    }

    public void SaveEulersOnManipRPC()
    {
        photonView.RPC("SaveEulersOnManip", RpcTarget.All);
    }

    [PunRPC]
    public void RotateDegrees()
    {
        if (!pcRotate)
        {
            var startRotationValue = currentTileEulersOnManip.y;
            var onRealeaseRotationValue = currentTile.transform.localEulerAngles.y;
            float endRotationValue = 0;

            if (onRealeaseRotationValue <= 45 || onRealeaseRotationValue >= 315)
                currentTile.transform.localEulerAngles = new Vector3(currentTile.transform.localEulerAngles.x, 0,
                    currentTile.transform.localEulerAngles.z);
            else if (onRealeaseRotationValue <= 135 && onRealeaseRotationValue >= 45)
                currentTile.transform.localEulerAngles = new Vector3(currentTile.transform.localEulerAngles.x, 90,
                    currentTile.transform.localEulerAngles.z);
            else if (onRealeaseRotationValue <= 225 && onRealeaseRotationValue >= 135)
                currentTile.transform.localEulerAngles = new Vector3(currentTile.transform.localEulerAngles.x, 180,
                    currentTile.transform.localEulerAngles.z);
            else if (onRealeaseRotationValue <= 315 && onRealeaseRotationValue >= 225)
                currentTile.transform.localEulerAngles = new Vector3(currentTile.transform.localEulerAngles.x, 270,
                    currentTile.transform.localEulerAngles.z);

            if (startRotationValue == 270 && onRealeaseRotationValue == 0)
                endRotationValue = 1;
            else
                endRotationValue = (onRealeaseRotationValue - startRotationValue) / 90;

            endRotationValue = (float) Math.Abs(Math.Round(endRotationValue, 0));

            Debug.Log("DET HÄR START: " + startRotationValue + " MINUS DEN HÄR CURRENT EURLERS " +
                      currentTile.transform.localEulerAngles.y + " DELAS PÅ 90! ÄR LIKA MED " + endRotationValue);

            if (startRotationValue > (int) onRealeaseRotationValue && endRotationValue == 1 &&
                onRealeaseRotationValue != 0)
            {
                endRotationValue = 3;
                Debug.Log("I ifsatsena " + endRotationValue);
            }

            for (var i = 0; i < Math.Abs(endRotationValue); i++) RotateTileRPC();
        }

        pcRotate = false;
        isManipulating = false;
    }

    [PunRPC]
    public void SaveEulersOnManip()
    {
        currentTileEulersOnManip = currentTile.transform.localEulerAngles;
        Debug.Log(currentTileEulersOnManip);
        isManipulating = true;
    }


    private void UpdateDecisionButtons(bool confirm, bool rotate, GameObject tileOrMeeple)
    {
        if (currentPlayer.photonUser.GetComponent<PhotonView>().IsMine)
            confirmButton.SetActive(confirm);
        //rotateButton.SetActive(rotate);
        decisionButtons.GetComponent<Anchor_Script>().anchor = tileOrMeeple.transform.Find("North").gameObject;
    }

    public void ChangeStateToNewTurn()
    {
        state = GameStates.NewTurn;
    }

    public void AimMeeple()
    {
        try
        {
            if (placedTiles.getPlacedTiles(iMeepleAimX, iMeepleAimZ) == currentTile)
            {
                var tile = placedTiles.getPlacedTiles(iMeepleAimX, iMeepleAimZ);
                var tileScript = tile.GetComponent<TileScript>();

                var layerMask = 1 << 9;
                Physics.Raycast(currentMeeple.transform.position,
                    currentMeeple.transform.TransformDirection(Vector3.down), out meepleHitTileDirection,
                    Mathf.Infinity, layerMask);
                var id = tile.GetComponent<TileScript>().id;

                meepleGeography = TileScript.geography.Grass;
                direction = PointScript.Direction.CENTER;

                if (meepleHitTileDirection.collider != null)
                {
                    if (meepleHitTileDirection.collider.name == "East")
                    {
                        direction = PointScript.Direction.EAST;
                        meepleGeography = tileScript.East;
                    }
                    else if (meepleHitTileDirection.collider.name == "West")
                    {
                        direction = PointScript.Direction.WEST;
                        meepleGeography = tileScript.West;
                    }
                    else if (meepleHitTileDirection.collider.name == "North")
                    {
                        direction = PointScript.Direction.NORTH;
                        meepleGeography = tileScript.North;
                    }
                    else if (meepleHitTileDirection.collider.name == "South")
                    {
                        direction = PointScript.Direction.SOUTH;
                        meepleGeography = tileScript.South;
                    }
                    else if (meepleHitTileDirection.collider.name == "Center")
                    {
                        direction = PointScript.Direction.CENTER;
                        meepleGeography = tileScript.getCenter();
                    }

                    snapPosition = meepleHitTileDirection.collider.transform.position;

                    if (meepleGeography == TileScript.geography.City || meepleGeography == TileScript.geography.Road ||
                        meepleGeography == TileScript.geography.Cloister)
                    {
                        ChangeConfirmButtonApperance(true);
                        canConfirm = true;
                    }
                }
                else
                {
                    snapPosition = currentMeeple.transform.position;
                    ChangeConfirmButtonApperance(false);
                    canConfirm = false;
                }
            }
            else
            {
                snapPosition = currentMeeple.transform.position;
                meepleGeography = TileScript.geography.Grass;
                ChangeConfirmButtonApperance(false);
                canConfirm = false;
            }
        }
        catch (IndexOutOfRangeException e)
        {
            Debug.Log(e);
            errorOutput = e.ToString();
        }
    }

    private void ChangeConfirmButtonApperance(bool confirmed)
    {
        if (confirmed)
        {
            confirmButtonBackplate.GetComponent<MeshRenderer>().material = buttonMaterials[2];
            confirmButton.GetComponentInChildren<SpriteRenderer>().sprite = checkIcon;
        }
        else
        {
            confirmButtonBackplate.GetComponent<MeshRenderer>().material = buttonMaterials[0];
            confirmButton.GetComponentInChildren<SpriteRenderer>().sprite = crossIcon;
        }
    }

    public void SetMeepleSnapPos()
    {
        if (meepleHitTileDirection.collider != null)
        {
            currentMeeple.transform.position =
                new Vector3(snapPosition.x, currentMeeple.transform.position.y, snapPosition.z);

            if (direction == PointScript.Direction.WEST || direction == PointScript.Direction.EAST)
            {
                if (currentMeeple.transform.rotation.eulerAngles.y != 90)
                    currentMeeple.transform.Rotate(0.0f, 90.0f, 0.0f, Space.Self);
            }
            else if (direction == PointScript.Direction.NORTH || direction == PointScript.Direction.SOUTH ||
                     direction == PointScript.Direction.CENTER)
            {
                if (currentMeeple.transform.rotation.eulerAngles.y == 90)
                    currentMeeple.transform.Rotate(0.0f, -90.0f, 0.0f, Space.Self);
            }
        }
        else
        {
            snapPosition = currentMeeple.transform.position;
        }
    }

    private void CurrentMeepleRayCast()
    {
        RaycastHit hit;
        var layerMask = 1 << 8;

        Physics.Raycast(currentMeeple.transform.position, currentMeeple.transform.TransformDirection(Vector3.down),
            out hit, Mathf.Infinity, layerMask);

        var local = table.transform.InverseTransformPoint(hit.point);


        meepleControllerScript.fMeepleAimX = local.x;
        meepleControllerScript.fMeepleAimZ = local.z;


        if (meepleControllerScript.fMeepleAimX - stackScript.basePositionTransform.localPosition.x > 0)
        {
            iMeepleAimX = (int) ((meepleControllerScript.fMeepleAimX - stackScript.basePositionTransform.localPosition.x) * scale + 1f) / 2 +
                          85;
            var testX = ((meepleControllerScript.fMeepleAimX - stackScript.basePositionTransform.localPosition.x) * 10f + 1f) / 2f + 85f;
        }
        else
        {
            iMeepleAimX = (int) ((meepleControllerScript.fMeepleAimX - stackScript.basePositionTransform.localPosition.x) * scale - 1f) / 2 +
                          85;
            var testX = ((meepleControllerScript.fMeepleAimX - stackScript.basePositionTransform.localPosition.x) * 10f - 1f) / 2f + 85f;
        }

        if (meepleControllerScript.fMeepleAimZ - stackScript.basePositionTransform.localPosition.z > 0)
        {
            iMeepleAimZ = (int) ((meepleControllerScript.fMeepleAimZ - stackScript.basePositionTransform.localPosition.z) * scale + 1f) / 2 +
                          85;
            var testZ = ((fTileAimZ - stackScript.basePositionTransform.localPosition.z) * 10f + 1f) / 2f + 85f;
        }
        else
        {
            iMeepleAimZ = (int) ((meepleControllerScript.fMeepleAimZ - stackScript.basePositionTransform.localPosition.z) * scale - 1f) / 2 +
                          85;

            var testZ = ((fTileAimZ - stackScript.basePositionTransform.localPosition.z) * 10f - 1f) / 2f + 85f;
        }
    }

    public void ChangeButtonMaterialOnPress()
    {
    }

    public void ChangeButtonMaterialOnRelease()
    {
    }
}