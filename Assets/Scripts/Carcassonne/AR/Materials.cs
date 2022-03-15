using UnityEngine;

namespace Carcassonne.AR
{
    [CreateAssetMenu(fileName = "Materials", menuName = "Carcassonne/AR/Materials")]
    public class Materials : ScriptableObject
    {
        public Material[] playerMaterials;
        public Material[] buttonMaterials;
    }
}