using UnityEngine;
using UnityEngine.SceneManagement;

public class Init : MonoBehaviour
{
    // do this upon starting up the program a SINGLE time
    void Start()
    {
        VariableStorage.instance.Put("loadWithAnimation", false);
        SceneManager.LoadScene((int)LoadingScreen.Scene.TITLE); // load title screen
    }

}
