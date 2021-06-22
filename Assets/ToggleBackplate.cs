using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleBackplate : MonoBehaviour
{
    public Material pressedButtonMat, unpressedButtonMat;
    private bool toggle;
    void Start()
    {
        if (gameObject.transform.parent.name == "LockButton" || gameObject.transform.parent.name == "ScoreboardButton" || gameObject.transform.parent.name == "HandButton")
        {
            gameObject.GetComponent<MeshRenderer>().material = pressedButtonMat;
            toggle = true;
        }
    }

    // Update is called once per frame
    void Update()
    {

    }

    public void ToggleBackplateMaterial()
    {
        toggle ^= true;

        if (toggle)
        {
            gameObject.GetComponent<MeshRenderer>().material = pressedButtonMat;
        }
        else
        {
            gameObject.GetComponent<MeshRenderer>().material = unpressedButtonMat;
        }
    }
}
