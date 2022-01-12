using System;
using System.Collections.Generic;
using System.Linq;
using Carcassonne.State;
using Microsoft.MixedReality.Toolkit.UI;
using Photon.Pun;
using UnityEngine;

namespace Carcassonne
{
    public class MeepleControllerScript : MonoBehaviourPun
    {
    
        [SerializeField]
        internal GameControllerScript gameControllerScript;
        [HideInInspector] public List<MeepleScript> MeeplesInCity;
        internal float fMeepleAimX; //TODO Make Private
        internal float fMeepleAimZ; //TODO Make Private

        public MeepleState meeples;
        public PlayerState players;

        public MeepleControllerScript(GameControllerScript gameControllerScript)
        {
            this.gameControllerScript = gameControllerScript;
        }
        
        // Instantiation Stuff
        public GameObject prefab;
        public GameObject parent;
        
        /// <summary>
        /// Instantiate a new Meeple with the chosen prefab and parent object in the hierarchy.
        /// </summary>
        /// <returns>GameObject : An instance of MeepleScript.prefab.</returns>
        public MeepleScript GetNewInstance()
        {
            // return Instantiate(prefab, meepleSpawnPosition.transform.position, Quaternion.identity, GameObject.Find("Table").transform).GetComponent<MeepleScript>();
            GameObject newMeeple = PhotonNetwork.Instantiate(prefab.name, parent.transform.position, Quaternion.identity);//, GameObject.Find("Table").transform);
            newMeeple.gameObject.transform.parent = parent.transform;
            newMeeple.gameObject.name = $"Meeple {gameControllerScript.gameState.Meeples.InPlay.Count()}";
            newMeeple.SetActive(false);

            return newMeeple.GetComponent<MeepleScript>();
        }

        public void DrawMeepleRPC()
        {
            if (PhotonNetwork.LocalPlayer.NickName == (players.Current.getID() + 1).ToString())
                this.photonView.RPC("DrawMeeple",
                    RpcTarget.All);
        }
    
        public ParticleSystem drawMeepleEffect;
        [HideInInspector] public GameObject meepleMesh;
        [HideInInspector] public GameObject MeeplePrefab;
        public GameObject meepleSpawnPosition;
        internal int iMeepleAimX;
        internal int iMeepleAimZ;
        public Geography meepleGeography;
        public RaycastHit meepleHitTileDirection;

        /// <summary>
        /// Determines which tile space the currently moving Meeple is over.
        /// </summary>
        internal void CurrentMeepleRayCast() //TODO Should be private
        {
            RaycastHit hit;
            var layerMask = 1 << 8;
            var basePositionTransform = gameControllerScript.stackScript.basePositionTransform;

            Physics.Raycast(meeples.Current.gameObject.transform.position, meeples.Current.gameObject.transform.TransformDirection(Vector3.down),
                out hit, Mathf.Infinity, layerMask);

            var local = gameControllerScript.table.transform.InverseTransformPoint(hit.point);


            fMeepleAimX = local.x;
            fMeepleAimZ = local.z;


            if (fMeepleAimX - basePositionTransform.localPosition.x > 0)
            {
                iMeepleAimX = (int) ((fMeepleAimX - basePositionTransform.localPosition.x) * gameControllerScript.scale + 1f) / 2 +
                              GameRules.BoardSize / 2;
            }
            else
            {
                iMeepleAimX = (int) ((fMeepleAimX - basePositionTransform.localPosition.x) * gameControllerScript.scale - 1f) / 2 +
                              GameRules.BoardSize / 2;
            }

            if (fMeepleAimZ - basePositionTransform.localPosition.z > 0)
            {
                iMeepleAimZ = (int) ((fMeepleAimZ - basePositionTransform.localPosition.z) * gameControllerScript.scale + 1f) / 2 +
                              GameRules.BoardSize / 2;
            }
            else
            {
                iMeepleAimZ = (int) ((fMeepleAimZ - basePositionTransform.localPosition.z) * gameControllerScript.scale - 1f) / 2 +
                              GameRules.BoardSize / 2;
            }
        }
    
    
        // Start is called before the first frame update
        // void Start()
        // {
        //
        // }
        //
        // // Update is called once per frame
        // void Update()
        // {
        //
        // }
        //
        /// <summary>
        /// Find a meeple at the specified position, on the specified geography, in the specified direction.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="geography"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        // public MeepleScript FindMeeple(int x, int y, Geography geography, Vector2Int? direction = null)
        // {
        //     var position = new Vector2Int(x, y);
        //
        //     if (gameControllerScript.gameState.Meeples.Placement.ContainsKey(position))
        //     {
        //         var tile = gameControllerScript.gameState.Tiles.Played[position.x, position.y];
        //         Debug.Assert(tile != null, $"There is a meeple at position {position}, so the tile there should not be null.");
        //         var geo = tile.SubTileDictionary[position]; 
        //
        //         var meeplePlacement = gameControllerScript.gameState.Meeples.Placement[position];
        //         var dir = meeplePlacement.Direction;
        //         var meep = meeplePlacement.Meeple;
        //
        //         // If there is no direction passed, we just want dir == direction to be true
        //         if (direction == null)
        //         {
        //             direction = dir;
        //         }
        //
        //         // Test for meeple.
        //             if (dir == direction && geo == geography)
        //         {
        //             return meep;
        //         }
        //     }
        //
        //     return null;
        // }

