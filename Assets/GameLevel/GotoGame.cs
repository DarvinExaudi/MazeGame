using UnityEngine;
using UnityEngine.SceneManagement;

public class GotoGame : MonoBehaviour
{

    public void GoToEasy()
    {
        SceneManager.LoadScene("Game");
    }
    public void GoToNormal()
    {
        SceneManager.LoadScene("GameNormal");
    }
    public void GoToHard()
    {
        SceneManager.LoadScene("GameHard");
    }
}
