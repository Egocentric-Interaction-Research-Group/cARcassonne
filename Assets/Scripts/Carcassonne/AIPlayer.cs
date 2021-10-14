using Carcassonne;
using Carcassonne.State;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;

/// <summary>
/// The AI for the player. An AI user contains both a regular PlayerScript and this AI script to observe and take actions.
/// </summary>

public class AIPlayer :  Agent
{
    PlayerScript thisPlayer;
    /// <summary>
    /// Initial setup, called only once when the agent is enabled.
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        thisPlayer = GetComponent<PlayerScript>();
        //M�ste kanske skaffa mer info direkt h�r, t ex spelplan osv.
    }

    /// <summary>
    /// Perform actions based on a vector of numbers.
    /// </summary>
    /// <param name="actionBuffers">The struct of actions to take</param>
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        //Kolla om det �r AI's tur, annars returna direkt?

        //En if-else med olika fall beroende p� gamestate?
    }

    /// <summary>
    /// Read inputs from the keyboard and convert them to a list of actions.
    /// This is called only when the player wants to control the agent and has set
    /// Behavior Type to "Heuristic Only" in the Behavior Parameters inspector.
    /// </summary>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        //Denna kan sannolikt skippas helt nu, men kan underl�tta att ha en l�sning via denna n�r vi ska sammankoppla AI och spelare.
    }

    /// <summary>
    /// When a new episode begins, reset the agent and area
    /// </summary>
    public override void OnEpisodeBegin()
    {
        //Resetta spelet, har ej kollat hur l�tt/sv�rt detta �r.
    }

    /// <summary>
    /// Collect all non-Raycast observations
    /// </summary>
    /// <param name="sensor">The vector sensor to add observations to</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        //H�r l�gger vi till allt som kan observeras, t ex tile, meeples kvar, antal spelare(?) osv.
    }
}
