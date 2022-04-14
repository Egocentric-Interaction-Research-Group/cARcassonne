using System;
using System.Collections;
using Carcassonne;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Carcassonne.AI;
using Carcassonne.Controllers;
using Carcassonne.Models;
using Carcassonne.State;
using Carcassonne.State.Features;
using Carcassonne.Utilities;
using Unity.MLAgents.Policies;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// GameController handles all the game logic and the actual game loop
/// </summary>
public class AIGameController : MonoBehaviour//, IGameControllerInterface
{
    private bool[,] visited;
    
    public int minX, minZ, maxX, maxZ;

    public int nPlayers;

    public int turn => state.Tiles.Placement.Count - 1;

    public GameState state;

    private CarcassonneVisualization shader;

    public GameObject aiPrefab;
    public GameObject visualizationBoard;

    internal Player currentPlayer => state.Players.Current;

    public Transform playerParent;
    public Transform tileParent;
    public Transform meepleParent;

    private List<Player> m_players = new List<Player>();

    #region NewParams

    public GameController gameController;
    public TileController tileController;
    public MeepleController meepleController;
    
    public Tile tilePrefab;
    public Meeple meeplePrefab;

    public int startingTileID;

    #endregion

    /// <summary>
    /// MonoBehavior method that will create the necessary data before the game starts
    /// Some objects such as anything that is related to the ML Agent has to be set at this point so that it can be used
    /// at the first FixedUpdate call
    /// </summary>
    private void Start()
    {
        // Initialize the shader visualization in order to set the max array
        // size for upcoming shader data.
        shader = visualizationBoard.GetComponent<CarcassonneVisualization>();
        shader.Init();
        
        NewGame();
    }

    public void Restart()
    {
        foreach (var tile in GetComponentsInChildren<Tile>())
        {
            Destroy(tile.gameObject);
        }
            
        foreach (var meeple in GetComponentsInChildren<Meeple>())
        {
            Destroy(meeple.gameObject);
        }
        
        GetComponent<GameLog>().Reset();

        NewGame();
    }

    /// <summary>
    /// Setup for a new game session
    /// </summary>
    public void NewGame()
    {
        // List of Tiles
        var tiles = CreateDeck();
        
        // List of Players
        var players = CreatePlayers();
        
        // List of Meeples
        var meeples = CreateMeeples(players);
        
        gameController.NewGame(players, meeples, tiles);
        
        Debug.Assert(players.Count > 0, "Oops, there are no players.");
        Debug.Assert(state.Players.Current != null, "There was a problem getting the current Player (it was null).");
        Debug.Assert(currentPlayer != null, "There was a problem getting the current PlayerScript (it was null).");
        Debug.Assert(tiles.Count > 0, "There was a problem getting the current Tiles (there are no tiles).");
        Debug.Assert(state.Tiles.Remaining == tiles, $"The remaining tiles was not set correctly. It has a length of {state.Tiles.Remaining.Count}, but tiles has {tiles.Count}.");
    }

    public void OnNewGame()
    {
        tileController.PlaceFirst();
    }

    public void OnPlace(Tile tile, Vector2Int cell)
    {
        UpdateAIBoundary(cell.x, cell.y);
    }

    public void OnEndTurn()
    {
        foreach (var kvp in state.Tiles.Placement)
        {
            Debug.Log($"Turn End ({state.Tiles.Placement.Count - 1}): Tile {kvp.Value} at {kvp.Key}.");
        }

        WriteGraphToFile(state.Features.Graph);
    }

    /// <summary>
    /// The game is over and final points are calculated. Phase is moved to GameOver
    /// </summary>
    public void OnGameOver()
    {
        //TODO THIS IS BEING CALLED MULTIPLE TIMES! WHAT GIVES?
        
        foreach (Player p in state.Players.All)
        {
            Debug.Log("Player " + p.id + " achieved " + p.score + " points!");
        }

        WriteGraphToFile(state.Features.Graph);

        var playersByScore = state.Players.All.OrderByDescending(p => p.score);
        
        playersByScore.First().GetComponent<CarcassonneAgent>().SetReward(1f);
        // playersByScore.First().GetComponent<CarcassonneAgent>().EndEpisode();
        
        foreach (var player in playersByScore.Where(p => p != playersByScore.First()))
        {
            player.GetComponent<CarcassonneAgent>().SetReward(-1f);
            // player.GetComponent<CarcassonneAgent>().EndEpisode();
        }
        
        var coroutine = DelayedEndEpisode();
        StartCoroutine(coroutine); 
    }