        public void AimMeeple()
        {
            try
            {
                if (gameControllerScript.PlacedTiles.GetPlacedTile(this.iMeepleAimX, this.iMeepleAimZ) == gameControllerScript.tileControllerScript.currentTile)
                {
                    var tile = gameControllerScript.PlacedTiles.GetPlacedTile(this.iMeepleAimX, this.iMeepleAimZ);
                    var tileScript = tile.GetComponent<TileScript>();

                    var layerMask = 1 << 9;
                    Physics.Raycast(meeples.Current.gameObject.transform.position, meeples.Current.gameObject.transform.TransformDirection(Vector3.down), out this.meepleHitTileDirection,
                        Mathf.Infinity, layerMask);

                    this.meepleGeography = Geography.Field;
                    gameControllerScript.Direction = Vector2Int.zero;

                    if (this.meepleHitTileDirection.collider != null)
                    {
                        if (this.meepleHitTileDirection.collider.name == "East")
                        {
                            gameControllerScript.Direction = Vector2Int.right;
                            this.meepleGeography = tileScript.East;
                        }
                        else if (this.meepleHitTileDirection.collider.name == "West")
                        {
                            gameControllerScript.Direction = Vector2Int.left;
                            this.meepleGeography = tileScript.West;
                        }
                        else if (this.meepleHitTileDirection.collider.name == "North")
                        {
                            gameControllerScript.Direction = Vector2Int.up;
                            this.meepleGeography = tileScript.North;
                        }
                        else if (this.meepleHitTileDirection.collider.name == "South")
                        {
                            gameControllerScript.Direction = Vector2Int.down;
                            this.meepleGeography = tileScript.South;
                        }
                        else if (this.meepleHitTileDirection.collider.name == "Center")
                        {
                            gameControllerScript.Direction = Vector2Int.zero;
                            this.meepleGeography = tileScript.getCenter();
                        }

                        gameControllerScript.SnapPosition = this.meepleHitTileDirection.collider.transform.position;
                        
                        if (this.meepleGeography.IsFeature())
                        {
                            gameControllerScript.ChangeConfirmButtonApperance(true);
                            gameControllerScript.CanConfirm = true;
                        }
                    }
                    else
                    {
                        gameControllerScript.SnapPosition = meeples.Current.gameObject.transform.position;
                        gameControllerScript.ChangeConfirmButtonApperance(false);
                        gameControllerScript.CanConfirm = false;
                    }
                }
                else
                {
                    gameControllerScript.SnapPosition = meeples.Current.gameObject.transform.position;
                    this.meepleGeography = Geography.Field;
                    gameControllerScript.ChangeConfirmButtonApperance(false);
                    gameControllerScript.CanConfirm = false;
                }
            }
            catch (IndexOutOfRangeException e)
            {
                Debug.Log(e);
                gameControllerScript.ErrorOutput = e.ToString();
            }
        }

        public void FreeMeeple(MeepleScript meeple)
        {
            // If this is a meeple that has already been played on a tile (as opposed to one that is being placed).
            if(gameControllerScript.gameState.Meeples.InPlay.Contains(meeple)){
                var position = meeple.position;
                Debug.Log($"Meeple at position {position} has been freed.");
                // meeple.GetComponent<MeepleScript>().free = true;
                gameControllerScript.gameState.Meeples.Placement.Remove(position);
            }
            
            meeple.transform.position = new Vector3(20, 20, 20);
            Disable(meeple.gameObject);
        }

        public void FreeMeeple(GameObject meeple)
        {
            FreeMeeple(meeple.GetComponent<MeepleScript>());
        }

        private static void Enable(GameObject meepleGameObject)
        {
            meepleGameObject.SetActive(true);
            meepleGameObject.GetComponent<Rigidbody>().useGravity = true;
            meepleGameObject.GetComponent<BoxCollider>().enabled = true;
            meepleGameObject.GetComponent<ObjectManipulator>().enabled = true;
            meepleGameObject.GetComponentInChildren<MeshRenderer>().enabled = true;
        }

