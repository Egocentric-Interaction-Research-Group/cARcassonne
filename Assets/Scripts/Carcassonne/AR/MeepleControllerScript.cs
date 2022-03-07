using System;
using System.Collections.Generic;
using System.Linq;
using Carcassonne.AR.Buttons;
using Carcassonne.Meeples;
using Carcassonne.Models;
using Carcassonne.State;
using Carcassonne.Tiles;
using Carcassonne.Controllers;
using Microsoft.MixedReality.Toolkit.UI;
using Photon.Pun;
using PunTabletop;
using UI.Grid;
using UnityEngine;
using UnityEngine.Events;

namespace Carcassonne.AR
{
    [RequireComponent(typeof(GameControllerScript),typeof(GameState))]
    /// <summary>
    /// Controller script for meeples. It handles everything with 
    /// regards to meeple control from drawing to placement
    /// </summary>
    public class MeepleControllerScript : MonoBehaviourPun
    {
        private static int startingPosition = -10; 
        
        public Grid meepleGrid;
        public ConfirmButton confirmButton;
        
        private int MeepleCount = 0;
        
        private GameState state => GetComponent<GameState>();
        private GameControllerScript gameControllerScript => GetComponent<GameControllerScript>();
        
        public UnityEvent<Vector2Int> OnValidAim = new UnityEvent<Vector2Int>();
        public UnityEvent<Vector2Int> OnInvalidAim = new UnityEvent<Vector2Int>();
        
