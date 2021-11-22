using Carcassonne.State;
using Carcassonne;
using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Sensors;
using System;
using Assets.Scripts.Carcassonne.AI;
using static Carcassonne.PointScript;

/// <summary>
/// The AI for the player. An AI user contains both a regular PlayerScript and this AI script to observe and take actions.
/// </summary>

public class AIPlayer :  Agent
{
    //Static observations for normalization
    private float meeplesMax;
    private float boardMaxSize;
    private float tileIdMax;

    //Dynamic observations from real game (use getter properties if they exist, don't call these directly)
    //private int meeplesLeft;
    //private Phase phase;
    //private int id;
    private Direction meepleDirection = Direction.SELF;

    //AI Specific
    public AIWrapper wrapper;
    private const int maxBranchSize = 6;
    public int x=85, z=85 , y=1, rot=0;

    //Monitoring
    public float realX, realY, realZ, realRot;

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
        wrapper.player = gameObject.GetComponentInParent<PlayerScript>();
        boardMaxSize = wrapper.GetMaxBoardSize();
        meeplesMax = wrapper.GetMaxMeeples();
        tileIdMax = wrapper.GetMaxTileId();
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
                    wrapper.DrawMeeple(); //Take meeple
                }
                else
                {
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
            z += 1; //Up
        }
        else if (actionBuffers.DiscreteActions[0] == 1f)
        {
            z -= 1; //Down
        }
        else if (actionBuffers.DiscreteActions[0] == 2f)
        {
            x -= 1; //Left
        }
        else if (actionBuffers.DiscreteActions[0] == 3f)
        {
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
        else if (actionBuffers.DiscreteActions[0] == 5f) //Place tile
        {
            //Rotates the tile the amount of times AI has chosen (0-3).
            for (int i = 0; i <= rot; i++)
            {
                wrapper.RotateTile();
            }

            //Values are loaded into GameController that are used in the ConfirmPlacementRPC call.
            wrapper.PlaceTile(x, z);
            
            if (Phase == Phase.TileDown) //If the placement was successful, the phase changes to TileDown.
            {
                AddReward(0.1f);

                //Line below is only used for debugging. Ignore it.
                //Debug.LogError("Tile placed: " + wrapper.GetCurrentTile().transform.localPosition.x + ", Y: " + wrapper.GetCurrentTile().transform.localPosition.y + ", Z: " + wrapper.GetCurrentTile().transform.localPosition.z + ", Rotation: " + wrapper.GetCurrentTile().transform.rotation.eulerAngles.y);
            }
        }


        //After choice checks.
        if (x < 0 || x >= boardMaxSize || z < 0 || z >= boardMaxSize)
        {
            //Outside table area, reset values and add significant punishment.
            ResetAttributes();
            AddReward(-0.1f);
        } 

        //The rows below are only used to monitor the ai (shown on the AI gameobject in the scene while it plays). Ignore them.

        /*realX = wrapper.GetCurrentTile().transform.localPosition.x;
        realY = wrapper.GetCurrentTile().transform.localPosition.y;
        realZ = wrapper.GetCurrentTile().transform.localPosition.z;
        realRot = wrapper.GetCurrentTile().transform.rotation.eulerAngles.y;*/
    }

    /// <summary>
    /// Places the meeple on one of the 5 places available on the tile (Uses the tile to find the positions).
    /// </summary>
    /// <param name="actionBuffers"></param>
    //TODO: Fixa bättre lösning för meeple location. Enum? Lagt till meepleDirection högst upp.
    private void MeepleDrawnAction(ActionBuffers actionBuffers)
    {
        AddReward(-0.01f); //Each call gets a negative reward to avoid getting stuck just moving the meeple around in this stage.
        if (actionBuffers.DiscreteActions[0] == 0f)
        {
            meepleDirection = Direction.NORTH;
            //meepleX = 0.000f;
            //meepleZ = 0.011f;
        }
        else if (actionBuffers.DiscreteActions[0] == 1f)
        {
            meepleDirection = Direction.SOUTH;
            //meepleX = 0.000f;
            //meepleZ = -0.011f;
        }
        else if (actionBuffers.DiscreteActions[0] == 2f)
        {
            meepleDirection = Direction.WEST;
            //meepleX = -0.011f;
            //meepleZ = 0.000f;
        }
        else if (actionBuffers.DiscreteActions[0] == 3f)
        {
            meepleDirection = Direction.EAST;
            //meepleX = 0.011f;
            //meepleZ = 0.000f;
        }
        else if (actionBuffers.DiscreteActions[0] == 4f)
        {
            meepleDirection = Direction.CENTER;
            //meepleX = 0.000f;
            //meepleZ = 0.000f;
        }
        else if (actionBuffers.DiscreteActions[0] == 5f)
        {
            if (meepleDirection != Direction.SELF) //Checks so that a choice has been made since meeple was drawn.
            {
                float meepleX = 0;
                float meepleZ = 0;
                if (meepleDirection == Direction.NORTH || meepleDirection == Direction.SOUTH || meepleDirection == Direction.CENTER)
                {
                    meepleX = 0.000f;
                } else if (meepleDirection == Direction.EAST)
                {
                    meepleX = 0.011f;
                } else if (meepleDirection == Direction.WEST)
                {
                    meepleX = -0.011f;
                }

                if (meepleDirection == Direction.WEST || meepleDirection == Direction.EAST || meepleDirection == Direction.CENTER)
                {
                    meepleZ = 0.000f;
                }
                else if (meepleDirection == Direction.NORTH)
                {
                    meepleZ = 0.011f;
                }
                else if (meepleDirection == Direction.SOUTH)
                {
                    meepleZ = -0.011f;
                }
                wrapper.PlaceMeeple(meepleX, meepleZ);  //Either confirms and places the meeple if possible, or returns meeple and goes back to phase TileDown.
            }
           

            if (Phase == Phase.MeepleDown) //If meeple is placed.
            {
                AddReward(0.1f); //Rewards successfully placing a meeple
            }
            else if (Phase == Phase.TileDown) //If meeple gets returned.
            {
                AddReward(-0.1f); //Punishes returning a meeple & going back a phase (note: no punishment for never drawing a meeple).
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
        ResetAttributes();
    }


    /// <summary>
    /// Collect all observations, normalized.
    /// </summary>
    /// <param name="sensor">The vector sensor to add observations to</param>
    public override void CollectObservations(VectorSensor sensor)
    {
        sensor.AddObservation(MeeplesLeft/meeplesMax);
        sensor.AddObservation(Id/tileIdMax);
        sensor.AddObservation(rot/3f);
        sensor.AddObservation(x/boardMaxSize);
        sensor.AddObservation(z/boardMaxSize);
        sensor.AddObservation((int)meepleDirection);

        //One-Hot observations of enums (can be done with less code, but this is more readable)
        int MAX_PHASES = Enum.GetValues(typeof(Phase)).Length;
        int MAX_DIRECTIONS = Enum.GetValues(typeof(Direction)).Length;

        sensor.AddOneHotObservation((int)Phase.TileDrawn, MAX_PHASES);
        sensor.AddOneHotObservation((int)Phase.TileDown, MAX_PHASES);
        sensor.AddOneHotObservation((int)Phase.MeepleDrawn, MAX_PHASES);

        sensor.AddOneHotObservation((int)Direction.NORTH, MAX_DIRECTIONS);
        sensor.AddOneHotObservation((int)Direction.EAST, MAX_DIRECTIONS);
        sensor.AddOneHotObservation((int)Direction.SOUTH, MAX_DIRECTIONS);
        sensor.AddOneHotObservation((int)Direction.WEST, MAX_DIRECTIONS);
        sensor.AddOneHotObservation((int)Direction.CENTER, MAX_DIRECTIONS);
        sensor.AddOneHotObservation((int)Direction.SELF, MAX_DIRECTIONS);




        //Code below handles the input of the entire board, which is really ineffective in its current implemenetation.

        //The most reasonable approach seems to have a matrix of floats, each float representing one tile. The matrix should be the size
        //of the entire board, padded with 0 wherever a tile has not been placed. Read the entire board or just the placed tiles.

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
        meepleDirection = Direction.CENTER;
    }
}
