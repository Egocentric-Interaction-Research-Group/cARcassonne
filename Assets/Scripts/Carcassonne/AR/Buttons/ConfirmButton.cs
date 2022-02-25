using System;
using Carcassonne.Controllers;
using Carcassonne.Models;
using Carcassonne.Players;
using Carcassonne.State;
using Microsoft.MixedReality.Toolkit.UI;
using MRTK.Tutorials.MultiUserCapabilities;
using Photon.Pun;
using PunTabletop;
using UI.Grid;
using UnityEngine;
using UnityEngine.Events;

namespace Carcassonne.AR.Buttons
{
    public class ConfirmButton : MonoBehaviour
    {
        public UnityEvent ValidClick = new UnityEvent();
        public UnityEvent InvalidCick = new UnityEvent();
        
        public GameState state;
        public TileController tileController;
        public MeepleController meepleController;
        
        public Material ConfirmableMaterial;
        public Material NonConfirmableMaterial;

        public Sprite ConfirmableSprite;
        public Sprite NonConfirmableSprite;

        private bool confirmable;

        private Transform backplate => transform.Find("ConfirmButtonBackplate");
        
        private void Start()
        {
            // state = FindObjectOfType<GameState>();
            // Debug.Assert(state != null, "ConfirmButton: state is not found (is null).");
            
            GetComponent<ButtonConfigHelper>().OnClick.AddListener(OnClick);
        }

        public void OnClick()
        {
            if (confirmable) ValidClick.Invoke();
            else InvalidCick.Invoke();
        }
        
        public void SetConfirmable()
        {
            backplate.GetComponentInChildren<MeshRenderer>().material = ConfirmableMaterial; //materials.buttonMaterials[2];
            GetComponentInChildren<SpriteRenderer>().sprite = ConfirmableSprite; //checkIcon;
            confirmable = true;
        }

        public void SetNonConfirmable()
        {
            backplate.GetComponentInChildren<MeshRenderer>().material = NonConfirmableMaterial; //materials.buttonMaterials[0];
            GetComponentInChildren<SpriteRenderer>().sprite = NonConfirmableSprite; //crossIcon;
            confirmable = false;
        }

        public void ReAnchor(GameObject gamepiece)
        {
            GetComponentInParent<Anchor_Script>().anchor = gamepiece.transform.Find("North").gameObject;
        }

        public void OnDraw(MonoBehaviour gamePiece)
        {
            Debug.Assert(state != null, "ConfirmButton: state is not found (is null).");
            Debug.Assert(state.Players != null, "ConfirmButton: state players is not found (is null).");
            Debug.Assert(state.Players.Current != null, "Current player is null");
            Debug.Assert(state.Players.Current.GetComponent<PlayerScript>() != null, "Current player does not have a PlayerScript component");
            // Is local player current?
            if (state.Players.Current.GetComponent<PlayerScript>().IsLocal)
            {
                gameObject.SetActive(true);
            }
            ReAnchor(gamePiece.gameObject);

        }

        public void OnPlace(MonoBehaviour gamePiece, Vector2Int placement)
        {
            gameObject.SetActive(false);
        }

        public void OnDiscard(MonoBehaviour gamePiece)
        {
            gameObject.SetActive(false);
        }

        public void OnTileChange()
        {
            var cell = state.Tiles.Current.GetComponent<GridPosition>().cell;
            var valid = tileController.IsPlacementValid(cell);
            
            if(valid) SetConfirmable();
            else SetNonConfirmable();
        }

        public void OnMeepleChange()
        {
            var cell = state.Meeples.Current.GetComponent<GridPosition>().cell;
            var valid = meepleController.IsPlacementValid(cell);
            
            if(valid) SetConfirmable();
            else SetNonConfirmable();
        }

    }
}