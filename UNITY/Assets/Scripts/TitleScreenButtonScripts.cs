using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

public class TitleScreenButtonScripts : MonoBehaviour
{
    public void OnStartButtonClick()
    {
        LoadingScreen.instance.LoadScene(LoadingScreen.Scene.GAME);
        Debug.Log("start button clicked");
    }

    public void OnQuitButtonClick()
    {
#if UNITY_EDITOR
        EditorApplication.ExitPlaymode();
#else
        Application.Quit();
#endif
    }
}
