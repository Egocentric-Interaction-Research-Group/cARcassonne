using System;
using System.Collections.Generic;
using System.Linq;
using Carcassonne.State;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;
using Unity.MLAgents.Sensors;
using UnityEngine;


/// Action could be ? branches: X, Y, rotation, Meeple
/// 

namespace Carcassonne.AI
{
    //TODO need a default mask on each branch.
    public enum Actions
    {
        TileUpDown,         // 0, 1, 2
        TileLeftRight,      // 0, 1, 2
        TileRotate,         // 0, 1, 2, 3
        TilePlace,          // 0, 1
        MeeplePosition,     // 0, 1, 2, 3, 4
        MeepleDiscardPlace, // 0, 1, 2
        Decision            // 0, 1
    }

    public enum DecisionAction
    {
        DrawMeeple,
        EndTurn
    }

    /// <summary>
    /// The AI for the player. An AI user contains both a regular PlayerScript and this AI script to observe and take actions.
    /// Version 1.0
    /// </summary>
    public class CarcassonneAgent : Agent
    {
        private static readonly int[] DefaultActions = new int[] {1, 1, 0, 0, 0, 0, 0};

        private const int MAX_GAME_SCORE = 338; // https://boardgames.stackexchange.com/questions/7375/maximum-attainable-points-for-a-single-player-in-a-two-player-game-of-carcassonn

        //Enum Observations
        private Vector2Int meepleDirection;

        // Observation approach
        public ObservationApproach observationApproach = ObservationApproach.TileIds;
        private Action<AIWrapper, VectorSensor> AddTileObservations;

        //AI Specific
        public AIWrapper wrapper;
        private const int maxBranchSize = 6;
        // public int x = 0, z = 0;//, rot = 0;
        public Vector2Int cell;

        /// <summary>
        /// Initial setup which gets the scripts needed to AI calls and observations, called only once when the agent is enabled.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();
            wrapper = GetComponent<AIWrapper>();

            // Setup delegate for tile observation approach.
            switch (observationApproach)
            {
                case ObservationApproach.TileIds: // For each tile, observe the tile ID and rotation as one observation, and meeple data as another observation
                    AddTileObservations = BoardObservation.AddTileIdObservations;
                    break;

                case ObservationApproach.Packed: // For each tile, observe the tile ID, rotation, and meeple data as one packed observation
                    AddTileObservations = BoardObservation.AddPackedTileObservations;
                    break;

                case ObservationApproach.PackedIDs: // For each tile, pack all tile geographies explicitly, into one observation (instead of using tile IDs), and then meeple data as another observation
                    AddTileObservations = BoardObservation.AddPackedTileIdObservations;
                    break;

                // Note: There should only ever be one tile observations function in use, hence '=', and not '+='.
                case ObservationApproach.TileWise:
                    AddTileObservations = BoardObservation.AddTileWiseObservations;
                    break;
            }
        }

        /// <summary>
        /// Perform actions based on a vector of numbers. Which actions are made depend on the current game phase.
        /// </summary>
        /// <param name="actionBuffers">The struct of actions to take</param>
        public override void OnActionReceived(ActionBuffers actionBuffers)
        {
            Debug.Log(String.Join(", ", actionBuffers.DiscreteActions.Array.Select(p => p.ToString()).ToArray()));
            switch (wrapper.GetGamePhase())
            {
                case Phase.TileDrawn:
                    var upDown = actionBuffers.DiscreteActions[(int)Actions.TileUpDown];
                    var leftRight = actionBuffers.DiscreteActions[(int)Actions.TileLeftRight];
                    var rotate = actionBuffers.DiscreteActions[(int)Actions.TileRotate] % 4;
                    var tPlace = actionBuffers.DiscreteActions[(int)Actions.TilePlace] == 1;
                    TileDrawnAction(upDown, leftRight, rotate, tPlace);
                    break;
                case Phase.TileDown:
                    var decision = actionBuffers.DiscreteActions[(int)Actions.Decision];
                    Debug.Log("Decision Time.");
                    
                    if (decision == (int)DecisionAction.DrawMeeple)
                    {
                        Debug.Log("Decided to draw a meeple.");
                        wrapper.DrawMeeple(); //Take meeple
                    }
                    else if (decision == (int)DecisionAction.EndTurn)
                    {
                        Debug.Log("Decided to end the turn.");
                        EndOfTurnRewards();
                        wrapper.EndTurn(); //End turn without taking meeple
                    }
                    break;
                case Phase.MeepleDrawn:
                    var position = actionBuffers.DiscreteActions[(int)Actions.MeeplePosition];
                    var mPlace = actionBuffers.DiscreteActions[(int)Actions.MeepleDiscardPlace] == 1;
                    var mDiscard = actionBuffers.DiscreteActions[(int)Actions.MeepleDiscardPlace] == 2;
                    MeepleDrawnAction(position, mPlace, mDiscard);
                    break;
            }
        }

