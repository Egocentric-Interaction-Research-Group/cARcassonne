using UnityEngine;

namespace PunTabletop
{
    /// <summary>
    /// Responsible for re-positioning game objects if they have fallen off the table.
    /// TODO: This probably needs to be a MonoBehaviourPun
    /// </summary>
    public class TableBoundaryEnforcerScript : MonoBehaviour
    {
        [SerializeField] GameObject spawnPos;
        void Update()
        {
            if(transform.position.y <= -3.5f)
            {
                transform.position = spawnPos.transform.position;
                GetComponent<Rigidbody>().velocity = Vector3.zero;
            }
        }

    }
}
