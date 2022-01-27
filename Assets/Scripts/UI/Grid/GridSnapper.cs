using Microsoft.MixedReality.Toolkit.UI;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Rendering.Universal;

namespace UI.Grid
{
    
    public class RotationEvent: UnityEvent<int>{}
    public class CellEvent: UnityEvent<Vector2Int>{}
    public class PlaceEvent: UnityEvent<Vector2Int, int>{}
    
    /// <summary>
    /// Snaps a gameObject to a grid location on drop and tracks its location during movement.
    /// </summary>
    public class GridSnapper : MonoBehaviourPun
    {
            public UnityEngine.Grid grid;
            public ObjectManipulator manipulator;
            
            public Vector2Int cell;
            public int rotation;
            
            public RaycastHit raycast;
            public LayerMask tableLayerMask;

            public bool IsActive;

            #region Events

            [SerializeField]
            public UnityEvent<Vector2Int> OnChangeCell = new CellEvent();
            [SerializeField]
            public UnityEvent<int> OnChangeRotation = new RotationEvent();
            [SerializeField]
            public UnityEvent<Vector2Int,int> OnPlace = new PlaceEvent();

            #endregion

            private void Start()
            {
                manipulator.OnManipulationStarted.AddListener(StartProjection);
                manipulator.OnManipulationEnded.AddListener(StopProjection);

                IsActive = false;

                Physics.Raycast(transform.position, Vector3.down, out raycast);
            }

            private void Update()
            {
                if(IsActive){
                    Physics.Raycast(transform.position, Vector3.down, out raycast, tableLayerMask);
                    
                    // Update Cell Snap
                    UpdateCell();
                    
                    // Update Rotation Snap
                    UpdateRotation();
                }
            }

            private void StartProjection(ManipulationEventData eventData)
            {
                IsActive = true;
            }

            private void StopProjection(ManipulationEventData eventData)
            {
                IsActive = false;
                
                UpdateCell();
                UpdateRotation();
                
                OnPlace.Invoke(cell, rotation);
            }
    
            /// <summary>
            /// Update the cell snapping of a tile
            /// </summary>
            private void UpdateCell()
            {
                var oldCell = cell;
                var cell3d = grid.WorldToCell(raycast.point);
                cell = new Vector2Int(cell3d.x, cell3d.y);

                if (cell != oldCell)
                {
                    OnChangeCell.Invoke(cell);
                }

            }

            private void UpdateRotation()
            {
                var oldRotation = rotation;
                var angles = transform.localEulerAngles;
                
                rotation = (int)(angles.y) / 90;
                if (angles.y % 90 > 45)
                    rotation += 1;
                rotation = (rotation % 4) * 90;

                if (rotation != oldRotation)
                {
                    OnChangeRotation.Invoke(rotation);
                }
            }
    }
}