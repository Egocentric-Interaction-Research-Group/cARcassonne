using System;
using System.Linq;
using Carcassonne.Models;
using Carcassonne.State;
using Unity.MLAgents.Sensors;
using UnityEngine;

namespace Carcassonne.AI
{
    public class CurrentTile2DSensor : Carcassonne2DSensorBase
    {
        public CurrentTile2DSensor(GameState mState, string name) : base(mState, name)
        {
            // The minimum size is 20x20 for visual observations
            m_Height = 20; 
            m_Width = 20;
            m_Channels = 4;
        }

        public override int Write(ObservationWriter writer)
        {
            var offset = 0;

            var tiles = m_State.Tiles.Current.Matrix;
            var shield = m_State.Tiles.Current.Shield;
            
            for(int i=0; i < m_Width; i++)
            {
                for (int j = 0; j < m_Height; j++)
                {
                    for (int k = 0; k < m_Channels; k++)
                        writer[i, j, k] = 0.0f;
                }
            }
            
            // The minimum size is 20x20 for visual observations, so centre the observation here.
            for (int i = 0; i <= tiles.GetUpperBound(0); i++)
            {
                for (int j = 0; j <= tiles.GetUpperBound(1); j++)
                {
                    writer[i+9, j+9, 0] = tiles[i, j].HasCity() ? 1.0f : 0.0f;
                    writer[i+9, j+9, 1] = tiles[i, j].HasRoad() ? 1.0f : 0.0f;
                    writer[i+9, j+9, 2] = tiles[i, j] == Geography.Cloister ? 1.0f : 0.0f;
                    writer[i+9, j+9, 3] = shield && tiles[i, j].HasCity() ? 1.0f : 0.0f;

                    // Turns shield False if it has been assigned (shield && city). So that there will only be one shield on the tile.
                    shield = (shield ^ tiles[i, j].HasCity()) && shield;

                    offset += m_Channels;
                }
            }
            
            Debug.Log($"Board2DSensor: Recorded {offset} measurements to a {m_Height}x{m_Width}x{m_Channels} tensor.");
            
            offset = m_Height * m_Width * m_Channels;
            
            return offset;
        }
    }
}