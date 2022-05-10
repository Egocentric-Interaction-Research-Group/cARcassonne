using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Carcassonne.Controllers
{
    public abstract class GamePieceController<T> : MonoBehaviour
    {
        public abstract bool Draw();

        /// <summary>
        /// Thin wrapper around draw because Unity Events can't have return types.
        /// </summary>
        public void DoDraw()
        {
            Draw();
        }
        
        public void Rotate(){}
        
        public abstract bool Place(Vector2Int cell);

        public abstract void Discard();

        public void Free(){}

        public abstract bool IsPlacementValid(Vector2Int cell);

        public abstract bool CanBePlaced();
        
        
        [Tooltip("OnDraw")]
        public UnityEvent<T> OnDraw = new UnityEvent<T>();
        
        [Tooltip("OnInvalidDraw: This is invalid.")]
        public UnityEvent OnInvalidDraw = new UnityEvent();
        public UnityEvent<T, int> OnRotate = new UnityEvent<T, int>();
        public UnityEvent<T> OnDiscard = new UnityEvent<T>();
        public UnityEvent<T> OnFree = new UnityEvent<T>();
        public UnityEvent<T, Vector2Int> OnPlace = new UnityEvent<T, Vector2Int>();
        public UnityEvent<T, Vector2Int> OnInvalidPlace = new UnityEvent<T, Vector2Int>();
        
        public List<UnityEventBase> Events => new List<UnityEventBase>()
        {
            OnDraw,
            OnInvalidDraw,
            OnRotate,
            OnDiscard,
            OnFree,
            OnPlace,
            OnInvalidPlace
        };
    }
}