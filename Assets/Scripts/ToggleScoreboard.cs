using UnityEngine;

public class ToggleScoreboard : MonoBehaviour
{
    public GameObject scoreboard;
    private bool toggle;
    // Start is called before the first frame update
    void Start()
    {
        toggle = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Toggle()
    {
        toggle ^= true;

        if(toggle)
        {
            scoreboard.gameObject.SetActive(true);
        }
        else
        {
            scoreboard.gameObject.SetActive(false);
        }
    }
}
