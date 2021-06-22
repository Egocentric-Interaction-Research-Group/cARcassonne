using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardSlideScript : MonoBehaviour
{

    private Animator anim;

    public void InvalidTile(bool toggle)
    {
        anim.SetBool("TileNotValid", toggle);
    }
    public void InvalidMeeple(bool toggle)
    {
        anim.SetBool("InvalidMeeple", toggle);
    }

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
