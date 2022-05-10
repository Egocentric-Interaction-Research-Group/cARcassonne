using System;
using System.Collections.Generic;
using System.Linq;
using Carcassonne.AR.GamePieces;
using Carcassonne.Controllers;
using Carcassonne.Models;
using Carcassonne.Players;
using Carcassonne.State;
using Photon.Pun;
using UI.Grid;
using UnityEngine;
using PhotonPlayer = Photon.Realtime.Player;
using CarcassonnePlayer = Carcassonne.Models.Player;
using Random = UnityEngine.Random;

namespace Carcassonne.AR
{
    // Select these carefully because they can be nested.
    [RequireComponent(
        typeof(GameController),
        typeof(GameState),
        typeof(MeepleController)
        )]
    public partial class GameControllerScript : MonoBehaviourPun, IGameControllerInterface
    {
        #region Controllers

        public GameState state => GetComponent<GameState>();
        public GameController gameController => GetComponent<GameController>();
        public MeepleController meepleController => GetComponent<MeepleController>();
        public TileController tileController => GetComponent<TileController>();

        internal TileControllerScript tileControllerScript => GetComponent<TileControllerScript>();
        internal MeepleControllerScript meepleControllerScript => GetComponent<MeepleControllerScript>();

        #endregion
        
        public Materials materials;
        
        public float scale;

        [HideInInspector] public GameObject table;
        

        [Obsolete("Points to gameState.Players.Current for backwards compatibility. Please use gameState.Players.Current directly instead.")]
        public Player currentPlayer => state.Players.Current;//.GetComponent<PlayerScript>();
    
        public string ErrorOutput { set; get; } = "";

        private void OnEnable()
        {
            Debug.Log("Game Enabled. Starting new game.");
            
            NewGame();
        }

        //Startar nytt spel
        public void NewGame()
        {
            if (PhotonNetwork.IsMasterClient)
            {
                int nPlayers = PhotonNetwork.CurrentRoom.PlayerCount;

                //TODO Get rid of this once new positioning system works better
                table = GameObject.Find("Table");

                var players = CreatePlayers();
                var meeples = CreateMeeples(players);
                var tiles = CreateDeck();

                var tileOrder = tiles.Select(t => t.GetComponent<ARTile>().photonView.ViewID).ToArray();
                photonView.RPC("NewGameGuest", RpcTarget.Others, tileOrder);

                gameController.NewGame(players, meeples, tiles);

                PlaceStartingTile();

                Debug.Assert(players.Count > 0, "Oops, there are no players.");
                Debug.Assert(state.Players.Current != null,
                    "There was a problem getting the current Player (it was null).");
                Debug.Assert(currentPlayer != null,
                    "There was a problem getting the current PlayerScript (it was null).");
                Debug.Assert(tiles.Count > 0, "There was a problem getting the current Tiles (there are no tiles).");
                Debug.Assert(state.Tiles.Remaining == tiles,
                    $"The remaining tiles was not set correctly. It has a length of {state.Tiles.Remaining.Count}, but tiles has {tiles.Count}.");

                Debug.Log("Denna spelarese namn: " + PhotonNetwork.LocalPlayer.NickName);
            }
        }

        [PunRPC]
        private void NewGameGuest(IEnumerable<int> tileOrder)
        {
            // Find players and meeples and tiles
            var players = FindObjectsOfType<Player>().ToList();
            var meeples = FindObjectsOfType<Meeple>().ToList();
            var tiles = FindObjectsOfType<ARTile>().ToList();
            
            // Order tiles
            var tileStack = new Stack<Tile>();
            foreach (var id in tileOrder.Reverse())
            {
                tileStack.Push(tiles.Single(t => t.photonView.ViewID == id).GetComponent<Tile>());
            }
            
            // New Game
            gameController.NewGame(players, meeples, tileStack);
            
            PlaceStartingTile();

            Debug.Assert(players.Count > 0, "Oops, there are no players.");
            Debug.Assert(state.Players.Current != null,
                "There was a problem getting the current Player (it was null).");
            Debug.Assert(currentPlayer != null,
                "There was a problem getting the current PlayerScript (it was null).");
            Debug.Assert(tiles.Count > 0, "There was a problem getting the current Tiles (there are no tiles).");
            Debug.Assert(state.Tiles.Remaining == tileStack,
                $"The remaining tiles was not set correctly. It has a length of {state.Tiles.Remaining.Count}, but tiles has {tiles.Count}.");

            Debug.Log("Denna spelarese namn: " + PhotonNetwork.LocalPlayer.NickName);
        }

