using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;
using bluejayvrstudio;

public class NetworkObjectsManager : TempSingleton<NetworkObjectsManager>
{
    private HashSet<GameObject> AliveObjects;
    public List<GameObject> toAdd;
    public List<GameObject> toRemove;
    Dictionary<GameObject, string> UUIDLookUp;

    void Awake()
    {
        AliveObjects = new();
        toAdd = new();
        toRemove = new();
    }

    void Start() 
    {
        UUIDLookUp = new();
    }
    
    void Update()
    {
        if (NetworkInit.CurrInst.IsServer != true) return;
        add_items();
        remove_items();
    }

    private void add_items() {
        while (toAdd.Count > 0) {
            var newGuid = Guid.NewGuid().ToString();
            UUIDLookUp[toAdd[toAdd.Count - 1]] = newGuid;
            NetworkServer.CurrInst.AddItem(newGuid, toAdd[toAdd.Count - 1]);
            AliveObjects.Add(toAdd[toAdd.Count - 1]);
            toAdd.RemoveAt(toAdd.Count - 1);
        }
    }
    private void remove_items() {
        while (toRemove.Count > 0) {
            string uuid = UUIDLookUp[toRemove[toRemove.Count - 1]];
            NetworkServer.CurrInst.RemoveItem(uuid);
            UUIDLookUp.Remove(toRemove[toRemove.Count - 1]);
            Destroy(toRemove[toRemove.Count - 1]);
            AliveObjects.Remove(toRemove[toRemove.Count - 1]);
            toRemove.RemoveAt(toRemove.Count - 1);
        }
    }

    public void AddNetworkObject(GameObject go) {
        toAdd.Add(go);
    }

    public void RemoveNetworkObject(GameObject networkObject) {
        toRemove.Add(networkObject);
    }

    public void DeleteAll() {
        foreach (GameObject networkObject in AliveObjects) {
            toRemove.Add(networkObject);
        }
        remove_items();
    }
}
