using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using System.Collections.Concurrent;
using bluejayvrstudio;
using Google.Protobuf;
using TransformProto;
using ServerMessagesProto;
using ClientMessagesProto;

public class NetworkClient : MonoBehaviour
{
    NetworkDiscoveryClient DiscoveryClient;

    float interval;
    float timer;
    ulong time_stamp = 1;

    Queue<LWINPUT> InputQueue;
    LWINPUT CurrentInput;
    float ack_timeout = 30.0f / 1000;
    float ack_timer = 0.0f;
    public bool FlushInputs = false;

    Dictionary<string, GameObject> NetworkObjects = null;
    public GameObject PlayerPrefab = null;
    ulong stream_collection_time_stamp;
    Dictionary<string, ulong> object_time_stamps;

    public ConcurrentQueue<byte[]> collection_info_queue;
    public ConcurrentQueue<byte[]> network_object_queue;
    public ConcurrentQueue<byte[]> ack_queue;

    void Awake()
    {
        DiscoveryClient = GetComponent<NetworkDiscoveryClient>();
    }

    void OnEnable()
    {
        if (NetworkObjects != null) {
            foreach (string uuid in NetworkObjects.Keys.ToList()) {
                Destroy(NetworkObjects[uuid]);
                NetworkObjects.Remove(uuid);
            }
        }
        interval = 1.0f / NetworkInit.CurrInst.tickrate;
        timer = interval;
        InputQueue = new();
        CurrentInput = null;
        NetworkObjects = new();
        stream_collection_time_stamp = 0;
        object_time_stamps = new();
        collection_info_queue = new();
        network_object_queue = new();
        ack_queue = new();
        
    }

    void OnDisable() {
        OnEnable();
    }

    ulong fails = 0;
    ulong Frames = 0;
    double FailRate;

    public (ulong, ulong, double) GetSuccessRate() {
        return (fails, Frames, FailRate);
    }

    void Update()
    {
        if (FlushInputs) {
            FlushInputs = false;
            ack_timer = 0.0f;
            OnDisable();
        }
        if (DiscoveryClient.ServerIP == null) return;

        // FOR DEBUG
        timer -= Time.deltaTime;
        // if (timer <= 0f) Frames++;
        // if (network_object_queue.Count == 0 && timer <= 0f) fails++;
        // if (Frames % (ulong) NetworkInit.CurrInst.tickrate == 0 && Frames != 0)
        // {
        //     FailRate = (double) fails / Frames;
        //     Debug.Log("Receive fail rate: " + FailRate.ToString() + " %");
        // }

        // Debug.Log("Object Queue Size: " + network_object_queue.Count.ToString());
        while (collection_info_queue.Count > 0 && timer <= 0f) {
            collection_info_queue.TryDequeue(out byte[] toReceive);
            receive_collection(toReceive);}
        while (network_object_queue.Count > 0 && timer <= 0f) {
            network_object_queue.TryDequeue(out byte[] toReceive);
            receive_network_object(toReceive);}
        while (ack_queue.Count > 0 && timer <= 0f) {
            ack_queue.TryDequeue(out byte[] toReceive);
            receive_ack(toReceive);}

        // timer -= Time.deltaTime;
        if (timer <= 0f) {
            timer = interval;
            // Stream controller positions and thumbstick axes
            DiscoveryClient.SendToServer(ClientConsts.CONTROLLER_POSITIONS, prep_positions(), "position sender");
            DiscoveryClient.SendToServer(ClientConsts.AXES, prep_axes(), "axes sender");
            time_stamp++;
        }

        // Add button inputs to input queue
        LWINPUT _input = get_button_input();
        UniversalInputHandler.CurrInst.current_player_input = _input;
        if (_input != null)
        {
            InputQueue.Enqueue(_input);
        }

        // Process input queue
        if (CurrentInput == null) {
            if (InputQueue.Count > 0) {
                CurrentInput = InputQueue.Dequeue();
                DiscoveryClient.SendToServer(ClientConsts.INPUTS, CurrentInput.ToByteArray(), "input sender");
                ack_timer = 0.0f;
            }
        }
        
        // Re-request ack if timeout
        if (ack_timer >= ack_timeout) {
            if (CurrentInput != null) {
                DiscoveryClient.SendToServer(ClientConsts.INPUTS, CurrentInput.ToByteArray(), "ack requester");
                ack_timer = 0.0f;
            }
        }
        ack_timer += Time.deltaTime;
    }

