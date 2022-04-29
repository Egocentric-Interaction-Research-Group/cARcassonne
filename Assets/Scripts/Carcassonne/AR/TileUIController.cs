using System.Diagnostics;
using Carcassonne.Controllers;
using Carcassonne.Models;
using Microsoft.MixedReality.Toolkit.UI;
using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;
using Debug = UnityEngine.Debug;

namespace Carcassonne.AR
{
    /// <summary>
    /// Handles UI considerations for Tiles. Component goes on each tile.
    /// </summary>
    [RequireComponent(typeof(Tile))]
    public class TileUIController : MonoBehaviourPun
    {
        public UnityEvent OnTileActivation = new UnityEvent();
        public UnityEvent OnTileLock = new UnityEvent();

        internal GameControllerScript gameController;
        public GameObject tileSpawnPosition;

        private void Start()
        {
            gameController = GetComponentInParent<GameControllerScript>();

            var tileController = GetComponent<TileController>();
            if (tileController)
            {
                tileController.OnDraw.AddListener(ActivateRPC);
                tileController.OnPlace.AddListener(PlaceRPC);
            }
            
        }

        [PunRPC]
        public void Activate()
        {
            Debug.Log((new StackTrace()).GetFrame(1).GetMethod().Name);
            Debug.Log("Tile Activated.");
            
            transform.parent = gameController.table.transform;
            transform.rotation = gameController.table.transform.rotation;
            transform.position = tileSpawnPosition.transform.position;
            
            GetComponentInChildren<MeshRenderer>().enabled = true;
            GetComponentInChildren<Collider>().enabled = true;
            GetComponentInChildren<Rigidbody>().useGravity = true;
            GetComponentInChildren<BoxCollider>().enabled = true;
            
            //TODO Do this in a UI script.
            // gameControllerScript.smokeEffect.Play();
        }

        public void Lock()
        {
            GetComponent<BoxCollider>().enabled = false;
            GetComponent<Rigidbody>().useGravity = false;
            GetComponent<ObjectManipulator>().enabled = false;
            GetComponent<Rigidbody>().isKinematic = true;
        }

        public void ActivateRPC(Tile t)
        {
            Debug.Log((new StackTrace()).GetFrame(1).GetMethod().Name);
            ActivateRPC();
        }
        public void ActivateRPC()
        {
            Debug.Log((new StackTrace()).GetFrame(1).GetMethod().Name);
            photonView.RPC("Activate", RpcTarget.All);
            
            // Set ownership
            photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
        }

        public void PlaceRPC(Tile t, Vector2Int cell)
        {
            //TODO How does this know which tile to lock?
            photonView.RPC("Lock", RpcTarget.All);
        }
        
    }
}