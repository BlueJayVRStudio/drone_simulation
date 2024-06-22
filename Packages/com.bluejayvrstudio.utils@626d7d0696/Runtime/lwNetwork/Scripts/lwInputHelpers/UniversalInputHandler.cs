using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using bluejayvrstudio;
using ClientMessagesProto;
using Google.Protobuf;
using System.Collections.Concurrent;

public class UniversalInputHandler : TempSingleton<UniversalInputHandler>
{
    private Dictionary<string, Queue<LWINPUT>> input_queue;
    public Dictionary<string, LWINPUT> input_lookup;
    public LWINPUT current_player_input = null;
    public GameObject PlayerPrefab = null;
    public GameObject RelativeAnchor = null;

    public Dictionary<string, GameObject> Players;

    public GameObject GetController(string address, bool IsLeft) {
        if (IsLeft) return Players[address].GetComponent<IInputHandler>().LeftHand;
        else return Players[address].GetComponent<IInputHandler>().RightHand;
    }

    void AddSelf(GameObject prefab) {
        Players["SELF"] = prefab;
    }

    void Awake()
    {
        input_queue = new();
        input_lookup = new();
        Players = new();
        AddSelf(PlayerPrefab);
    }

    void Update()
    {
        input_lookup["SELF"] = get_button_input();
        current_player_input = input_lookup["SELF"];

        // FOR DEBUG
        // if (input_lookup["SELF"] != null) {
        //     if (input_lookup["SELF"].DownMask != 0 ) {
        //         foreach (OVRInput.RawButton button in Enum.GetValues(typeof(OVRInput.RawButton))) {
        //             if (((int) button & input_lookup["SELF"].DownMask) != 0) {
        //                 Debug.Log("down: " + button.ToString());}}
        //     }

        //     if (input_lookup["SELF"].UpMask != 0 ) {
        //         foreach (OVRInput.RawButton button in Enum.GetValues(typeof(OVRInput.RawButton))) {
        //             if (((int) button & input_lookup["SELF"].UpMask) != 0) {
        //                 Debug.Log("up: " + button.ToString());}}
        //     }
        // }

        foreach (string address in input_queue.Keys.ToList()) {
            if (input_queue[address].Count > 0) {
                var input = input_queue[address].Dequeue();
                input_lookup[address] = input;}
            else input_lookup[address] = null;}
    }

    public void InitializePlayerInputs(string address, GameObject playerPrefab) {
        input_queue[address] = new Queue<LWINPUT>();
        input_lookup[address] = null;
        playerPrefab.GetComponent<IInputHandler>().LeftHand.GetComponent<IControllerCollider>().address = address;
        playerPrefab.GetComponent<IInputHandler>().RightHand.GetComponent<IControllerCollider>().address = address;
        playerPrefab.GetComponent<IInputHandler>().LeftHand.GetComponent<IControllerCollider>().Left = true;
        playerPrefab.GetComponent<IInputHandler>().RightHand.GetComponent<IControllerCollider>().Left = false;
    }

    public void CleanUpPlayer(string address) {
        input_queue.Remove(address);
        input_lookup.Remove(address);
    }

    public void AddInput(string address, LWINPUT input) {
        input_queue[address].Enqueue(input);
    }

    private LWINPUT get_button_input() {
        LWINPUT toReturn = new() { 
            UUID = ByteString.CopyFrom(Guid.NewGuid().ToByteArray()),
            UpMask = 0,
            DownMask = 0
        };
        
        foreach (OVRInput.RawButton button in Enum.GetValues(typeof(OVRInput.RawButton))) {
            if (button != OVRInput.RawButton.Any && button != OVRInput.RawButton.None && OVRInput.GetDown(button)) {
                // Debug.Log("down: " + button.ToString());
                toReturn.DownMask |= (int)button;}}

        foreach (OVRInput.RawButton button in Enum.GetValues(typeof(OVRInput.RawButton))) {
            if (button != OVRInput.RawButton.Any && button != OVRInput.RawButton.None && OVRInput.GetUp(button)) {
                // Debug.Log("up: " + button.ToString());
                toReturn.UpMask |= (int)button; }}

        if (toReturn.DownMask == 0 && toReturn.UpMask == 0) return null;
        return toReturn;
    }
}
