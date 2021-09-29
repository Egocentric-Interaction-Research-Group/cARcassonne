using UnityEngine;

public class ChangeColorScript : MonoBehaviour
{
    public GameObject pile;
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ChangeToWhite()
    {
        pile.GetComponent<Renderer>().material.color = Color.white;
        Debug.Log("Ass");
    }
}
