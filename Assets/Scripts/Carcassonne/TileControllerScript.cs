using Carcassonne.State;
using JetBrains.Annotations;
using Photon.Pun;
using UnityEngine;

namespace Carcassonne
{
    public class TileControllerScript : MonoBehaviourPun
    {
        private GameControllerScript gameControllerScript;
        public Vector3 currentTileEulersOnManip;
        public ParticleSystem drawTileEffect;
        public TileState tiles;
        
        [CanBeNull]
        public GameObject currentTile
        {
            get
            {
                if (tiles.Current is null)
                    return null;
                return tiles.Current.gameObject;
            }
            set => tiles.Current = value.GetComponent<TileScript>();
        }

        public GameObject drawTile;
        public GameObject tileSpawnPosition;
        public float fTileAimX;
        public float fTileAimZ;

        public TileControllerScript(GameControllerScript gameControllerScript)
        {
            this.gameControllerScript = gameControllerScript;
        }

        public void ChangeCurrentTileOwnership()
        {
            if (currentTile.GetComponent<PhotonView>().Owner.NickName != (gameControllerScript.currentPlayer.getID() + 1).ToString())
                currentTile.GetComponent<TileScript>().transferTileOwnership(gameControllerScript.currentPlayer.getID());
        }

        public void ActivateCurrentTile(GameControllerScript gameControllerScript)
        {
            gameControllerScript.TileControllerScript2.currentTile.GetComponentInChildren<MeshRenderer>().enabled = true;
            gameControllerScript.TileControllerScript2.currentTile.GetComponentInChildren<Collider>().enabled = true;
            gameControllerScript.TileControllerScript2.currentTile.GetComponentInChildren<Rigidbody>().useGravity = true;
            gameControllerScript.TileControllerScript2.currentTile.transform.parent = gameControllerScript.table.transform;
            gameControllerScript.TileControllerScript2.currentTile.transform.rotation = gameControllerScript.table.transform.rotation;
            gameControllerScript.TileControllerScript2.currentTile.transform.position = gameControllerScript.TileControllerScript2.tileSpawnPosition.transform.position;
            gameControllerScript.smokeEffect.Play();
        }
    }
}