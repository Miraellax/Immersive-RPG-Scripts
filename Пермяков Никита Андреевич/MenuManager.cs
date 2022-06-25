using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    public void GoToGame()
    {
        SceneTransition.SwitchToScene("Main scene");
    }

    public void ExitGame()
    {
        Debug.Log("Игра закрылась");
        Application.Quit();
    }
}
