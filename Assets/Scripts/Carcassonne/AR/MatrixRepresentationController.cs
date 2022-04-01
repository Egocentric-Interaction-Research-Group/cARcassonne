using System;
using System.IO;
using System.Text;
using Carcassonne.State;
using Newtonsoft.Json;
using Photon.Pun;
//using UnityEditor.Scripting.Python;
using UnityEngine;

namespace Carcassonne.AR
{
    public class MatrixRepresentationController : MonoBehaviourPun
    {
        private GameControllerScript GameController => GetComponent<GameControllerScript>();
        private GameState state => GetComponent<GameState>();

        private DateTimeOffset currentTime = DateTimeOffset.Now;
        private string JsonBoundingBox;
        public StringBuilder sb;
        public StringWriter sw;
        public JsonWriter writer;

        public void Start()
        {
            sb = new StringBuilder();
            sw = new StringWriter(sb);
            writer = new JsonTextWriter(sw);
            writer.WriteStartObject();
            writer.WritePropertyName("bbox");
            writer.WriteStartArray();
        }

        public void OnApplicationQuit()
        {
            writer.WriteEndArray();
            writer.WriteEndObject();
            JsonBoundingBox = sb.ToString();
            File.WriteAllText("Assets/PythonImageGenerator/TxtFiles/"+"Output" + currentTime.ToUnixTimeMilliseconds() + ".txt", state.Tiles.ToString());
            File.WriteAllText("Assets/PythonImageGenerator/TxtFiles/"+"Output" + currentTime.ToUnixTimeMilliseconds() + ".json", JsonBoundingBox);

            
            RunPythonImageGenerator();

        }

        public void RunPythonImageGenerator()
        {
            Debug.Log("Running Python File");
            //PythonRunner.RunFile($"{Application.dataPath}/PythonImageGenerator/MatrixToGreyImage.py");
        }
    }
}