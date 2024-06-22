using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Linq;
using ClientMessagesProto;
using System.Threading;
// using UnityEditor.PackageManager;
// using Google.Protobuf;

[RequireComponent(typeof(NetworkServer))]
public class NetworkDiscoveryServer : MonoBehaviour
{
    private UdpClient udpClient;
    [SerializeField] private const int listenPort = 8100;
    float timeout_limit = 2.0f;

    NetworkServer Server;
    Dictionary<string, float> ClientLookUp = null;
    public bool ClientExists(string address) {
        return ClientLookUp.ContainsKey(address);
    }

    Queue<(string , int)> DestroyQueue;
    
    void Awake() 
    {
        Server = GetComponent<NetworkServer>();
        DestroyQueue = new();
    }
    void OnEnable() 
    {
        if (ClientLookUp == null) ClientLookUp = new();
        foreach (string address in ClientLookUp.Keys.ToList()) {
            init_player_destroy(address);
            Server.destroy_player(address);
            ClientLookUp.Remove(address);
        }

        udpClient = new UdpClient();
        udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, listenPort));
        Socket udpSocket = udpClient.Client;
        // udpSocket.DontFragment = true;
        udpSocket.ReceiveBufferSize = 65507;

        StartListening();
        // StartThreadedListener();
        Debug.Log("Is Listening");
    }

    void OnDisable() {
        udpClient?.Close();
        foreach (string address in ClientLookUp.Keys.ToList()) {
            init_player_destroy(address);
            Server.destroy_player(address);
            ClientLookUp.Remove(address);
        }
    }

    void Update() 
    {
        // 1. destroy players in queue (order matters)
        List<(string, int)> toAddBack = new();
        while (DestroyQueue.Count > 0) {
            var toDestroy = DestroyQueue.Dequeue();
            // if it the destroy request is from the first loop, put it back into the queue with updated time-to-live of 0
            // this will ensure that all network objects will see the manual input
            if (toDestroy.Item2 > 0) toAddBack.Add((toDestroy.Item1, 0));
            else Server.destroy_player(toDestroy.Item1); }

        foreach ((string, int) addBack in toAddBack) DestroyQueue.Enqueue(addBack);

        // 2. initialize player destroy (order matters)
        foreach (string address in ClientLookUp.Keys.ToList()) {
            ClientLookUp[address] += Time.deltaTime;
            if (ClientLookUp[address] >= timeout_limit) {
                init_player_destroy(address);
                DestroyQueue.Enqueue((address, 1));
                ClientLookUp.Remove(address);
            }
        }
    }

    void StartListening() => udpClient.BeginReceive(new AsyncCallback(ReceiveCallback), null);
    
    void ReceiveCallback(IAsyncResult ar) {
        try {
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, listenPort);
            byte[] bytes = udpClient.EndReceive(ar, ref ip);
            string clientAddress = ip.Address.ToString();

            if (bytes.Length >= 9)
            {
                string message = Encoding.UTF8.GetString(bytes.Skip(0).Take(9).ToArray());

                if (message == ClientConsts.CONNECTION_REQUEST) {
                    Debug.Log("Received response from: " + clientAddress + " Message: " + message);
                    var serverPort = 8100;
                    var sendBytes = Encoding.UTF8.GetBytes(ServerConsts.CONNECTION_REPLY);
                    IPEndPoint endPoint = new IPEndPoint(ip.Address, serverPort);
                    udpClient.Send(sendBytes, sendBytes.Length, endPoint);
                    ClientLookUp[clientAddress] = 0;
                    Server.InstantiateQueue.Enqueue(clientAddress);
                }
                if (message == ClientConsts.CONTROLLER_POSITIONS) {
                    if (ClientLookUp.ContainsKey(clientAddress) && ClientLookUp[clientAddress] < timeout_limit) {
                        ClientLookUp[clientAddress] = 0f;
                        Server.positions_queue.Enqueue((clientAddress, bytes.Skip(9).Take(bytes.Length - 9).ToArray()));
                    }
                }
                if (message == ClientConsts.AXES) {
                    if (ClientLookUp.ContainsKey(clientAddress) && ClientLookUp[clientAddress] < timeout_limit) {
                        ClientLookUp[clientAddress] = 0f;
                        Server.axes_queue.Enqueue((clientAddress, bytes.Skip(9).Take(bytes.Length - 9).ToArray()));
                    }
                }
                if (message == ClientConsts.INPUTS) {
                    if (ClientLookUp.ContainsKey(clientAddress) && ClientLookUp[clientAddress] < timeout_limit) {
                        ClientLookUp[clientAddress] = 0f;
                        Server.input_queue.Enqueue((clientAddress, bytes.Skip(9).Take(bytes.Length - 9).ToArray()));
                    }
                }
            }

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
                string clientAddress = ip.Address.ToString();

                if (bytes.Length >= 9)
                {
                    string message = Encoding.UTF8.GetString(bytes.Skip(0).Take(9).ToArray());

                    if (message == ClientConsts.CONNECTION_REQUEST) {
                        Debug.Log("Received response from: " + clientAddress + " Message: " + message);
                        var serverPort = 8100;
                        var sendBytes = Encoding.UTF8.GetBytes(ServerConsts.CONNECTION_REPLY);
                        IPEndPoint endPoint = new IPEndPoint(ip.Address, serverPort);
                        udpClient.Send(sendBytes, sendBytes.Length, endPoint);
                        ClientLookUp[clientAddress] = 0;
                        Server.InstantiateQueue.Enqueue(clientAddress);
                    }
                    if (message == ClientConsts.CONTROLLER_POSITIONS) {
                        if (ClientLookUp.ContainsKey(clientAddress) && ClientLookUp[clientAddress] < timeout_limit) {
                            ClientLookUp[clientAddress] = 0f;
                            Server.positions_queue.Enqueue((clientAddress, bytes.Skip(9).Take(bytes.Length - 9).ToArray()));
                        }
                    }
                    if (message == ClientConsts.AXES) {
                        if (ClientLookUp.ContainsKey(clientAddress) && ClientLookUp[clientAddress] < timeout_limit) {
                            ClientLookUp[clientAddress] = 0f;
                            Server.axes_queue.Enqueue((clientAddress, bytes.Skip(9).Take(bytes.Length - 9).ToArray()));
                        }
                    }
                    if (message == ClientConsts.INPUTS) {
                        if (ClientLookUp.ContainsKey(clientAddress) && ClientLookUp[clientAddress] < timeout_limit) {
                            ClientLookUp[clientAddress] = 0f;
                            Server.input_queue.Enqueue((clientAddress, bytes.Skip(9).Take(bytes.Length - 9).ToArray()));
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



    

    public void SendToAll(string header, byte[] message) {
        byte[] bytes = Encoding.UTF8.GetBytes(header).Concat(message).ToArray();

        foreach (string address in ClientLookUp.Keys.ToList()) {
            if (ClientLookUp[address] < timeout_limit) {
                IPAddress remoteIPAddress = IPAddress.Parse(address);
                IPEndPoint remoteEndPoint = new IPEndPoint(remoteIPAddress, listenPort);
                udpClient.Send(bytes, bytes.Length, remoteEndPoint);}}}

    public void SendToSpecific(string address, string header, byte[] message) {
        if (!ClientLookUp.ContainsKey(address) || ClientLookUp[address] >= timeout_limit) return;
        IPAddress remoteIPAddress = IPAddress.Parse(address);
        IPEndPoint remoteEndPoint = new IPEndPoint(remoteIPAddress, listenPort);
        byte[] bytes = Encoding.UTF8.GetBytes(header).Concat(message).ToArray();
        udpClient.Send(bytes, bytes.Length, remoteEndPoint);}

    public void init_player_destroy(string address) {
        if (!UniversalInputHandler.CurrInst.Players.ContainsKey(address)) return;
        // Add fake index trigger up input and wait one frame
        LWINPUT fake = new();
        fake.DownMask = 0;
        fake.UpMask = 0;
        fake.UpMask |= (int)OVRInput.RawButton.RIndexTrigger;
        fake.UpMask |= (int)OVRInput.RawButton.LIndexTrigger;
        UniversalInputHandler.CurrInst.AddInput(address, fake);}

    void OnApplicationQuit() { udpClient?.Close(); }
    void OnDestroy() { udpClient?.Close(); }
}