        private void TileDrawnAction(int upDown, int leftRight, int rotate, bool place)
        {
            AddReward(-0.001f); //Each call to this method comes with a very minor penalty to promote performing quick actions.
            cell += new Vector2Int(leftRight, upDown) - Vector2Int.one;
            // x += leftRight - 2;
            // z += upDown - 2;
            // rot = (rot + rotate) % 4;
            
            //Rotates the tile the amount of times AI has chosen (0-3).
            for (int i = 0; i <= rotate; i++)
            {
                wrapper.RotateTile();
            }
            
            if (place) //Place tile
            {
                //Values are loaded into GameController that are used in the ConfirmPlacementRPC call.
                wrapper.PlaceTile(cell);

                if (wrapper.GetGamePhase() == Phase.TileDown) //If the placement was successful, the phase changes to TileDown.
                {
                    AddReward(0.05f);
                }
                else
                {
                    AddReward(-0.001f); // Demerit for failed placement.
                }
            }

            // //After choice checks to determine if AI is Out of Bounds. Bounds a defined by the minimum and maximum coordinates in each axis for tiles placed
            // if (x < wrapper.GetMinX() - 1 || x > wrapper.GetMaxX() + 1 || z < wrapper.GetMinZ() - 1 || z > wrapper.GetMaxZ() + 1)
            // {
            //     //Outside table area, reset values and add significant punishment.
            //     ResetAttributes();
            //     AddReward(-0.05f);
            // }
        }

        /// <summary>
        /// Places the meeple on one of the 5 places available on the tile (Uses the tile to find the positions).
        /// </summary>
        /// <param name="actionBuffers"></param>
        private void MeepleDrawnAction(int position, bool place, bool discard)
        {
            AddReward(-0.01f); //Each call gets a negative reward to avoid getting stuck just moving the meeple around in this stage.
            if (discard || !wrapper.CanBePlaced())
            {
                AddReward(-0.001f); // Don't want people picking up and discarding meeples for no reason.
                wrapper.meepleController.Discard();
                return;
            }
            
            if (position == 1)
            {
                meepleDirection = Vector2Int.up;
            }
            else if (position == 2)
            {
                meepleDirection = Vector2Int.down;
            }
            else if (position == 3)
            {
                meepleDirection = Vector2Int.left;
            }
            else if (position == 4)
            {
                meepleDirection = Vector2Int.right;
            }
            else if (position == 0)
            {
                meepleDirection = Vector2Int.zero;
            }

            if (place)
            {
                wrapper.PlaceMeeple(meepleDirection);  //Either confirms and places the meeple if possible, or returns meeple and goes back to phase TileDown.
                
                if (wrapper.GetGamePhase() == Phase.MeepleDown) //If meeple is placed.
                {
                    AddReward(0.1f); //Rewards successfully placing a meeple
                    EndOfTurnRewards();
                    wrapper.EndTurn();
                }
                else if (wrapper.GetGamePhase() == Phase.MeepleDrawn) //If meeple gets returned.
                {
                    AddReward(-0.1f); //Punishes returning a meeple & going back a phase (note: no punishment for never drawing a meeple).
                }
                else
                {
                    Debug.LogWarning($"Unexpected game phase {wrapper.GetGamePhase()} on Meeple placement attempt.");
                }
            }
        }

        /// <summary>
        /// When a new episode begins, reset the agent and area
        /// </summary>
        public override void OnEpisodeBegin()
        {
            //This occurs every X steps (Max Steps). It only serves to reset tile position if AI is stuck, and for AI to process current learning
            ResetAttributes();
            wrapper.Restart();
        }

