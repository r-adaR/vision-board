using DG.Tweening;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GameState;

public class TitleScreenButtonScripts : MonoBehaviour
{
    [SerializeField] private Transform camTransform;
    [SerializeField] private Image camImage;
    [SerializeField] private TMP_Text boardText;
    [SerializeField] private GameObject networkErrorText;


    private int cameraIndex;
    [SerializeField] private TMP_Text indexText;


    private void Start()
    {
        Client.network_instance.startCameraFeed(camImage);

        // this is only for tracking in Unity's UI. does not set the actual camera index
        cameraIndex = VariableStorage.instance.TryGet<int>("camIndex", 0);
        indexText.text = "Camera " + (cameraIndex + 1).ToString();
        indexText.color = VariableStorage.instance.TryGet<bool>("camWorks", true) ? Color.white : Color.red;
    }

    public void OpenGithub()
    {
        Application.OpenURL("https://github.com/r-adaR/vision-board");
    }

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

    public void ToggleCameraPosition()
    {
        camTransform.DOKill();
        if (camTransform.localPosition.x < 0)
        {
            camTransform.DOLocalMoveX(240, 0.5f).SetEase(Ease.InOutCubic);
            enableScanning = true;
        } 
        else
        {
            camTransform.DOLocalMoveX(-260, 0.5f).SetEase(Ease.InOutCubic);
            enableScanning = false;
        }
    }



    private float clock = 0;
    public bool enableScanning = false;
    private bool isChecking = false;

    private void Update()
    {
        if (!enableScanning) return;

        clock += Time.deltaTime;
        if (clock > 0.5f)
        {
            clock = 0;
            if (!isChecking)
                _ = CheckBoardLoopAsync();
        }
    }


    private async Task CheckBoardLoopAsync()
    {
        isChecking = true;
        try
        {
            Side[,] newBoard = await Client.network_instance.GetBoardStateAsync();
            if (Client.network_instance.notConnected) networkErrorText.SetActive(true);
            else networkErrorText.SetActive(false);


            if (newBoard == null)
            {
                boardText.text = "Board not\nfound";
            }
            else
            {
                string str = "";
                for (int y = 0; y < 5; y++)
                {
                    for (int x = 0; x < 5; x++)
                    {
                        str += newBoard[y, x] == Side.X ? "X " : newBoard[y, x] == Side.O ? "O " : "_ ";
                    }
                    str = str.TrimEnd();
                    str += '\n';
                }
                boardText.text = str;
            }

        }
        finally
        {
            isChecking = false;
        }
    }


    public async void IncrementCamera()
    {
        bool result = await Client.network_instance.ChangeCameraFeed(++cameraIndex);
        indexText.text = "Camera " + (cameraIndex + 1).ToString();
        indexText.color = result ? Color.white : Color.red;
        VariableStorage.instance.Put("camIndex", cameraIndex);
        VariableStorage.instance.Put("camWorks", result);
    }

    public async void DecrementCamera()
    {
        if (cameraIndex == 0) return; // no negative indexes allowed
        bool result = await Client.network_instance.ChangeCameraFeed(--cameraIndex);
        indexText.text = "Camera " + (cameraIndex + 1).ToString();
        indexText.color = result ? Color.white : Color.red;
        VariableStorage.instance.Put("camIndex", cameraIndex);
        VariableStorage.instance.Put("camWorks", result);
    }

}
