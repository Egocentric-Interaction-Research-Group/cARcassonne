using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Carcassonne;
using Carcassonne.AI;
using Carcassonne.Controllers;
using Carcassonne.Models;
using Carcassonne.State;
using Carcassonne.State.Features;
using Unity.MLAgents;
using Unity.MLAgents.Policies;
using UnityEngine;

public enum RewardMode : int
{
    Winner=0,
    Score=1
}

/// <summary>
/// GameController handles all the game logic and the actual game loop
/// </summary>
public class AIGameController : MonoBehaviour, IGameControllerInterface
{
    
    public int minX, minZ, maxX, maxZ;

    // This reflects the value that a single-player score will be divided by to keep it roughly between 0 and 1. It is not truly the maximum Carcassonne score.
    public float MaxScore = 250f;
    public RewardMode Mode;
    
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

    public List<Player> m_players = new List<Player>();

    
    
    #region NewParams

    public GameController gameController;
    public TileController tileController;
    public MeepleController meepleController;
    
    public Tile tilePrefab;
    public Meeple meeplePrefab;

    public int startingTileID;

    #endregion

    private void Awake()
    {
        Academy.Instance.AutomaticSteppingEnabled = false;
        Academy.Instance.OnEnvironmentReset += Restart;
    }

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

        StartCoroutine(WaitForAcademyInitialization());
    }
    
    private IEnumerator WaitForAcademyInitialization(){
        yield return new WaitUntil((() => Academy.IsInitialized));
        
        Mode = (RewardMode)Academy.Instance.EnvironmentParameters.GetWithDefault("RewardMode", (float)RewardMode.Winner);
        
        Academy.Instance.EnvironmentStep();
    }

    public void Restart()
    {
        // To ensure things aren't still getting placed.
        foreach (var agent in GetComponentsInChildren<CarcassonneAgent>())
        {
            agent.StopAllCoroutines();
        }

        Debug.Log("Calling AIGameController.Restart");
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
        Debug.Assert(state.Players.Current != null, "There was a problem getting the current Playe (it was null).");
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
        // foreach (var kvp in state.Tiles.Placement)
        // {
        //     Debug.Log($"Turn End ({state.Tiles.Placement.Count - 1}): Tile {kvp.Value} at {kvp.Key}.");
        // }

        //WriteGraphToFile(state.Features.Graph);
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

        var maxScore = state.Players.All.Select(p => p.FinalScore).Max();
        var winners = state.Players.All.Where(p => p.FinalScore == maxScore);

        // var playersByScore = state.Players.All.OrderByDescending(p => p.FinalScore);
        // 
        // var winner = playersByScore.First(); //.GetComponent<CarcassonneAgent>().SetReward(1f);
        // Debug.Log($"Player {playersByScore.First().id} ({playersByScore.First().GetComponent<BehaviorParameters>().TeamId}) is first with a score of {playersByScore.First().FinalScore}");
        // 
        // foreach (var player in playersByScore.Where(p => p != playersByScore.First()))
        // {
        //     //player.GetComponent<CarcassonneAgent>().SetReward(-1f);
        //     Debug.Log($"Player {player.id} ({player.GetComponent<BehaviorParameters>().TeamId}) is last with a score of {player.FinalScore}");
        // }
        
        var coroutine = DelayedEndEpisode(winners.ToList());
        StartCoroutine(coroutine); 
    }

    /// <summary>
    /// Wait one frame before starting next turn to allow for end-of-turn computations.
    /// </summary>
    /// <returns></returns>
    private IEnumerator DelayedEndEpisode(List<Player> winners)
    {
        yield return 0; // Wait for a second

        foreach (var p in state.Players.All)
        {
            switch (Mode)
            {
                case RewardMode.Winner:
                    if (winners.Count == 1 && p == winners[0])
                    {
                        p.GetComponent<CarcassonneAgent>().SetReward(1f);
                        Debug.Log(
                            $"DelayedEnd (Winner Mode): Player {p.id} ({p.GetComponent<BehaviorParameters>().TeamId})" +
                            $" is first with a score of {p.FinalScore}");
                    }
                    else if (winners.Count > 1)
                    {
                        p.GetComponent<CarcassonneAgent>().SetReward(0f);
                        Debug.Log(
                            $"DelayedEnd (Winner Mode): Player {p.id} ({p.GetComponent<BehaviorParameters>().TeamId})" +
                            $" is tied for the win with a score of {p.FinalScore}");
                    }
                    else
                    {
                        p.GetComponent<CarcassonneAgent>().SetReward(-1f);
                        Debug.Log(
                            $"DelayedEnd (Winner Mode): Player {p.id} ({p.GetComponent<BehaviorParameters>().TeamId})" +
                            $" has lost with a score of {p.FinalScore}");
                    }
                    break;
                case RewardMode.Score:
                    p.GetComponent<CarcassonneAgent>().SetReward(p.FinalScore / MaxScore);
                    Debug.Log(
                        $"DelayedEnd (Score Mode): Player {p.id} ({p.GetComponent<BehaviorParameters>().TeamId}) is first with a score of {p.FinalScore}");
                    break;
            }
            
            p.GetComponent<CarcassonneAgent>().EndEpisode();
        }

        yield return 0;
        
        Restart();
    }
    
    public void WriteGraphToFile(BoardGraph g)
    {
        // string gv = g.ToString();
        //
        // File.WriteAllText($"Learning/{state.Timestamp.ToString("yyyyMMdd_HHmmss")}_{state.GameID.ToString()}_{turn}.gv", gv);
        var path = $"Learning/Games/{state.Timestamp.ToString("yyyyMMdd_HHmmss")}_{state.GameID.ToString()}";
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        g.GenerateGraphML($"{path}/{turn}.graphml");
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
        
        Debug.Assert(tiles.Count == 72, $"There should be 72 tiles to start the game of Carcassonne. Found {tiles.Count}.");

        return tiles;
    }

    private List<Player> CreatePlayers()
    {
        // if (m_players.Count == 0) // First new game, create players
        // {
        //     // Instantiate AI Players
        //     for (int i = 0; i < nPlayers; i++) // Creates all the players
        //     {
        //         GameObject Agent = Instantiate(aiPrefab, playerParent); // Initiate AI prefab 
        //         Player player = Agent.GetComponent<Player>();
        //         player.id = i;
        //         player.isAI = true;
        //         Agent.GetComponent<BehaviorParameters>().TeamId = i;
        //         
        //         gameController.OnTurnStart.AddListener(player.OnNewTurn);
        //
        //         m_players.Add(player);
        //     }
        // }
        // else // New game, same players
        // {
            foreach (var p in m_players)
            {
                p.score = 0;
                p.previousScore = 0;
                p.unscoredPoints = 0;
                p.previousUnscoredPoints = 0;
                p.potentialPoints = 0;
                p.previousPotentialPoints = 0;
            }
        //}

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
