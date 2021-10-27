using Carcassonne;
using Carcassonne.State;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System;

/// <summary>
/// The AI for the player. An AI user contains both a regular PlayerScript and this AI script to observe and take actions.
/// </summary>

public class AIPlayer :  Agent
{
    public PlayerScript thisPlayer;
    public GameState gameState; //Contains TileState, MeepleState, FeatureState, PlayerState and a GameLog.
    public GameControllerScript gc;
    private const int maxBranchSize = 6;
    public int x =85, z=85 , y=1, rot=0;
    public float meepleX, meepleZ;
    public float realX, realY, realZ, realRot;
    public Phase phase;
    private string placement = "";


    /// <summary>
    /// Initial setup which gets the scripts needed to AI calls and observations, called only once when the agent is enabled.
    /// </summary>
    public override void Initialize()
    {
        base.Initialize();
        gc = GameObject.Find("GameController").GetComponent<GameControllerScript>();
        gameState = gc.gameState;
    }


    /// <summary>
    /// Perform actions based on a vector of numbers. Which actions are made depend on the current game phase.
    /// </summary>
    /// <param name="actionBuffers">The struct of actions to take</param>
    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        phase = gameState.phase;
        switch (gameState.phase)
        {
            case Phase.TileDrawn:
                TileDrawnAction(actionBuffers);
                break;
            case Phase.TileDown:
                if (actionBuffers.DiscreteActions[0] == 0f)
                {
                    Debug.LogError("Meeple drawn");
                    gc.meepleControllerScript.DrawMeepleRPC(); //Take meeple
                }
                else
                {
                    Debug.LogError("No meeple drawn, ending turn");
                    gc.EndTurnRPC(); //End turn without taking meeple
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
                //AddReward(-0.01f); //Punishment for rotating more than needed, i.e. returning back to default rotation state.
            }
        }
        else if (actionBuffers.DiscreteActions[0] == 5f)
        {
            //Rotates the tile the amount of times AI has chosen (0-3).
            for (int i = 0; i < rot; i++)
            {
                gc.pcRotate = true;
                gc.RotateTileRPC();
            }

            //Values are loaded into GameController that are used in the ConfirmPlacementRPC call.
            gc.iTileAimX = x;
            gc.iTileAimZ = z;
            gc.ConfirmPlacementRPC();
            
            if (gameState.phase == Phase.TileDown) //If the placement was successful, the phase changes to TileDown.
            {
                Debug.LogError("Tile placed: " + gameState.Tiles.Current.transform.localPosition.x + ", Y: " + gameState.Tiles.Current.transform.localPosition.y + ", Z: " + gameState.Tiles.Current.transform.localPosition.z + ", Rotation: " + gameState.Tiles.Current.transform.rotation.eulerAngles.y);
                AddReward(1f);
            }
        }


        //After choice checks.
        if (x < 0 || x >= gameState.Tiles.Played.GetLength(0) || z < 0 || z >= gameState.Tiles.Played.GetLength(1))
        {
            //Outside table area, reset values and add significant punishment.
            SetTileStartPosition();
            AddReward(-1f);
            Debug.LogError("AI outside table area. Retteing position.");
        } /*else if (gc.PlacedTiles.HasNeighbor(x, z) && gameState.Tiles.Played[x, z] != null)
        {
            AddReward(0.01f);
        } else
        {
            AddReward(-0.01f); //Punishment for walking outside the edge of the built area, to avoid the AI looking through the entire grid each time.
        }*/

        //These are useless, only to monitor the ai (shown on the AI gameobject in the scene while it plays).
        realX = gameState.Tiles.Current.transform.localPosition.x;
        realY = gameState.Tiles.Current.transform.localPosition.y;
        realZ = gameState.Tiles.Current.transform.localPosition.z;
        realRot = gameState.Tiles.Current.transform.rotation.eulerAngles.y;
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
                gameState.Meeples.Current.gameObject.transform.localPosition = gameState.Tiles.Current.transform.localPosition + new Vector3(meepleX, 0.86f, meepleZ);
                gc.meepleControllerScript.CurrentMeepleRayCast();
                gc.meepleControllerScript.AimMeeple(gc);
                gc.SetMeepleSnapPos();
                gc.ConfirmPlacementRPC(); //Either confirms and places the meeple if possible, or returns meeple and goes back to phase TileDown.
                
