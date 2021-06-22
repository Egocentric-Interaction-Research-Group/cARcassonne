using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Anchor_Script : MonoBehaviour
{
    // Start is called before the first frame update
    public GameObject anchor, facing;
    public float yOffset;
    void Start()
    {
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if(anchor != null)
        {
            transform.position = new Vector3(anchor.transform.position.x, anchor.transform.position.y + yOffset, anchor.transform.position.z);
        }

        if (facing != null)
        {
            transform.LookAt(facing.transform);

        }
    }
}