        // [HideInInspector] public List<MeepleScript> MeeplesInCity;
        //
        // #region Aim
        //
        // private Vector2Int iMeepleAim => new Vector2Int(iMeepleAimX, iMeepleAimZ);
        //
        // internal int iMeepleAimX;
        // internal int iMeepleAimZ;
        //
        // internal float fMeepleAimX
        // {
        //     get { return fMeepleAim.x; }
        //     set { fMeepleAim.x = value;  }
        // }
        // internal float fMeepleAimZ
        // {
        //     get { return fMeepleAim.y; }
        //     set { fMeepleAim.y = value;  }
        // }
        // public float aiMeepleX
        // {
        //     get { return aiMeeple.x; }
        //     set { fMeepleAim.x = value;  }
        // }
        // public float aiMeepleZ
        // {
        //     get { return aiMeeple.y; }
        //     set { fMeepleAim.y = value;  }
        // }
        //
        // internal Vector2 fMeepleAim;
        // public Vector2 aiMeeple;
        //
        // public Geography meepleGeography;
        // public RaycastHit meepleHitTileDirection;
        //
        // Transform basePositionTransform;
        //
        // private void Start()
        // {
        //     basePositionTransform = GameObject.Find("BaseSpawnPosition").transform;
        // }
        //
        // /// <summary>
        // /// Determines which tile space the currently moving Meeple is over.
        // /// </summary>
        // internal void CurrentMeepleRayCast() //TODO Should be private
        // {
        //     RaycastHit hit;
        //     var layerMask = 1 << 8;
        //
        //     Physics.Raycast(meeples.Current.gameObject.transform.position, meeples.Current.gameObject.transform.TransformDirection(Vector3.down),
        //         out hit, Mathf.Infinity, layerMask);
        //
        //     var local = gameControllerScript.table.transform.InverseTransformPoint(hit.point);
        //
        //
        //     fMeepleAimX = local.x;
        //     fMeepleAimZ = local.z;
        //
        //
        //     if (fMeepleAimX - basePositionTransform.localPosition.x > 0)
        //     {
        //         iMeepleAimX = (int) ((fMeepleAimX - basePositionTransform.localPosition.x) * gameControllerScript.scale + 1f) / 2 +
        //                       GameRules.BoardSize / 2;
        //     }
        //     else
        //     {
        //         iMeepleAimX = (int) ((fMeepleAimX - basePositionTransform.localPosition.x) * gameControllerScript.scale - 1f) / 2 +
        //                       GameRules.BoardSize / 2;
        //     }
        //
        //     if (fMeepleAimZ - basePositionTransform.localPosition.z > 0)
        //     {
        //         iMeepleAimZ = (int) ((fMeepleAimZ - basePositionTransform.localPosition.z) * gameControllerScript.scale + 1f) / 2 +
        //                       GameRules.BoardSize / 2;
        //     }
        //     else
        //     {
        //         iMeepleAimZ = (int) ((fMeepleAimZ - basePositionTransform.localPosition.z) * gameControllerScript.scale - 1f) / 2 +
        //                       GameRules.BoardSize / 2;
        //     }
        // }
        //
        // public void AimMeeple()
        // {
        //     try
        //     {
        //         if (state.Tiles.Placement[iMeepleAim] == state.Meeples.Current.gameObject)
        //         {
        //             var tile = state.Tiles.Placement[iMeepleAim];
        //             var tileScript = tile.GetComponent<TileScript>();
        //
        //             var layerMask = 1 << 9;
        //             Physics.Raycast(meeples.Current.gameObject.transform.position, meeples.Current.gameObject.transform.TransformDirection(Vector3.down), out this.meepleHitTileDirection,
        //                 Mathf.Infinity, layerMask);
        //
        //             this.meepleGeography = Geography.Field;
        //             gameControllerScript.Direction = Vector2Int.zero;
        //
        //             if (this.meepleHitTileDirection.collider != null)
        //             {
        //                 if (this.meepleHitTileDirection.collider.name == "East")
        //                 {
        //                     gameControllerScript.Direction = Vector2Int.right;
        //                     this.meepleGeography = tileScript.tile.East;
        //                 }
        //                 else if (this.meepleHitTileDirection.collider.name == "West")
        //                 {
        //                     gameControllerScript.Direction = Vector2Int.left;
        //                     this.meepleGeography = tileScript.tile.West;
        //                 }
        //                 else if (this.meepleHitTileDirection.collider.name == "North")
        //                 {
        //                     gameControllerScript.Direction = Vector2Int.up;
        //                     this.meepleGeography = tileScript.tile.North;
        //                 }
        //                 else if (this.meepleHitTileDirection.collider.name == "South")
        //                 {
        //                     gameControllerScript.Direction = Vector2Int.down;
        //                     this.meepleGeography = tileScript.tile.South;
        //                 }
        //                 else if (this.meepleHitTileDirection.collider.name == "Center")
        //                 {
        //                     gameControllerScript.Direction = Vector2Int.zero;
        //                     this.meepleGeography = tileScript.tile.Center;
        //                 }
        //
        //                 gameControllerScript.SnapPosition = this.meepleHitTileDirection.collider.transform.position;
        //                 
        //                 if (this.meepleGeography.IsFeature())
        //                 {
        //                     // gameControllerScript.ChangeConfirmButtonApperance(true);
        //                     gameControllerScript.CanConfirm = true;
        //                 }
        //             }
        //             else
        //             {
        //                 gameControllerScript.SnapPosition = meeples.Current.gameObject.transform.position;
        //                 // gameControllerScript.ChangeConfirmButtonApperance(false);
        //                 gameControllerScript.CanConfirm = false;
        //             }
        //         }
        //         else
        //         {
        //             gameControllerScript.SnapPosition = meeples.Current.gameObject.transform.position;
        //             this.meepleGeography = Geography.Field;
        //             // gameControllerScript.ChangeConfirmButtonApperance(false);
        //             gameControllerScript.CanConfirm = false;
        //         }
        //     }
        //     catch (KeyNotFoundException e)
        //     {
        //         Debug.Log(e);
        //         gameControllerScript.ErrorOutput = e.ToString();
        //     }
        // }
        //
        // #endregion

        private MeepleState meeples => state.Meeples;
        private PlayerState players => state.Players;

        // Instantiation Stuff
        public GameObject prefab;
        public GameObject parent;

        #region Photon
        
