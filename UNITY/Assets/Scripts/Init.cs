/*
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Init : MonoBehaviour
{
    void Start()
    {
        DontDestroyOnLoad(gameObject);

        // global var setup
        VariableStorage.instance.Put("loadWithAnimation", false);

        // first show INIT screen
        SceneManager.LoadScene((int)LoadingScreen.Scene.INIT);

        // wait until network connection confirmed
        StartCoroutine(WaitForServerThenTitle());
    }

    private IEnumerator WaitForServerThenTitle()
    {
        //Client client = null;

        Client client = Client.network_instance;

        // Wait until Client.network_instance is assigned
        while (client == null)
        {
            client = Client.network_instance;
            yield return null; // wait a frame
            print("WAITING FOR CLIENT");
        }

        Debug.Log("Client found, waiting for server connection...");

        while (!client.IsConnected)
        {
            yield return null; // keep waiting
        }

        Debug.Log("Server connection established!");
        yield return new WaitForSecondsRealtime(0.5f);

        SceneManager.LoadScene((int)LoadingScreen.Scene.TITLE);
        Destroy(gameObject);
    }
    */



using System.Threading;
using System.Threading.Tasks;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.SceneManagement;





public class Init : MonoBehaviour
{
    private CancellationTokenSource cts = new CancellationTokenSource();
    public static Init init_instance;


    private void OnApplicationQuit()
    {
        cts.Cancel(); 
    }

    private void OnDisable()
    {
        cts.Cancel();
    }


    private async void Start()
    {

        //ensures only one init instance
        if (init_instance != null && init_instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            init_instance = this;
        }

        VariableStorage.instance.Put("loadWithAnimation", false);

        if (Client.network_instance != null)
        {
            print("CONNECTED TO CLIENT");
        }

        bool connected = false;

        // loops connection requests until ACK recieved from server
        // cancelation token used in case the game is ended before connection is made with the server
        while (!connected && !cts.IsCancellationRequested)
        {
            connected = await Client.network_instance.ConnectToServerAsync(cts.Token);
            if (!connected)
                await Task.Delay(500, cts.Token);
        }

        SceneManager.LoadScene((int)LoadingScreen.Scene.TITLE);
        Destroy(gameObject);
    }
}
