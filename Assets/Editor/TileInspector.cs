using System;
using System.Collections.Generic;
using Carcassonne.Models;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.UIElements;

// [CustomEditor(typeof(Tile))]
public class TileInspector : UnityEditor.Editor
{
    // public VisualTreeAsset inspectorXML;
    //
    // public override VisualElement CreateInspectorGUI()
    // {
    //     
    //     // Create a new VisualElement to be the root of our inspector UI
    //     VisualElement myInspector = new VisualElement();
    //
    //     // Add a simple label
    //     myInspector.Add(new Label("This is a custom inspector"));
    //     
    //     // Get a reference to the default inspector foldout control
    //     // VisualElement inspectorFoldout = myInspector.Q("Default_Inspector");
    //     //
    //     // // Attach a default inspector to the foldout
    //     // InspectorElement.FillDefaultInspector(inspectorFoldout, serializedObject, this);
    //     
    //     // Return the finished inspector UI
    //     return myInspector;
    // }

    public override void OnInspectorGUI()
    {
        Tile tile = (Tile ) target;

        try
        {
            EditorGUILayout.LabelField("North: ", tile.North.ToString());
        }
        catch (KeyNotFoundException e) { }
        
        EditorGUILayout.LabelField("Test Label");
        
        
        DrawDefaultInspector();
    }
}