    public void receive_collection(byte[] message) {
        NetworkCollection collection = NetworkCollection.Parser.ParseFrom(message);
        if (collection.TimeStamp < stream_collection_time_stamp) return;

        Dictionary<string, NetworkObjectID> currentCollection = new();

        foreach (NetworkObjectID _objectInfo in collection.Collection) {
            Guid guid = new Guid(_objectInfo.UUID.ToByteArray());
            currentCollection[guid.ToString()] = _objectInfo;
        }
        foreach (string uuid in NetworkObjects.Keys.ToList()) {
            if (!currentCollection.ContainsKey(uuid)) {
                // FOR DEBUG
                // Debug.Log("Destroying network object!");
                Destroy(NetworkObjects[uuid]);
                NetworkObjects.Remove(uuid);
                object_time_stamps.Remove(uuid);
            }
        }
        foreach(string uuid in currentCollection.Keys.ToList()) {
            if (!NetworkObjects.ContainsKey(uuid)) {
                NetworkObjects[uuid] = Instantiate(NetworkItemsManager.CurrInst.ItemLookUp[currentCollection[uuid].Name]);
                object_time_stamps[uuid] = 0;
            }
        }
        stream_collection_time_stamp = collection.TimeStamp;
    }

    public void receive_network_object(byte[] message) {
        NetworkObject _object = NetworkObject.Parser.ParseFrom(message);
        string uuid = new Guid(_object.UUID.ToByteArray()).ToString();
        if (!NetworkObjects.ContainsKey(uuid)) return;
        if (_object.TimeStamp < object_time_stamps[uuid]) {
            // Debug.Log("packet out of order!");
            return; }
        NetworkObjects[uuid.ToString()].GetComponent<INetworkSaveLoad>().NetworkDeserializeBytes(_object.SerializedInfo.ToByteArray());
        object_time_stamps[uuid] = _object.TimeStamp;
    }

    public void receive_ack(byte[] message) {
        ACK ack = ACK.Parser.ParseFrom(message);
        string uuid = new Guid(ack.UUID.ToByteArray()).ToString();
        if (CurrentInput != null && uuid == new Guid(CurrentInput.UUID.ToByteArray()).ToString()) {
            CurrentInput = null;
        }
    }

    private byte[] prep_positions() {
        if (PlayerPrefab == null) return new LWPos().ToByteArray();
        var left = new GameObject();
        left.transform.position = CustomM.GetRelativePosition(PlayerPrefab.GetComponent<IInputHandler>().LeftHand, UniversalInputHandler.CurrInst.RelativeAnchor);
        left.transform.rotation = CustomM.GetRelativeRotation(PlayerPrefab.GetComponent<IInputHandler>().LeftHand, UniversalInputHandler.CurrInst.RelativeAnchor);
        var right = new GameObject();
        right.transform.position = CustomM.GetRelativePosition(PlayerPrefab.GetComponent<IInputHandler>().RightHand, UniversalInputHandler.CurrInst.RelativeAnchor);
        right.transform.rotation = CustomM.GetRelativeRotation(PlayerPrefab.GetComponent<IInputHandler>().RightHand, UniversalInputHandler.CurrInst.RelativeAnchor);

        LWPos lwPositions = new() {
            TimeStamp = time_stamp,
            LControllerTransform = ProtoTransformHelper.FromUnityLocalTransform(left.transform),
            RControllerTransform = ProtoTransformHelper.FromUnityLocalTransform(right.transform)
        };
        Destroy(left);
        Destroy(right);
        return lwPositions.ToByteArray();
    }

    private byte[] prep_axes() {
        if (PlayerPrefab == null) return new LWAxes().ToByteArray();

        LWAxes lwAxes = new() {
            TimeStamp = time_stamp,
            LThumbStick = ProtoTransformHelper.FromUnityVector2(OVRInput.Get(OVRInput.RawAxis2D.LThumbstick)),
            RThumbStick = ProtoTransformHelper.FromUnityVector2(OVRInput.Get(OVRInput.RawAxis2D.RThumbstick)),
        };
        return lwAxes.ToByteArray();
    }

    private LWINPUT get_button_input() {
        if (PlayerPrefab == null) return null;

        LWINPUT toReturn = new() { 
            UUID = ByteString.CopyFrom(Guid.NewGuid().ToByteArray()),
            UpMask = 0,
            DownMask = 0
        };
        
        foreach (OVRInput.RawButton button in Enum.GetValues(typeof(OVRInput.RawButton))) {
            if (button != OVRInput.RawButton.Any && button != OVRInput.RawButton.None && OVRInput.GetDown(button)) {
                toReturn.DownMask |= (int)button;}}

        foreach (OVRInput.RawButton button in Enum.GetValues(typeof(OVRInput.RawButton))) {
            if (button != OVRInput.RawButton.Any && button != OVRInput.RawButton.None && OVRInput.GetUp(button)) {
                toReturn.UpMask |= (int)button; }}

        if (toReturn.DownMask == 0 && toReturn.UpMask == 0) return null;
        return toReturn;
    }
}