        private static void Fix(GameObject meepleGameObject)
        {
            meepleGameObject.GetComponent<Rigidbody>().useGravity = false;
            meepleGameObject.GetComponent<BoxCollider>().enabled = false;
            meepleGameObject.GetComponent<ObjectManipulator>().enabled = false;
            meepleGameObject.GetComponentInChildren<MeshRenderer>().enabled = true;
        }

        private void Disable(GameObject meepleGameObject)
        {
            meepleGameObject.SetActive(false);
            meepleGameObject.GetComponent<Rigidbody>().useGravity = false;
            meepleGameObject.GetComponent<BoxCollider>().enabled = false;
            meepleGameObject.GetComponent<ObjectManipulator>().enabled = false;
            meepleGameObject.GetComponentInChildren<MeshRenderer>().enabled = false;
        }

        [PunRPC]
        public void DrawMeeple()
        {
            if (gameControllerScript.gameState.phase == Phase.TileDown)
            {
                var meeple = players.Current.meeples.FirstOrDefault(m => !gameControllerScript.gameState.Meeples.InPlay.Contains(m));
                
                if (meeple != null)
                {
                    var meepleGameObject = meeple.gameObject; 
                    Enable(meepleGameObject);
                    meepleGameObject.transform.parent = gameControllerScript.table.transform;
                    meepleGameObject.transform.position = meepleSpawnPosition.transform.position;
                    // meepleGameObject.transform.parent = GameObject.Find("MeepleDrawPosition").transform.parent;
                    // meepleGameObject.transform.localPosition = new Vector3(0,0,0);
                    // meepleGameObject.transform.SetParent(GameObject.Find("Table").transform, true);

                    meeples.Current = meeple;
                    // meepleGameObject.transform.rotation = Quaternion.identity;

                    gameControllerScript.UpdateDecisionButtons(true, meepleGameObject);
                    gameControllerScript.gameState.phase = Phase.MeepleDrawn;
                }
            }
            else
            {
                var deniedSound = gameObject.GetComponent<AudioSource>();
                deniedSound.Play();
            }
        }

        public bool IsValidPlacement(Vector2Int position, Vector2Int direction)
        {
            var feature = gameControllerScript.gameState.Features.GetFeatureAt(position, direction);
            
            // Placement is invalid if not on a type of feature that can have a meeple
            if (feature == null) return false;
            
            // Placement is invalid if feature already has meeple
            if (meeples.InFeature(feature).Any()) return false;

            // Nothing makes it invalid, so return true
            return true;
        }

        public bool PlaceMeeple(Vector2Int position, Vector2Int direction)
        {
            MeepleScript meeple = gameControllerScript.gameState.Meeples.Current;
            
            // Test if Meeple placement is valid
            if (!IsValidPlacement(position, direction)) return false;
            
            // Place meeple
            gameControllerScript.gameState.Meeples.Placement.Add(position, new PlacedMeeple(meeple, direction));
            
            // Turn off meeple collider
            Fix(meeple.gameObject);

            // Move game to next phase
            gameControllerScript.gameState.phase = Phase.MeepleDown;

            return true;
        }

        public void CancelPlacement()
        {
            FreeMeeple(meeples.Current);
            meeples.Current = null;
        }
        
        public void SetMeepleSnapPos()
        {
            var current = gameControllerScript.gameState.Meeples.Current;

            if (current == null) throw new ArgumentException("Current Meeple is null.");
            
            if (meepleHitTileDirection.collider != null)
            {
                current.transform.position =
                    new Vector3(gameControllerScript.SnapPosition.x, current.transform.position.y, gameControllerScript.SnapPosition.z);

                if (gameControllerScript.Direction == Vector2Int.left || gameControllerScript.Direction == Vector2Int.right)
                {
                    if (current.transform.rotation.eulerAngles.y != 90) current.transform.Rotate(0.0f, 90.0f, 0.0f, Space.Self);
                }
                else if (gameControllerScript.Direction == Vector2Int.up || gameControllerScript.Direction == Vector2Int.down ||
                         gameControllerScript.Direction == Vector2Int.zero)
                {
                    if (current.transform.rotation.eulerAngles.y == 90) current.transform.Rotate(0.0f, -90.0f, 0.0f, Space.Self);
                }
            }
            
            gameControllerScript.SnapPosition = current.transform.position;
        }

        // public bool PlaceMeeple(GameObject meeple, Vector2Int position, Vector2Int direction)
        // {
        //     var meepleScript = meeple.GetComponent<MeepleScript>();
        //
        //     return PlaceMeeple(meepleScript, position, direction);
        // }
    }
}