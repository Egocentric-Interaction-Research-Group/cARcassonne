﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Carcassonne.State;
using Microsoft.MixedReality.Toolkit.Input;
using Microsoft.MixedReality.Toolkit.UI;
using Microsoft.MixedReality.Toolkit.UI.BoundsControl;
using Newtonsoft.Json;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEditor.Scripting.Python;
using UnityEditor;
using System.IO;

namespace Carcassonne
{
    public class GameControllerScript : MonoBehaviourPun
    {
        // private void OnEnable()
        // {
        //     gameState.game = this;
        // }

        /// <summary>
        /// Stores the full state of the game for processing.
        /// </summary>
        public GameState gameState; 
        
        // Add Meeple Down state functionality

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

        //private int xs, zs;

        private float aimX = 0, aimZ = 0;

        private bool cityIsFinished;

        [Obsolete("Points to gameState.Players.Current for backwards compatibility. Please use gameState.Players.Current directly instead.")]
        public PlayerScript currentPlayer
        {
            get => gameState.Players.Current;
            set => gameState.Players.Current = value;
        }

        private GameObject decisionButtons;

        private int firstTurnCounter;

        private bool isPunEnabled;
        //float xOffset, zOffset, yOffset;

        [HideInInspector]
        public int iTileAimX, iTileAimZ;

        private int NewTileRotation;

        //public ErrorPlaneScript ErrorPlane;

        private DateTimeOffset currentTime = DateTimeOffset.Now;
        private string JsonBoundingBox;
        public StringBuilder sb;
        public StringWriter sw;
        public JsonWriter writer;

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

        [HideInInspector] public StackScript stackScript;

        public bool IsPunEnabled
        {
            set => isPunEnabled = value;
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

        public PointScript.Direction Direction;

        public Vector3 SnapPosition;

        public bool CanConfirm;

        public string ErrorOutput { set; get; } = "";

        //FIXME: I don't understand what the point of these two properties is.
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
            sb = new StringBuilder();
            sw = new StringWriter(sb);
            writer = new JsonTextWriter(sw);
            writer.WriteStartObject();
            writer.WritePropertyName("bbox");
            writer.WriteStartArray();
        }

