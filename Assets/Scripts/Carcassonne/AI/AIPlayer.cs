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
    public float realX, realY, realZ, realRot;
    public Phase phase;


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
            //Makes a rotation call to rotate the tile 90 degrees.
            gc.pcRotate = true;
            gc.RotateTileRPC();
            rot++;
            if (rot == 4)
            {
                rot = 0;
                //AddReward(-0.01f); //Punishment for rotating more than needed, i.e. returning back to default rotation state.
            }
        }
        else if (actionBuffers.DiscreteActions[0] == 5f)
        {
            //Values are loaded into tileControllerScript by the other actions in this method. THe are used during the ConfirmPlacementRPC call.
            gc.iTileAimX = x;
            gc.iTileAimZ = z;
            gc.ConfirmPlacementRPC();
            if (gameState.phase == Phase.TileDown)
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
        } else if (gc.PlacedTiles.HasNeighbor(x, z) && gameState.Tiles.Played[x, z] != null)
        {
            AddReward(0.01f);
        } else
        {
            AddReward(-0.01f); //Punishment for walking outside the edge of the built area, to avoid the AI looking through the entire grid each time.
        }

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
        string placement = "";
        if (actionBuffers.DiscreteActions[0] == 0f)
        {
            //North
            placement = "North";
            gc.meepleControllerScript.meeples.Current.gameObject.transform.localPosition = gameState.Tiles.Current.transform.localPosition + new Vector3(0, y, 0.011f);
        }
        else if (actionBuffers.DiscreteActions[0] == 1f)
        {
            //South
            placement = "South";
            gc.meepleControllerScript.meeples.Current.gameObject.transform.localPosition = gameState.Tiles.Current.transform.localPosition + new Vector3(0, y, -0.011f);
        }
        else if (actionBuffers.DiscreteActions[0] == 2f)
        {
            //West
            placement = "West";
            gc.meepleControllerScript.meeples.Current.gameObject.transform.localPosition = gameState.Tiles.Current.transform.localPosition + new Vector3(-0.011f, y, 0);
        }
        else if (actionBuffers.DiscreteActions[0] == 3f)
        {
            //East
            placement = "East";
            gc.meepleControllerScript.meeples.Current.gameObject.transform.localPosition = gameState.Tiles.Current.transform.localPosition + new Vector3(0.011f, y, 0);
        }
        else if (actionBuffers.DiscreteActions[0] == 4f)
        {
            //Center
            placement = "Center";
            gc.meepleControllerScript.meeples.Current.gameObject.transform.localPosition = gameState.Tiles.Current.transform.localPosition + new Vector3(0, y, 0);
        }
        else if (actionBuffers.DiscreteActions[0] == 5f)
        {
            gc.SetMeepleSnapPos();
            gc.ConfirmPlacementRPC(); //Either confirms and places meeple, or returns meeple and goes back to phase TileDown.

            //Workaround to get the meeples to actually be on top of the board instead of below (don't know why they end up there).
            //ToDo: This workaround does not help with the problem.
            Vector3 meeplePos = gc.meepleControllerScript.meeples.Current.gameObject.transform.localPosition;
            meeplePos.y = 0.86f;
            gc.meepleControllerScript.meeples.Current.gameObject.transform.localPosition = meeplePos;

            if (gameState.phase == Phase.MeepleDown)
            {
                AddReward(1f); //Rewards successfully placing a meeple
                MeepleScript meeple = gc.meepleControllerScript.meeples.Current;
                Debug.LogError("Meeple placed successfully in place " + placement + ". Its actual position is direction '" + meeple.direction + "' and geography '" + meeple.geography + "', ending turn");
                gc.EndTurnRPC();
                
            }
            else if (gameState.phase == Phase.TileDown)
            {
                AddReward(-1f); //Punishes returning a meeple & going back a phase (note: no punishment for never drawing a meeple).
                Debug.LogError("Tried to place meeple in inaccessible place. Reset meeple and return to TileDown phase. X: " + gc.meepleControllerScript.meeples.Current.gameObject.transform.localPosition.x + ", Z: " + gc.meepleControllerScript.meeples.Current.gameObject.transform.localPosition.z);
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
        //This needs to reset the game for another playthrough.
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
        sensor.AddObservation(gameState.Tiles.Current.rotation);
        if (gameState.phase == Phase.MeepleDrawn)
        {
            sensor.AddObservation(gc.meepleControllerScript.meeples.Current.gameObject.transform.position);
        }
        sensor.AddObservation(x);
        sensor.AddObservation(z);

        //Is there an easier way to add this information? This becomes a huge amount of data, may need to be analyzed in a different way, e.g. matrix representation.
        foreach (TileScript tile in gameState.Tiles.Played) //Does this allow tile to be null?
        {
            if (tile != null)
            {
                sensor.AddObservation(tile.id);
                sensor.AddObservation(tile.rotation);
                sensor.AddObservation(tile.transform.position.x);
                sensor.AddObservation(tile.transform.position.y);
            }
        }

        //Possibly relevant for multiplayer.
        //sensor.AddObservation(thisPlayer.GetPlayerScore()); 
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
    /// Resets tile position to base position before next tile placement.
    /// </summary>
    internal void SetTileStartPosition()
    {
        x = 85;
        z = 85;
        y = 1;
        rot = 0;
    }
}