        #region Proton

        // public void PickupTileRPC()
        // {
        //     if (PhotonNetwork.LocalPlayer.NickName == (currentPlayer.id + 1).ToString())
        //         photonView.RPC("PickupTile", RpcTarget.All);
        // }
        
        //TODO This needs to be separated into a confirmMeeplePlacement and confirmTilePlacement
        //Only Called by the AI wrapper right now.
        public void ConfirmDiscardRPC()
        {
            switch (state.phase)
            {
                case Phase.TileDrawn:
                    break;
                case Phase.MeepleDrawn:
                    photonView.RPC("DiscardMeeple", RpcTarget.All);
                    break;
                default:
                    Debug.LogWarning($"The confirm button should not be visible in the game phase {state.phase}.");
                    break;
            }
        }

        //TODO This needs to be separated into a confirmMeeplePlacement and confirmTilePlacement
        //Only Called by the AI wrapper right now.
        public void ConfirmPlacementRPC()
        {
            switch (state.phase)
            {
                case Phase.TileDrawn:
                    photonView.RPC("PlaceTile", RpcTarget.All);
                    break;
                case Phase.MeepleDrawn:
                    photonView.RPC("PlaceMeeple", RpcTarget.All);
                    break;
                default:
                    Debug.LogWarning($"The confirm button should not be visible in the game phase {state.phase}.");
                    break;
            }
        }
        
        // NOT USED. FOR REFERENCE ONLY.
        public void ConfirmPlacementRPC_AI(){
            if (PhotonNetwork.LocalPlayer.NickName == (currentPlayer.id + 1).ToString())
            {
                // if (currentPlayer.controlledByAI) //This section is only used by the AI. As it does not move the tile physically, the aim has to be set manually before the call.
                // {
                    //photonView.RPC("ConfirmPlacementAI", RpcTarget.All, tileController.position.x, tileController.position.y, meepleControllerScript.aiMeepleX, meepleControllerScript.aiMeepleZ);
                // }
                // else
                // {
                //     photonView.RPC("ConfirmPlacement", RpcTarget.All);
                // }
            }
        }
        
        public void EndTurnRPC()
        {
            if (PhotonNetwork.LocalPlayer.NickName == (currentPlayer.id + 1).ToString())
            {
                photonView.RPC("EndTurn", RpcTarget.All);
                // photonView.RPC("DebugStuff", RpcTarget.All);
            }
        }

        #endregion
        
        #region ToEventify

        [PunRPC]
        public void EndTurn()
        {
            var ended = gameController.EndTurn();
            if (ended)
            {
                
            }
            else
            {
                var deniedAudio = gameObject.GetComponent<AudioSource>();
                deniedAudio.Play();
            }
        }

        public void Reset()
        {
        }

        #endregion

        #region Meeple Things To Change

        internal Vector2Int Direction;

        internal Vector3 SnapPosition;

        internal bool CanConfirm;

        //This method replaces the ConfirmPLacementRPC method for the AI agent, which does not move the game objects. The placements has to be explicitly set before ConfirmPlacement()-call.
        // [PunRPC]
        // public void ConfirmPlacementAI(int tileX, int tileZ, float meepleX, float meepleZ)
        // {
        //     if (state.phase == Phase.TileDrawn)
        //     {
        //         tileController.position.x = tileX;
        //         tileController.position.y = tileZ;
        //         ConfirmPlacement();
        //     } else if (state.phase == Phase.MeepleDrawn) //TODO: Replace the complex meeple placement code with something less tied to the gameObjects physical position. Something more AI Friendly.
        //     {
        //         //The following code is needed as the meeple placement is heavily tied to the physical position of the meeple. May be better with a separate and simpler AI method for this as it may not
        //         //work in multiplayer when the meeple position needs to be updated.
        //
        //         System.Diagnostics.Debug.Assert(state.Meeples.Current != null, "gameState.Meeples.Current != null");
        //         var meepleGameObject = state.Meeples.Current.gameObject; 
        //         
        //         meepleGameObject.transform.localPosition = state.Tiles.Current.transform.localPosition + new Vector3(meepleX, 0.86f, meepleZ);
        //         // meepleControllerScript.CurrentMeepleRayCast();
        //         // meepleControllerScript.AimMeeple();
        //         // meepleControllerScript.SetMeepleSnapPos();
        //         ConfirmPlacement();
        //
        //         //The two rows below are just a workaround to get meeples to stay on top of the table and not have a seemingly random Y coordinate. This may need a mode solid fix for multiplayer mode.
        //         meepleGameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY;
        //         meepleGameObject.transform.localPosition = new Vector3(meepleGameObject.transform.localPosition.x, 0.86f, meepleGameObject.transform.localPosition.z);
        //     }
        //     
        // }

