using System;
using System.Collections.Generic;
using System.Linq;
using Carcassonne.Models;
using Carcassonne.State.Features;
using JetBrains.Annotations;
using UnityEngine;

namespace Carcassonne.State
{
    public struct PlacedMeeple
    {
        public PlacedMeeple(Meeple meeple, Vector2Int direction)
        {
            Meeple = meeple;
            Direction = direction;
        }

        public Meeple Meeple { get; }
        public Vector2Int Direction { get; }
    }
    
    /// <summary>
    /// MeepleState hold all of the information about the position, availability, and ownership of meeples.
    /// Player meeple list derive from this information store.
    /// </summary>
    public class MeepleState : IGamePieceState<Meeple>
    {
        public IEnumerable<Meeple> Remaining => All.Where(meeple => !InPlay.Contains(meeple)); 
        // public Stack<Meeple> Remaining { get; } = new Stack<Meeple>();

        /// <summary>
        /// The current Meeple being played.
        /// </summary>
        [CanBeNull] public Meeple Current { get; set; }

        public Meeple[,] Played { get; }
        public Vector2Int MatrixOrigin { get; }

        /// <summary>
        /// Dictionary of meeple placement. Key is coordinate in Tile coordinate system.
        /// </summary>
        public Dictionary<Vector2Int, Meeple> Placement = new Dictionary<Vector2Int, Meeple>();
        public IEnumerable<Meeple> InPlay => Placement.Select(p => p.Value);
        
        // public Dictionary<Vector2Int, Meeple> Placement => getSubTilePlacement();
        //
        // private Dictionary<Vector2Int, Meeple> getSubTilePlacement()
        // {
        //     Dictionary<Vector2Int, Meeple> subTilePlacement = new Dictionary<Vector2Int, Meeple>();
        //     
        //     foreach (var kvp in Placement)
        //     {
        //         subTilePlacement.Add(Coordinates.TileToSubTile(kvp.Key, kvp.Value.Direction), kvp.Value.Meeple);
        //     }
        //
        //     return subTilePlacement;
        // }

        /// <summary>
        /// The set of all Meeples in the game.
        /// </summary>
        public IList<Meeple> All;
        

        public MeepleState()
        {
            All = new List<Meeple>();
            Current = null;
        }

        public List<Meeple> MeeplesForPlayer(Player p)
        {
            return (from meeple in All where meeple.player == p select meeple).ToList();
        }

        public IEnumerable<Meeple> InFeature(CarcassonneGraph feature)
        {
            var inFeature = Placement.Where(locationMeeple => feature.Locations.Contains(locationMeeple.Key));
            var meeples = inFeature.Select(locationMeeple => locationMeeple.Value);

            return meeples;
        }

        [CanBeNull]
        public Meeple MeepleAt(Vector2Int xy)
        {
            throw new System.NotImplementedException();
        }

        public bool IsFree(Meeple m)
        {
            return Remaining.Contains(m);
        }

        public IEnumerable<Meeple> ForPlayer(Player p)
        {
            return All.Where(m => m.player == p);
        }

    }
}