using UnityEngine;
using UnityEngine.SceneManagement;

public class StartUIscript : MonoBehaviour
{
   
    public void playGame()
    {
        Debug.Log("Kommer till startUi script");
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
}
