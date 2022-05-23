using System.Collections.Generic;
using Carcassonne.AI;
using Carcassonne.Models;
using Carcassonne.State;
using Microsoft.MixedReality.Toolkit.UI;
using MRTK.Tutorials.MultiUserCapabilities;
using Photon.Pun;
using UnityEngine;

namespace Carcassonne.AR
{
    public class TurnController : MonoBehaviour
    {
        private GameState state => GetComponent<GameState>();

        public List<Interactable> buttons;
        public List<ObjectManipulator> manipulators;

        public void OnTurnStart()
        {
            if(IsLocalHumanTurn()){
                Debug.Log($"TurnController: Got local human turn. Enabling Buttons and Bell.");
                // Enable Buttons
                foreach (var button in buttons)
                {
                    button.enabled = true;
                }
                
                // Enable Bell
                foreach (var manipulator in manipulators)
                {
                    manipulator.enabled = true;
                }
                
                // Enable Tile Movement
                
            }
        }

        public void OnTurnEnd(){
        
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
            
            Debug.Log($"Found current user {aiUser.GetComponent<Player>().username} ({aiUser.GetComponent<Player>().id}), IsLocal: {PhotonNetwork.IsMasterClient}");
            
            if( PhotonNetwork.IsMasterClient )
                return true;

            return false;
        }

        public bool IsLocalHumanTurn()
        {
            var photonUser = state.Players.Current.GetComponent<PhotonUser>();
            if (photonUser == null)
                return false;
            
            Debug.Log($"Found current user {photonUser.GetComponent<Player>().username} ({photonUser.GetComponent<Player>().id}), IsLocal: {photonUser.IsLocal}");
            
            if( photonUser.IsLocal )
                return true;

            return false;
        }
    }
}