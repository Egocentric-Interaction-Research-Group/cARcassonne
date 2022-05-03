using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Carcassonne.Models;
using Carcassonne.State;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public struct TurnPoints
{
    public int scoredPoints;
    public int unscoredPoints;
    public int potentialPoints;
        
    public static TurnPoints operator -(TurnPoints a, TurnPoints b) => new TurnPoints
    {
        scoredPoints = a.scoredPoints - b.scoredPoints,
        unscoredPoints = a.unscoredPoints - b.unscoredPoints,
        potentialPoints = a.potentialPoints - b.potentialPoints
    };

    public string ToCSV()
    {
        return $"{scoredPoints}, {unscoredPoints}, {potentialPoints}";
    }

    public override string ToString()
    {
        return ToCSV();
    }
}
    
/// <summary>
/// Represents the entire moves of a single player during one round of play. 
/// </summary>
/// <remarks>Stores the player, their tile placement, and their meeple placement (if any) for a given turn
/// </remarks>
[Serializable]
public struct Turn
{
    public Player player;
    public Tile tile;
    public Vector2Int cell;
    public Vector2Int? meeplePlacement; // The position in Meeple space
    public int meeplesRemaining;
    public Dictionary<Player, TurnPoints> points;
    public Dictionary<Player, TurnPoints> pointDifference;
        
    public bool MeeplePlayed => meeplePlacement != null;

    public override string ToString()
    {
        var points = String.Join(", ", pointDifference.Select(p => p.Value.ToCSV()));
        return $"{player.id}, {tile.ID}, {tile.Rotations}, {cell.x}, {cell.y}, {meeplePlacement?.x}, {meeplePlacement?.y}, {meeplesRemaining}, {points}";
    }
}
    
/// <summary>
/// A log of the <see cref="Turn">Turns</see> for a game.
/// </summary>
/// <remarks>Turns are stored in a <see cref="Stack{T}"/> which is built as the game progresses.</remarks>
[Serializable]
// [CreateAssetMenu(fileName = "GameLog", menuName = "GameLog")]
public class GameLog : MonoBehaviour
{
    public Stack<Turn> Turns = new Stack<Turn>();
    public GameState state;
    public GridMapper grid => GetComponent<GridMapper>();

    public UnityEvent<Turn> TurnLogged = new UnityEvent<Turn>();

    public static string[] CSV_HEADER =
    {
        ", , Tile, , , ,Meeple, , , Player 1, , , Player 2, , , ", 
        "Turn, Player, ID, Rot, X, Y, X, Y, Remain, Score, Unscored, Potential, Score, Unscored, Potential"
    };

    private string path => "Learning";
    private string filepath => $"{path}/{state.Timestamp.ToString("yyyyMMdd_HHmmss")}_{state.GameID.ToString()}.csv";

    public void LogTurn()
    {
        var points = new Dictionary<Player, TurnPoints>();
        var pointDifference = new Dictionary<Player, TurnPoints>();
        foreach (var player in state.Players.All)
        {
            var current = new TurnPoints
            {
                scoredPoints = player.score, unscoredPoints = player.unscoredPoints,
                potentialPoints = player.potentialPoints
            };
            points.Add(player, current);
                
            TurnPoints diff;
            if (Turns.Count == 0)
            {
                diff = new TurnPoints() { scoredPoints = 0, unscoredPoints = 0, potentialPoints = 0 };
            }
            else{
                diff = current - Turns.Peek().points[player];
            }
            pointDifference.Add(player, diff);
        }
            
        var position = state.Tiles.Placement.Single(kvp => kvp.Value == state.Tiles.Current).Key;
        var t = new Turn
        {
            player = state.Players.Current,
            tile = state.Tiles.Current,
            cell = position,
            meeplePlacement = state.Meeples.Placement.Keys.SingleOrDefault(mCell => grid.MeepleToTile(mCell) == position),
            meeplesRemaining = state.Meeples.RemainingForPlayer(state.Players.Current).Count(),
            points = points,
            pointDifference = pointDifference
        };

        Turns.Push(t);
        
        TurnLogged.Invoke(t);
            
        File.AppendAllText(filepath, Environment.NewLine + $"{Turns.Count}, " + t);
    }
        
    private void OnEnable()
    {
        Turns.Clear();
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        File.AppendAllLines(filepath, CSV_HEADER);
    }

    public void Reset()
    {
        Turns.Clear();
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }
        File.AppendAllLines(filepath, CSV_HEADER);
    }

    public void OnGameOver()
    {
        var points = "";
        foreach (var player in state.Players.All)
        {
            points += $"{player.score}, {player.unscoredPoints}, {player.potentialPoints},";
        }
        File.AppendAllText(filepath, $", , , , , , , , , {points}");
    }
}