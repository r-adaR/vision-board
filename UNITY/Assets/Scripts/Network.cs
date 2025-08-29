using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameState;

public class Client : MonoBehaviour
{
    public string host = "127.0.0.1";
    public int port = 8181;
    //IPAddress address;
    IPEndPoint endpt;
    UdpClient udpClient = new UdpClient();
    bool cameraFeedActive = false;
    Texture2D tex;

    public bool notConnected = false;
    public bool IsConnected { get; private set; }

    public static Client network_instance;
    private Image targetImg;

    private void Awake()
    {
        if (network_instance != null && network_instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            network_instance = this;
            DontDestroyOnLoad(gameObject);
        }

        SceneManager.sceneUnloaded += (scene) => { endCameraFeed(); targetImg = null; };
    }

 



    
    void Start()
    {
        tex = new Texture2D(320, 240);
    }
    

    /// <summary>
    /// Sends a ping to the server to ensure that the server is up and running before continuing to the game
    /// </summary>
    /// <param name="token"></param>
    /// <returns></returns>
    public Task<bool> ConnectToServerAsync(CancellationToken token)
    {
        return Task.Run(() =>
        {
            lock (streamLock)
            {
                try
                {
                    udpClient = new UdpClient();
                    endpt = new IPEndPoint(IPAddress.Parse(host), port);
                    udpClient.Connect(endpt);

                    byte[] bufferWrite = Encoding.ASCII.GetBytes("HELLO");
                    udpClient.Send(bufferWrite, bufferWrite.Length);

                    Debug.Log("HELLO sent, waiting for ACK...");

                    byte[] buffer = udpClient.Receive(ref endpt);
                    string data = Encoding.ASCII.GetString(buffer);

                    Debug.Log(data);
                    if (data == "ACK")
                    {
                        IsConnected = true;
                        return IsConnected;
                    }
                    else
                    {
                        IsConnected = false;
                        return IsConnected;
                    }
                }
                catch
                {
                    IsConnected = false;
                    return IsConnected;
                }
            }
        });
    }



    private void OnApplicationQuit()
    {
        closeConnection();
    }

    private readonly object streamLock = new object();

    public Task<Side[,]> GetBoardStateAsync()
    {
        return Task.Run(() =>
        {
            lock (streamLock)
            {

                if (GameFlow.flow_instance != null && GameFlow.flow_instance.canScan == false) return null;

                // Send REQUEST GAME STATE command to server.
                byte[] bufferWrite = Encoding.ASCII.GetBytes("RGS");
                udpClient.Send(bufferWrite, bufferWrite.Length);
                

                // Receive data from server about the game state.
                byte[] buffer = udpClient.Receive(ref endpt);
                string data = Encoding.ASCII.GetString(buffer);


                if (data.StartsWith("ERR"))
                {
                    Debug.LogWarning("Error reading board.\nBackend stack trace: "+data);
                    return null;
                }


                GameState.Side[,] boardState = new GameState.Side[5, 5];
                int count = 0;

                for (int i = 0; i < 5; i++)
                {
                    for (int j = 0; j < 5; j++)
                    {
                        count = (i * 5) + j;


                        if (data[count] == 'X')
                        {
                            boardState[i, j] = GameState.Side.X;
                        }
                        else if (data[count] == 'O')
                        {
                            boardState[i, j] = GameState.Side.O;
                        }
                        else if (data[count] == '?')
                        {
                            
                            // if uncertain for a tile that used to be X or O, then just treat it as X or O
                            if (game_instance.board[i, j] == Side.X || game_instance.board[i, j] == Side.O)
                            {
                                boardState[i, j] = game_instance.board[i, j];
                                print($"1 UNCERTAIN, REPLACED ? WITH :{game_instance.board[i, j].ToString()}");
                                // boardState[i, j] = GameState.Side.UNSURE;
                            }
                            else
                            {
                                // if uncertain about a spot that used to be empty, return board scan fail
                                print($"2 UNCERTAIN, REPLACED ? WITH :{game_instance.board[i, j].ToString()}");
                                boardState[i, j] = game_instance.board[i, j];
                                // return null;
                            }
                        }
                        else
                        {
                            boardState[i, j] = GameState.Side.NONE;
                        }
                    }
                }

                return boardState;
            }
        });
    }





    /// <summary>
    /// displays camera feed onto image passed in as a parameter
    /// </summary>
    /// <param name="img"></param>
    public void startCameraFeed(Image img)
    {
        cameraFeedActive = true;
        targetImg = img;
    }

    public void endCameraFeed()
    {
        cameraFeedActive = false;

    }


    /// <summary>
    /// tries to change the index of which camera python will use. Task returns false if the indexed camera can't be used.
    /// Also prints stack trace.
    /// </summary>
    /// <param name="index"></param>
    /// <returns></returns>
    public Task<bool> ChangeCameraFeed(int index)
    {
        return Task.Run(() =>
        {
            lock (streamLock)
            {
                byte[] bufferWrite = Encoding.ASCII.GetBytes($"CAM{index}");
                udpClient.Send(bufferWrite, bufferWrite.Length);

                byte[] buffer = udpClient.Receive(ref endpt);
                string message = Encoding.ASCII.GetString(buffer);

                bool isError = message.StartsWith("ERR");
                if (isError)
                {
                    Debug.LogError("Error switching camera.\nBackend stack trace: " + message);
                    return message.Contains("ALREADY OPEN");
                }
                Debug.Log(message);
                return true;
            }
        });
    }


    public async Task DisplayCameraFeedAsync()
    {
        byte[] imageData = null;

        //runs the camera I/O data request on a background thread --> doesnt block animations on main thread
        await Task.Run(() =>
        {
            lock (streamLock)
            {
                if (GameFlow.flow_instance != null && !GameFlow.flow_instance.canScan) return;

                // Send SEND CAMERA FRAME command to server.
                byte[] bufferWrite = Encoding.ASCII.GetBytes("SCF");
                udpClient.Send(bufferWrite, bufferWrite.Length);

                // Receive camera frame from the server.
                byte[] buffer = udpClient.Receive(ref endpt);
                imageData = buffer;
                Array.Copy(buffer, imageData, imageData.Length);
            }
        });

        // exited background thread --> can create the image sprite on the main thread (this unity api only works on main thread)
        if (imageData != null && targetImg != null)
        {
            tex.LoadImage(imageData);
            targetImg.sprite = Sprite.Create(tex, new Rect(0, 0, 320, 240), Vector2.zero);
        }
    }




    //used to not overload the network by blocking multiple camera/network calls at the same time
    private bool isLoadingCameraFrame = false;

    private async void Update()
    {
        if (cameraFeedActive && targetImg != null && !isLoadingCameraFrame)
        {
            isLoadingCameraFrame = true;
            await DisplayCameraFeedAsync();
            isLoadingCameraFrame = false;
        }
    }




    void closeConnection()
    {
        // Send QUIT command to close the server.
        byte[] bufferWrite = Encoding.ASCII.GetBytes("QUIT");
        udpClient.Send(bufferWrite, bufferWrite.Length);

        udpClient.Close();
    }




}