        [PunRPC]
        public void ConfirmPlacement()
        {
            //The raycast should only happen for base tile and human players. AI does not move the tile. Why this tile raycast call was done outside phase check I dont know, but I left it there.
            // if (currentPlayer == null || !currentPlayer.controlledByAI) 
            // {
            //     tileUIController.position = tileUIController.RaycastPosition();
            //     tileControllerScript.position = tileUIController.BoardPosition(tileUIController.position);
            // }
            
            // if (state.phase == Phase.TileDrawn)
            // {
            //     if (PlacedTiles.TilePlacementIsValid(state.Tiles.Current.GetComponent<TileScript>(), tileController.position.x, tileController.position.y))
            //     {
            //         PlaceTile(state.Tiles.Current.GetComponent<TileScript>(), tileController.position.x, tileController.position.y, false);
            //
            //         // confirmButton.SetActive(false);
            //         state.phase = Phase.TileDown;
            //
            //         Debug.Log("Tile placed in (" + tileController.position.x + ", " + tileController.position.y + ")");
            //     }
            // }
            // else if (state.phase == Phase.MeepleDrawn)
            if (state.phase == Phase.MeepleDrawn)
            {
                if (state.Meeples.Current != null)
                {
                    if (CanConfirm)
                    {
                        // var position = new Vector2Int(meepleControllerScript.iMeepleAimX,
                        //     meepleControllerScript.iMeepleAimZ);
                        // var placed = meepleController.Place(Coordinates.TileToSubTile(position, Direction));
                        // if (!placed)
                        // {
                        //     Debug.LogWarning($"Something has gone wrong with meeple placement. {position}, {Direction} should be a valid position for your Meeple, but for some reason it is not.");
                        // }
                    }
                    else // Cancel Meeple Placement
                    {
                        meepleController.Discard();

                        //TODO test multiplayer on this!
                        state.phase = Phase.TileDown;
                    }
                }
            }
        }

        private void Update()
        {
            if(state.phase == Phase.MeepleDrawn){
                // meepleControllerScript.CurrentMeepleRayCast();
                // meepleControllerScript.AimMeeple();
            }
        }
        
        #endregion
        
        #region Redesigned Functions

        public void NewGame1()
        {
            CreatePlayers();
        }

        private void PlaceStartingTile()
        {
            var startingTile = state.Tiles.Remaining.Peek();
            
            tileController.PlaceFirst();
            
            // Enable tile view
            startingTile.transform.SetParent(tileControllerScript.tileGrid.transform);
            startingTile.gameObject.SetActive(true);
            startingTile.GetComponent<BoxCollider>().enabled = true;
            startingTile.GetComponent<Rigidbody>().useGravity = true;
            startingTile.GetComponent<Rigidbody>().isKinematic = false;
            startingTile.GetComponentInChildren<MeshRenderer>().enabled = true;
            
            // Move to middle
            startingTile.GetComponent<GridPosition>().MoveTo(Vector2Int.zero);
            
            startingTile.GetComponent<BoxCollider>().enabled = false;
            startingTile.GetComponent<Rigidbody>().useGravity = false;
            startingTile.GetComponent<Rigidbody>().isKinematic = true;
            startingTile.GetComponent<GridKeyboardMovable>().enabled = false;
            startingTile.GetComponent<GridKeyboardRotatable>().enabled = false;
        }

        public void PlaceTile()
        {
            var tile = state.Tiles.Current;
            var gridPosition = tile.transform.parent.GetComponent<GridPosition>();

            tileController.Place(gridPosition.cell);
            
            //IF AI
            // if (state.Players.Current.isAI) //The snapposition cannot be used for the AI as it does not move the tile. It uses iTileAim instead.
            // {
            //     var localPosition = stackScript.basePositionTransform.localPosition;
            //     tile.transform.localPosition = new Vector3(localPosition.x + (tileController.position.x - GameRules.BoardSize/2) * 0.033f,
            //         tile.transform.localPosition.y, localPosition.z + (tileController.position.y - GameRules.BoardSize/2) * 0.033f);
            // }
        }
        
