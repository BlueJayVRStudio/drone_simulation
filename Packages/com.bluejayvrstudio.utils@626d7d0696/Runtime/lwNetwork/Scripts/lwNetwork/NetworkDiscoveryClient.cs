using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using System.Linq;

[RequireComponent(typeof(NetworkClient))]
public class NetworkDiscoveryClient : MonoBehaviour
{
    private UdpClient udpClient;
    private IPAddress ServerAddress = null;
    [SerializeField] private const int listenPort = 8100;
    [SerializeField] float timeout_limit = 2.0f;
    private float alive_timer = 0f;

    public IPAddress ServerIP {
        get { return ServerAddress; }
    }

    NetworkClient Client;

    void Awake() 
    {
        Client = GetComponent<NetworkClient>();
    }
    
    void OnEnable()
    {
        udpClient = new UdpClient() { EnableBroadcast = true };
        udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, listenPort));
        Socket udpSocket = udpClient.Client;
        // udpSocket.DontFragment = true;
        udpSocket.ReceiveBufferSize = 65507;

        StartListening();
        // StartThreadedListener();
        SendBroadcast();
    }

    float broadcast_interval = 1.5f;
    float timer = 0f;
    void Update()
    {
        if (timer >= broadcast_interval) {
            timer = 0f;
            if (ServerAddress == null) {
                SendBroadcast();
                // Debug.Log("Sending discovery request");
            }
        }
        timer += Time.deltaTime;

        if (ServerAddress != null)
        {
            if (alive_timer >= timeout_limit)
            {
                ServerAddress = null;
                Client.FlushInputs = true;
            }

            alive_timer += Time.deltaTime;
        }
    }

    void SendBroadcast()
    {
        var sendBytes = Encoding.UTF8.GetBytes(ClientConsts.CONNECTION_REQUEST);
        IPEndPoint endPoint = new IPEndPoint(IPAddress.Broadcast, listenPort);
        udpClient.Send(sendBytes, sendBytes.Length, endPoint);
    }

    void StartListening() => udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);

    void ReceiveCallback(IAsyncResult ar) {
        try {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, listenPort);
            byte[] bytes = udpClient.EndReceive(ar, ref ip);
        
            if (bytes.Length >= 9)
            {
                string message = Encoding.UTF8.GetString(bytes.Skip(0).Take(9).ToArray());

                if (message == ServerConsts.CONNECTION_REPLY) {
                    ServerAddress = ip.Address;
                    alive_timer = 0f;
                    Debug.Log("Received response from: " + ip.Address.ToString() + " Message: " + message);
                }
                if (message == ServerConsts.COLLECTION_INFO) {
                    if (ServerAddress != null && ip.Address.ToString() == ServerAddress.ToString() && alive_timer < timeout_limit) {
                        alive_timer = 0f;
                        Client.collection_info_queue.Enqueue(bytes.Skip(9).Take(bytes.Length - 9).ToArray());
                    }
                }
                if (message == ServerConsts.NETWORK_OBJECT) {
                    if (ServerAddress != null && ip.Address.ToString() == ServerAddress.ToString() && alive_timer < timeout_limit) {
                        alive_timer = 0f;
                        Client.network_object_queue.Enqueue(bytes.Skip(9).Take(bytes.Length - 9).ToArray());
                    }
                }
                if (message == ServerConsts.ACK) {
                    if (ServerAddress != null && ip.Address.ToString() == ServerAddress.ToString() && alive_timer < timeout_limit) {
                        alive_timer = 0f;
                        Client.ack_queue.Enqueue(bytes.Skip(9).Take(bytes.Length - 9).ToArray());
                    }
                }
            }
            // continue listening
            udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
        }
        catch (SocketException e) {
            if (e.SocketErrorCode == SocketError.ConnectionReset) {
                Debug.LogWarning("Connection was forcibly closed by the remote host.");
                udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
                return;
            }
            Debug.Log(e);
            Debug.Log("UDP Client Closed");
        }
    }

    

    private Thread listenerThread;
    // private UdpClient udpClient;
    private bool isListening = false;
    
    void StartThreadedListener()
    {
        // udpClient = new UdpClient(8100); // Replace with your port number
        isListening = true;
        listenerThread = new Thread(ThreadedReceive);
        listenerThread.IsBackground = true;
        listenerThread.Start();
    }

    void ThreadedReceive() {
        while (true) {
            try{
                IPEndPoint ip = new IPEndPoint(IPAddress.Any, listenPort);
                byte[] bytes = udpClient.Receive(ref ip);

                if (bytes.Length >= 9)
                {
                    string message = Encoding.UTF8.GetString(bytes.Skip(0).Take(9).ToArray());

                    if (message == ServerConsts.CONNECTION_REPLY)
                    {
                        ServerAddress = ip.Address;
                        alive_timer = 0f;
                        Debug.Log("Received response from: " + ip.Address.ToString() + " Message: " + message);
                    }
                    if (message == ServerConsts.COLLECTION_INFO)
                    {
                        if (ServerAddress != null && ip.Address.ToString() == ServerAddress.ToString() && alive_timer < timeout_limit)
                        {
                            alive_timer = 0f;
                            Client.collection_info_queue.Enqueue(bytes.Skip(9).Take(bytes.Length - 9).ToArray());
                        }
                    }
                    if (message == ServerConsts.NETWORK_OBJECT)
                    {
                        if (ServerAddress != null && ip.Address.ToString() == ServerAddress.ToString() && alive_timer < timeout_limit)
                        {
                            alive_timer = 0f;
                            Client.network_object_queue.Enqueue(bytes.Skip(9).Take(bytes.Length - 9).ToArray());
                        }
                    }
                    if (message == ServerConsts.ACK)
                    {
                        if (ServerAddress != null && ip.Address.ToString() == ServerAddress.ToString() && alive_timer < timeout_limit)
                        {
                            alive_timer = 0f;
                            Client.ack_queue.Enqueue(bytes.Skip(9).Take(bytes.Length - 9).ToArray());
                        }
                    }
                }
            }
            catch (SocketException e) {
                if (e.SocketErrorCode == SocketError.ConnectionReset) Debug.LogWarning("Connection was forcibly closed by the remote host.");
                else {
                    Debug.Log(e);
                    Debug.Log("UDP Client Closed");
                    break;
                }
            }
        }
    }




    public void SendToServer(string header, byte[] message, string caller) {
        if (ServerAddress == null) return;
        byte[] bytes = Encoding.UTF8.GetBytes(header).Concat(message).ToArray();

        IPAddress remoteIPAddress = IPAddress.Parse(ServerAddress.ToString());
        IPEndPoint remoteEndPoint = new IPEndPoint(remoteIPAddress, listenPort);
        udpClient.Send(bytes, bytes.Length, remoteEndPoint);
    }

    void OnApplicationQuit() {
        udpClient?.Close();
    }
    void OnDestroy() {
        udpClient?.Close();
    }
    void OnDisable() {
        udpClient?.Close();
    }
}
