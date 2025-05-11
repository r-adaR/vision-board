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
    bool running;
    Texture2D tex;
    [SerializeField] private Image testimg;


    void Start()
    {
        tex = new Texture2D(320, 240);
        //ThreadStart t = new ThreadStart(Info);
        //Thread net_thread = new Thread(t);
        //net_thread.Start();
        Info();
    }

    void Info()
    {
        address = IPAddress.Parse(host);
        tcpClient.Connect(new IPEndPoint(address, port));

        running = false;
        Communication();
        while (running)
        {
            Communication();
        }
        tcpClient.Close();
    }

    void Communication()
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
        testimg.sprite = Sprite.Create(tex, new Rect(0,0,320,240), Vector2.zero);

        byte[] b = Encoding.ASCII.GetBytes("q");
        stream.Write(b, 0, b.Length);

        running = false;
    }
}