                //The two rows below are just a workaround to get meeples to stay on top of the table and not have a seemingly random Y coordinate.
                gameState.Meeples.Current.gameObject.GetComponent<Rigidbody>().constraints = RigidbodyConstraints.FreezePositionY;
                gameState.Meeples.Current.gameObject.transform.localPosition = new Vector3(gameState.Meeples.Current.gameObject.transform.localPosition.x, 0.86f, gameState.Meeples.Current.gameObject.transform.localPosition.z);
            }
            else
            {
                Debug.LogError("Tried to place meeple, placement is empty");
            }
           

            if (gameState.phase == Phase.MeepleDown) //If meeple is placed.
            {
                AddReward(1f); //Rewards successfully placing a meeple
                Debug.LogError("Meeple placed successfully in place " + placement + ". Its actual position is direction '" 
                    + gc.meepleControllerScript.meeples.Current.direction + "' and geography '" + gc.meepleControllerScript.meeples.Current.geography + "', ending turn");
                Debug.LogError("X: " + gc.meepleControllerScript.meeples.Current.gameObject.transform.localPosition.x + ",Y: " + gc.meepleControllerScript.meeples.Current.gameObject.transform.localPosition.y + ", Z: " + gc.meepleControllerScript.meeples.Current.gameObject.transform.localPosition.z);
                placement = "";
            }
            else if (gameState.phase == Phase.TileDown) //If meeple gets returned.
            {
                AddReward(-1f); //Punishes returning a meeple & going back a phase (note: no punishment for never drawing a meeple).
                Debug.LogError("Tried to place meeple in inaccessible place. Reset meeple and return to TileDown phase. X: " + gc.meepleControllerScript.meeples.Current.gameObject.transform.localPosition.x + ", Z: " + gc.meepleControllerScript.meeples.Current.gameObject.transform.localPosition.z);
                placement = "";
            }
            else //Workaround for a bug where you can draw an unplacable meeple and never be able to change state.
            {
                gc.meepleControllerScript.FreeMeeple(gameState.Meeples.Current.gameObject, gc);
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
        SetTileStartPosition();
    }


    /// <summary>
    /// Collect all non-Raycast observations
    /// </summary>
    /// <param name="sensor">The vector sensor to add observations to</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation((int)gameState.phase);
        sensor.AddObservation(thisPlayer.AmountOfFreeMeeples());
        sensor.AddObservation(gameState.Tiles.Current.id);
        sensor.AddObservation(rot);
        sensor.AddObservation(x);
        sensor.AddObservation(z);
        sensor.AddObservation(meepleX);
        sensor.AddObservation(meepleZ);


        //Is there an easier way to add this information? This becomes a huge amount of data, may need to be analyzed in a different way, e.g. matrix representation.
        foreach (TileScript tile in gameState.Tiles.Played)
        {
            if (tile != null)
            {
                sensor.AddObservation(tile.id);
                sensor.AddObservation(tile.rotation);
                sensor.AddObservation(tile.transform.position.x);
                sensor.AddObservation(tile.transform.position.z);
            }
        }

        //Possibly relevant to add player scores for multiplayer, so AI knows if it is losing or not
    }

    /// <summary>
    /// Masks certain inputs so they cannot be used. Amount of viable inputs depends on the game phase.
    /// </summary>
    /// <param name="actionMask">The actions (related to ActionBuffer actioons) to disable or enable</param>
    public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
    {
        int allowedActions = 0;
        switch (gameState.phase)
        {
            case Phase.TileDrawn:
                //AI can choose to step one tile place in either of the 4 directions (-X, X, -Z, Z), rotate 90 degrees, or confirm place.
                //Note that this step calls for many decisions until the AI is happy and confirms placement. The other do not.
                allowedActions = 6;
                break;
            case Phase.TileDown:
                //AI can choose to take or not take a meeple.
                allowedActions = 2;
                break;
            case Phase.MeepleDrawn:
                //AI can choose to place a drawn meeple in 5 different places (N, S, W, E, C) or confirm/deny current placement.
                //This step is called many times, allowing the AI to replace the meeple, remove it, grab it again etc.
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
    internal void SetTileStartPosition()
    {
        x = 85;
        z = 85;
        y = 1;
        rot = 0;
        placement = "";
    }
}