    /// <summary>
    /// Wait one frame before starting next turn to allow for end-of-turn computations.
    /// </summary>
    /// <returns></returns>
    private IEnumerator DelayedEndEpisode()
    {
        yield return new WaitForSeconds(1f); // Wait for a second

        foreach (var p in state.Players.All)
        {
            p.GetComponent<CarcassonneAgent>().EndEpisode();
        }
    }
    
    public void WriteGraphToFile(BoardGraph g)
    {
        // string gv = g.ToString();
        //
        // File.WriteAllText($"Learning/{state.Timestamp.ToString("yyyyMMdd_HHmmss")}_{state.GameID.ToString()}_{turn}.gv", gv);
        
        g.GenerateGraphML($"Learning/{state.Timestamp.ToString("yyyyMMdd_HHmmss")}_{state.GameID.ToString()}_{turn}.graphml");
    }

    /// <summary>
    /// Update the boundaries that the AI can place tiles within. Variables are based on the
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
        
        Debug.Log($"New AI Boundaries: ({minX}, {minZ}) -- ({maxX}, {maxZ})");
    }

    #region New
    
    private Stack<Tile> CreateDeck()
    {
        var tileDistribution = Tile.GetIDDistribution();
        var tileList = new List<Tile>();
        foreach (var kvp in tileDistribution)
        {
            var id = kvp.Key;
            var count = kvp.Value;

            for (int i = 0; i < count; i++)
            {
                var tile = Instantiate(tilePrefab.GetComponent<Tile>(), tileParent);
                tile.set = Tile.TileSet.Base;
                tile.ID = id; // This *should* work because Start is not called immediately.
                tileList.Add(tile);
            }
        }

        var startingTile = tileList.First(t => t.ID == startingTileID);
        tileList.Remove(startingTile);
        Debug.Assert(tileList.Count(t=> t.ID == startingTile.ID) == (tileDistribution[startingTile.ID] - 1), 
            $"There should be 1 less tile #{startingTile.ID} in the stack." +
            $"Found {tileList.Count(t=> t.ID == startingTile.ID)}, expected {tileDistribution[startingTile.ID] - 1}.");
        
        // Shuffle and stack
        var tiles = new Stack<Tile>(tileList.Where(x => x.set == Tile.TileSet.Base). // This should not be hardcoded
            Select(x => new { r = Random.value, Tile = x }).
            OrderBy(x => x.r).
            Select(x=> x.Tile));
        
        // Put the first one on top
        tiles.Push(startingTile);
        
        Debug.Assert(tiles.Count == 71, $"There should be 71 tiles to start the game of Carcassonne. Found {tiles.Count}.");

        return tiles;
    }

    private List<Player> CreatePlayers()
    {
        if (m_players.Count == 0) // First new game, create players
        {
            // Instantiate AI Players
            for (int i = 0; i < nPlayers; i++) // Creates all the players
            {
                GameObject Agent = Instantiate(aiPrefab, playerParent); // Initiate AI prefab 
                Player player = Agent.GetComponent<Player>();
                player.id = i;
                player.isAI = true;
                Agent.GetComponent<BehaviorParameters>().TeamId = i;
                
                gameController.OnTurnStart.AddListener(player.OnNewTurn);

                m_players.Add(player);
            }
        }
        else // New game, same players
        {
            foreach (var p in m_players)
            {
                p.score = 0;
                p.previousScore = 0;
                p.unscoredPoints = 0;
                p.previousUnscoredPoints = 0;
                p.potentialPoints = 0;
                p.previousPotentialPoints = 0;
            }
        }

        return m_players;
    }

    private List<Meeple> CreateMeeples(List<Player> players)
    {
        var meeples = new List<Meeple>();
        foreach (var player in players)
        {
            for (var i = 0; i < GameRules.MeeplesPerPlayer; i++)
            {
                // Should be a meeple factory method
                var meeple = Instantiate(meeplePrefab, meepleParent).GetComponent<Meeple>();
                Debug.Assert(meeple != null, $"meeple {i} is null in GameController");
                
                meeple.player = player;
                meeples.Add(meeple);
            }
        }

        return meeples;
    }

    #endregion
}
