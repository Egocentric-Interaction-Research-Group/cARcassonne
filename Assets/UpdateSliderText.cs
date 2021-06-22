using Microsoft.MixedReality.Toolkit.UI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpdateSliderText : MonoBehaviour
{
    public GameObject slider;
    private float sliderValue;
    public int nrOfPlayers;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        sliderValue = slider.GetComponent<PinchSlider>().SliderValue;

        if (sliderValue < 0.125f)
        {
            nrOfPlayers = 1;
            gameObject.GetComponent<TextMeshPro>().text = "1";
        }
        else if(sliderValue > 0.125f && sliderValue < 0.5f)
        {
            nrOfPlayers = 2;
            gameObject.GetComponent<TextMeshPro>().text = "2";
        }
        else if (sliderValue > 0.5f && sliderValue < 0.875f)
        {
            nrOfPlayers = 3;
            gameObject.GetComponent<TextMeshPro>().text = "3";
        }
        else if (sliderValue > 0.875f)
        {
            nrOfPlayers = 4;
            gameObject.GetComponent<TextMeshPro>().text = "4";
        }

        PlayerPrefs.SetInt("nrOfPlayers", nrOfPlayers);
    }
}
