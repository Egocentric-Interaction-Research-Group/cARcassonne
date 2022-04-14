using System.Collections.Generic;
using System.Linq;
using Carcassonne.Models;

namespace Carcassonne.State
{
    public class PlayerState
    {
        public IList<Player> All = new List<Player>();
        public Player Current => All[_currentIndex];
        public IEnumerable<Player> Others => All.Where(p => p != Current);

        private int _currentIndex;
        
        public PlayerState()
        {
            All.Clear();
            _currentIndex = -1;
        }

        public Player Next()
        {
            _currentIndex = (_currentIndex + 1) % All.Count;
            return Current;
        }
        
    }
}