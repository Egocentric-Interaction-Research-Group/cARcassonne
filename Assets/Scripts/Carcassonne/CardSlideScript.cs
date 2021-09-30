using UnityEngine;

public class CardSlideScript : MonoBehaviour
{
    private Animator anim;

    // Start is called before the first frame update
    private void Start()
    {
        anim = GetComponent<Animator>();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    public void InvalidTile(bool toggle)
    {
        anim.SetBool("TileNotValid", toggle);
    }

    public void InvalidMeeple(bool toggle)
    {
        anim.SetBool("InvalidMeeple", toggle);
    }
}