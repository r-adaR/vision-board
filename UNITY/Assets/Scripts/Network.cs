using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.IO;
using System.Drawing;
using Unity.VisualScripting;
using UnityEngine;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine.UI;
using System;

public class Client : MonoBehaviour
{
    public string host = "127.0.0.1";
    public int port = 8181;
    IPAddress address;
    TcpClient tcpClient = new TcpClient();
    bool cameraFeedActive = false;
    Texture2D tex;
    [SerializeField] private Image testimg;


    void Start()
    {
        tex = new Texture2D(320, 240);
        address = IPAddress.Parse(host);
        tcpClient.Connect(new IPEndPoint(address, port));
    }




    void getBoardState()
    {
        NetworkStream stream = tcpClient.GetStream();

        byte[] bufferWrite = Encoding.ASCII.GetBytes("RGS");
        stream.Write(bufferWrite, 0, bufferWrite.Length);
        byte[] buffer = new byte[tcpClient.ReceiveBufferSize];

        // int bytesRead = stream.Read(buffer, 0, buffer.Length);


        int totalBytesRead = 0;
        do
        {
            int bytesRead = stream.Read(buffer, totalBytesRead, buffer.Length - totalBytesRead);
            totalBytesRead += bytesRead;
        }
        while (stream.DataAvailable);

        string data = Encoding.ASCII.GetString(buffer, 0, totalBytesRead);
        print(data);

    }



    void startCameraFeed()
    {
        cameraFeedActive = true;

    }

    void endCameraFeed()
    {
        cameraFeedActive = false;

    }



    void displayCameraFeed()
    {
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
        testimg.sprite = Sprite.Create(tex, new Rect(0, 0, 320, 240), Vector2.zero);

    }




    private void Update()
    {

        if (cameraFeedActive)
        {
            displayCameraFeed();
        }


        if (Input.GetKeyDown(KeyCode.S))
        {
            startCameraFeed();
        }


        if (Input.GetKeyDown(KeyCode.E))
        {
            endCameraFeed();
        }

        if (Input.GetKeyDown(KeyCode.B))
        {
            getBoardState();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            closeConnection();
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