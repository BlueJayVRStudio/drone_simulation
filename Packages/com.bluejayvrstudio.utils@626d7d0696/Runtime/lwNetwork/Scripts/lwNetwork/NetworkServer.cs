using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using bluejayvrstudio;
using UnityEngine;
using System.Collections.Concurrent;
using ServerMessagesProto;
using ClientMessagesProto;
using Google.Protobuf;
using TransformProto;
using System.Threading.Tasks;

public class NetworkServer : TempSingleton<NetworkServer>
{
    NetworkDiscoveryServer DiscoveryServer;
    [SerializeField] GameObject PlayerPrefab = null;

    float interval;
    float timer;
    ulong time_stamp;

    Dictionary<string, GameObject> NetworkObjects;
    public void AddItem(string uuid, GameObject _object) {
        NetworkObjects[uuid] = _object;
    }
    public void RemoveItem(string uuid) {
        NetworkObjects.Remove(uuid);
    }

    public bool ClientExists(string address) {
        return DiscoveryServer.ClientExists(address);
    } 

    public ConcurrentQueue<string> InstantiateQueue;
    public ConcurrentQueue<(string, byte[])> positions_queue;
    public ConcurrentQueue<(string, byte[])> axes_queue;
    public ConcurrentQueue<(string, byte[])> input_queue;
    Dictionary<string, ulong> players_transform_time_stamps;
    Dictionary<string, ulong> players_axes_time_stamps;
    HashSet<string> input_uuids;

    void Awake()
    {
        DiscoveryServer = GetComponent<NetworkDiscoveryServer>();
    }

    void OnEnable() 
    {
        time_stamp = 1;
        interval = 1.0f / NetworkInit.CurrInst.tickrate;
        timer = interval;
        NetworkObjects = new();
        players_transform_time_stamps = new();
        players_axes_time_stamps = new();
        InstantiateQueue = new();
        positions_queue = new();
        axes_queue = new();
        input_queue = new();
        input_uuids = new();
    }

    void Update()
    {
        while (InstantiateQueue.Count > 0) {
            InstantiateQueue.TryDequeue(out string address);
            instantiate_player(address);}
        while (positions_queue.Count > 0) {
            positions_queue.TryDequeue(out (string, byte[]) toReceive);
            receive_positions(toReceive.Item1, toReceive.Item2);}
        while (axes_queue.Count > 0) {
            axes_queue.TryDequeue(out (string, byte[]) toReceive);
            receive_axes(toReceive.Item1, toReceive.Item2);}
        while (input_queue.Count > 0) {
            input_queue.TryDequeue(out (string, byte[]) toReceive);
            receive_input(toReceive.Item1, toReceive.Item2);}

        timer -= Time.deltaTime;
        if (timer <= 0f) {
            timer = interval;
            // Stream network objects
            DiscoveryServer.SendToAll(ServerConsts.COLLECTION_INFO, alive_objects());
            foreach (byte[] streamable in prep_stream()) 
                DiscoveryServer.SendToAll(ServerConsts.NETWORK_OBJECT, streamable);
            time_stamp++;
        }
    }

    public void receive_positions(string address, byte[] message) {
        if (!UniversalInputHandler.CurrInst.Players.ContainsKey(address)) return;
        var positions = LWPos.Parser.ParseFrom(message);
        if (positions.TimeStamp < players_transform_time_stamps[address]) return;

        LOCALTRANSFORM LTransform = positions.LControllerTransform;
        LOCALTRANSFORM RTransform = positions.RControllerTransform;
        // Debug.Log(LTransform); 
        UniversalInputHandler.CurrInst.Players[address].GetComponent<IInputHandler>().AddLTransform(ProtoTransformHelper.ToUnityVector3(LTransform.LocalPosition), ProtoTransformHelper.ToUnityQuaternion(LTransform.LocalRotation));
        UniversalInputHandler.CurrInst.Players[address].GetComponent<IInputHandler>().AddRTransform(ProtoTransformHelper.ToUnityVector3(RTransform.LocalPosition), ProtoTransformHelper.ToUnityQuaternion(RTransform.LocalRotation));

        players_transform_time_stamps[address] = positions.TimeStamp;
    }

    public void receive_axes(string address, byte[] message) {
        if (!UniversalInputHandler.CurrInst.Players.ContainsKey(address)) return;
        var axes = LWAxes.Parser.ParseFrom(message);
        if (axes.TimeStamp < players_axes_time_stamps[address]) return;

        VECTOR2 LThumbStick = axes.LThumbStick;
        VECTOR2 RThumbStick = axes.RThumbStick;

        UniversalInputHandler.CurrInst.Players[address].GetComponent<IInputHandler>().AddRAxes(ProtoTransformHelper.ToUnityVector2(LThumbStick));
        UniversalInputHandler.CurrInst.Players[address].GetComponent<IInputHandler>().AddLAxes(ProtoTransformHelper.ToUnityVector2(RThumbStick));

        players_axes_time_stamps[address] = axes.TimeStamp;
    }

    public void receive_input(string address, byte[] message) {
        var lwInput = LWINPUT.Parser.ParseFrom(message);
        string uuid = new Guid(lwInput.UUID.ToByteArray()).ToString();
        
        DiscoveryServer.SendToSpecific(address, ServerConsts.ACK , new ACK(){UUID = ByteString.CopyFrom(lwInput.UUID.ToByteArray())}.ToByteArray());

        if (!input_uuids.Contains(uuid)) {
            input_uuids.Add(uuid);
            // Add input to input_queue in universal input handler
            UniversalInputHandler.CurrInst.AddInput(address, lwInput);
        }
    }

    public byte[] alive_objects() {
        NetworkCollection collection = new();
        collection.TimeStamp = time_stamp;
        foreach (string uuid in NetworkObjects.Keys.ToList()) {
            NetworkObjectID objectID = new() {
                UUID = ByteString.CopyFrom(Guid.Parse(uuid).ToByteArray()),
                Name = NetworkObjects[uuid].transform.GetComponent<INetworkSaveLoad>().Name};

            collection.Collection.Add(objectID);
        }

        return collection.ToByteArray();
    }

    public List<byte[]> prep_stream() {
        List<byte[]> objects = new();
        foreach (string uuid in NetworkObjects.Keys.ToList()) {
            NetworkObject _object = new() {
                UUID = ByteString.CopyFrom(Guid.Parse(uuid).ToByteArray()),
                TimeStamp = time_stamp,
                SerializedInfo = ByteString.CopyFrom(NetworkObjects[uuid].transform.GetComponent<INetworkSaveLoad>().NetworkSerializeBytes())};
            
            objects.Add(_object.ToByteArray());
        }
        return objects;
    }

    public void instantiate_player(string address) {
        if (PlayerPrefab == null) return;
        if (!UniversalInputHandler.CurrInst.Players.ContainsKey(address)) {
            UniversalInputHandler.CurrInst.Players[address] = Instantiate(PlayerPrefab);
            players_transform_time_stamps[address] = 0;
            players_axes_time_stamps[address] = 0;
            UniversalInputHandler.CurrInst.InitializePlayerInputs(address, UniversalInputHandler.CurrInst.Players[address]);
        }
    }

    public void destroy_player(string address) {
        Destroy(UniversalInputHandler.CurrInst.Players[address]);
        UniversalInputHandler.CurrInst.Players.Remove(address);
        players_transform_time_stamps.Remove(address);
        players_axes_time_stamps.Remove(address);
        UniversalInputHandler.CurrInst.CleanUpPlayer(address);
    }
}
