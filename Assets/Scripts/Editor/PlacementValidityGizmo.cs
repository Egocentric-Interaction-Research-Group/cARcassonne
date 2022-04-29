#if UNITY_EDITOR
using System;
using Carcassonne.Models;
using UnityEditor;
using UnityEngine;

namespace Editor
{
    public class PlacementValidityGizmo : MonoBehaviour
    {
    
        [Flags]
        public enum ValidityType
        {
            Valid,
            Occupied,
            Detached,
            MismatchNorth,
            MismatchSouth,
            MismatchEast,
            MismatchWest
        }

        public ValidityType PlacementValidity;
        
        

        [DrawGizmo(GizmoType.Selected)]
        static void DrawValidity(Tile tile, GizmoType gizmoType)
        {
            
        }
    }
}
#endif