using System.Collections.Generic;
using Carcassonne.Models;

namespace Carcassonne.Interfaces
{
    public interface IGameControllerInterface
    {
        public void NewGame();
        public void EndTurn();
        public void Reset();

    }
}