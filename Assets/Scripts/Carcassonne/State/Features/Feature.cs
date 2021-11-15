using System.Collections.Generic;

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
        
        public bool Completable { get; }
        
        public Dictionary<PlayerScript, int> Meeples { get; }


}
}