using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using static GameState;

public class Client : MonoBehaviour
{
    public string host = "127.0.0.1";
    public int port = 8181;
    IPAddress address;
    TcpClient tcpClient = new TcpClient();
    bool cameraFeedActive = false;
    Texture2D tex;

    public bool notConnected = false;

    public static Client network_instance;
    private Image targetImg;

    private void Awake()
    {
        if (network_instance != null)
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
        address = IPAddress.Parse(host);
        tcpClient.Connect(new IPEndPoint(address, port));
    }


    private void OnApplicationQuit()
    {
        closeConnection();
    }



    public Side[,] getBoardState()
    {
        if (GameFlow.flow_instance != null && GameFlow.flow_instance.canScan == false) return null;

        NetworkStream stream;
        try
        {
            stream = tcpClient.GetStream();
            notConnected = false;
        } catch(System.InvalidOperationException)
        {
            notConnected = true;
            return null;
        }

        byte[] bufferWrite = Encoding.ASCII.GetBytes("RGS");
        stream.Write(bufferWrite, 0, bufferWrite.Length);
        byte[] buffer = new byte[tcpClient.ReceiveBufferSize];

        int totalBytesRead = 0;
        do
        {
            int bytesRead = stream.Read(buffer, totalBytesRead, buffer.Length - totalBytesRead);
            totalBytesRead += bytesRead;
        }
        while (stream.DataAvailable);

        string data = Encoding.ASCII.GetString(buffer, 0, totalBytesRead);

        if (data == "ERROR")
        {
            return null;
        }

        GameState.Side[,] boardState = new GameState.Side[5,5];
        int count = 0;

        for (int i = 0; i < 5; i++)
        {
            for (int j = 0; j < 5; j++)
            {
                count = (i * 5) + j;

                //print(data[count]);

                //print(count);
                //print(data.Length);

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
                    if (game_instance.board[i,j] == Side.X || game_instance.board[i, j] == Side.O)
                    {
                        boardState[i, j] = game_instance.board[i, j];
                        // boardState[i, j] = GameState.Side.UNSURE;
                    }
                    else
                    {
                        // if uncertain about a spot that used to be empty, return board scan fail
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

        /*
        string debugString = "\n";
        for (int y=0; y<5; y++)
        {
            for (int x=0; x<5; x++)
            {
                if (boardState[y, x] == Side.NONE) debugString += "_";
                else debugString += boardState[y, x].ToString();
                debugString += " ";
            }
            debugString += '\n';
        }

        print("BOARD STATE RECEIVED: " + debugString);*/
        return boardState;
   

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



    public void displayCameraFeed()
    {
        if (GameFlow.flow_instance != null && GameFlow.flow_instance.canScan == false) return;
        NetworkStream stream = tcpClient.GetStream();

        byte[] bufferWrite = Encoding.ASCII.GetBytes("SCF");
        stream.Write(bufferWrite, 0, bufferWrite.Length);
        byte[] buffer = new byte[tcpClient.ReceiveBufferSize];

        int totalBytesRead = 0;
        do
        {
            int bytesRead = stream.Read(buffer, totalBytesRead, buffer.Length - totalBytesRead);
            totalBytesRead += bytesRead;
        }
        while (stream.DataAvailable);

        tex.LoadImage(buffer);
        targetImg.sprite = Sprite.Create(tex, new Rect(0, 0, 320, 240), Vector2.zero);

    }




    private void Update()
    {

        if (cameraFeedActive && targetImg != null)
        {
            displayCameraFeed();
        }


    }


    void closeConnection()
    {
        NetworkStream stream = tcpClient.GetStream();

        byte[] bufferWrite = Encoding.ASCII.GetBytes("QUIT");
        stream.Write(bufferWrite, 0, bufferWrite.Length);

        tcpClient.Close();
    }




}