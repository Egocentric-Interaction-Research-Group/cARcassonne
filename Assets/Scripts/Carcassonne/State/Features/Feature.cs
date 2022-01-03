using System.Collections.Generic;
using System.Linq;

namespace Carcassonne.State.Features
{
    public interface IFeature
    {
        public bool Complete { get; }
        
        public int Segments
        {
            get;
        }
        
        public int OpenSides
        {
            get;
        }
        
        // TODO Implement this. The intention is to do so, but it isn't a simple task and requires referencing the remaining tiles.
        // public bool Completable { get; }
        
        // public IEnumerable<MeepleScript> Meeples { get; }

        public int Points
        {
            get;
        }

    }
    
    public abstract class FeatureGraph : CarcassonneGraph, IFeature{
        public abstract bool Complete { get; }
        public abstract int Segments { get; }
        public abstract int OpenSides { get; }
        public abstract int Points { get; }
    }
    
}