using System;
using System.Linq;
using Carcassonne.State.Features;
using Unity.MLAgents;
using UnityEngine;

namespace Carcassonne.AI
{
    
    public class EventScorer : MonoBehaviour
    {
        public CarcassonneAgent agent;
        
        [HideInInspector]
        public float OwnCompletedFeatureMultiplier;
        [HideInInspector]
        public float UnownedCompletedFeatureMultiplier;
        [HideInInspector]
        public float OtherCompletedFeatureMultiplier;
        
        [HideInInspector]
        public float OwnCompletedFeatureScore;
        [HideInInspector]
        public float UnownedCompletedFeatureScore;
        [HideInInspector]
        public float OtherCompletedFeatureScore;
        
        public void Start()
        {
            OwnCompletedFeatureMultiplier = Academy.Instance.EnvironmentParameters.
                GetWithDefault("OwnCompletedFeatureMultiplier",0.0f);
            UnownedCompletedFeatureMultiplier = Academy.Instance.EnvironmentParameters.
                GetWithDefault("UnownedCompletedFeatureMultiplier",0.0f);
            OtherCompletedFeatureMultiplier = Academy.Instance.EnvironmentParameters.
                GetWithDefault("OtherCompletedFeatureMultiplier",0.0f);
            OwnCompletedFeatureScore = Academy.Instance.EnvironmentParameters.
                GetWithDefault("OwnCompletedFeatureScore",0.0f);
            UnownedCompletedFeatureScore = Academy.Instance.EnvironmentParameters.
                GetWithDefault("UnownedCompletedFeatureScore",0.0f);
            OtherCompletedFeatureScore = Academy.Instance.EnvironmentParameters.
                GetWithDefault("OtherCompletedFeatureScore",0.0f);
        }

        public void ScoreCompletedFeature(FeatureGraph g)
        {
            if (agent.wrapper.IsAITurn())
            {
                if (!g.HasMeeples)
                {
                    agent.AddReward(g.Points * UnownedCompletedFeatureMultiplier);
                    agent.AddReward(UnownedCompletedFeatureScore);
                } else if (g.ScoresPoints(agent.wrapper.player))
                {
                    agent.AddReward(g.Points * OwnCompletedFeatureMultiplier);
                    agent.AddReward(OwnCompletedFeatureScore);
                } else
                {
                    agent.AddReward(g.Points * OtherCompletedFeatureMultiplier);
                    agent.AddReward(OtherCompletedFeatureScore);
                }
            }
        }
    }
}