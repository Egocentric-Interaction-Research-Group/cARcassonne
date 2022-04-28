using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Carcassonne.Models;
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
    public static class Extensions
    {
        // public static Dictionary<MeeplePosition, Vector2Int> MeepleDirection = new Dictionary<MeeplePosition, Vector2Int>()
        // {
        //     { MeeplePosition.Up, Vector2Int.up },
        //     { MeeplePosition.Right, Vector2Int.right },
        //     { MeeplePosition.Down, Vector2Int.down },
        //     { MeeplePosition.Left, Vector2Int.left },
        //     { MeeplePosition.Centre, Vector2Int.zero },
        // };
        public static Vector2Int Direction(this MeeplePosition position)
        {
            switch (position)
            {
                case MeeplePosition.Up:
                    return Vector2Int.up;
                case MeeplePosition.Right:
                    return Vector2Int.right;
                case MeeplePosition.Down:
                    return Vector2Int.down;
                case MeeplePosition.Left:
                    return Vector2Int.left;
                case MeeplePosition.Centre:
                    return Vector2Int.zero;
            }

            throw new ArgumentException($"{position} doesn't have a direction.");
        }
    }
    
    public enum MeeplePosition : int
    {
        None = 0,
        Up,
        Right,
        Down,
        Left,
        Centre
    }
    
    public struct Rewards
    {
        public static float ActionBias = -0.0001f;
        public static float InvalidAction = -0.0001f;//-0.010f;
        public static float ValidAction = 0.001f;//0.020f;
        public static float Score = 0.05f;
        public static float OtherScore = -0.01f;
        public static float Meeple = -0.01f;
    }
    
    //TODO need a default mask on each branch.
    public enum SBSActions
    {
        TileUpDown,         // 0, 1, 2
        TileLeftRight,      // 0, 1, 2
        TileRotate,         // 0, 1, 2, 3
        TilePlace,          // 0, 1
        MeeplePosition,     // 0, 1, 2, 3, 4
        MeepleDiscardPlace, // 0, 1, 2
        Decision            // 0, 1
    }

    public enum BrdActions
    {
        TileX,
        TileY,
        TileRotate,
        MeeplePosition
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
        private static readonly int[] DefaultActions = new int[] { 1, 1, 0, 0, 0, 0, 0 };

        private static int NumTiles = Tile.TileCount;
        private static readonly RectInt m_boardLimits = new RectInt(-NumTiles, -NumTiles, 2*NumTiles, 2*NumTiles);

        private const int
            MAX_GAME_SCORE =
                338; // https://boardgames.stackexchange.com/questions/7375/maximum-attainable-points-for-a-single-player-in-a-two-player-game-of-carcassonn

        //Enum Observations
        private Vector2Int meepleDirection;

        // Observation approach
        public ObservationApproach observationApproach = ObservationApproach.TileIds;
        public ActionApproach actionApproach = ActionApproach.SpaceBySpace;
        private Action<AIWrapper, VectorSensor> AddTileObservations;

        // Action Approach

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
                case ObservationApproach.TileIds
                    : // For each tile, observe the tile ID and rotation as one observation, and meeple data as another observation
                    AddTileObservations = BoardObservation.AddTileIdObservations;
                    break;

                case ObservationApproach.Packed
                    : // For each tile, observe the tile ID, rotation, and meeple data as one packed observation
                    AddTileObservations = BoardObservation.AddPackedTileObservations;
                    break;

                case ObservationApproach.PackedIDs
                    : // For each tile, pack all tile geographies explicitly, into one observation (instead of using tile IDs), and then meeple data as another observation
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
            switch (actionApproach)
            {
                case ActionApproach.SpaceBySpace:
                    SpaceBySpaceActions(actionBuffers);
                    break;
                case ActionApproach.Board:
                    BoardActions(actionBuffers);
                    break;
                case ActionApproach.Integrated:
                    var actions = StartCoroutine(IntegratedActions(actionBuffers));
                    break;
            }
        }
        

        /// <summary>
        /// When a new episode begins, reset the agent and area
        /// </summary>
        public override void OnEpisodeBegin()
        {
            //This occurs every X steps (Max Steps). It only serves to reset tile position if AI is stuck, and for AI to process current learning
            ResetAttributes();
            //wrapper.Restart();
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
            sensor.AddOneHotObservation(wrapper.GetCurrentTileId(), wrapper.GetMaxTileId() + 1);
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
            Debug.Log($"Agent {wrapper.player.id}: Writing Discrete Action Mask.");
            switch (actionApproach)
            {
                case ActionApproach.SpaceBySpace:
                    WriteDiscreteActionMaskSBS(actionMask);
                    break;
                case ActionApproach.Board:
                    WriteDiscreteActionMaskBrd(actionMask);
                    break;
                case ActionApproach.Integrated:
                    WriteDiscreteActionMaskIntegrated(actionMask);
                    break;
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
            var scoreChange = Rewards.Score * wrapper.GetScoreChange();
            AddReward(scoreChange);
            var unscoredPointsChange = 0.25f * Rewards.Score * wrapper.GetUnscoredPointsChange();
            AddReward(unscoredPointsChange);
            var potentialPointsChange = 0.25f * Rewards.Score * wrapper.GetPotentialPointsChange();
            AddReward(potentialPointsChange);

            var otherScoreChange = Rewards.OtherScore * wrapper.GetOtherScoreChange();
            AddReward(otherScoreChange);
            var otherUnscoredPointsChange = 0.25f * Rewards.OtherScore * wrapper.GetOtherUnscoredPointsChange();
            AddReward(otherUnscoredPointsChange);
            var otherPotentialPointsChange = 0.25f * Rewards.OtherScore * wrapper.GetOtherPotentialPointsChange();
            AddReward(otherPotentialPointsChange);
            
            // Meeples Remaining
            var meeplesRemaingingScore = Rewards.Meeple * (1.0f / ((float)(wrapper.GetMeeplesLeft() + 1)) - 0.125f);
            AddReward(meeplesRemaingingScore);

            Debug.Log($"EOT Rewards (P{wrapper.player.id}) dScore={scoreChange}, dUnscore={unscoredPointsChange}, dPotential={potentialPointsChange}, " +
                      $"dOther={otherScoreChange}, dOtherUnscore={otherUnscoredPointsChange}, dOtherPot={otherPotentialPointsChange}, " +
                      //$"meeples={meeplesRemaingingScore}, " +
                      $"score={wrapper.player.score}, unscore={wrapper.player.unscoredPoints}, potential={wrapper.player.potentialPoints}");
        }
        
        #region Space By Space Actions
        private void SpaceBySpaceActions(ActionBuffers actionBuffers)
        {
            switch (wrapper.GetGamePhase())
            {
                case Phase.TileDrawn:
                    var upDown = actionBuffers.DiscreteActions[(int)SBSActions.TileUpDown];
                    var leftRight = actionBuffers.DiscreteActions[(int)SBSActions.TileLeftRight];
                    var rotate = actionBuffers.DiscreteActions[(int)SBSActions.TileRotate] % 4;
                    var tPlace = actionBuffers.DiscreteActions[(int)SBSActions.TilePlace] == 1;
                    TileDrawnAction(upDown, leftRight, rotate, tPlace);
                    break;
                case Phase.TileDown:
                    var decision = actionBuffers.DiscreteActions[(int)SBSActions.Decision];
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
                    var position = actionBuffers.DiscreteActions[(int)SBSActions.MeeplePosition];
                    var mPlace = actionBuffers.DiscreteActions[(int)SBSActions.MeepleDiscardPlace] == 1;
                    var mDiscard = actionBuffers.DiscreteActions[(int)SBSActions.MeepleDiscardPlace] == 2;
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

                if (wrapper.GetGamePhase() ==
                    Phase.TileDown) //If the placement was successful, the phase changes to TileDown.
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
                wrapper.PlaceMeeple(
                    meepleDirection); //Either confirms and places the meeple if possible, or returns meeple and goes back to phase TileDown.

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
        
        private void WriteDiscreteActionMaskSBS(IDiscreteActionMask actionMask)
        {
            List<SBSActions> allowedActions = new List<SBSActions>();
            switch (wrapper.GetGamePhase())
            {
                case Phase.TileDrawn:
                    //AI can choose to step one tile place in either of the 4 directions (-X, X, -Z, Z), rotate 90 degrees, or confirm place.
                    allowedActions = new List<SBSActions>
                    {
                        SBSActions.TileUpDown, SBSActions.TileLeftRight, SBSActions.TileRotate, SBSActions.TilePlace
                    };

                    // Disallow moves that go far outside the bounds of the board.
                    DisallowWandering(ref actionMask);

                    break;
                case Phase.TileDown:
                    //AI can choose to take or not take a meeple.
                    allowedActions = new List<SBSActions> { SBSActions.Decision };
                    break;
                case Phase.MeepleDrawn:
                    //AI can choose to place a drawn meeple in 5 different places (N, S, W, E, C) or confirm/deny current placement.
                    allowedActions = new List<SBSActions> { SBSActions.MeeplePosition, SBSActions.MeepleDiscardPlace };
                    break;
            }

            var branchSizes = GetComponent<BehaviorParameters>().BrainParameters.ActionSpec.BranchSizes;

            //Disables all actions of branch 0, index i (on that branch) for any i larger than the allowed actions.
            foreach (SBSActions action in Enum.GetValues(typeof(SBSActions)))
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
                actionMask.SetActionEnabled((int)SBSActions.TileLeftRight, actionIndex, false);
            }
            
            // Disable up/down if it is too far from centre
            if (Math.Abs(fromCentre.y) > limits.height / 2f)
            {
                var actionIndex = fromCentre.y > 0 ? 2 : 0;
                actionMask.SetActionEnabled((int)SBSActions.TileUpDown, actionIndex, false);
            }
        }
        
        #endregion
        
        #region Board Actions
        private void BoardActions(ActionBuffers actionBuffers)
        {
            if (wrapper.state.Tiles.Current == null)
            {
                Debug.Log($"Null tile. Skipping action.");
                return;
            }
            
            var x = actionBuffers.DiscreteActions[(int)BrdActions.TileX] + m_boardLimits.xMin;
            var y = actionBuffers.DiscreteActions[(int)BrdActions.TileY] + m_boardLimits.yMin;
            var rotate = actionBuffers.DiscreteActions[(int)BrdActions.TileRotate];
            var meeplePos = actionBuffers.DiscreteActions[(int)BrdActions.MeeplePosition];

            Debug.Log($"Placing at ({x},{y}), rotation {rotate} and meeple {meeplePos}.");

            AddReward(Rewards.ActionBias); //Each call to this method comes with a very minor penalty to promote performing quick actions.

            // Tile actions
            cell = new Vector2Int(x, y);

            //Rotates the tile the amount of times AI has chosen (0-3).
            for (int i = 0; i <= rotate; i++)
            {
                wrapper.RotateTile();
            }

            if (wrapper.PlaceTile(cell)) //If the placement was successful
            {
                AddReward(Rewards.ValidAction);
            }
            else // Placement was unsuccessful. Try again.
            {
                return; // Break, don't end turn. Try again.
            }

            // If the tile placement was successful, we get one chance to place a meeple properly. 
            if (meeplePos > 0)
            {
                if (!wrapper.DrawMeeple())
                {
                    AddReward(Rewards.InvalidAction);
                }

                var meeplePlaced = false;
                switch (meeplePos)
                {
                    case 1:
                        meeplePlaced = wrapper.PlaceMeeple(Vector2Int.up);
                        break;
                    case 2:
                        meeplePlaced = wrapper.PlaceMeeple(Vector2Int.down);
                        break;
                    case 3:
                        meeplePlaced = wrapper.PlaceMeeple(Vector2Int.left);
                        break;
                    case 4:
                        meeplePlaced = wrapper.PlaceMeeple(Vector2Int.right);
                        break;
                    case 5:
                        meeplePlaced = wrapper.PlaceMeeple(Vector2Int.zero);
                        break;
                }

                if (meeplePlaced)
                {
                    AddReward(Rewards.ValidAction);
                }
                else
                {
                    AddReward(Rewards.InvalidAction);
                    wrapper.DiscardMeeple();
                }

            }

            EndOfTurnRewards();
            wrapper.EndTurn();
        }
        
        
        private void WriteDiscreteActionMaskBrd(IDiscreteActionMask actionMask)
        {
            var lim = wrapper.state.Tiles.Limits;

            Debug.Log($"Disabling on X between {m_boardLimits.xMin-m_boardLimits.xMin} and {lim.xMin-1 - m_boardLimits.xMin} and " +
                      $"{lim.xMax+1-m_boardLimits.xMin} and {m_boardLimits.xMax - m_boardLimits.xMin}");
            
            // Disable unreasonable x values
            foreach (var i in Enumerable.Range(m_boardLimits.xMin, (lim.xMin-1) - m_boardLimits.xMin))
            {
                actionMask.SetActionEnabled((int)BrdActions.TileX, i-m_boardLimits.xMin, false);
            }
            foreach (var i in Enumerable.Range(lim.xMax+2, m_boardLimits.xMax - (lim.xMax+2)))
            {
                actionMask.SetActionEnabled((int)BrdActions.TileX, i-m_boardLimits.xMin, false);
            }
            
            // Disable unreasonable y-values
            foreach (var i in Enumerable.Range(m_boardLimits.yMin, (lim.yMin-1) - m_boardLimits.yMin))
            {
                actionMask.SetActionEnabled((int)BrdActions.TileY, i-m_boardLimits.yMin, false);
            }
            foreach (var i in Enumerable.Range(lim.yMax+2, m_boardLimits.yMax - (lim.yMax+2)))
            {
                actionMask.SetActionEnabled((int)BrdActions.TileY, i-m_boardLimits.yMin, false);
            }
        }

        #endregion
        
        #region Integrated Actions

        private IEnumerator IntegratedActions(ActionBuffers actionBuffers)
        {
            var action = actionBuffers.DiscreteActions[0];
            Debug.Log($"IA {action}: Started Integrated Actions. Phase: {wrapper.state.phase}");
            yield return new WaitUntil(() => wrapper.state.phase == Phase.TileDrawn);
            Debug.Assert(wrapper.state.phase == Phase.TileDrawn, $"IA: Phase should be Phase.TileDrawn. Instead it is {wrapper.state.phase}.");
            
            var (x, y, rotate, meeple) = IndexToParameters(action);
            
            Debug.Log($"IA {action}: Placing at ({x},{y}), rotation {rotate} and meeple {Enum.GetName(typeof(MeeplePosition),meeple)}.");
            
            // Tile actions
            cell = new Vector2Int(x, y);

            //Rotates the tile the amount of times AI has chosen (0-3).
            while(wrapper.GetCurrentTileRotations() != rotate)
            {
                wrapper.RotateTile();
            }

            if (!wrapper.PlaceTile(cell)) //If the placement was successful
            {
                Debug.LogWarning($"IA {action}: Tile was supposed to be placed at {cell} but it didn't work!");
                yield break;
            }
            
            yield return new WaitUntil(() => wrapper.state.phase == Phase.TileDown);

            if(meeple != 0)
            {
                var drawnMeeple = wrapper.DrawMeeple();
                if (!drawnMeeple)
                {
                    Debug.LogWarning($"IA {action}: Meeple couldn't be drawn.");
                    yield break;
                }
                
                yield return new WaitUntil(() => wrapper.state.phase == Phase.MeepleDrawn);
                
                // If the tile placement was successful, place a meeple. 
                var meeplePlaced = true;
                switch ((MeeplePosition)meeple)
                {
                    case MeeplePosition.Up:
                        meeplePlaced = wrapper.PlaceMeeple(Vector2Int.up);
                        break;
                    case MeeplePosition.Down:
                        meeplePlaced = wrapper.PlaceMeeple(Vector2Int.down);
                        break;
                    case MeeplePosition.Left:
                        meeplePlaced = wrapper.PlaceMeeple(Vector2Int.left);
                        break;
                    case MeeplePosition.Right:
                        meeplePlaced = wrapper.PlaceMeeple(Vector2Int.right);
                        break;
                    case MeeplePosition.Centre:
                        meeplePlaced = wrapper.PlaceMeeple(Vector2Int.zero);
                        break;
                }

                if (!meeplePlaced)
                {
                    Debug.LogWarning($"IA {action}: Meeple was supposed to be placed {(MeeplePosition)meeple} but it didn't work.");
                    yield break;
                }
                
                yield return new WaitUntil(() => wrapper.state.phase == Phase.MeepleDown);
            }

            EndOfTurnRewards();
            wrapper.EndTurn();
        }

        private void WriteDiscreteActionMaskIntegrated(IDiscreteActionMask actionMask)
        {
            Debug.Log("IntegratedActions: Searching for allowed actions.");
            
            var allowedActions = new List<int>();

            var openPositions = wrapper.state.Tiles.OpenPositions();
            
            foreach (var p in openPositions)
            {
                foreach (var rotation in Enumerable.Range(0,4))
                {
                    if (wrapper.tileController.IsPlacementValid(p))
                    {
                        var index = ParametersToIndex(p.x, p.y, wrapper.GetCurrentTileRotations(), 0);
                        Debug.Log($"Found a valid placement at {p.x}, {p.y} with rotation {wrapper.GetCurrentTileRotations()} ({rotation}): index {index}.");
                        allowedActions.Add(index);
                        if (wrapper.GetMeeplesLeft() > 0)
                        {
                            Debug.Log($"{wrapper.GetMeeplesLeft()} meeples left. Finding meeple placements.");
                            foreach (var meeple in Enumerable.Range(1, 5).Cast<MeeplePosition>())
                            {
                                var valid = wrapper.meepleController.IsPlacementValid(p, meeple.Direction());
                                if (valid)
                                {
                                    index = ParametersToIndex(p.x, p.y, wrapper.GetCurrentTileRotations(), (int)meeple);
                                    Debug.Log(
                                        $"Found a valid placement at {p.x}, {p.y} with rotation {wrapper.GetCurrentTileRotations()} ({rotation}) and meeple {meeple} ({(int)meeple}): index {index}.");
                                    allowedActions.Add(index);
                                }
                            }
                        }

                    }
                    wrapper.RotateTile();
                }
            }
            
            // Set not allowed actions as disabled.
            foreach (var i in Enumerable.Range(0,boardSize * boardSize * rotationMax * meepleMax).Except(allowedActions))
            {
                actionMask.SetActionEnabled(0, i, false);
            }
            
            Debug.Log($"IntegratedActions: Found {allowedActions.Count} allowed actions of {boardSize * boardSize * rotationMax * meepleMax}");
        }
        
        private int boardSize => wrapper.GetMaxBoardSize();
        private int rotationMax = 4;
        private int meepleMax => Enum.GetValues(typeof(MeeplePosition)).Length;

        private int ParametersToIndex(int x, int y, int rotation, int meeple)
        {
            return (((x+boardSize/2) * boardSize + (y+boardSize/2)) * rotationMax + rotation) * meepleMax + meeple;
        }

        private (int, int, int, int) IndexToParameters(int index)
        {
            var meeple = index % meepleMax;
            index = (index - meeple) / meepleMax;
            var rotation = index % rotationMax;
            index = (index - rotation) / rotationMax;
            var y = index % boardSize;
            var x = (index - y) / boardSize;

            y -= boardSize / 2;
            x -= boardSize / 2;
            
            return (x, y, rotation, meeple);
        }
        #endregion
    }
}