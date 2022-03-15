using System;
using Carcassonne.State;
using Photon.Pun;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Carcassonne.AR
{
    [RequireComponent(typeof(GameControllerScript))]
    public class KeyboardController : MonoBehaviourPun
    {
        private GameControllerScript _gameControllerScript;

        private void Start()
        {
            _gameControllerScript = GetComponent<GameControllerScript>();
        }

        private void Update()
        {
            var keyboard = Keyboard.current;
            if( keyboard != null && _gameControllerScript.photonView.IsMine)
            {
                if (keyboard.pKey.wasReleasedThisFrame) _gameControllerScript.EndTurnRPC();

                if (keyboard.tKey.wasReleasedThisFrame) {
                    _gameControllerScript.meepleController.Free(_gameControllerScript.state.Meeples.Current); //FIXME: Throws error when no meeple assigned!}

                    _gameControllerScript.state.phase = Phase.TileDown;
                }

                if (keyboard.bKey.wasReleasedThisFrame) _gameControllerScript.gameController.GameOver();

            }
        }
    }
}