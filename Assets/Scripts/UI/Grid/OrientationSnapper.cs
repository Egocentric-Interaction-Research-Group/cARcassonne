using Carcassonne.Models;
using Microsoft.MixedReality.Toolkit.UI;
using Photon.Pun;

namespace UI.Grid
{
    public class OrientationSnapper : MonoBehaviourPun
    {
        private GridOrientation orientation => GetComponent<GridOrientation>();
        public ObjectManipulator manipulator;
        public Tile tile => GetComponent<Tile>();

        private int direction
        {
            get { return orientation.direction; }
            // set { position.orientation = value; }
        }
        
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
        }
        
        private void Update()
        {
            if (IsActive)
            {
                // Update Rotation Snap
                UpdateOrientation();
            }
        }
        
        private void StartProjection(ManipulationEventData eventData)
        {
            IsActive = true;
        }

        private void StopProjection(ManipulationEventData eventData)
        {
            IsActive = false;

            UpdateOrientation();
            
            orientation.OrientToRPC(direction);
        }

        /// <summary>
        /// Update the snapped orientation of a tile as the tile is being manipulated.
        /// </summary>
        private void UpdateOrientation()
        {
            var oldDirection = direction; // Log the old orientation
            
            // Check the current orientation snap
            var angles = transform.eulerAngles;
            var o = (int)(angles.y) / 90;
            if (angles.y % 90 > 45)
                o += 1;
            o = o % 4;
            
            // Update the tile's internal orientation state without moving the rotation of the GameObject.
            // This is so that the answers about the validity of the placement are correct.
            tile.RotateTo(o);

            if (o != oldDirection)
            {
                orientation.OnChangeOrientation.Invoke(direction);
            }
        }
    }
}