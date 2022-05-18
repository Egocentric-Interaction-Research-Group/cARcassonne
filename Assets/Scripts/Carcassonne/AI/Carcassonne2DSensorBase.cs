using System;
using Carcassonne.State;
using Unity.MLAgents.Sensors;

namespace Carcassonne.AI
{
    public abstract class Carcassonne2DSensorBase : ISensor, IDisposable
    {
        protected string m_Name;
        protected int m_Height;
        protected int m_Width;
        protected int m_Channels;
        protected GameState m_State;

        public Carcassonne2DSensorBase(GameState mState, string name)
        {
            m_Name = name;
            m_State = mState;
        }

        public ObservationSpec GetObservationSpec()
        {
            return ObservationSpec.Visual(m_Height, m_Width, m_Channels);
        }

        public abstract int Write(ObservationWriter writer);

        public byte[] GetCompressedObservation()
        {
            throw new NotImplementedException();
            //     var offset = 0;
            //
            //     var tiles = state.Tiles.Matrix;
            //     var shields = state.Tiles.ShieldMatrix;
            //     var meeples = state.Meeples.Matrix;
            //     var maxPlayerID = (float)state.Players.All.Select(p => p.id).Max();
            //
            //     Debug.Assert(tiles.Length == shields.Length && shields.Length == meeples.Length,
            //         $"The lengths of the tile ({tiles.Length}), shield ({shields.Length}), and meeple ({meeples.Length})" +
            //         $" matrices must be the same.");
            //
            //     for (int i = 0; i < tiles.GetUpperBound(0); i++)
            //     {
            //         for (int j = 0; j < tiles.GetUpperBound((1)); j++)
            //         {
            //             writer[i, j, 0] = tiles[i, j].HasValue && ((Geography)tiles[i, j]).HasCity();
            //             writer[i, j, 1] = tiles[i, j].HasValue && ((Geography)tiles[i, j]).HasRoad();
            //             writer[i, j, 2] = tiles[i, j].HasValue && ((Geography)tiles[i, j]) == Geography.Cloister;
            //             writer[i, j, 3] = shields[i, j];
            //             writer[i, j, 4] = (float)meeples[i, j] / maxPlayerID;
            //
            //             offset += 5;
            //         }
            //     }
            //     
            //     return ImageConversion.EncodeArrayToPNG();
        }

        public void Update()
        {
        }

        public void Reset()
        {
        }

        public CompressionSpec GetCompressionSpec()
        {
            // return new CompressionSpec(SensorCompressionType.PNG);
            return new CompressionSpec(SensorCompressionType.None);
        }

        public string GetName()
        {
            return m_Name;
        }

        public void Dispose()
        {
        }
    }
}