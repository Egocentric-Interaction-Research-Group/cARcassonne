using System;
using System.Collections.Generic;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;


public class GameControllerScript : MonoBehaviourPun
{
    // Add Meeple Down state functionality
    /// <summary>
    /// Describes different phases of gameplay.
    /// </summary>
    public enum Phases
    {
        NewTurn,
        TileDrawn,
        TileDown,
        MeepleDrawn,
        MeepleDown,
        GameOver
    }

    public bool gravity;
    public bool startGame, pcRotate, isManipulating;

    public Material[] playerMaterials;
    public Material[] buttonMaterials;
    public GameObject[] playerHuds;
    public GameObject endButtonBackplate, confirmButtonBackplate;
    public GameObject meepleInButton;
    public ParticleSystem bellSparkleEffect, smokeEffect;

    public float scale;

    [HideInInspector] public GameObject table;


    [HideInInspector] public GameObject playerHUD;

    public GameObject confirmButton, rotateButton;
    public Sprite crossIcon, checkIcon;

    public RectTransform mPanelGameOver;
    
    /// <summary>
    /// Describes what is happening currently in the game.
    /// </summary>
    [FormerlySerializedAs("state")] public Phases phase;

    //private int xs, zs;

    private float aimX = 0, aimZ = 0;

    private bool canConfirm;

    private bool cityIsFinished;

    [HideInInspector] public PlayerScript.Player currentPlayer;

    private GameObject decisionButtons;

    private PointScript.Direction direction;

    private string errorOutput = "";

    private int firstTurnCounter;

    private bool isPunEnabled;
    //float xOffset, zOffset, yOffset;

    private int iTileAimX, iTileAimZ;

    private int NewTileRotation;

    private Vector3 snapPosition;

    //public ErrorPlaneScript ErrorPlane;


    private int tempX;
    private int tempY;

    // private TurnScript turnScript;

    private int VertexItterator;

    private bool[,] visited;
    
    /* SCRIPTS */

    //The points of each player where each index represents a player (index+1).
    // public int[] points;
    //The matrix of tiles (separated by 2.0f in all 2D directions)
    //private GameObject[,] placedTiles;
    private PlacedTilesScript placedTiles;

    internal PlayerScript playerScript;

    [HideInInspector] public StackScript stackScript;

    public bool IsPunEnabled
    {
        set => isPunEnabled = value;
    }

    public PlayerScript PlayerScript
    {
        set => playerScript = value;
        get => playerScript;
    }

    public PlacedTilesScript PlacedTiles
    {
        set => placedTiles = value;
        get => placedTiles;
    }

    public TileControllerScript TileControllerScript
    {
        set => tileControllerScript = value;
        get => tileControllerScript;
    }

    public PointScript.Direction Direction
    {
        set => direction = value;
        get => direction;
    }

    public Vector3 SnapPosition
    {
        set => snapPosition = value;
        get => snapPosition;
    }

    public bool CanConfirm
    {
        set => canConfirm = value;
        get => canConfirm;
    }

    public string ErrorOutput
    {
        set => errorOutput = value;
        get => errorOutput;
    }

    public PlayerScript PlayerScript1
    {
        set => playerScript = value;
        get => playerScript;
    }

    public PlayerScript PlayerScript2
    {
        set => playerScript = value;
        get => playerScript;
    }

    public TileControllerScript TileControllerScript1
    {
        set => tileControllerScript = value;
        get => tileControllerScript;
    }

    public TileControllerScript TileControllerScript2
    {
        set { tileControllerScript = value; }
        get { return tileControllerScript; }
    }

    [SerializeField]
    internal MeepleControllerScript meepleControllerScript;
    
    [SerializeField]
    internal TileControllerScript tileControllerScript;

    private void Start()
    {
    }

