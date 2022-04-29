using Photon.Pun;

namespace PunTabletop
{
    public class PunStartButton : MonoBehaviourPun
    {
        private ChangeScene changeScene;

        private void Start()
        {
            // Cache references
            changeScene = GetComponent<ChangeScene>();

            // Subscribe to PunPlacementHintsController events
            changeScene.OnChangeScene += OnChangeSceneHandler;

            // Enable PUN feature
            changeScene.IsPunEnabled = true;
        }

        private void OnChangeSceneHandler()
        {
            changeScene.LoadScene();
        }

        //[PunRPC]
        //private void Pun_RPC_ChangeScene()
        //{
        //    changeScene.LoadScene();

        //}
    }
}
