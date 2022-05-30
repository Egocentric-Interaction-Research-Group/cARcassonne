using System;
using System.Collections.Generic;
using System.Linq;
using Carcassonne.AI;
using Carcassonne.AR.GamePieces;
using Carcassonne.Controllers;
using Carcassonne.Models;
using Carcassonne.Players;
using Carcassonne.State;
using MRTK.Tutorials.MultiUserCapabilities;
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
    public class GameControllerScript : MonoBehaviourPun, IGameControllerInterface
    {
        #region Controllers

        public GameState state => GetComponent<GameState>();
        public GameController gameController => GetComponent<GameController>();
        public MeepleController meepleController => GetComponent<MeepleController>();
        public TileController tileController => GetComponent<TileController>();
        public TurnController turnController => GetComponent<TurnController>();

        internal TileControllerScript tileControllerScript => GetComponent<TileControllerScript>();
        internal MeepleControllerScript meepleControllerScript => GetComponent<MeepleControllerScript>();

        #endregion
        
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
                
                var players = CreatePlayers();
                var meeples = CreateMeeples(players);
                var tiles = CreateDeck();
                

                var tileOrder = tiles.Select(t => t.GetComponent<ARTile>().photonView.ViewID).ToArray();
                photonView.RPC("NewGameGuest", RpcTarget.Others, tileOrder);

                gameController.NewGame(players, meeples, tiles);

                AssignScoreboards(players);
                PlaceStartingTile();

                Debug.Assert(players.Count > 0, "Oops, there are no players.");
                Debug.Assert(state.Players.Current != null,
                    "There was a problem getting the current Player (it was null).");
                Debug.Assert(state.Players.Current != null,
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
            players.Sort();
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
            
            AssignScoreboards(players);
            PlaceStartingTile();

            Debug.Assert(players.Count > 0, "Oops, there are no players.");
            Debug.Assert(state.Players.Current != null,
                "There was a problem getting the current Player (it was null).");
            Debug.Assert(state.Players.Current != null,
                "There was a problem getting the current PlayerScript (it was null).");
            Debug.Assert(tiles.Count > 0, "There was a problem getting the current Tiles (there are no tiles).");
            Debug.Assert(state.Tiles.Remaining == tileStack,
                $"The remaining tiles was not set correctly. It has a length of {state.Tiles.Remaining.Count}, but tiles has {tiles.Count}.");

            Debug.Log("Denna spelarese namn: " + PhotonNetwork.LocalPlayer.NickName);
        }

        private void AssignScoreboards(List<Player> players)
        {
            var scoreboards = FindObjectsOfType<PlayerScoreScript>().ToList();
            scoreboards.Sort((pss1, pss2) =>
                pss1.transform.GetSiblingIndex() - pss2.transform.GetSiblingIndex());

            var i = 0;
            foreach (var scoreboard in scoreboards)
            {
                if (i < players.Count)
                {
                    // Set player
                    var player = players[i];
                    scoreboard.player = player;
                    
                    // Connect to events
                    gameController.OnGameStart.AddListener(scoreboard.OnGameStart);
                    gameController.OnTurnEnd.AddListener(scoreboard.ChangeMaterial);
                    gameController.OnTurnStart.AddListener(scoreboard.UpdateCurrentPlayer);
                    gameController.OnGameOver.AddListener(scoreboard.ChangeMaterial);
                    gameController.OnScoreChanged.AddListener(scoreboard.UpdateScore);
                    
                    scoreboard.OnGameStart();
                    scoreboard.UpdateCurrentPlayer();
                }
                else
                {
                    scoreboard.gameObject.SetActive(false);
                }
                i++;
            }
        }

        #region Proton

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
        
        public void EndTurnRPC()
        {
            if (PhotonNetwork.LocalPlayer.NickName == (state.Players.Current.id + 1).ToString())
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

        #endregion

        #region Meeple Things To Change

        internal Vector2Int Direction;

        internal Vector3 SnapPosition;

        internal bool CanConfirm;

        [PunRPC]
        public void ConfirmPlacement()
        {
            if (state.phase == Phase.MeepleDrawn)
            {
                if (state.Meeples.Current != null)
                {
                    if (!CanConfirm)
                    {
                        meepleController.Discard();

                        //TODO test multiplayer on this!
                        state.phase = Phase.TileDown;
                    }
                }
            }
        }
        
        #endregion
        
        #region Redesigned Functions

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
        
        private List<Player> CreatePlayers()
        {
            List<Player> players = FindObjectsOfType<Player>().ToList();

            // var i = 0;
            // foreach (var player in players)
            // {
            //     player.GetComponent<ARPlayer>().photonView.RPC("SetPlayerID", RpcTarget.All, i);
            //     i++;
            // }
            
            players.Sort();
            
            return players;
        }

        private List<Meeple> CreateMeeples(List<Player> players)
        {
            var meeples = new List<Meeple>();
            foreach (var player in players)
            {
                Debug.Log($"Creating Meeples for player {player.id} ({player.GetComponent<PhotonView>().CreatorActorNr})");
                for (var i = 0; i < GameRules.MeeplesPerPlayer; i++)
                {
                    var meepleData = new[] { player.id }.Cast<object>().ToArray();
                    var meeple = PhotonNetwork.Instantiate(meepleControllerScript.prefab.name, meepleControllerScript.parent.transform.position,
                        Quaternion.identity, meepleControllerScript.meepleGroup, meepleData);
                    
                    meeples.Add(meeple.GetComponent<Meeple>());
                }
            }

            Debug.Assert(meeples.Count == players.Count * GameRules.MeeplesPerPlayer, 
                $"There should be {players.Count * GameRules.MeeplesPerPlayer} meeples to start the game of Carcassonne. Found {meeples.Count}.");

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