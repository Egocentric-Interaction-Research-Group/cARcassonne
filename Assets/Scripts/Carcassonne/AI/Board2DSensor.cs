using System;
using System.Linq;
using Carcassonne.Models;
using Carcassonne.State;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Carcassonne.AI
{
    public class Board2DSensor : Carcassonne2DSensorBase
    {
        public Board2DSensor(GameState mState, string name) : base(mState, name)
        {
            m_Height = GameRules.BoardLimits.height*3;
            m_Width = GameRules.BoardLimits.width*3;
            m_Channels = 5;
        }

        public override int Write(ObservationWriter writer)
        {
            var offset = 0;

            var tiles = m_State.Tiles.Matrix;
            var shields = m_State.Tiles.ShieldMatrix;
            var meeples = m_State.Meeples.Matrix;
            var maxPlayerID = (float)m_State.Players.All.Select(p => p.id).Max()+1;
            
            Debug.Assert(maxPlayerID > 0.0f, $"maxPlayerID should be a positive integer (before its conversion to float) but is {maxPlayerID}.");

            Debug.Assert(tiles.Length == shields.Length && shields.Length == meeples.Length,
                $"The lengths of the tile ({tiles.Length}), shield ({shields.Length}), and meeple ({meeples.Length})" +
                $" matrices must be the same.");
            
            // for(int i=0; i < m_Width; i++)
            // {
            //     for (int j = 0; j < m_Height; j++)
            //     {
            //         for (int k = 0; k < m_Channels; k++)
            //             writer[i, j, k] = 0.0f;
            //     }
            // }

            Debug.Log($"Observation dimensions: {tiles.GetUpperBound(0)}x{tiles.GetUpperBound(1)}" +
                      $" vs {m_Width}x{m_Height} tiles.");

            for (int i = 0; i <= tiles.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= tiles.GetUpperBound((1)); j++)
                {
                    var city = tiles[i, j].HasValue && ((Geography)tiles[i, j]).HasCity() ? 1.0f : 0.0f;
                    var road = tiles[i, j].HasValue && ((Geography)tiles[i, j]).HasRoad() ? 1.0f : 0.0f;
                    var cloister = tiles[i, j].HasValue && ((Geography)tiles[i, j]) == Geography.Cloister ? 1.0f : 0.0f;
                    var shield = shields[i, j] ? 1.0f : 0.0f;
                    var meeple = meeples[i, j] / maxPlayerID;
                    
                    // Debug.Log($"({i},{j}): {city}, {road}, {cloister}, {shield}, {meeple}");
                    
                    writer[i, j, 0] = city;
                    
                    writer[i, j, 1] = road;
                    writer[i, j, 2] = cloister;
                    writer[i, j, 3] = shield;
                    writer[i, j, 4] = meeple;

                    offset += m_Channels;
                }
            }
            
            Debug.Log($"Board2DSensor: Recorded {offset} measurements to a {m_Height}x{m_Width}x{m_Channels} tensor.");
            
            return offset;
        }
    }
}