        /// <summary>
        /// Collect all observations, normalized.
        /// </summary>
        /// <param name="sensor">The vector sensor to add observations to</param>
        public override void CollectObservations(VectorSensor sensor)
        {
            var obsCount = 0;
            sensor.AddObservation(wrapper.GetScore() / MAX_GAME_SCORE);
            obsCount++; // 1
            sensor.AddOneHotObservation(wrapper.GetCurrentTileId(),wrapper.GetMaxTileId()+1);
            obsCount += wrapper.GetMaxTileId() + 1; // 26
            sensor.AddObservation(wrapper.GetCurrentTileRotations() / 3f);
            obsCount++; // 27
            // sensor.AddObservation(x / wrapper.GetMaxBoardSize());
            // sensor.AddObservation(z / wrapper.GetMaxBoardSize());
            sensor.AddObservation(cell / wrapper.GetMaxBoardSize());
            obsCount += 2; // 29
            sensor.AddObservation(wrapper.GetNumberOfPlacedTiles() / wrapper.GetTotalTiles());
            obsCount++; // 30
            sensor.AddObservation(wrapper.GetMeeplesLeft() / wrapper.GetMaxMeeples());
            obsCount++; // 31

            //One-Hot observations of enums (can be done with less code, but this is more readable)
            int MAX_PHASES = Enum.GetValues(typeof(Phase)).Length;
            // int MAX_DIRECTIONS = 6; //TODO: DIRECTION DEPRECATION CHANGE - Enum.GetValues(typeof(Direction)).Length;
            sensor.AddOneHotObservation((int)wrapper.GetGamePhase(), MAX_PHASES);
            obsCount += MAX_PHASES; // 37
            
            //TODO: DIRECTION DEPRICATION CHANGE -sensor.AddOneHotObservation((int)meepleDirection, MAX_DIRECTIONS);
            // if (meepleDirection != null) sensor.AddObservation((Vector2)meepleDirection); //FIXME This not going to work. Can't be conditional.
            sensor.AddOneHotObservation(MeepleDirectionToOneHot(meepleDirection), nDirections);
            obsCount += nDirections; // 43

            Debug.Log($"Added {obsCount} general observations.");
            
            // Call the tile observation method that was assigned at initialization,
            // using the editor-exposed 'observationApproach' field.
            // This will observe the entire Carcassonne game board.
            AddTileObservations?.Invoke(wrapper, sensor);
        }

        /// <summary>
        /// Masks certain inputs so they cannot be used. Amount of viable inputs depends on the game phase.
        /// </summary>
        /// <param name="actionMask">The actions (related to ActionBuffer actioons) to disable or enable</param>
        public override void WriteDiscreteActionMask(IDiscreteActionMask actionMask)
        {
            List<Actions> allowedActions = new List<Actions>();
            switch (wrapper.GetGamePhase())
            {
                case Phase.TileDrawn:
                    //AI can choose to step one tile place in either of the 4 directions (-X, X, -Z, Z), rotate 90 degrees, or confirm place.
                    allowedActions = new List<Actions> { Actions.TileUpDown, Actions.TileLeftRight, Actions.TileRotate, Actions.TilePlace};
                    
                    // Disallow moves that go far outside the bounds of the board.
                    DisallowWandering(ref actionMask);

                    break;
                case Phase.TileDown:
                    //AI can choose to take or not take a meeple.
                    allowedActions = new List<Actions> { Actions.Decision };
                    break;
                case Phase.MeepleDrawn:
                    //AI can choose to place a drawn meeple in 5 different places (N, S, W, E, C) or confirm/deny current placement.
                    allowedActions = new List<Actions> { Actions.MeeplePosition, Actions.MeepleDiscardPlace };
                    break;
            }

            var branchSizes = GetComponent<BehaviorParameters>().BrainParameters.ActionSpec.BranchSizes;

            //Disables all actions of branch 0, index i (on that branch) for any i larger than the allowed actions.
            foreach (Actions action in Enum.GetValues(typeof(Actions)))
            {
                if (!allowedActions.Contains(action))
                {
                    for (int i = 0; i < branchSizes[(int)action]; i++)
                    {
                        var isEnabled = DefaultActions[(int)action] == i; // Enable the default action for each branch
                        actionMask.SetActionEnabled((int)action, i, isEnabled);
                    }
                }
            }
        }

