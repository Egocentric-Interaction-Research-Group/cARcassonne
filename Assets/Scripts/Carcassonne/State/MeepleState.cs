using System;
using System.Collections.Generic;
using System.Linq;
using Carcassonne.Models;
using Carcassonne.State.Features;
using JetBrains.Annotations;
using UnityEngine;

namespace Carcassonne.State
{
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
        /// Dictionary of meeple placement. Key is coordinate in Meeple coordinate system.
        /// </summary>
        public Dictionary<Vector2Int, Meeple> Placement = new Dictionary<Vector2Int, Meeple>();
        public IEnumerable<Meeple> InPlay => Placement.Values;
        
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
    
        /// <summary>
        /// All of the meeples (in play and remaining) for Player p.
        /// </summary>
        /// <param name="p"></param>
        /// <returns></returns>
        public List<Meeple> MeeplesForPlayer(Player p)
        {
            return (from meeple in All where meeple.player == p select meeple).ToList();
        }

        public IEnumerable<Meeple> ForPlayer(Player p)
        {
            return All.Where(m => m.player == p);
        }

        public IEnumerable<Meeple> RemainingForPlayer(Player p) => ForPlayer(p).Where(m => IsFree(m));
        
        public IEnumerable<Meeple> InFeature(CarcassonneGraph feature)
        {
            //FIXME: This won't find meeples placed on the middle of a feature, where there is no vertex.
            //Use Vertex.meeple to search instead.
            // var inFeature = Placement.Where(locationMeeple => feature.Locations.Contains(locationMeeple.Key));
            // var meeples = inFeature.Select(locationMeeple => locationMeeple.Value);
            var meeplesVertices = feature.Vertices.Where(v=> v.meeple != null);
            if (meeplesVertices.Any())
                return meeplesVertices.Select(v => v.meeple);
            
            return new List<Meeple>();
        }

        [CanBeNull]
        public Meeple MeepleAt(Vector2Int xy)
        {
            throw new NotImplementedException();
        }

        public bool IsFree(Meeple m)
        {
            return Remaining.Contains(m);
        }

    }
}