        /// <summary>
        /// Instantiate a new Meeple with the chosen prefab and parent object in the hierarchy.
        /// </summary>
        /// <returns>GameObject : An instance of MeepleScript.prefab.</returns>
        public MeepleScript GetNewInstance()
        {
            // return Instantiate(prefab, meepleSpawnPosition.transform.position, Quaternion.identity, GameObject.Find("Table").transform).GetComponent<MeepleScript>();
            GameObject newMeeple = PhotonNetwork.Instantiate(prefab.name, parent.transform.position, Quaternion.identity);//, GameObject.Find("Table").transform);
            newMeeple.gameObject.transform.parent = parent.transform;
            newMeeple.gameObject.name = $"Meeple {MeepleCount}";
            
            var gridPosition = newMeeple.GetComponent<GridPosition>(); 
            gridPosition.grid = meepleGrid;
            gridPosition.OnChangeCell.AddListener(i => confirmButton.OnMeepleChange());

            newMeeple.GetComponent<TableBoundaryEnforcerScript>().spawnPos = GameObject.Find("MeepleDrawPosition");
            
            newMeeple.SetActive(false);
            MeepleCount++;

            return newMeeple.GetComponent<MeepleScript>();
        }

        #endregion
    
        // public ParticleSystem drawMeepleEffect;
        // [HideInInspector] public GameObject meepleMesh;
        // [HideInInspector] public GameObject MeeplePrefab;
        public GameObject meepleSpawnPosition;
        

        private static void Enable(Meeple meeple)
        {
            var meepleGameObject = meeple.gameObject;
            meepleGameObject.SetActive(true);
            meepleGameObject.GetComponent<Rigidbody>().useGravity = true;
            meepleGameObject.GetComponent<BoxCollider>().enabled = true;
            meepleGameObject.GetComponent<ObjectManipulator>().enabled = true;
            meepleGameObject.GetComponentInChildren<MeshRenderer>().enabled = true;
        }

        private static void Fix(Meeple meeple)
        {
            var meepleGameObject = meeple.gameObject;
            meepleGameObject.GetComponent<Rigidbody>().useGravity = false;
            meepleGameObject.GetComponent<BoxCollider>().enabled = false;
            meepleGameObject.GetComponent<ObjectManipulator>().enabled = false;
            meepleGameObject.GetComponentInChildren<MeshRenderer>().enabled = true;
        }

        private void Disable(Meeple meeple)
        {
            var meepleGameObject = meeple.gameObject;
            meepleGameObject.GetComponent<Rigidbody>().useGravity = false;
            meepleGameObject.GetComponent<BoxCollider>().enabled = false;
            meepleGameObject.GetComponent<ObjectManipulator>().enabled = false;
            meepleGameObject.GetComponentInChildren<MeshRenderer>().enabled = false;
            meepleGameObject.SetActive(false);
        }

        #region Updated Functions

        public void OnDraw(Meeple meeple)
        {
            // var meepleGameObject = meeple.gameObject; 
            Enable(meeple);
            // meepleGameObject.transform.parent = gameControllerScript.table.transform;
            // meepleGameObject.transform.position = meepleSpawnPosition.transform.position;
            
            meeple.transform.SetParent(meepleGrid.transform);
            meeple.gameObject.SetActive(true);
            // meeple.GetComponent<BoxCollider>().enabled = true;
            meeple.GetComponent<GridKeyboardMovable>().enabled = true;
            // meeple.GetComponent<Rigidbody>().useGravity = true;
            // meeple.GetComponent<ObjectManipulator>().enabled = true;
            meeple.GetComponent<Rigidbody>().isKinematic = false;
            // meeple.GetComponentInChildren<MeshRenderer>().enabled = true;
            
            meeple.GetComponent<GridPosition>().MoveToRPC(new Vector2Int(startingPosition, startingPosition));
            
            gameControllerScript.gameController.state.phase = Phase.MeepleDrawn;
            
            Debug.Log(state.Features.Graph);
        }

        public void OnInvalidDraw()
        {
            var deniedSound = gameObject.GetComponent<AudioSource>();
            deniedSound.Play();
        }
        
        public void OnPlace(Meeple meeple, Vector2Int cell)
        {
            meeple.GetComponent<BoxCollider>().enabled = false;
            meeple.GetComponent<GridKeyboardMovable>().enabled = false;
            meeple.GetComponent<Rigidbody>().useGravity = false;
            meeple.GetComponent<ObjectManipulator>().enabled = false;
            meeple.GetComponent<Rigidbody>().isKinematic = true;
        }

