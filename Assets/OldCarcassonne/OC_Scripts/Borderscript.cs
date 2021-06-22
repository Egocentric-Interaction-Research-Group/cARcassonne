using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Borderscript : MonoBehaviour
{

    public Image targetImage;

    public void Start()
    {

    }

    public void ChangeCurrentPlayer(Color32 c)
    {
        targetImage.GetComponent<Image>().color = c;
    }
}