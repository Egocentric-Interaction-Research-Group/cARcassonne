using UnityEngine;

namespace Carcassonne
{
    public class MusicScript : MonoBehaviour
    {
        private void Awake()
        {
            DontDestroyOnLoad(transform.gameObject);
        }
    }
}