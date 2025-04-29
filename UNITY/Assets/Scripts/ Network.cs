using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using Unity.VisualScripting;
using UnityEngine;

public class Client : MonoBehaviour
{
    public string host = "127.0.0.1";
    public int port = 8181;
    IPAddress address;
    TcpClient tcpClient = new TcpClient();
    bool running;

    void Start()
    {
        print("Starting.");
        ThreadStart t = new ThreadStart(Info);
        Thread net_thread = new Thread(t);
        net_thread.Start();
    }

    void Info()
    {
        address = IPAddress.Parse(host);
        print("Where.");
        tcpClient.Connect(new IPEndPoint(address, port));

        running = false;
        print("Running.");
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

        print("Connection successful?");

        byte[] bufferWrite = Encoding.ASCII.GetBytes("Hello...");
        stream.Write(bufferWrite, 0, bufferWrite.Length);

        byte[] buffer = new byte[tcpClient.ReceiveBufferSize];

        int bytesRead = stream.Read(buffer, 0, tcpClient.ReceiveBufferSize);
        string data = Encoding.UTF8.GetString(buffer, 0, bytesRead);
        print(data);

        running = false;
    }
}