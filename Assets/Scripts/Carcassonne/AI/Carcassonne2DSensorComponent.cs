using System;
using Carcassonne.State;
using Unity.MLAgents.Sensors;
using UnityEngine;
using UnityEngine.Serialization;

namespace Carcassonne.AI
{
    public class Carcassonne2DSensorComponent : SensorComponent, IDisposable
    {
        [HideInInspector, SerializeField]
        string m_SensorName = "Board2D Sensor";

        public GameState state;

        /// <summary>
        /// Name of the generated Board2DSensor object.
        /// Note that changing this at runtime does not affect how the Agent sorts the sensors.
        /// </summary>
        public string SensorName
        {
            get => m_SensorName;
            set => m_SensorName = value;
        }

        private ISensor[] m_Sensors;

        /// <inheritdoc/>
        public override ISensor[] CreateSensors()
        {
            // Clean up any existing sensors
            Dispose();

            state = GetComponentInParent<GameState>();;
            Debug.Assert(state != null, $"State should not be null.");

            var board2DSensor = new Board2DSensor(state, m_SensorName + " (board)");
            var tile2DSensor = new CurrentTile2DSensor(state, m_SensorName + " (tile)");
            m_Sensors = new ISensor[] { board2DSensor, tile2DSensor };
            // m_Sensors = new ISensor[] { board2DSensor };

            Debug.Log($"Created {m_Sensors.Length} new sensors.");
            
            return m_Sensors;
        }

        /// <summary>
        /// Clean up the sensors created by CreateSensors().
        /// </summary>
        public void Dispose()
        {
            if (m_Sensors != null)
            {
                for (var i = 0; i < m_Sensors.Length; i++)
                {
                    ((Carcassonne2DSensorBase)m_Sensors[i]).Dispose();
                }

                m_Sensors = null;
            }
        }
    }
}