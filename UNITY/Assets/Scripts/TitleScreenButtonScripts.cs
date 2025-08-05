using DG.Tweening;
using System;
using System.Threading.Tasks;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using static GameState;
using System.Collections;

public class TitleScreenButtonScripts : MonoBehaviour
{
    [SerializeField] private Transform camTransform;
    [SerializeField] private Image camImage;
    [SerializeField] private TMP_Text boardText;
    [SerializeField] private GameObject networkErrorText;

    private void Start()
    {
        Client.network_instance.startCameraFeed(camImage);
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



    private IEnumerator GetBoardCoroutine()
    {
        Task<Side[,]> task = Client.network_instance.GetBoardStateAsync();

        //while getBoardStateAsync is still running wait here for another frame --> yield time back to the main thread (doesn't block)
        while (!task.IsCompleted)
            yield return null;

        Side[,] newBoard = task.Result;

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



    // update calibration screen w/ what the camera sees
    private float clock = 0;
    public bool enableScanning = false;
    private void Update()
    {
        if (!enableScanning) return;
        clock += Time.deltaTime; // only increase clock counter if we're currently not confirming the board state
        if (clock > 1) // every second, see if the read board state is different
        {
            clock = 0;
            //Side[,] newBoard = Client.network_instance.getBoardState();
            StartCoroutine(GetBoardCoroutine());
            //Side[,] newBoard = null;      
        }
    }

}