    // Update is called once per frame
    private void FixedUpdate()
    {
        if (tileControllerScript.currentTile != null)
        {
            CurrentTileRaycastPosition();

            if (placedTiles.TilePlacementIsValid(tileControllerScript.currentTile, iTileAimX, iTileAimZ))
                ChangeConfirmButtonApperance(true);
            else
                ChangeConfirmButtonApperance(false);

            snapPosition = new Vector3
            (stackScript.basePositionTransform.localPosition.x + (iTileAimX - 85) * 0.033f, tileControllerScript.currentTile.transform.localPosition.y,
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

        if (Input.GetKeyDown(KeyCode.J)) meepleControllerScript.FreeMeeple(meepleControllerScript.currentMeeple, this); //FIXME: Throws error when no meeple assigned!
        if (Input.GetKeyDown(KeyCode.B)) GameOver(); //FIXME Doesn't work/no effect

        switch (phase)
        {
            case Phases.NewTurn:
                bellSparkleEffect.Stop();
                meepleControllerScript.drawMeepleEffect.Stop();

                if (firstTurnCounter != 0) tileControllerScript.drawTileEffect.Play();

                endButtonBackplate.GetComponent<MeshRenderer>().material = buttonMaterials[0];
                tileControllerScript.drawTile.GetComponent<BoxCollider>().enabled = true;


                break;
            case Phases.TileDrawn:
                //drawTile.GetComponent<BoxCollider>().enabled = false;
                tileControllerScript.drawTileEffect.Stop();

                break;
            case Phases.TileDown:

                if (firstTurnCounter != 0) meepleControllerScript.drawMeepleEffect.Play();
                tileControllerScript.currentTile.transform.localPosition = new Vector3
                (stackScript.basePositionTransform.localPosition.x + (iTileAimX - 85) * 0.033f, 0.5900002f,
                    stackScript.basePositionTransform.localPosition.z + (iTileAimZ - 85) * 0.033f);
                endButtonBackplate.GetComponent<MeshRenderer>().material = buttonMaterials[1];

                break;
            case Phases.MeepleDrawn:

                //confirmButton.SetActive(true);
                //confirmButton.transform.position = new Vector3(currentMeeple.transform.position.x + 0.05f, currentMeeple.transform.position.y + 0.05f, currentMeeple.transform.position.z + 0.07f);
                ////confirmButton.transform.up = table.transform.forward;
                meepleControllerScript.drawMeepleEffect.Stop();
                meepleControllerScript.CurrentMeepleRayCast();
                meepleControllerScript.AimMeeple(this);

                break;
            case Phases.MeepleDown:
                //currentMeeple.transform.position = snapPosition;
                endButtonBackplate.GetComponent<MeshRenderer>().material = buttonMaterials[2];

                bellSparkleEffect.Play();

                break;
            case Phases.GameOver:
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

        // turnScript = GetComponent<TurnScript>();
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

        PlaceTile(tileControllerScript.currentTile, 85, 85, true);

        currentPlayer = playerScript.players[0];

        Debug.Log("Denna spelarese namn: " + PhotonNetwork.LocalPlayer.NickName);
        Debug.Log("Current " + (currentPlayer.getID() + 1));

        playerHuds[0].GetComponentInChildren<MeshRenderer>().material = playerMaterials[0];
        meepleInButton.GetComponent<MeshRenderer>().material = buttonMaterials[3];

        phase = Phases.NewTurn;
    }

    private void BaseTileCreation()
    {
        tileControllerScript.currentTile = stackScript.Pop();
        tileControllerScript.currentTile.name = "BaseTile";
        tileControllerScript.currentTile.transform.parent = table.transform;
        tileControllerScript.currentTile.GetComponent<ObjectManipulator>().enabled = false;
        tileControllerScript.currentTile.GetComponent<NearInteractionGrabbable>().enabled = false;
    }


    public bool CityIsFinishedDirection(int x, int y, PointScript.Direction direction)
    {
        meepleControllerScript.MeeplesInCity = new List<MeepleScript>();
        meepleControllerScript.MeeplesInCity.Add(meepleControllerScript.FindMeeple(x, y, TileScript.Geography.City, direction, this));

        cityIsFinished = true;
        visited = new bool[170, 170];
        RecursiveCityIsFinishedDirection(x, y, direction);
        Debug.Log(
            "DIRECTION__________________________CITY IS FINISHED EFTER DIRECTION REKURSIV: ___________________________" +
            cityIsFinished + " X: " + x + " Z: " + y + " MEEPLEINCITY: " + meepleControllerScript.FindMeeple(x, y, TileScript.Geography.City, this));
        return cityIsFinished;
    }

    //Test City checker
    public bool CityIsFinished(int x, int y)
    {
        meepleControllerScript.MeeplesInCity = new List<MeepleScript>();
        meepleControllerScript.MeeplesInCity.Add(meepleControllerScript.FindMeeple(x, y, TileScript.Geography.City, this));


        cityIsFinished = true;
        visited = new bool[170, 170];
        RecursiveCityIsFinished(x, y);
        Debug.Log("__________________________CITY IS FINISHED EFTER REKURSIV: ___________________________" +
                  cityIsFinished + " X: " + x + " Z: " + y + " MEEPLEINCITY: " + meepleControllerScript.FindMeeple(x, y, TileScript.Geography.City, this));

        return cityIsFinished;
    }

    public void RecursiveCityIsFinishedDirection(int x, int y, PointScript.Direction direction)
    {
        visited[x, y] = true;
        if (direction == PointScript.Direction.NORTH)
            if (placedTiles.getPlacedTiles(x, y).GetComponent<TileScript>().North == TileScript.Geography.City)
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
            if (placedTiles.getPlacedTiles(x, y).GetComponent<TileScript>().East == TileScript.Geography.City)
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
            if (placedTiles.getPlacedTiles(x, y).GetComponent<TileScript>().South == TileScript.Geography.City)
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
            if (placedTiles.getPlacedTiles(x, y).GetComponent<TileScript>().West == TileScript.Geography.City)
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
            if (placedTiles.getPlacedTiles(x, y).GetComponent<TileScript>().North == TileScript.Geography.City)
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

            if (placedTiles.getPlacedTiles(x, y).GetComponent<TileScript>().East == TileScript.Geography.City)
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

            if (placedTiles.getPlacedTiles(x, y).GetComponent<TileScript>().South == TileScript.Geography.City)
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

            if (placedTiles.getPlacedTiles(x, y).GetComponent<TileScript>().West == TileScript.Geography.City)
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

        Physics.Raycast(tileControllerScript.currentTile.transform.position, tileControllerScript.currentTile.transform.TransformDirection(Vector3.down), out hit,
            Mathf.Infinity, layerMask);


        var local = table.transform.InverseTransformPoint(hit.point);

        tileControllerScript.fTileAimX = local.x;
        tileControllerScript.fTileAimZ = local.z;


        if (tileControllerScript.fTileAimX - stackScript.basePositionTransform.localPosition.x > 0)
        {
            iTileAimX = (int) ((tileControllerScript.fTileAimX - stackScript.basePositionTransform.localPosition.x) * scale + 1f) / 2 + 85;

            var testX = ((tileControllerScript.fTileAimX - stackScript.basePositionTransform.localPosition.x) * 10f + 1f) / 2f + 85f;
            //Debug.Log("Float X: " + Math.Round(testX));
        }
        else
        {
            iTileAimX = (int) ((tileControllerScript.fTileAimX - stackScript.basePositionTransform.localPosition.x) * scale - 1f) / 2 + 85;
            var testX = ((tileControllerScript.fTileAimX - stackScript.basePositionTransform.localPosition.x) * 10f - 1f) / 2f + 85f;
            //Debug.Log("Float X: " + Math.Round(testX));
        }

        if (tileControllerScript.fTileAimZ - stackScript.basePositionTransform.localPosition.z > 0)
        {
            iTileAimZ = (int) ((tileControllerScript.fTileAimZ - stackScript.basePositionTransform.localPosition.z) * scale + 1f) / 2 + 85;

            var testZ = ((tileControllerScript.fTileAimZ - stackScript.basePositionTransform.localPosition.z) * 10f + 1f) / 2f + 85f;
            //Debug.Log("Float Z: " + Math.Round(testZ));
        }
        else
        {
            iTileAimZ = (int) ((tileControllerScript.fTileAimZ - stackScript.basePositionTransform.localPosition.z) * scale - 1f) / 2 + 85;

            var testZ = ((tileControllerScript.fTileAimZ - stackScript.basePositionTransform.localPosition.z) * 10f - 1f) / 2f + 85f;
            //Debug.Log("Float Z: " + Math.Round(testZ));
        }
    }

    //Metod för att placera en tile på brädan
    public void PlaceTile(GameObject tile, int x, int z, bool firstTile)
    {
        tempX = x;
        tempY = z;
        tile.GetComponent<TileScript>().vIndex = VertexItterator;

        GetComponent<PointScript>().placeVertex(VertexItterator, placedTiles.GetNeighbors(tempX, tempY),
            placedTiles.getWeights(tempX, tempY), tileControllerScript.currentTile.GetComponent<TileScript>().getCenter(),
            placedTiles.getCenters(tempX, tempY), placedTiles.getDirections(tempX, tempY));

        VertexItterator++;

        tile.GetComponent<BoxCollider>().enabled = false;
        tile.GetComponent<Rigidbody>().useGravity = false;
        tile.GetComponent<ObjectManipulator>().enabled = false;
        tile.GetComponent<Rigidbody>().isKinematic = true;

        if (!firstTile)
        {
            placedTiles.PlaceTile(x, z, tile);


            tileControllerScript.currentTile.transform.localPosition = snapPosition;
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
        if (phase == Phases.NewTurn)
        {
            tileControllerScript.currentTile = stackScript.Pop();
            UpdateDecisionButtons(true, true, tileControllerScript.currentTile);
            TileControllerScript.ActivateCurrentTile(this);
            if (!TileCanBePlaced(tileControllerScript.currentTile.GetComponent<TileScript>()))
            {
                Debug.Log("Tile not possible to place: discarding and drawing a new one. " + "Tile id: " + tileControllerScript.currentTile.GetComponent<TileScript>().id);
                Destroy(tileControllerScript.currentTile);
                PickupTile();
            }
            else
            {
                ResetTileRotation();
                phase = Phases.TileDrawn;
            }
        }
        else
        {
            var deniedSound = gameObject.GetComponent<AudioSource>();
            deniedSound.Play();
        }
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
        if (phase == Phases.TileDrawn)
        {
            if (placedTiles.TilePlacementIsValid(tileControllerScript.currentTile, iTileAimX, iTileAimZ))
            {
                PlaceTile(tileControllerScript.currentTile, iTileAimX, iTileAimZ, false);

                confirmButton.SetActive(false);
                //rotateButton.SetActive(false);
                phase = Phases.TileDown;
            }
            else if (!placedTiles.TilePlacementIsValid(tileControllerScript.currentTile, iTileAimX, iTileAimZ))
            {
                Debug.Log("Tile cant be placed");
            }
        }
        else if (phase == Phases.MeepleDrawn)
        {
            if (meepleControllerScript.currentMeeple != null)
            {
                if (canConfirm)
                {
                    if (meepleControllerScript.meepleGeography == TileScript.Geography.City || meepleControllerScript.meepleGeography == TileScript.Geography.Cloister || meepleControllerScript.meepleGeography == TileScript.Geography.Road) meepleControllerScript.PlaceMeeple(meepleControllerScript.currentMeeple, meepleControllerScript.iMeepleAimX, meepleControllerScript.iMeepleAimZ, direction, meepleControllerScript.meepleGeography, this);
                }
                else
                {
                    meepleControllerScript.FreeMeeple(meepleControllerScript.currentMeeple, this);
                }
            }
        }
    }


    //Funktion för undo
    public void UndoAction()
    {
        if (phase == Phases.TileDown || phase == Phases.MeepleDrawn)
        {
            placedTiles.removeTile(tempX, tempY);
            tileControllerScript.currentTile.GetComponentInChildren<MeshRenderer>().enabled = false;
            phase = Phases.TileDrawn;

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
        if (phase == Phases.TileDown || phase == Phases.MeepleDown)
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

                phase = Phases.NewTurn;
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
                    if (meeple.geography == TileScript.Geography.City)
                    {
                        //CITY DIRECTION
                        if (placedTiles.getPlacedTiles(meeple.x, meeple.z).GetComponent<TileScript>().getCenter() ==
                            TileScript.Geography.Stream ||
                            placedTiles.getPlacedTiles(meeple.x, meeple.z).GetComponent<TileScript>().getCenter() ==
                            TileScript.Geography.Grass ||
                            placedTiles.getPlacedTiles(meeple.x, meeple.z).GetComponent<TileScript>().getCenter() ==
                            TileScript.Geography.Road ||
                            placedTiles.getPlacedTiles(meeple.x, meeple.z).GetComponent<TileScript>().getCenter() ==
                            TileScript.Geography.Village)
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
                            TileScript.Geography.Village ||
                            placedTiles.getPlacedTiles(meeple.x, meeple.z).GetComponent<TileScript>().getCenter() ==
                            TileScript.Geography.Grass)
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
                            TileScript.Geography.Cloister &&
                            meeple.direction == PointScript.Direction.CENTER)
                            finalscore = placedTiles.CheckSurroundedCloister(meeple.x, meeple.z, GameEnd);
                    }

                    if (finalscore > 0 && RealCheck)
                    {
                        Debug.Log(currentPlayer.getID() + " recieved " + finalscore + " points. MEEPLEGEO: " + meepleControllerScript.meepleGeography);
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
        if (phase == Phases.TileDrawn)
        {
            NewTileRotation++;
            if (NewTileRotation > 3) NewTileRotation = 0;
            tileControllerScript.currentTile.GetComponent<TileScript>().Rotate();

            if (pcRotate) tileControllerScript.currentTile.transform.Rotate(0.0f, 90.0f, 0.0f, Space.Self);
        }
    }

    public void ResetTileRotation()
    {
        NewTileRotation = 0;
        tileControllerScript.currentTile.GetComponent<TileScript>().rotation = 0;
    }

    private void GameOver()
    {
        calculatePoints(true, true);
        ChangePlayerHud();
        phase = Phases.GameOver;
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
        tileControllerScript.currentTile.transform.localPosition = snapPosition;
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
            var startRotationValue = tileControllerScript.currentTileEulersOnManip.y;
            var onRealeaseRotationValue = tileControllerScript.currentTile.transform.localEulerAngles.y;
            float endRotationValue = 0;

            if (onRealeaseRotationValue <= 45 || onRealeaseRotationValue >= 315)
                tileControllerScript.currentTile.transform.localEulerAngles = new Vector3(tileControllerScript.currentTile.transform.localEulerAngles.x, 0, tileControllerScript.currentTile.transform.localEulerAngles.z);
            else if (onRealeaseRotationValue <= 135 && onRealeaseRotationValue >= 45)
                tileControllerScript.currentTile.transform.localEulerAngles = new Vector3(tileControllerScript.currentTile.transform.localEulerAngles.x, 90, tileControllerScript.currentTile.transform.localEulerAngles.z);
            else if (onRealeaseRotationValue <= 225 && onRealeaseRotationValue >= 135)
                tileControllerScript.currentTile.transform.localEulerAngles = new Vector3(tileControllerScript.currentTile.transform.localEulerAngles.x, 180, tileControllerScript.currentTile.transform.localEulerAngles.z);
            else if (onRealeaseRotationValue <= 315 && onRealeaseRotationValue >= 225)
                tileControllerScript.currentTile.transform.localEulerAngles = new Vector3(tileControllerScript.currentTile.transform.localEulerAngles.x, 270, tileControllerScript.currentTile.transform.localEulerAngles.z);

            if (startRotationValue == 270 && onRealeaseRotationValue == 0)
                endRotationValue = 1;
            else
                endRotationValue = (onRealeaseRotationValue - startRotationValue) / 90;

            endRotationValue = (float) Math.Abs(Math.Round(endRotationValue, 0));

            Debug.Log("DET HÄR START: " + startRotationValue + " MINUS DEN HÄR CURRENT EURLERS " + tileControllerScript.currentTile.transform.localEulerAngles.y + " DELAS PÅ 90! ÄR LIKA MED " + endRotationValue);

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
        tileControllerScript.currentTileEulersOnManip = tileControllerScript.currentTile.transform.localEulerAngles;
        Debug.Log(tileControllerScript.currentTileEulersOnManip);
        isManipulating = true;
    }


    public void UpdateDecisionButtons(bool confirm, bool rotate, GameObject tileOrMeeple)
    {
        if (currentPlayer.photonUser.GetComponent<PhotonView>().IsMine)
            confirmButton.SetActive(confirm);
        //rotateButton.SetActive(rotate);
        decisionButtons.GetComponent<Anchor_Script>().anchor = tileOrMeeple.transform.Find("North").gameObject;
    }

    public void ChangeStateToNewTurn()
    {
        phase = Phases.NewTurn;
    }

    public void ChangeConfirmButtonApperance(bool confirmed)
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
        if (meepleControllerScript.meepleHitTileDirection.collider != null)
        {
            meepleControllerScript.currentMeeple.transform.position =
                new Vector3(snapPosition.x, meepleControllerScript.currentMeeple.transform.position.y, snapPosition.z);

            if (direction == PointScript.Direction.WEST || direction == PointScript.Direction.EAST)
            {
                if (meepleControllerScript.currentMeeple.transform.rotation.eulerAngles.y != 90) meepleControllerScript.currentMeeple.transform.Rotate(0.0f, 90.0f, 0.0f, Space.Self);
            }
            else if (direction == PointScript.Direction.NORTH || direction == PointScript.Direction.SOUTH ||
                     direction == PointScript.Direction.CENTER)
            {
                if (meepleControllerScript.currentMeeple.transform.rotation.eulerAngles.y == 90) meepleControllerScript.currentMeeple.transform.Rotate(0.0f, -90.0f, 0.0f, Space.Self);
            }
        }
        else
        {
            snapPosition = meepleControllerScript.currentMeeple.transform.position;
        }
    }

    public void ChangeButtonMaterialOnPress()
    {
    }

    public void ChangeButtonMaterialOnRelease()
    {
    }
}