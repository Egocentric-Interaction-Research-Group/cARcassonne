using Carcassonne.State;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System;
using Assets.Scripts.Carcassonne.AI;

/// <summary>
/// The AI for the player. An AI user contains both a regular PlayerScript and this AI script to observe and take actions.
/// </summary>

public class AIPlayer :  Agent
{
    //Observations from real game (use getter properties, don't call these directly)
    public int meeplesLeft;
    public int boardGridSize;
    public Phase phase;
    public int id;

    //AI Specific
    public AIWrapper wrapper;
    private const int maxBranchSize = 6;
    public int x =85, z=85 , y=1, rot=0;
    public float meepleX, meepleZ;

    //Monitoring
    public float realX, realY, realZ, realRot;
    private string placement = "";

    public Phase Phase
    {
        get
        {
            return wrapper.GetGamePhase();
        }
    }

    public int MeeplesLeft
    {
        get
        {
            return wrapper.GetMeeplesLeft();
        }
    }

    public int BoardGridSize
    {
        get
        {
            return wrapper.GetBoardSize();
        }
    }

    public int Id
    {
        get
        {
            return wrapper.GetCurrentTileId();
        }
    }

    /// <summary>
    /// Initial setup which gets the scripts needed to AI calls and observations, called only once when the agent is enabled.
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        wrapper = new AIWrapper();
    }


    /// <summary>
    /// Perform actions based on a vector of numbers. Which actions are made depend on the current game phase.
    /// </summary>
    /// <param name="actionBuffers">The struct of actions to take</param>
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        switch (Phase)
        {
            case Phase.TileDrawn:
                TileDrawnAction(actionBuffers);
                break;
            case Phase.TileDown:
                if (actionBuffers.DiscreteActions[0] == 0f)
                {
                    Debug.LogError("Meeple drawn");
                    wrapper.DrawMeeple(); //Take meeple
                }
                else
                {
                    Debug.LogError("No meeple drawn, ending turn");
                    wrapper.EndTurn(); //End turn without taking meeple
                }
                break;
            case Phase.MeepleDrawn:
                MeepleDrawnAction(actionBuffers);
                break;
        }
    }

    private void TileDrawnAction(ActionBuffers actionBuffers)
    {
        AddReward(-0.001f); //Each call to this method comes with a very minor penalty to promote performing quick actions.
        if (actionBuffers.DiscreteActions[0] == 0f)
        {
            Debug.Log("Up");
            z += 1; //Up
        }
        else if (actionBuffers.DiscreteActions[0] == 1f)
        {
            Debug.Log("Down");
            z -= 1; //Down
        }
        else if (actionBuffers.DiscreteActions[0] == 2f)
        {
            Debug.Log("Left");
            x -= 1; //Left
        }
        else if (actionBuffers.DiscreteActions[0] == 3f)
        {
            Debug.Log("Right");
            x += 1; //Right
        }
        else if (actionBuffers.DiscreteActions[0] == 4f)
        {
            //Each step in rot represents a 90 degree rotation of the tile.
            rot++;
            if (rot == 4)
            {
                rot = 0;

                //Punishment for rotating more than needed, i.e. returning back to default rotation state.
                //AddReward(-0.01f); 
            }
        }
        else if (actionBuffers.DiscreteActions[0] == 5f)
        {
            //Rotates the tile the amount of times AI has chosen (0-3).
            for (int i = 0; i < rot; i++)
            {
                wrapper.RotateTile();
            }

            //Values are loaded into GameController that are used in the ConfirmPlacementRPC call.
            wrapper.PlaceTile(x, z);
            
            if (Phase == Phase.TileDown) //If the placement was successful, the phase changes to TileDown.
            {
                AddReward(1f);

                //Line below is only used for debugging. Ignore it.
                //Debug.LogError("Tile placed: " + wrapper.GetCurrentTile().transform.localPosition.x + ", Y: " + wrapper.GetCurrentTile().transform.localPosition.y + ", Z: " + wrapper.GetCurrentTile().transform.localPosition.z + ", Rotation: " + wrapper.GetCurrentTile().transform.rotation.eulerAngles.y);
            }
        }


        //After choice checks.
        if (x < 0 || x >= BoardGridSize || z < 0 || z >= BoardGridSize)
        {
            //Outside table area, reset values and add significant punishment.
            ResetAttributes();
            AddReward(-1f);
            Debug.LogError("AI outside table area. Retteing position.");
        } 

        //These are only used to monitor the ai (shown on the AI gameobject in the scene while it plays). Ignore them.

        /*realX = wrapper.GetCurrentTile().transform.localPosition.x;
        realY = wrapper.GetCurrentTile().transform.localPosition.y;
        realZ = wrapper.GetCurrentTile().transform.localPosition.z;
        realRot = wrapper.GetCurrentTile().transform.rotation.eulerAngles.y;*/
    }

    /// <summary>
    /// Places the meeple on one of the 5 places available on the tile (Uses the tile to find the positions).
    /// </summary>
    /// <param name="actionBuffers"></param>
    private void MeepleDrawnAction(ActionBuffers actionBuffers)
    {
        AddReward(-0.1f); //Each call (each change of position) gets a negative reward to avoid getting stuck in this stage.
        if (actionBuffers.DiscreteActions[0] == 0f)
        {
            placement = "North";
            meepleX = 0.000f;
            meepleZ = 0.011f;
        }
        else if (actionBuffers.DiscreteActions[0] == 1f)
        {
            placement = "South";
            meepleX = 0.000f;
            meepleZ = -0.011f;
        }
        else if (actionBuffers.DiscreteActions[0] == 2f)
        {
            placement = "West";
            meepleX = -0.011f;
            meepleZ = 0.000f;
        }
        else if (actionBuffers.DiscreteActions[0] == 3f)
        {
            placement = "East";
            meepleX = 0.011f;
            meepleZ = 0.000f;
        }
        else if (actionBuffers.DiscreteActions[0] == 4f)
        {
            placement = "Center";
            meepleX = 0.000f;
            meepleZ = 0.000f;
        }
        else if (actionBuffers.DiscreteActions[0] == 5f)
        {
            if (!String.IsNullOrEmpty(placement)) //Checks so that a choice has been made since meeple was drawn.
            {
                Debug.LogError("Trying to place meeple");
                wrapper.PlaceMeeple(meepleX, meepleZ);  //Either confirms and places the meeple if possible, or returns meeple and goes back to phase TileDown.
            }
            else
            {
                Debug.LogError("Tried to place meeple, placement is empty");
            }
           

            if (Phase == Phase.MeepleDown) //If meeple is placed.
            {
                AddReward(1f); //Rewards successfully placing a meeple

                //Below are printouts used for debugging. Ignore them.

                /*Debug.LogError("Meeple placed successfully in place " + placement + ". Its actual position is direction '" 
                    + wrapper.GetCurrentMeeple().direction + "' and geography '" + wrapper.GetCurrentMeeple().geography + "', ending turn");
                Debug.LogError("X: " + wrapper.GetCurrentMeeple().gameObject.transform.localPosition.x + ",Y: " + wrapper.GetCurrentMeeple().gameObject.transform.localPosition.y + ", Z: " + wrapper.GetCurrentMeeple().gameObject.transform.localPosition.z);
                placement = "";*/
            }
            else if (Phase == Phase.TileDown) //If meeple gets returned.
            {
                AddReward(-1f); //Punishes returning a meeple & going back a phase (note: no punishment for never drawing a meeple).
                
                //Below are printouts used for debugging. Ignore them.
                
                /*Debug.LogError("Tried to place meeple in inaccessible place. Reset meeple and return to TileDown phase. X: "
                    + wrapper.GetCurrentMeeple().gameObject.transform.localPosition.x + ", Z: " + wrapper.GetCurrentMeeple().gameObject.transform.localPosition.z); ;
                placement = "";*/
            }
            else //Workaround for a bug where you can draw an unconfirmable meeple and never be able to change phase.
            {
                wrapper.FreeCurrentMeeple();
            }
        }
    }


    /// <summary>
    /// Read inputs from the keyboard and convert them to a list of actions.
    /// This is called only when the player wants to control the agent and has set
    /// Behavior Type to "Heuristic Only" in the Behavior Parameters inspector.
    /// </summary>
    public override void Heuristic(in ActionBuffers actionsOut)
    {
        //Not implemented.
    }


    /// <summary>
    /// When a new episode begins, reset the agent and area
    /// </summary>
    public override void OnEpisodeBegin()
    {
        //This occurs every X steps (Max Steps). It only serves to reset tile position if AI is stuck, and for AI to process current learning
        Debug.LogError("New Episode");
        ResetAttributes();
    }


    /// <summary>
    /// Collect all non-Raycast observations
    /// </summary>
    /// <param name="sensor">The vector sensor to add observations to</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        //ToDo: All these should be Normalized for optimal learning. Read up on Normalization

        
        sensor.AddObservation((int)Phase); //This one might need to be changed, read up on One-Hot observation
        sensor.AddObservation(MeeplesLeft);
        sensor.AddObservation(Id);
        sensor.AddObservation(rot);
        sensor.AddObservation(x);
        sensor.AddObservation(z);
        sensor.AddObservation(meepleX);
        sensor.AddObservation(meepleZ);


        
        //Code below handles the input of the entire board, which is really ineffective in its current implemenetation.

        //This could be handled with BufferSensor component. Read up on Variable Length Observations.
        //Tiles = Entities. The attributes added = floats (observation size)
        //In the code below we would add the BufferSensorComponent.AppendObservation() instead.
        //Add a float array of size 'Observation Size' as argument, with normalized values.
        
        /*foreach (TileScript tile in gameState.Tiles.Played)
        {
            if (tile != null)
            {
                sensor.AddObservation(tile.id);
                sensor.AddObservation(tile.rotation);
                sensor.AddObservation(tile.transform.position.x);
                sensor.AddObservation(tile.transform.position.z);
            }
        }*/

        //Possibly relevant to add each player's scores for multiplayer as well, so AI knows if it is losing or not.
    }

    /// <summary>
    /// Masks certain inputs so they cannot be used. Amount of viable inputs depends on the game phase.
    /// </summary>
    /// <param name="actionMask">The actions (related to ActionBuffer actioons) to disable or enable</param>
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        int allowedActions = 0;
        switch (Phase)
        {
            case Phase.TileDrawn:
                //AI can choose to step one tile place in either of the 4 directions (-X, X, -Z, Z), rotate 90 degrees, or confirm place.
                allowedActions = 6;
                break;
            case Phase.TileDown:
                //AI can choose to take or not take a meeple.
                allowedActions = 2;
                break;
            case Phase.MeepleDrawn:
                //AI can choose to place a drawn meeple in 5 different places (N, S, W, E, C) or confirm/deny current placement.
                allowedActions = 6;
                break;
        }

        //Disables all actions of branch 0, index i (on that branch) for any i larger than the allowed actions.
        for (int i = allowedActions; i < maxBranchSize; i++)
        {
            actionMask.SetActionEnabled(0, i, false); //The rest are enabled by default, as it resets to all enabled after a decision.
        }
        Debug.Log("Allowed actions: " + allowedActions + "/" + maxBranchSize);
    }

    /// <summary>
    /// Resets tile position and placement (meeple position) to base position before next action.
    /// </summary>
    internal void ResetAttributes()
    {
        x = 85;
        z = 85;
        y = 1;
        rot = 0;
        placement = "";
    }
}
