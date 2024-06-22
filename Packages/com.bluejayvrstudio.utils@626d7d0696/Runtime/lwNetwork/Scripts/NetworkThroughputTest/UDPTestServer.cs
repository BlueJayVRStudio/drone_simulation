using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using bluejayvrstudio;
using System.Linq;

public class UDPTestServer : MonoBehaviour
{
    private UdpClient udpClient;
    private string ServerAddress = "your_local_address";
    [SerializeField] private const int listenPort = 8100;

    void Start()
    {
        udpClient = new UdpClient();
        // udpClient.EnableBroadcast = true;
        // udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, listenPort));
    }

    float broadcast_interval = 1.0f / 120;
    float timer = 0f;
    void Update()
    {
        if (timer >= broadcast_interval) {
            timer = 0f;
            for (int i=0; i<50; i++)
                SendBroadcast(String.Concat(Enumerable.Repeat("Hello world! ;)", 670)));
            // Debug.Log("sent!");
        }
        timer += Time.deltaTime;
    }

    public void SendBroadcast(string message) {
        IPAddress remoteIPAddress = IPAddress.Parse(ServerAddress);
        IPEndPoint remoteEndPoint = new IPEndPoint(remoteIPAddress, listenPort);
        byte[] bytes = Encoding.UTF8.GetBytes("AnyRecC_D" + message);
        udpClient.Send(bytes, bytes.Length, remoteEndPoint);
    }

    void OnApplicationQuit() {
        udpClient.Close();
    }
}
