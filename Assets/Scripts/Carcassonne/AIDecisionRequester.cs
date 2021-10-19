using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Carcassonne.State;

/// <summary>
/// The AIDecisionRequester sets up the allowed number of actions for the AI and and requests a decision when needed.
/// </summary>
public class AIDecisionRequester : MonoBehaviour
{
    public AIPlayer ai;
    public int maxSteps; //Currently not used
    public int currentSteps = 0; //Currently not used
    public float reward = 0; //Used for displaying the reward in the Unity editor.
    private Phase startPhase;

    public void Awake()
    {
        //ai = GetComponent<AIPlayer>();
    }

    /// <summary>
    /// Checks if its the AI players turn. If so, it acts on its own or by requesting actions from the actual AI depending the game phase.
    /// </summary>
    void FixedUpdate()
    {
        if (ai == null | ai.gameState == null || ai.thisPlayer.getID() != ai.gameState.Players.Current.getID())
        {
            return;
        }

        if (ai.gameState.phase == Phase.NewTurn)
        {
            //Picks a new tile automatically
            ai.gc.PickupTileRPC();
        } else if (ai.gameState.phase == Phase.MeepleDown)
        {
            //Ends turn automatically and resets AI for next move.
            ai.gc.EndTurnRPC();
            ai.SetTileStartPosition();

        } else if (ai.gameState.phase == Phase.GameOver)
        {
            //Add reinforcement based on score here.
            ai.EndEpisode();
        }
        else
        {
            startPhase = ai.gameState.phase;
            ai.RequestDecision();
            Debug.Log("Decision made. Current state: " + ai.gameState.phase);
            if (ai.gameState.phase != startPhase)
            {
                Debug.Log("AI is done with this phase:" + ai.gameState.phase);
            }
        }
        currentSteps++;
        DisplayCurrentReward();
    }

    public void DisplayCurrentReward()
    {
        //ToDo: Add this as some form of GUI-display to show many agents seperate values?
        reward = ai.GetCumulativeReward();
    }
}
