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

        private int rotation
        {
            get { return position.rotation; }
            set { position.rotation = value; }
        }

        public RaycastHit raycast;
        public LayerMask tableLayerMask;

        public bool IsActive;

        private void Start()
        {
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
            
            position.MoveToRPC(cell);
            // position.RotateToRPC(rotation);
            position.OnPlace.Invoke(cell, rotation);
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
                position.OnChangeCell.Invoke(cell);
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
                position.OnChangeRotation.Invoke(rotation);
            }
        }
    }
}