        public void OnDiscardMeeple(Meeple meeple)
        {
            Disable(meeple);
        }

        [PunRPC]
        public void PlaceMeeple()
        {
            var meepleController = GetComponent<MeepleController>();
            var state = GetComponent<GameState>();
            var meeple = state.Meeples.Current;
            var gridPosition = meeple.GetComponent<GridPosition>();
            
            // Invoke the TileController's place call.
            meepleController.Place(gridPosition.cell);
        }

        [PunRPC]
        public void DiscardMeeple()
        {
            var meepleController = GetComponent<MeepleController>();
            
            // Invoke the TileController's place call.
            meepleController.Discard();
        }
        

        #endregion

        [PunRPC]
        public void DrawMeeple()
        {
            var meepleController = GetComponent<MeepleController>();
            meepleController.Draw();

            // if (gameControllerScript.gameController.state.phase == Phase.TileDown)
            // {
            //     var meeple = state.Meeples.ForPlayer(players.Current).FirstOrDefault(m => state.Meeples.IsFree(m));
            //     
            //     if (meeple != null)
            //     {
            //         
            //         // meepleGameObject.transform.parent = GameObject.Find("MeepleDrawPosition").transform.parent;
            //         // meepleGameObject.transform.localPosition = new Vector3(0,0,0);
            //         // meepleGameObject.transform.SetParent(GameObject.Find("Table").transform, true);
            //
            //         meeples.Current = meeple;
            //         // meepleGameObject.transform.rotation = Quaternion.identity;
            //
            //         // gameControllerScript.UpdateDecisionButtons(true, meepleGameObject);
            //         gameControllerScript.gameController.state.phase = Phase.MeepleDrawn;
            //     }
            // }
            // else
            // {
            //     
            // }
        }
        
        // public void SetMeepleSnapPos()
        // {
        //     var current = gameControllerScript.gameController.state.Meeples.Current;
        //
        //     if (current == null) throw new ArgumentException("Current Meeple is null.");
        //     
        //     if (meepleHitTileDirection.collider != null)
        //     {
        //         current.transform.position =
        //             new Vector3(gameControllerScript.SnapPosition.x, current.transform.position.y, gameControllerScript.SnapPosition.z);
        //
        //         if (gameControllerScript.Direction == Vector2Int.left || gameControllerScript.Direction == Vector2Int.right)
        //         {
        //             if (current.transform.rotation.eulerAngles.y != 90) current.transform.Rotate(0.0f, 90.0f, 0.0f, Space.Self);
        //         }
        //         else if (gameControllerScript.Direction == Vector2Int.up || gameControllerScript.Direction == Vector2Int.down ||
        //                  gameControllerScript.Direction == Vector2Int.zero)
        //         {
        //             if (current.transform.rotation.eulerAngles.y == 90) current.transform.Rotate(0.0f, -90.0f, 0.0f, Space.Self);
        //         }
        //     }
        //     
        //     gameControllerScript.SnapPosition = current.transform.position;
        // }

        // public bool PlaceMeeple(GameObject meeple, Vector2Int position, Vector2Int direction)
        // {
        //     var meepleScript = meeple.GetComponent<MeepleScript>();
        //
        //     return PlaceMeeple(meepleScript, position, direction);
        // }

        #region Photon

        public void DrawMeepleRPC()
        {
            if (PhotonNetwork.LocalPlayer.NickName == (players.Current.id + 1).ToString())
                this.photonView.RPC("DrawMeeple",
                    RpcTarget.All);
        }
        

        #endregion
        
        #region ToEventify

        public void OnFreeMeeple()
        {
            var meeple = meeples.Current;
            
            meeple.transform.position = new Vector3(20, 20, 20);
            Disable(meeple);
        }
        
        public void OnMeeplePlaced(Meeple meeple)
        {
            // Turn off meeple collider
            Fix(meeple);
        }
        
        #endregion
    }
}