        public void EndTurn1(){}
        
        public void GameOver1(){}
        
        
        private List<Player> CreatePlayers()
        {
            List<Player> players = FindObjectsOfType<Player>().ToList();

            return players;
        }

        private List<Meeple> CreateMeeples(List<Player> players)
        {
            var meeples = new List<Meeple>();
            // var meepleControllerScript = FindObjectOfType<MeepleControllerScript>();
            // Debug.Assert(meepleControllerScript != null, "meepleControllerScript is null in PlayerScript");
            foreach (var player in players)
            {
                for (var i = 0; i < GameRules.MeeplesPerPlayer; i++)
                {
                    // Should be a meeple factory method
                    // var meeple = meepleControllerScript.GetNewInstance();
                    // Debug.Assert(meeple != null, $"meeple {i} is null in PlayerScript");
                    // Debug.Assert(meeple.meeple != null, $"meeple.meeple {i} is null in PlayerScript");
                    //
                    // meeple.player = player;

                    var meepleData = new[] { player.id }.Cast<object>().ToArray();
                    var meeple = PhotonNetwork.Instantiate(meepleControllerScript.prefab.name, meepleControllerScript.parent.transform.position,
                        Quaternion.identity, meepleControllerScript.meepleGroup, meepleData);
                    
                    meeples.Add(meeple.GetComponent<Meeple>());
                }
            }

            Debug.Assert(meeples.Count == players.Count * GameRules.MeeplesPerPlayer, 
                $"There should be {players.Count * GameRules.MeeplesPerPlayer} meeples to start the game of Carcassonne. Found {meeples.Count}.");
            Debug.Assert(FindObjectsOfType<Meeple>().Length == players.Count * GameRules.MeeplesPerPlayer, 
                $"There should be {players.Count * GameRules.MeeplesPerPlayer} meeple game objects to start the game of Carcassonne. Found {FindObjectsOfType<Meeple>().Length}.");

            return meeples;
        }


        private Stack<Tile> CreateDeck()
        {
            // var tileDistribution = Tile.GetIDDistribution();
            var tileIDList = Tile.GetIDDistribution().SelectMany(pair => Enumerable.Repeat(pair.Key, pair.Value))
                .Select(x => new { r = Random.value, id = x }).OrderBy(x => x.r).Select(x => x.id).ToList();

            Debug.Log($"Tile Order: {string.Join(", ", tileIDList)}.");

            // Tile Stack
            var tiles = new List<Tile>();
            
            // Starting tile
            tileIDList.Remove( tileControllerScript.startingTileID );
            var tileData = new[] { tileControllerScript.startingTileID, (int)Tile.TileSet.Base }.Cast<object>().ToArray();
            Debug.Log($"Sending tile data (id, set): ({tileData[0]}, {tileData[1]})");
            var tile = PhotonNetwork.Instantiate(tileControllerScript.tilePrefab.name, tileControllerScript.tileParent.position,
                Quaternion.identity, tileControllerScript.tileGroup, tileData);
            tiles.Add(tile.GetComponent<Tile>());
                
            foreach (var id in tileIDList)
            {
                tileData = new[] { id, (int)Tile.TileSet.Base }.Cast<object>().ToArray();
                Debug.Log($"Sending tile data (id, set): ({tileData[0]}, {tileData[1]})");
                tile = PhotonNetwork.Instantiate(tileControllerScript.tilePrefab.name, tileControllerScript.tileParent.position,
                    Quaternion.identity, tileControllerScript.tileGroup, tileData);
                tiles.Add(tile.GetComponent<Tile>());
            }

            var stack = new Stack<Tile>(Enumerable.Reverse(tiles));

            Debug.Assert(tiles.Count == 72, $"There should be 72 tiles to start the game of Carcassonne. Found {tiles.Count}.");
            Debug.Assert(stack.Peek().ID == 8, $"The first tile should be an 8. Found {stack.Peek().ID}.");
            
            Debug.Log($"Tile Order: {string.Join(", ", stack.Select(t=>t.ID))}.");

            return stack;
        }
        
        #endregion
    }
}