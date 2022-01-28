using System;
using Microsoft.MixedReality.Toolkit.UI;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Tilemaps;

namespace UI.Grid
{
    /// <summary>
    /// Maps a manipulated object to a tilemap grid below.
    ///
    /// THIS CLASS IS NOT USED RIGHT NOW.
    /// </summary>
    public class TilemapProjection : MonoBehaviourPun
    {
        public Tilemap tilemap;
        public Tile tile;
        public ObjectManipulator manipulator;
        public Vector3 tileScale;

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
            if(IsActive){
                var oldCell = tilemap.WorldToCell(raycast.point);
                tilemap.SetTile(oldCell, null);
                
                Physics.Raycast(transform.position, Vector3.down, out raycast, tableLayerMask);
                
                var newCell = tilemap.WorldToCell(raycast.point);
                tilemap.SetTile(newCell, tile);
                
                tile.transform = Matrix4x4.Scale(tileScale);
            }
        }

        private void StartProjection(ManipulationEventData eventData)
        {
            IsActive = true;
        }

        private void StopProjection(ManipulationEventData eventData)
        {
            IsActive = false;
            
            tilemap.ClearAllTiles();
        }

    }
}