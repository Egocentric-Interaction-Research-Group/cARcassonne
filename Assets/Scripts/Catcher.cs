using UnityEngine;

public class Catcher : MonoBehaviour
{
    [SerializeField] GameObject spawnPos;
    void Update()
    {
        if(name == "BaseTile")
        {
            return;
        }
        if(transform.position.y <= -3.5f)
        {
            transform.position = spawnPos.transform.position;
            GetComponent<Rigidbody>().velocity = Vector3.zero;
        }
    }

}
