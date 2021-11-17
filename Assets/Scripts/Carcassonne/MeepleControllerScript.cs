using System;
using System.Collections.Generic;
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
                              85;
            }
            else
            {
                iMeepleAimX = (int) ((fMeepleAimX - basePositionTransform.localPosition.x) * gameControllerScript.scale - 1f) / 2 +
                              85;
            }

            if (fMeepleAimZ - basePositionTransform.localPosition.z > 0)
            {
                iMeepleAimZ = (int) ((fMeepleAimZ - basePositionTransform.localPosition.z) * gameControllerScript.scale + 1f) / 2 +
                              85;
            }
            else
            {
                iMeepleAimZ = (int) ((fMeepleAimZ - basePositionTransform.localPosition.z) * gameControllerScript.scale - 1f) / 2 +
                              85;
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

        public MeepleScript FindMeeple(int x, int y, TileScript.Geography geography, GameControllerScript gameControllerScript)
        {
            MeepleScript res = null;

            foreach (var m in meeples.All)
            {
                var tmp = m.GetComponent<MeepleScript>();

                if (tmp.geography == geography && tmp.x == x && tmp.z == y) return tmp;
            }

            return res;
        }

        public void AimMeeple(GameControllerScript gameControllerScript)
        {
            try
            {
                if (gameControllerScript.PlacedTiles.getPlacedTiles(this.iMeepleAimX, this.iMeepleAimZ) == gameControllerScript.TileControllerScript.currentTile)
                {
                    var tile = gameControllerScript.PlacedTiles.getPlacedTiles(this.iMeepleAimX, this.iMeepleAimZ);
                    var tileScript = tile.GetComponent<TileScript>();

                    var layerMask = 1 << 9;
                    Physics.Raycast(meeples.Current.gameObject.transform.position, meeples.Current.gameObject.transform.TransformDirection(Vector3.down), out this.meepleHitTileDirection,
                        Mathf.Infinity, layerMask);

                    this.meepleGeography = TileScript.Geography.Grass;
                    gameControllerScript.Direction = PointScript.Direction.CENTER;

                    if (this.meepleHitTileDirection.collider != null)
                    {
                        if (this.meepleHitTileDirection.collider.name == "East")
                        {
                            gameControllerScript.Direction = PointScript.Direction.EAST;
                            this.meepleGeography = tileScript.East;
                        }
                        else if (this.meepleHitTileDirection.collider.name == "West")
                        {
                            gameControllerScript.Direction = PointScript.Direction.WEST;
                            this.meepleGeography = tileScript.West;
                        }
                        else if (this.meepleHitTileDirection.collider.name == "North")
                        {
                            gameControllerScript.Direction = PointScript.Direction.NORTH;
                            this.meepleGeography = tileScript.North;
                        }
                        else if (this.meepleHitTileDirection.collider.name == "South")
                        {
                            gameControllerScript.Direction = PointScript.Direction.SOUTH;
                            this.meepleGeography = tileScript.South;
                        }
                        else if (this.meepleHitTileDirection.collider.name == "Center")
                        {
                            gameControllerScript.Direction = PointScript.Direction.CENTER;
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
                    this.meepleGeography = TileScript.Geography.Grass;
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

        public void FreeMeeple(GameObject meeple, GameControllerScript gameControllerScript)
        {
            meeple.GetComponent<MeepleScript>().free = true;
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
                foreach (MeepleScript meeple in players.Current.meeples) //TODO Inefficient. Just want the first free meeple.
                {
                    GameObject meepleGameObject = meeple.gameObject;
                    if (meeple.free)
                    {
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

                        gameControllerScript.UpdateDecisionButtons(true, false, meepleGameObject);
                        gameControllerScript.gameState.phase = Phase.MeepleDrawn;
                        break;
                    }
                }
            }
            else
            {
                var deniedSound = gameObject.GetComponent<AudioSource>();
                deniedSound.Play();
            }
        }

        public MeepleScript FindMeeple(int x, int y, TileScript.Geography geography, PointScript.Direction direction, GameControllerScript gameControllerScript)
        {
            MeepleScript res = null;

            foreach (var m in meeples.All)
            {
                var tmp = m.GetComponent<MeepleScript>();

                if (tmp.geography == geography && tmp.x == x && tmp.z == y && tmp.direction == direction) return tmp;
            }

            return res;
        }

        public void PlaceMeeple(GameObject meeple, int xs, int zs, PointScript.Direction direction,
            TileScript.Geography meepleGeography, GameControllerScript gameControllerScript)
        {
            var currentTileScript = gameControllerScript.TileControllerScript1.currentTile.GetComponent<TileScript>();
            var currentCenter = currentTileScript.getCenter();
            bool res;
            if (currentCenter == TileScript.Geography.Village || currentCenter == TileScript.Geography.Grass ||
                currentCenter == TileScript.Geography.Cloister && direction != PointScript.Direction.CENTER)
                res = GetComponent<PointScript>()
                    .testIfMeepleCantBePlacedDirection(currentTileScript.vIndex, meepleGeography, direction);
            else if (currentCenter == TileScript.Geography.Cloister && direction == PointScript.Direction.CENTER)
                res = false;
            else
                res = GetComponent<PointScript>().testIfMeepleCantBePlaced(currentTileScript.vIndex, meepleGeography);

            if (meepleGeography == TileScript.Geography.City)
            {
                if (currentCenter == TileScript.Geography.City)
                    res = gameControllerScript.CityIsNotFinishedIfEmptyTileBesideCity(xs, zs) || res;
                else
                    res = gameControllerScript.CityIsFinishedDirection(xs, zs, direction) || res;
            }

            if (!currentTileScript.IsOccupied(direction) && !res)
            {
                meeple.GetComponentInChildren<Rigidbody>().useGravity = false;
                meeple.GetComponentInChildren<BoxCollider>().enabled = false;
                meeple.GetComponent<ObjectManipulator>().enabled = false;

                currentTileScript.occupy(direction);
                if (meepleGeography == TileScript.Geography.CityRoad) meepleGeography = TileScript.Geography.City;

                meeple.GetComponent<MeepleScript>().assignAttributes(xs, zs, direction, meepleGeography);
                meeple.GetComponent<MeepleScript>().free = false;


                gameControllerScript.gameState.phase = Phase.MeepleDown;
            }
        }
    }
}