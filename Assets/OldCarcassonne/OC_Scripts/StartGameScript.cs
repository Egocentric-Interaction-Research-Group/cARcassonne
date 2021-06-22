using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGameScript: MonoBehaviour
{
    int nbrOfPlayers = 2;
    public Slider playerSlider;
    public Text playerCount;
    public PlayerPrefs players;

    // Update is called once per frame
    void Update()
    {
        nbrOfPlayers = (int) playerSlider.value;
        playerCount.text = nbrOfPlayers + " players";
    }

    public void GameStartTablet()
    {
        PlayerPrefs.SetString("Platform", "Tablet");
        gameSet();
    }

    public void GameStartComputer()
    {
        PlayerPrefs.SetString("Platform", "Computer");
        gameSet();
    }

    public void gameSet()
    {
        PlayerPrefs.SetInt("PlayerCount", nbrOfPlayers);
        //Debug.Log(nbrOfPlayers);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }

   
}
