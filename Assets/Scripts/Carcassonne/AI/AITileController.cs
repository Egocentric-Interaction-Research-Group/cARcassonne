using Carcassonne.Models;
using UnityEngine;

namespace Carcassonne.AI
{
    public class AITileController : MonoBehaviour
    {
        public Tile current;
        public Vector2Int cell;
        
        public void Draw(){}
        
        public void Rotate(){}

        public void MoveTo(Vector2Int cell)
        {
            this.cell = cell;
            
            //UpdateAIBoundary??
            
            
        }

        public void PlaceTile()
        {
            
        }
    }
}