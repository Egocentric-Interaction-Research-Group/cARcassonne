using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class StartGameScript : MonoBehaviour
{
    public Slider playerSlider;
    public Text playerCount;
    private int nbrOfPlayers = 2;
    public PlayerPrefs players;

    // Update is called once per frame
    private void Update()
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