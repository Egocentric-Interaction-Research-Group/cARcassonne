using System.Collections.Generic;
using Carcassonne.AI;
using Carcassonne.Models;
using Carcassonne.State;
using Carcassonne.State.Features;
using Microsoft.MixedReality.Toolkit.UI;
using MRTK.Tutorials.MultiUserCapabilities;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

namespace Carcassonne.AR
{
    public class TurnController : MonoBehaviour
    {
        private GameState state => GetComponent<GameState>();

        public List<Interactable> buttons;
        public List<ObjectManipulator> manipulators;

        public UnityEvent OnLocalTurnStart = new UnityEvent();
        public UnityEvent OnLocalTurnEnd = new UnityEvent();
        public UnityEvent OnLocalAITurnStart = new UnityEvent();
        public UnityEvent OnLocalAITurnEnd = new UnityEvent();
        public UnityEvent OnLocalHumanTurnStart = new UnityEvent();
        public UnityEvent OnLocalHumanTurnEnd = new UnityEvent();

        public void OnTurnStart()
        {
            if (IsLocalTurn())
            {
                OnLocalTurnStart.Invoke();
                if (IsLocalHumanTurn())
                {
                    Debug.Log($"TurnController: Got local human turn. Enabling Buttons and Bell.");
                    OnLocalHumanTurnStart.Invoke();

                    // Enable Buttons
                    foreach (var button in buttons)
                    {
                        button.enabled = true;
                        button.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
                    }

                    // Enable Bell
                    foreach (var manipulator in manipulators)
                    {
                        manipulator.enabled = true;
                        manipulator.GetComponent<PhotonView>().TransferOwnership(PhotonNetwork.LocalPlayer);
                    }

                    // Enable Tile Movement
                }
                else if (IsLocalAITurn())
                {
                    OnLocalAITurnStart.Invoke();
                }
            }
        }

        public void OnTurnEnd()
        {
            Debug.Log($"TurnController: Ending turn. Disabling Buttons and Bell.");
            // Disable buttons
            foreach (var button in buttons)
            {
                button.enabled = false;
            }

            // Disable bell 
            foreach (var manipulator in manipulators)
            {
                manipulator.enabled = false;
            }

            if (IsLocalTurn())
            {
                if (IsLocalHumanTurn())
                {
                    OnLocalHumanTurnEnd.Invoke();
                }

                else if (IsLocalAITurn())
                {
                    OnLocalAITurnEnd.Invoke();
                }

                OnLocalTurnEnd.Invoke();
            }
        }

        /// <summary>
        /// Is AI turn or human turn being executed on the local machine.
        /// </summary>
        /// <returns></returns>
        public bool IsLocalTurn()
        {
            return IsLocalHumanTurn() || IsLocalAITurn();
        }

        public bool IsLocalAITurn()
        {
            var aiUser = state.Players.Current.GetComponent<CarcassonneAgent>();
            if (aiUser == null)
                return false;

            Debug.Log(
                $"Found current user {aiUser.GetComponent<Player>().username} ({aiUser.GetComponent<Player>().id}), IsLocal: {PhotonNetwork.IsMasterClient}");

            if (PhotonNetwork.IsMasterClient)
                return true;

            return false;
        }

        public bool IsLocalHumanTurn()
        {
            var photonUser = state.Players.Current.GetComponent<PhotonUser>();
            if (photonUser == null)
                return false;

            // Debug.Log(
            //     $"Found current user {photonUser.GetComponent<Player>().username} ({photonUser.GetComponent<Player>().id}), IsLocal: {photonUser.IsLocal}");

            if (photonUser.IsLocal)
                return true;

            return false;
        }
    }
}