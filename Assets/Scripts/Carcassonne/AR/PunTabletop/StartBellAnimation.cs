using Carcassonne.AR;
using Carcassonne.State;
using Photon.Pun;
using UnityEngine;

namespace PunTabletop
{
    public class StartBellAnimation : MonoBehaviourPun
    {
        public GameObject gameController;

        public void StartAni()
        {
            Animation animation = gameObject.GetComponent<Animation>();
            AudioSource bellAudio = gameObject.GetComponent<AudioSource>();
            animation.Play();
            bellAudio.Play();
        }
    }
}