        private void DisallowWandering(ref IDiscreteActionMask actionMask)
        {
            var limits = wrapper.GetLimits();
            
            // Extend the limits by one cell on every side
            limits.height += 2;
            limits.width += 2;
            limits.position -= Vector2Int.one;

            // Do nothing if the tile is within the limits
            if (limits.Contains(cell))
            {
                return;
            }

            var fromCentre = cell - limits.center;
            
            // Disable left/right if it is too far from centre
            if (Math.Abs(fromCentre.x) > limits.width / 2f)
            {
                var actionIndex = fromCentre.x > 0 ? 2 : 0;
                actionMask.SetActionEnabled((int)Actions.TileLeftRight, actionIndex, false);
            }
            
            // Disable up/down if it is too far from centre
            if (Math.Abs(fromCentre.y) > limits.height / 2f)
            {
                var actionIndex = fromCentre.y > 0 ? 2 : 0;
                actionMask.SetActionEnabled((int)Actions.TileUpDown, actionIndex, false);
            }
        }

        /// <summary>
        /// Resets tile position and placement (meeple position) to base position before next action.
        /// </summary>
        internal void ResetAttributes()
        {
            // x = 0;//GameRules.BoardSize/2;
            // z = 0;//GameRules.BoardSize/2;
            // rot = 0;
            cell = Vector2Int.zero;
            meepleDirection = Vector2Int.zero;
        }

        private const int nDirections = 6;
    
        // This is to account for the fact that it is ambiguous in the docs how Vector2.Angle treats angles at +-180 degrees.
        private static readonly int angleLimit = (int)(Vector2.Angle(Vector2.zero, Vector2Int.left) / 90.0);
        private static readonly int angleAdjustment = angleLimit == -2 ? 3 : 2;

        /// <summary>
        /// Encodes meeple direction as 0 if not placed, 1-4 if placed on the edge, and 5 if placed in the middle.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        private int MeepleDirectionToOneHot(Vector2Int? direction)
        {
            if (direction == null) return 0;
            if (direction == Vector2Int.zero) return nDirections-1;

            var oneHotAngle = (int)(Vector2.Angle(Vector2.zero, (Vector2)direction) / 90.0) + angleAdjustment;

            Debug.Assert(oneHotAngle < nDirections - 1 && oneHotAngle > 0, 
                $"The meeple direction is neither null (not placed) nor the centre and should be" +
                $"0 < direction < {nDirections}, but is {oneHotAngle}.");
            
            return oneHotAngle;
        }

        private void EndOfTurnRewards()
        {
            // Score changed reward
            var scoreChange = wrapper.GetScoreChange();
            AddReward(scoreChange);
            var unscoredPointsChange = 0.5f * wrapper.GetUnscoredPointsChange();
            AddReward(unscoredPointsChange);
            var potentialPointsChange = 0.5f * wrapper.GetPotentialPointsChange();
            AddReward(potentialPointsChange);

            var otherScoreChange = wrapper.GetOtherScoreChange();
            AddReward(otherScoreChange);
            var otherUnscoredPointsChange = 0.5f * wrapper.GetOtherUnscoredPointsChange();
            AddReward(otherUnscoredPointsChange);
            var otherPotentialPointsChange = 0.5f * wrapper.GetOtherPotentialPointsChange();
            AddReward(otherPotentialPointsChange);
            
            // Meeples Remaining
            var meeplesRemaingingScore =
                wrapper.GetMeeplesLeft() < 2 ? -0.1f * 1.0f / ((float)(wrapper.GetMeeplesLeft() + 1)) : 0.0f;
            AddReward(meeplesRemaingingScore);

            Debug.Log($"EOT Rewards (P{wrapper.player.id}) dScore={scoreChange}, dUnscore={unscoredPointsChange}, dPotential={potentialPointsChange}, " +
                      $"dOther={otherScoreChange}, dOtherUnscore={otherUnscoredPointsChange}, dOtherPot={otherPotentialPointsChange}, " +
                      $"meeples={meeplesRemaingingScore}, " +
                      $"score={wrapper.player.score}, unscore={wrapper.player.unscoredPoints}, potential={wrapper.player.potentialPoints}");
        }
    }
}