using UnityEngine;
using Carcassonne.State;

/// <summary>
/// The AIDecisionRequester sets up the allowed number of actions for the AI and and requests a decision when needed.
/// </summary>
public class AIDecisionRequester : MonoBehaviour
{
    public AIPlayer ai;
    public float reward = 0; //Used for displaying the reward in the Unity editor.

    /// <summary>
    /// Acts on its own or repeatedly requests actions from the actual AI depending the game phase and state.
    /// </summary>
    void FixedUpdate()
    {
        if (ai == null || !ai.wrapper.IsAITurn())
        {
            return;
        }

        switch (ai.wrapper.GetGamePhase())
        {
            case Phase.NewTurn: // Picks a new tile automatically
                ai.ResetAttributes();
                ai.wrapper.PickUpTile();
                break;
            case Phase.MeepleDown: //Ends turn automatically and resets AI for next move.
                ai.wrapper.EndTurn();
                break;
            case Phase.GameOver: //ToDo: Add reinforcement based on score
                ai.EndEpisode();
                break;
            default: //Calls for one AI action repeatedly with each FixedUpdate until the phase changes.
                ai.RequestDecision();
                break;
        }

        //ToDo: Add this info to some form of GUI-display instead to visualize many separate agents values concurrently.
        reward = ai.GetCumulativeReward();
    }
}