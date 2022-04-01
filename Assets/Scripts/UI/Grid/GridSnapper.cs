using Microsoft.MixedReality.Toolkit.UI;
using Photon.Pun;
using UnityEngine;

namespace UI.Grid
{
    /// <summary>
    /// Snaps a gameObject to a grid location on drop and tracks its location during movement.
    /// </summary>
    public class GridSnapper : MonoBehaviourPun
    {
        private GridPosition position => GetComponent<GridPosition>();
        private UnityEngine.Grid grid => position.grid;
        public ObjectManipulator manipulator;

        private Vector2Int cell
        {
            get { return position.cell; }
            set { position.cell = value; }
        }

        public RaycastHit raycast;
        public LayerMask tableLayerMask;

        public bool IsActive;

        private void Start()
        {
            if (manipulator == null)
            {
                manipulator = GetComponent<ObjectManipulator>();
            }
            
            manipulator.OnManipulationStarted.AddListener(StartProjection);
            manipulator.OnManipulationEnded.AddListener(StopProjection);

            IsActive = false;

            Physics.Raycast(transform.position, Vector3.down, out raycast);
        }

        private void Update()
        {
            if (IsActive)
            {
                Physics.Raycast(transform.position, Vector3.down, out raycast, tableLayerMask);

                // Update Cell Snap
                UpdateCell();
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
            
            position.MoveToRPC(cell);
            position.OnPlace.Invoke(cell);
        }

        /// <summary>
        /// Update the snapped cell position of a tile as the tile is being manipulated. 
        /// </summary>
        private void UpdateCell()
        {
            var oldCell = cell;
            var cell3d = grid.WorldToCell(raycast.point);
            cell = new Vector2Int(cell3d.x, cell3d.y);

            if (cell != oldCell)
            {
                position.OnChangeCell.Invoke(cell);
            }
        }
    }
}