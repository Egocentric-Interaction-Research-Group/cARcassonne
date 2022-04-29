using System.Collections;
using Carcassonne.Controllers;
using Carcassonne.Models;
using Unity.MLAgents;
using UnityEngine;

namespace Carcassonne.AI
{
    /// <summary>
    /// 
    /// </summary>
    public class EventDecisionRequester : MonoBehaviour
    {
        public CarcassonneAgent ai;
        public bool decisionRequested = false;

        private void Awake()
        {
            Debug.Log("Adding listeners.");
            GetComponentInParent<GameController>().OnTurnStart.AddListener(NewTurn);
            GetComponentInParent<TileController>().OnDraw.AddListener(RequestDecision);
            GetComponentInParent<TileController>().OnInvalidPlace.AddListener(RequestDecision);
        }

        public void NewTurn()
        {
            StartCoroutine(WaitForWrapperStart());
        }

        private IEnumerator WaitForWrapperStart()
        {
            if (ai.wrapper.state == null)
            {
                Debug.Log("Waiting for ai.wrapper.state != null.");
                yield return new WaitUntil(() => ai.wrapper.state != null);
            }

            if (!ai.wrapper.IsAITurn())
            {
                Debug.Log($"EDR {ai.wrapper.player.id} found new turn. Ignored.");
                yield return 0;
            }
            else
            {

                Debug.Log($"EDR {ai.wrapper.player.id} found new turn.");

                ai.ResetAttributes();
                ai.wrapper.PickUpTile();

                yield return 0;
            }
        }

        public void RequestDecision(Tile t, Vector2Int v)
        {
            Debug.Log($"EDR {ai.wrapper.player.id} invalid place decision requested.");
            RequestDecision();
        }
        
        public void RequestDecision(Tile t)
        {
            Debug.Log($"EDR {ai.wrapper.player.id} draw decision requested.");
            RequestDecision();
        }
        
        public void RequestDecision()
        {
            if (ai == null || !ai.wrapper.IsAITurn())
            {
                return;
            }
            
            Debug.Log($"EDR {ai.wrapper.player.id} decision requested.");
            ai.RequestDecision();

            decisionRequested = true;
        }

        public void Update()
        {
            if (decisionRequested)
            {
                Academy.Instance.EnvironmentStep();

                decisionRequested = false;
            }
        }

        // /// <summary>
        // /// Acts on its own or repeatedly requests actions from the actual AI depending the game phase and state.
        // /// </summary>
        // void FixedUpdate()
        // {
        //     if (ai == null || !ai.wrapper.IsAITurn())
        //     {
        //         return;
        //     }
        //     switch (ai.wrapper.GetGamePhase())
        //     {
        //         case Phase.NewTurn: // Picks a new tile automatically
        //             ai.ResetAttributes();
        //             ai.wrapper.PickUpTile();
        //             break;
        //         // case Phase.MeepleDown: //Ends turn automatically and resets AI for next move.
        //         //     ai.wrapper.EndTurn();
        //         //     break;
        //         case Phase.GameOver: //ToDo: Add reinforcement based on score
        //             // ai.EndEpisode();
        //             break;
        //         default: //Calls for one AI action repeatedly with each FixedUpdate until the phase changes.
        //             if (ai.wrapper.state.Tiles.Current != null)
        //             {
        //                 ai.RequestDecision();
        //             }
        //
        //             break;
        //     }
        // }
    }
}