        // Update is called once per frame
        private void FixedUpdate()
        {
            // I think this creates problems for meeples. This is the wrong check. 
            if (gameState.Tiles.Current != null)
            {
                CurrentTileRaycastPosition();

                if (placedTiles.TilePlacementIsValid(tileControllerScript.currentTile, iTileAimX, iTileAimZ))
                    ChangeConfirmButtonApperance(true);
                else
                    ChangeConfirmButtonApperance(false);

                SnapPosition = new Vector3
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

            if (Input.GetKeyDown(KeyCode.J)) meepleControllerScript.FreeMeeple(gameState.Meeples.Current.gameObject, this); //FIXME: Throws error when no meeple assigned!
            if (Input.GetKeyDown(KeyCode.B)) GameOver(); //FIXME Doesn't work/no effect

            switch (gameState.phase)
            {
                case Phase.NewTurn:
                    bellSparkleEffect.Stop();
                    meepleControllerScript.drawMeepleEffect.Stop();

                    if (firstTurnCounter != 0) tileControllerScript.drawTileEffect.Play();

                    endButtonBackplate.GetComponent<MeshRenderer>().material = buttonMaterials[0];
                    tileControllerScript.drawTile.GetComponent<BoxCollider>().enabled = true;


                    break;
                case Phase.TileDrawn:
                    //drawTile.GetComponent<BoxCollider>().enabled = false;
                    tileControllerScript.drawTileEffect.Stop();

                    break;
                case Phase.TileDown:

                    if (firstTurnCounter != 0) meepleControllerScript.drawMeepleEffect.Play();
                    tileControllerScript.currentTile.transform.localPosition = new Vector3
                    (stackScript.basePositionTransform.localPosition.x + (iTileAimX - 85) * 0.033f, 0.5900002f,
                        stackScript.basePositionTransform.localPosition.z + (iTileAimZ - 85) * 0.033f);
                    endButtonBackplate.GetComponent<MeshRenderer>().material = buttonMaterials[1];

                    break;
                case Phase.MeepleDrawn:

                    //confirmButton.SetActive(true);
                    //confirmButton.transform.position = new Vector3(currentMeeple.transform.position.x + 0.05f, currentMeeple.transform.position.y + 0.05f, currentMeeple.transform.position.z + 0.07f);
                    ////confirmButton.transform.up = table.transform.forward;
                    meepleControllerScript.drawMeepleEffect.Stop();
                    meepleControllerScript.CurrentMeepleRayCast();
                    meepleControllerScript.AimMeeple(this);

                    break;
                case Phase.MeepleDown:
                    //currentMeeple.transform.position = SnapPosition;
                    endButtonBackplate.GetComponent<MeshRenderer>().material = buttonMaterials[2];

                    bellSparkleEffect.Play();

                    break;
                case Phase.GameOver:
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

            BaseTileCreation();

            for (var i = 0; i < players; i++)
            {
                // var newPlayer = Instantiate(PlayerScript);
                var newPhotonUser = GameObject.Find("User" + (i + 1));
                var newPlayer = newPhotonUser.GetComponent<PlayerScript>();
                newPlayer.Setup(i, "player " + i, playerMaterials[i]);
                gameState.Players.All.Add(newPlayer);
                
                playerHuds[i].SetActive(true);
                playerHuds[i].GetComponentInChildren<TextMeshPro>().text = "Score: 0";
                // newPlayer.meeples = GameObject.FindGameObjectsWithTag("Meeple " + i);
                // foreach (var meeple in newPlayer.meeples)
                // {
                //     meeple.GetComponent<MeepleScript>().player = newPlayer;
                // }
            }

            if (PhotonNetwork.IsMasterClient)
                playerHuds[0].transform.GetChild(3).gameObject.GetComponent<TextMeshPro>().text = "Player 1    (You)";
            else
                playerHuds[1].transform.GetChild(3).gameObject.GetComponent<TextMeshPro>().text = "Player 2    (You)";

            NewTileRotation = 0;
            VertexItterator = 1;

            PlaceTile(tileControllerScript.currentTile, 85, 85, true);

            currentPlayer = gameState.Players.All[0];

            Debug.Log("Denna spelarese namn: " + PhotonNetwork.LocalPlayer.NickName);
            Debug.Log("Current " + (currentPlayer.getID() + 1));

            playerHuds[0].GetComponentInChildren<MeshRenderer>().material = playerMaterials[0];
            meepleInButton.GetComponent<MeshRenderer>().material = buttonMaterials[3];

            gameState.phase = Phase.NewTurn;
        }

        private void BaseTileCreation()
        {
            //These two lines are only a workaround for an unknown bug making the basetile spawn not in the center, as the BaseSpawnPosition GameObject is not set to the center.
            GameObject tileSpawn = GameObject.Find("BaseSpawnPosition");
            tileSpawn.transform.localPosition = new Vector3(0, tileSpawn.transform.localPosition.y, 0);

            tileControllerScript.currentTile = stackScript.firstTile;
            // tileControllerScript.currentTile.name = "BaseTile";
            tileControllerScript.currentTile.transform.parent = table.transform;
            tileControllerScript.currentTile.GetComponent<ObjectManipulator>().enabled = false;
            tileControllerScript.currentTile.GetComponent<NearInteractionGrabbable>().enabled = false;
        }

        //TODO Replace this
        public bool CityIsFinishedDirection(int x, int y, PointScript.Direction direction)
        {
            meepleControllerScript.MeeplesInCity = new List<MeepleScript>();
            meepleControllerScript.MeeplesInCity.Add(meepleControllerScript.FindMeeple(x, y, TileScript.Geography.City, direction, this));

            cityIsFinished = true;
            visited = new bool[170, 170];
            RecursiveCityIsFinishedDirection(x, y, direction);
            /*
            Debug.Log(
                "DIRECTION__________________________CITY IS FINISHED EFTER DIRECTION REKURSIV: ___________________________" +
                cityIsFinished + " X: " + x + " Z: " + y + " MEEPLEINCITY: " + meepleControllerScript.FindMeeple(x, y, TileScript.Geography.City, this));

            */
            
            // Test code to print the bounding boxes of a completed city.
            if (cityIsFinished)
            {
                var city = gameState.Features.Cities.Single(c => c.Contains(new Vector2Int(x, y)));
                //Debug.Log(city.positions.Vertices.ToList()[0].tile.ToString());
                

                var limits = city.BoundingBox;
                //Debug.Log($"Bounding box is: ({limits.xMin},{limits.yMin}) - ({limits.xMax},{limits.yMax}");

                //Hardcoded the length of matrix to start on X=65 in TileState.cs therefore needing to subtract with 65 to get the correct coordinates.
                int baseLength = 170;
                int matrixX = 65;
                int matrixY = 65;

                //Multiplying by three because the matrix is increased by 3 on every side.
                //Debug.Log($"Modified Bounding box is: ({(limits.xMin - matrixX)*3},{(limits.yMin - matrixY)*3}) - ({(limits.xMax - matrixX)*3},{(limits.yMax - matrixY)*3}");

            }
            
            return cityIsFinished;
        }

        //Method for checking if city is completed when a brick is placed.
        public bool CheckIfCityIsFinished(int x, int y, PointScript.Direction direction) 
        {
            cityIsFinished = true;
            visited = new bool[170, 170];
            RecursiveCityIsFinishedDirection(x, y, direction);

            // Test code to print the bounding boxes of a completed city.
            if (cityIsFinished)
            {
                var city = gameState.Features.Cities.Single(c => c.Contains(new Vector2Int(x, y)));
                //Debug.Log(city.positions.Vertices.ToList()[0].tile.ToString());


                var limits = city.BoundingBox;
                Debug.Log($"Bounding box is: ({limits.xMin},{limits.yMin}) - ({limits.xMax},{limits.yMax}");

                //Hardcoded the length of matrix to start on X=65 in TileState.cs therefore needing to subtract with 65 to get the correct coordinates.
                int baseLength = 170;
                int matrixX = 65;
                int matrixY = 65;

                int modifiedX = ((limits.xMin - matrixX) * 3);
                int modifiedY = ((limits.yMax - matrixY) * 3);
                int height = ((limits.yMax - matrixY) * 3) - ((limits.yMin - matrixY) * 3);
                int width = ((limits.xMax - matrixX) * 3) - ((limits.xMin - matrixX) * 3);

                //TODO ADD JSON APPENDING

                writer.WriteStartArray();
                writer.WriteValue(modifiedX);
                writer.WriteValue(modifiedY);
                writer.WriteValue(width);
                writer.WriteValue(height);
                writer.WriteEndArray();
               

               
                /*JsonBoundingBox = "{\n" +
                    $"\"bbox: [{modifiedX},{modifiedY},{width},{height}]\n" +
                    "}";
                */
              
                //Multiplying by three because the matrix is increased by 3 on every side.
                Debug.Log($"Modified Bounding box is: ({((limits.xMin - matrixX) * 3)-1},{((limits.yMin - matrixY) * 3)-1}) - ({((limits.xMax - matrixX) * 3)-1},{((limits.yMax - matrixY) * 3)-1}");
                Debug.Log(JsonBoundingBox);
            }

            return cityIsFinished;
        }

        //Test City checker
        //FIXME: Only called in MeepleControllerScript. Is this still used?
        public bool CityIsNotFinishedIfEmptyTileBesideCity(int x, int y)
        { 
            // Create a list of meeples in the city
            meepleControllerScript.MeeplesInCity = new List<MeepleScript>();
            meepleControllerScript.MeeplesInCity.Add(meepleControllerScript.FindMeeple(x, y, TileScript.Geography.City, this));

            // Set up variables
            cityIsFinished = true;
            visited = new bool[170, 170];
            
            // Check to see if city is not finished due to empty tiles
            RecursiveSetCityIsNotFinishedIfEmptyTileBesideCity(x, y);
            Debug.Log("__________________________CITY IS FINISHED EFTER REKURSIV: ___________________________" +
                      cityIsFinished + " X: " + x + " Z: " + y + " MEEPLEINCITY: " + meepleControllerScript.FindMeeple(x, y, TileScript.Geography.City, this));
            
            if (cityIsFinished)
            {
                var city = gameState.Features.Cities.Single(c => c.Contains(new Vector2Int(x, y)));
                var limits = city.BoundingBox;
                Debug.Log($"Bounding box is: ({limits.xMin},{limits.yMin}) - ({limits.xMax},{limits.yMax}");

            }
            
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
                        if (!visited[x, y + 1]) RecursiveSetCityIsNotFinishedIfEmptyTileBesideCity(x, y + 1);
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
                        if (!visited[x + 1, y]) RecursiveSetCityIsNotFinishedIfEmptyTileBesideCity(x + 1, y);
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
                        if (!visited[x, y - 1]) RecursiveSetCityIsNotFinishedIfEmptyTileBesideCity(x, y - 1);
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
                        if (!visited[x - 1, y]) RecursiveSetCityIsNotFinishedIfEmptyTileBesideCity(x - 1, y);
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
        
        
        /// <summary>
        /// This tells you if a city is NOT finished due to there being an empty square on the city side of a tile.
        /// It does not return anything, just sets cityIsFinished as false.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        public void RecursiveSetCityIsNotFinishedIfEmptyTileBesideCity(int x, int y)
        {
            visited[x, y] = true;


            if (placedTiles.getPlacedTiles(x, y) != null) // If there is a tile here
            {
                if (placedTiles.getPlacedTiles(x, y).GetComponent<TileScript>().North == TileScript.Geography.City)
                    if (!placedTiles.CityTileHasGrassOrStreamCenter(x, y))
                    {
                        if (placedTiles.getPlacedTiles(x, y + 1) != null)

                        {
                            if (!visited[x, y + 1]) RecursiveSetCityIsNotFinishedIfEmptyTileBesideCity(x, y + 1);
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
                            if (!visited[x + 1, y]) RecursiveSetCityIsNotFinishedIfEmptyTileBesideCity(x + 1, y);
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
                            if (!visited[x, y - 1]) RecursiveSetCityIsNotFinishedIfEmptyTileBesideCity(x, y - 1);
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
                            if (!visited[x - 1, y]) RecursiveSetCityIsNotFinishedIfEmptyTileBesideCity(x - 1, y);
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

                if (gameState.Players.Current.controlledByAI) //The snapposition cannot be used for the AI as it does not move the tile. It uses iTileAim instead.
                {
                    tileControllerScript.currentTile.transform.localPosition = new Vector3(stackScript.basePositionTransform.localPosition.x + (iTileAimX - 85) * 0.033f,
                        tileControllerScript.currentTile.transform.localPosition.y, stackScript.basePositionTransform.localPosition.z + (iTileAimZ - 85) * 0.033f);
                }
                else
                {
                    tileControllerScript.currentTile.transform.localPosition = SnapPosition;
                }
            }
            else
            {
                placedTiles.PlaceTile(x, z, tile);
                tile.transform.localPosition = stackScript.basePositionTransform.localPosition;
            }

            if(tile.GetComponent<TileScript>().North == TileScript.Geography.City)
            {
                CheckIfCityIsFinished(x, z, PointScript.Direction.NORTH);
            }
            if (tile.GetComponent<TileScript>().East == TileScript.Geography.City)
            {
                CheckIfCityIsFinished(x, z, PointScript.Direction.EAST);
            }
            if (tile.GetComponent<TileScript>().West == TileScript.Geography.City)
            {
                CheckIfCityIsFinished(x, z, PointScript.Direction.WEST);
            }
            if (tile.GetComponent<TileScript>().South == TileScript.Geography.City)
            {
                CheckIfCityIsFinished(x, z, PointScript.Direction.SOUTH);
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
            if (gameState.phase == Phase.NewTurn)
            {
                stackScript.Pop();
                UpdateDecisionButtons(true, true, tileControllerScript.currentTile);
                TileControllerScript.ActivateCurrentTile(this);
                if (!TileCanBePlaced(gameState.Tiles.Current))
                {
                    Debug.Log("Tile not possible to place: discarding and drawing a new one. " + "Tile id: " + tileControllerScript.currentTile.GetComponent<TileScript>().id);
                    Destroy(tileControllerScript.currentTile);
                    PickupTile();
                }
                else
                {
                    ResetTileRotation();
                    gameState.phase = Phase.TileDrawn;
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
            if (currentPlayer == null || !currentPlayer.controlledByAI) //This should only happen for base tile and human players. AI does not move the tile.
            {
                CurrentTileRaycastPosition();
            }
            if (gameState.phase == Phase.TileDrawn)
            {
                if (placedTiles.TilePlacementIsValid(tileControllerScript.currentTile, iTileAimX, iTileAimZ))
                {
                    PlaceTile(tileControllerScript.currentTile, iTileAimX, iTileAimZ, false);

                    confirmButton.SetActive(false);
                    //rotateButton.SetActive(false);
                    gameState.phase = Phase.TileDown;

                    Debug.Log("Tile placed in (" + iTileAimX + ", " + iTileAimZ + ")");
                }
                else if (!placedTiles.TilePlacementIsValid(tileControllerScript.currentTile, iTileAimX, iTileAimZ))
                {
                    //Debug.Log("Tile cant be placed in (" + iTileAimX + ", " + iTileAimZ + ")");
                }
            }
            else if (gameState.phase == Phase.MeepleDrawn)
            {
                if (gameState.Meeples.Current != null)
                {
                    if (CanConfirm)
                    {
                        if (meepleControllerScript.meepleGeography == TileScript.Geography.City ||
                            meepleControllerScript.meepleGeography == TileScript.Geography.Cloister ||
                            meepleControllerScript.meepleGeography == TileScript.Geography.Road)
                        {
                            meepleControllerScript.PlaceMeeple(gameState.Meeples.Current.gameObject,
                                meepleControllerScript.iMeepleAimX, meepleControllerScript.iMeepleAimZ,
                                Direction, meepleControllerScript.meepleGeography, this);
                        }
                    }
                    else
                    {
                        meepleControllerScript.FreeMeeple(gameState.Meeples.Current.gameObject, this);
                    }
                }
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
            gameState.Log.LogTurn();
            
            if (gameState.phase == Phase.TileDown || gameState.phase == Phase.MeepleDown)
            {
                calculatePoints(true, false);
                NewTileRotation = 0;
                if (stackScript.isEmpty())
                {
                    GameOver();
                }
                else
                {
                    if (gameState.Players.All.Count > 1)
                    {
                        if (currentPlayer == gameState.Players.All[0])
                            currentPlayer = gameState.Players.All[1];
                        else
                            currentPlayer = gameState.Players.All[0];
                    }


                    Debug.Log("CurrentPlayer = " + currentPlayer.getID());
                    ChangePlayerHud();

                    if (firstTurnCounter != 0) firstTurnCounter -= 1;

                    gameState.phase = Phase.NewTurn;
                }
                
                Debug.Log($"Board Matrix Dims: {gameState.Tiles.Matrix.GetLength(0)}" +
                          $"x{gameState.Tiles.Matrix.GetLength(1)}" +
                          $" ({gameState.Tiles.Matrix.Length})");
                //Debug.Log($"Board Matrix:\n{gameState.Tiles}");
                //File.WriteAllText("Output" + currentTime.ToUnixTimeMilliseconds() + ".txt", gameState.Tiles.ToString());
            }
            else
            {
                var deniedAudio = gameObject.GetComponent<AudioSource>();
                deniedAudio.Play();
            }
        }

        private void ChangePlayerHud()
        {
            for (var i = 0; i < gameState.Players.All.Count; i++)
                playerHuds[i].GetComponentInChildren<TextMeshPro>().text =
                    "Score: " + gameState.Players.All[i].GetPlayerScore();


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
            foreach (var p in gameState.Players.All)
                for (var j = 0; j < p.meeples.Count; j++)
                {
                    var meeple = p.meeples[j];
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
                                TileScript.Geography.Village) // If it's a Stream, Grass, Road, Village
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
                                if (CityIsNotFinishedIfEmptyTileBesideCity(meeple.x, meeple.z))
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
                            meeple.player.SetPlayerScore(
                                meeple.player.GetPlayerScore() + finalscore);

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
            if (gameState.phase == Phase.TileDrawn)
            {
                NewTileRotation++;
                if (NewTileRotation > 3) NewTileRotation = 0;
                gameState.Tiles.Current.Rotate();

                if (pcRotate) tileControllerScript.currentTile.transform.Rotate(0.0f, 90.0f, 0.0f, Space.Self);
            }
        }

        public void ResetTileRotation()
        {
            NewTileRotation = 0;
            gameState.Tiles.Current.rotation = 0;
        }

        private void GameOver()
        {
            calculatePoints(true, true);
            ChangePlayerHud();
            gameState.phase = Phase.GameOver;
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
            tileControllerScript.currentTile.transform.localPosition = SnapPosition;
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
            gameState.phase = Phase.NewTurn;
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
                gameState.Meeples.Current.transform.position =
                    new Vector3(SnapPosition.x, gameState.Meeples.Current.transform.position.y, SnapPosition.z);

                if (Direction == PointScript.Direction.WEST || Direction == PointScript.Direction.EAST)
                {
                    if (gameState.Meeples.Current.transform.rotation.eulerAngles.y != 90) gameState.Meeples.Current.transform.Rotate(0.0f, 90.0f, 0.0f, Space.Self);
                }
                else if (Direction == PointScript.Direction.NORTH || Direction == PointScript.Direction.SOUTH ||
                         Direction == PointScript.Direction.CENTER)
                {
                    if (gameState.Meeples.Current.transform.rotation.eulerAngles.y == 90) gameState.Meeples.Current.transform.Rotate(0.0f, -90.0f, 0.0f, Space.Self);
                }
            }
            else
            {
                SnapPosition = gameState.Meeples.Current.transform.position;
            }
        }

        public void ChangeButtonMaterialOnPress()
        {
        }

        public void ChangeButtonMaterialOnRelease()
        {
        }
        private void OnApplicationQuit()
        {
            writer.WriteEndArray();
            writer.WriteEndObject();
            JsonBoundingBox = sb.ToString();
            File.WriteAllText("Assets/PythonImageGenerator/TxtFiles/"+"Output" + currentTime.ToUnixTimeMilliseconds() + ".txt", gameState.Tiles.ToString());
            File.WriteAllText("Assets/PythonImageGenerator/TxtFiles/"+"Output" + currentTime.ToUnixTimeMilliseconds() + ".json", JsonBoundingBox);
            RunPythonImageGenerator();

        }
        public void RunPythonImageGenerator()
        {
            Debug.Log("Running Python File");
            PythonRunner.RunFile($"{Application.dataPath}/PythonImageGenerator/MatrixToGreyImage.py");
        }
    }
}