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

        private int meepleCount = 0;

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
            meepleCount++;
            GameObject newMeeple = PhotonNetwork.Instantiate(prefab.name, parent.transform.position, Quaternion.identity);//, GameObject.Find("Table").transform);
            newMeeple.gameObject.transform.parent = parent.transform;
            newMeeple.gameObject.name = $"Meeple {meepleCount}";
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
        public TileScript.Geography meepleGeography;
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
        void Start()
        {
        
        }

        // Update is called once per frame
        void Update()
        {
        
        }
        
        /// <summary>
        /// Find a meeple at the specified position, on the specified geography, in the specified direction.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="geography"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        public MeepleScript FindMeeple(int x, int y, TileScript.Geography geography, Vector2Int? direction = null)
        {
            var position = new Vector2Int(x, y);

            if (gameControllerScript.gameState.Meeples.Placement.ContainsKey(position))
            {
                var tile = gameControllerScript.gameState.Tiles.Played[position.x, position.y];
                var geo = tile.SubTileDictionary[position]; 
                Debug.Assert(tile != null, $"There is a meeple at position {position}, so the tile there should not be null.");

                var meeplePlacement = gameControllerScript.gameState.Meeples.Placement[position];
                var dir = meeplePlacement.Direction;
                var meep = meeplePlacement.Meeple;

                // If there is no direction passed, we just want dir == direction to be true
                if (direction == null)
                {
                    direction = dir;
                }

                // Test for meeple.
                    if (dir == direction && geo == geography)
                {
                    return meep;
                }
            }

            return null;
        }

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

                    this.meepleGeography = TileScript.Geography.Field;
                    gameControllerScript.Direction = PointScript.Centre;

                    if (this.meepleHitTileDirection.collider != null)
                    {
                        if (this.meepleHitTileDirection.collider.name == "East")
                        {
                            gameControllerScript.Direction = PointScript.East;
                            this.meepleGeography = tileScript.East;
                        }
                        else if (this.meepleHitTileDirection.collider.name == "West")
                        {
                            gameControllerScript.Direction = PointScript.West;
                            this.meepleGeography = tileScript.West;
                        }
                        else if (this.meepleHitTileDirection.collider.name == "North")
                        {
                            gameControllerScript.Direction = PointScript.North;
                            this.meepleGeography = tileScript.North;
                        }
                        else if (this.meepleHitTileDirection.collider.name == "South")
                        {
                            gameControllerScript.Direction = PointScript.South;
                            this.meepleGeography = tileScript.South;
                        }
                        else if (this.meepleHitTileDirection.collider.name == "Center")
                        {
                            gameControllerScript.Direction = PointScript.Centre;
                            this.meepleGeography = tileScript.getCenter();
                        }

                        gameControllerScript.SnapPosition = this.meepleHitTileDirection.collider.transform.position;

                        if (this.meepleGeography == TileScript.Geography.City || this.meepleGeography == TileScript.Geography.Road || this.meepleGeography == TileScript.Geography.Cloister)
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
                    this.meepleGeography = TileScript.Geography.Field;
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

        public void FreeMeeple(GameObject meeple)
        {
            // meeple.GetComponent<MeepleScript>().free = true;
            gameControllerScript.gameState.Meeples.Placement.Remove(meeple.GetComponent<MeepleScript>().position);
            
            meeple.transform.position = new Vector3(20, 20, 20);
            meeple.GetComponentInChildren<Rigidbody>().useGravity = false;
            meeple.GetComponentInChildren<BoxCollider>().enabled = false;
            meeple.GetComponentInChildren<MeshRenderer>().enabled = false;
            gameControllerScript.gameState.phase = Phase.TileDown;
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
                    meepleGameObject.SetActive(true);
                    meepleGameObject.GetComponentInChildren<Rigidbody>().useGravity = true;
                    meepleGameObject.GetComponentInChildren<BoxCollider>().enabled = true;
                    meepleGameObject.GetComponentInChildren<MeshRenderer>().enabled = true;
                    meepleGameObject.GetComponentInChildren<ObjectManipulator>().enabled = true;
                    meepleGameObject.transform.parent = gameControllerScript.table.transform;
                    meepleGameObject.transform.position = meepleSpawnPosition.transform.position;
                    // meepleGameObject.transform.parent = GameObject.Find("MeepleDrawPosition").transform.parent;
                    // meepleGameObject.transform.localPosition = new Vector3(0,0,0);
                    // meepleGameObject.transform.SetParent(GameObject.Find("Table").transform, true);
                        
                    meeples.Current = meepleGameObject.GetComponent<MeepleScript>();
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

        public bool GeographyCanHoldMeeples(TileScript.Geography geography)
        {
            if ((geography & TileScript.Geography.City) == TileScript.Geography.City ||
                (geography & TileScript.Geography.Road) == TileScript.Geography.Road ||
                geography == TileScript.Geography.Cloister)
            {
                return true;
            }

            return false;
        }

        public bool IsValidPlacement(Vector2Int position, Vector2Int direction)
        {
            var tile = gameControllerScript.gameState.Tiles.Played[position.x, position.y];
            var geography = tile.getGeographyAt(direction);
            
            // Placement is invalid if not on a type of feature that can have a meeple
            if (!GeographyCanHoldMeeples(geography)) return false;
            
            // Placement is invalid if feature already has meeple
            if (gameControllerScript.gameState.Features.GetFeatureAt(position, direction).Meeples.Any())
                return false;

            // Nothing makes it invalid, so return true
            return true;
        }

        public bool PlaceMeeple(GameObject meeple, Vector2Int position, Vector2Int direction)
        {
            var meepleScript = meeple.GetComponent<MeepleScript>();
            
            // Test if Meeple placement is valid
            if (!IsValidPlacement(position, direction)) return false;
            
            // Place meeple
            gameControllerScript.gameState.Meeples.Placement.Add(position, new PlacedMeeple(meepleScript, direction));
            
            // Move game to next phase
            gameControllerScript.gameState.phase = Phase.MeepleDown;

            return true;

            // var currentCenter = currentTileScript.getCenter();
            //
            // // Determine whether a meeple is [res]tricted from being placed
            // bool res;
            // if (currentCenter == TileScript.Geography.Village || currentCenter == TileScript.Geography.Field ||
            //     currentCenter == TileScript.Geography.Cloister && direction != PointScript.Centre)
            //     res = GetComponent<PointScript>()
            //         .testIfMeepleCantBePlacedDirection(currentTileScript.vIndex, meepleGeography, direction);
            // else if (currentCenter == TileScript.Geography.Cloister && direction == PointScript.Centre)
            //     res = false;
            // else
            //     res = GetComponent<PointScript>().testIfMeepleCantBePlaced(currentTileScript.vIndex, meepleGeography);
            //
            // if (meepleGeography == TileScript.Geography.City)
            // {
            //     if (currentCenter == TileScript.Geography.City)
            //         res = gameControllerScript.CityIsNotFinishedIfEmptyTileBesideCity(xs, zs) || res;
            //     else
            //         res = gameControllerScript.CityIsFinishedDirection(xs, zs, direction) || res;
            // }
            //
            // if (!currentTileScript.IsOccupied(direction) && !res)
            // {
            //     meeple.GetComponentInChildren<Rigidbody>().useGravity = false;
            //     meeple.GetComponentInChildren<BoxCollider>().enabled = false;
            //     meeple.GetComponent<ObjectManipulator>().enabled = false;
            //
            //     currentTileScript.occupy(direction);
            //     if (meepleGeography == TileScript.Geography.CityRoad) meepleGeography = TileScript.Geography.City;
            //
            //     meeple.GetComponent<MeepleScript>().assignAttributes(xs, zs, direction, meepleGeography);
            //     meeple.GetComponent<MeepleScript>().free = false;
            //
            //
            //     gameControllerScript.gameState.phase = Phase.MeepleDown;
            // }
        }
    }
}