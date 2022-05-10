using System.Collections.Generic;
using Carcassonne.Models;
using Carcassonne.State;
using UnityEngine;

namespace Carcassonne.Controllers
{
    public interface IGameControllerInterface
    {

        /// <summary>
        /// Start a new game. This controller is responsible for instantiating the deck, the players, and the meeples.
        /// Then, it calls the low-level GameController's NewGame function
        /// </summary>
        public void NewGame();

    }
}