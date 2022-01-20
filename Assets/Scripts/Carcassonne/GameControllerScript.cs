using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using Carcassonne.State;
using Carcassonne.State.Features;
using Microsoft.MixedReality.Toolkit;
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
using UnityEngine.InputSystem;

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
        public bool startGame;

        public Material[] playerMaterials;
        public Material[] buttonMaterials;
        public GameObject[] playerHuds;
        public GameObject endButtonBackplate, confirmButtonBackplate;
        public GameObject meepleInButton;
        public ParticleSystem bellSparkleEffect, smokeEffect;

        public float scale;

        [HideInInspector] public GameObject table;


        [HideInInspector] public GameObject playerHUD;

        public GameObject confirmButton;//, rotateButton;
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

        public Vector2Int iTileAim = new Vector2Int();
        public int iTileAimX
        {
            get { return iTileAim.x; }
            set { iTileAim.x = value;}
        }

        public int iTileAimZ
        {
            get { return iTileAim.y; }
            set { iTileAim.y = value;}
        }
        
        [HideInInspector]
        public int minX, maxX, minZ, maxZ; //These are only used for limiting AI agents movement.


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
        public Vector2Int Direction;

        public Vector3 SnapPosition;

        public bool CanConfirm;

        public string ErrorOutput { set; get; } = "";

        [SerializeField]
        internal MeepleControllerScript meepleControllerScript;
    
        [SerializeField]
        public TileControllerScript tileControllerScript;

        private void Start()
        {
            gameState.Features.Graph.Changed += UpdateFeatures;
            
            sb = new StringBuilder();
            sw = new StringWriter(sb);
            writer = new JsonTextWriter(sw);
            writer.WriteStartObject();
            writer.WritePropertyName("bbox");
            writer.WriteStartArray();
        }
        
        public void UpdateFeatures(object sender, BoardChangedEventArgs args)
        {
            BoardGraph graph = args.graph;
            gameState.Features.Cities = City.FromBoardGraph(graph);
            gameState.Features.Roads = Road.FromBoardGraph(graph);
            gameState.Features.Cloisters = Cloister.FromBoardGraph(graph);

            string debugString = "Cities: \n\n";
            foreach (var city in gameState.Features.Cities)
            {
                debugString += city.ToString();
                debugString += "\n";
                debugString += $"Segments: {city.Segments}, Open Sides: {city.OpenSides}, Complete: {city.Complete}";
                debugString += "\n\n";
            }
            Debug.Log(debugString);
            
            debugString = "Roads: \n\n";
            foreach (var road in gameState.Features.Roads)
            {
                debugString += road.ToString();
                debugString += "\n";
                debugString += $"Segments: {road.Segments}, Open Sides: {road.OpenSides}, Complete: {road.Complete}";
                debugString += "\n\n";
            }
            Debug.Log(debugString);
            
            debugString = "Cloisters: \n\n";
            foreach (var cloister in gameState.Features.Cloisters)
            {
                debugString += cloister.ToString();
                debugString += "\n";
                debugString += $"Segments: {cloister.Segments}, Open Sides: {cloister.OpenSides}, Complete: {cloister.Complete}";
                debugString += "\n\n";
            }
            Debug.Log(debugString);
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if( keyboard != null && photonView.IsMine)
            {
                if (keyboard.rKey.wasReleasedThisFrame) tileControllerScript.RotateTileRPC();

                if (keyboard.pKey.wasReleasedThisFrame) EndTurnRPC();

                if (keyboard.tKey.wasReleasedThisFrame) {
                    
                    meepleControllerScript.FreeMeeple(gameState.Meeples.Current.gameObject); //FIXME: Throws error when no meeple assigned!}
                
                    gameState.phase = Phase.TileDown;
                }

                if (keyboard.bKey.wasReleasedThisFrame) GameOver();

                // Keyboard based movements.
                var direction = Vector2Int.zero;
                if (keyboard.jKey.wasPressedThisFrame) direction += Vector2Int.left;
                if (keyboard.lKey.wasPressedThisFrame) direction += Vector2Int.right;
                if (keyboard.iKey.wasPressedThisFrame) direction += Vector2Int.up;
                if (keyboard.kKey.wasPressedThisFrame) direction += Vector2Int.down;

                if (gameState.phase == Phase.TileDrawn & direction != Vector2Int.zero)
                {
                    Debug.Log($"Moving the current tile in {direction}. Sending RPC.");
                    tileControllerScript.MoveTileRPC(direction);
                } /*else if (gameState.phase == Phase.MeepleDrawn)
                {
                    meepleControllerScript.MoveMeepleRPC(direction);
                }*/
            }
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

                SnapPosition = TileControllerScript.BoardToUnity(iTileAim) + stackScript.basePositionTransform.localPosition;
                SnapPosition.y = gameState.Tiles.Current.transform.localPosition.y;
            }

            if (startGame)
            {
                NewGame();
                startGame = false;
            }

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

                    if (firstTurnCounter != 0)
                    {
                        meepleControllerScript.drawMeepleEffect.Play();
                        tileControllerScript.currentTile.transform.localPosition = new Vector3
                        (stackScript.basePositionTransform.localPosition.x + (iTileAimX - GameRules.BoardSize / 2) * 0.033f, 0.5900002f,
                            stackScript.basePositionTransform.localPosition.z + (iTileAimZ - GameRules.BoardSize / 2) * 0.033f);
                        endButtonBackplate.GetComponent<MeshRenderer>().material = buttonMaterials[1];
                    }

                    break;
                case Phase.MeepleDrawn:

                    //confirmButton.SetActive(true);
                    //confirmButton.transform.position = new Vector3(currentMeeple.transform.position.x + 0.05f, currentMeeple.transform.position.y + 0.05f, currentMeeple.transform.position.z + 0.07f);
                    ////confirmButton.transform.up = table.transform.forward;
                    meepleControllerScript.drawMeepleEffect.Stop();
                    meepleControllerScript.CurrentMeepleRayCast();
                    meepleControllerScript.AimMeeple();

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
            placedTiles.PlacedTilesArrayIsEmptyCheck();

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

            VertexItterator = 1;
            //Variables used for AI placing boundary. It starts at the starting tiles coordinates which would be [20,20] 
            minX = GameRules.BoardSize / 2;
            minZ = GameRules.BoardSize / 2;
            maxX = GameRules.BoardSize / 2;
            maxZ = GameRules.BoardSize / 2;

            PlaceTile(tileControllerScript.currentTile, GameRules.BoardSize / 2, GameRules.BoardSize / 2, true);

            currentPlayer = gameState.Players.All[0];

            Debug.Log("Denna spelarese namn: " + PhotonNetwork.LocalPlayer.NickName);
            Debug.Log("Current " + (currentPlayer.getID() + 1));

            playerHuds[0].GetComponentInChildren<MeshRenderer>().material = playerMaterials[0];
            meepleInButton.GetComponent<MeshRenderer>().material = buttonMaterials[3];

            gameState.phase = Phase.NewTurn;
        }

        private void BaseTileCreation()
        {
            tileControllerScript.currentTile = stackScript.firstTile;
            // tileControllerScript.currentTile.name = "BaseTile";
            tileControllerScript.currentTile.transform.parent = table.transform;
            tileControllerScript.currentTile.GetComponent<ObjectManipulator>().enabled = false;
            tileControllerScript.currentTile.GetComponent<NearInteractionGrabbable>().enabled = false;
        }

        [PunRPC]
        public void CurrentTileRaycastPosition()
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
                iTileAimX = (int) ((tileControllerScript.fTileAimX - stackScript.basePositionTransform.localPosition.x) * scale + 1f) / 2 + GameRules.BoardSize / 2;
            }
            else
            {
                iTileAimX = (int) ((tileControllerScript.fTileAimX - stackScript.basePositionTransform.localPosition.x) * scale - 1f) / 2 + GameRules.BoardSize / 2;
            }

            if (tileControllerScript.fTileAimZ - stackScript.basePositionTransform.localPosition.z > 0)
            {
                iTileAimZ = (int) ((tileControllerScript.fTileAimZ - stackScript.basePositionTransform.localPosition.z) * scale + 1f) / 2 + GameRules.BoardSize / 2;
            }
            else
            {
                iTileAimZ = (int) ((tileControllerScript.fTileAimZ - stackScript.basePositionTransform.localPosition.z) * scale - 1f) / 2 + GameRules.BoardSize / 2;
            }
        }

        //Metod för att placera en tile på brädan
        public void PlaceTile(GameObject tile, int x, int z, bool firstTile)
        {
            tempX = x;
            tempY = z;

            UpdateAIBoundary(x, z);
            
            tile.GetComponent<BoxCollider>().enabled = false;
            tile.GetComponent<Rigidbody>().useGravity = false;
            tile.GetComponent<ObjectManipulator>().enabled = false;
            tile.GetComponent<Rigidbody>().isKinematic = true;

            if (!firstTile)
            {
                placedTiles.PlaceTile(x, z, tile);

                if (gameState.Players.Current.controlledByAI) //The snapposition cannot be used for the AI as it does not move the tile. It uses iTileAim instead.
                {
                    tileControllerScript.currentTile.transform.localPosition = new Vector3(stackScript.basePositionTransform.localPosition.x + (iTileAimX - GameRules.BoardSize/2) * 0.033f,
                        tileControllerScript.currentTile.transform.localPosition.y, stackScript.basePositionTransform.localPosition.z + (iTileAimZ - GameRules.BoardSize/2) * 0.033f);
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
                var currentTileGameObject = stackScript.Pop();
                UpdateDecisionButtons(true, currentTileGameObject);
                tileControllerScript.ActivateCurrentTile();
                var currentTile = gameState.Tiles.Current;
                
                if (!PlacedTiles.TileCanBePlaced(currentTile, this))
                {
                    Debug.Log($"Tile (ID: {currentTile.id}) not possible to place: discarding and drawing a new one.");
                    Destroy(currentTile);
                    PickupTile();
                }
                else
                {
                    tileControllerScript.ResetTileRotation();
                    gameState.phase = Phase.TileDrawn;
                }
            }
            else
            {
                var deniedSound = gameObject.GetComponent<AudioSource>();
                deniedSound.Play();
            }
        }
        
        //TODO This needs to be separated into a confirmMeeplePlacement and confirmTilePlacement
        public void ConfirmPlacementRPC()
        {
            if (PhotonNetwork.LocalPlayer.NickName == (currentPlayer.getID() + 1).ToString())
            {
                if (currentPlayer.controlledByAI) //This section is only used by the AI. As it does not move the tile physically, the aim has to be set manually before the call.
                {
                    photonView.RPC("ConfirmPlacementAI", RpcTarget.All, iTileAimX, iTileAimZ, meepleControllerScript.aiMeepleX, meepleControllerScript.aiMeepleZ);
                }
                else
                {
                    photonView.RPC("ConfirmPlacement", RpcTarget.All);
                }
            }
        }

        //This method replaces the ConfirmPLacementRPC method for the AI agent, which does not move the game objects. The placements has to be explicitly set before ConfirmPlacement()-call.
        [PunRPC]
        public void ConfirmPlacementAI(int tileX, int tileZ, float meepleX, float meepleZ)
        {
            if (gameState.phase == Phase.TileDrawn)
            {
                iTileAimX = tileX;
                iTileAimZ = tileZ;
                ConfirmPlacement();
            } else if (gameState.phase == Phase.MeepleDrawn) //TODO: Replace the complex meeple placement code with something less tied to the gameObjects physical position. Something more AI Friendly.
            {
                //The following code is needed as the meeple placement is heavily tied to the physical position of the meeple. May be better with a separate and simpler AI method for this as it may not
                //work in multiplayer when the meeple position needs to be updated.

                System.Diagnostics.Debug.Assert(gameState.Meeples.Current != null, "gameState.Meeples.Current != null");
                var meepleGameObject = gameState.Meeples.Current.gameObject; 
                
                meepleGameObject.transform.localPosition = gameState.Tiles.Current.transform.localPosition + new Vector3(meepleX, 0.86f, meepleZ);
                meepleControllerScript.CurrentMeepleRayCast();
                meepleControllerScript.AimMeeple();
                meepleControllerScript.SetMeepleSnapPos();
                ConfirmPlacement();

                //The two rows below are just a workaround to get meeples to stay on top of the table and not have a seemingly random Y coordinate. This may need a mode solid fix for multiplayer mode.
                meepleGameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY;
                meepleGameObject.transform.localPosition = new Vector3(meepleGameObject.transform.localPosition.x, 0.86f, meepleGameObject.transform.localPosition.z);
            }
            
        }

        [PunRPC]
        public void ConfirmPlacement()
        {
            //The raycast should only happen for base tile and human players. AI does not move the tile. Why this tile raycast call was done outside phase check I dont know, but I left it there.
            if (currentPlayer == null || !currentPlayer.controlledByAI) 
            {
                CurrentTileRaycastPosition();
            }
            
            if (gameState.phase == Phase.TileDrawn)
            {
                if (placedTiles.TilePlacementIsValid(tileControllerScript.currentTile, iTileAimX, iTileAimZ))
                {
                    PlaceTile(tileControllerScript.currentTile, iTileAimX, iTileAimZ, false);

                    confirmButton.SetActive(false);
                    gameState.phase = Phase.TileDown;

                    Debug.Log("Tile placed in (" + iTileAimX + ", " + iTileAimZ + ")");
                }
            }
            else if (gameState.phase == Phase.MeepleDrawn)
            {
                if (gameState.Meeples.Current != null)
                {
                    if (CanConfirm)
                    {
                        var position = new Vector2Int(meepleControllerScript.iMeepleAimX,
                            meepleControllerScript.iMeepleAimZ);
                        var placed = meepleControllerScript.PlaceMeeple(position, Direction);
                        if (!placed)
                        {
                            Debug.LogWarning($"Something has gone wrong with meeple placement. {position}, {Direction} should be a valid position for your Meeple, but for some reason it is not.");
                        }
                    }
                    else // Cancel Meeple Placement
                    {
                        meepleControllerScript.CancelPlacement();

                        //TODO test multiplayer on this!
                        gameState.phase = Phase.TileDown;
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
            if (gameState.phase == Phase.TileDown || gameState.phase == Phase.MeepleDown)
            {
                
                gameState.Log.LogTurn();
                
                // Check finished features
                var features = gameState.Features.CompleteWithMeeples;
                ScoreFeatures(features);
                
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
                    gameState.Tiles.Current = null;
                    gameState.Meeples.Current = null;
                }
                
                Debug.Log($"Board Matrix Dims: {gameState.Tiles.Matrix.GetLength(0)}" +
                          $"x{gameState.Tiles.Matrix.GetLength(1)}" +
                          $" ({gameState.Tiles.Matrix.Length})\n" +
                          $"Board Matrix Origin: {gameState.Tiles.MatrixOrigin}\n" +
                          $"Board Matrix:\n{gameState.Tiles}\n" +
                          $"City Bounds: {gameState.Features.Cities[0]}");
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
                    "Score: " + gameState.Players.All[i].Score;


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

        /// <summary>
        /// Calculates scores, assigns points to players, and frees meeples from features.
        /// Should be called with a list of newly completed features after each turn OR at the end of the game for all
        /// incomplete features.
        /// </summary>
        /// <param name="features"></param>
        public void ScoreFeatures(IEnumerable<FeatureGraph> features)
        {
            foreach( var f in features)
            {
                var meeples = gameState.Meeples.InFeature(f).ToList();

                var playerMeeples = meeples.GroupBy(m => m.player);
                var playerMeepleCount = playerMeeples.ToDictionary(g => g.Key, g => g.Count());

                var scoringPlayers = playerMeepleCount.
                    Where(kvp => kvp.Value == playerMeepleCount.Values.Max())
                    .Select((kvp => kvp.Key));

                // Calculate points for those that are finished
                foreach (var p in scoringPlayers)
                {
                    p.Score += f.Points;
                }

                // Free meeples
                foreach( var m in meeples)
                {
                    meepleControllerScript.FreeMeeple(m.gameObject);
                }
            }
        }
        
        public void ToggleBoundsOnOff()
        {
            table.GetComponent<BoundsControl>().enabled ^= true;
            table.GetComponent<ObjectManipulator>().enabled ^= true;
        }


        private void GameOver()
        {
            Debug.Log("Game Over.");
            var features = gameState.Features.Incomplete;
            ScoreFeatures(features);
            
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


        public void UpdateDecisionButtons(bool confirm, GameObject tileOrMeeple)
        {
            if (currentPlayer.photonUser.GetComponent<PhotonView>().IsMine)
                confirmButton.SetActive(confirm);
            decisionButtons.GetComponent<Anchor_Script>().anchor = tileOrMeeple.transform.Find("North").gameObject;
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

        public bool CurrentPlayerIsLocal
        {
            //TODO This probably should not be hardcoded. See if there is a better way to do this!
            get
            {
                return PhotonNetwork.LocalPlayer.NickName ==
                       (currentPlayer.getID() + 1).ToString();
            }
        }

        /// <summary>
        /// Update the boundaries that the AI can place tiles within. Variablesare based on the
        /// on tiles furthest in each direction on the grid
        /// </summary>
        /// <param name="x"></param>
        /// <param name="z"></param>
        public void UpdateAIBoundary(int x, int z)
        {
            if (x < minX)
            {
                minX = x;
            }
            if (z < minZ)
            {
                minZ = z;
            }
            if (x > maxX)
            {
                maxX = x;
            }
            if (z > maxZ)
            {
                maxZ = z;
            }
        }
    }
}