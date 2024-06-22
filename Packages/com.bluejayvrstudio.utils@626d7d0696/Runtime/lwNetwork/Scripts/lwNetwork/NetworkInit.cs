using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bluejayvrstudio;

public class NetworkInit : TempSingleton<NetworkInit>
{
    public bool? IsServer = null;
    public int tickrate;
    void Awake()
    {
        Application.targetFrameRate = 300;
        OVRManager.display.displayFrequency = 120.0f;
    }

    public void StartServer() {
        if (IsServer != null) return;
        GetComponent<NetworkDiscoveryServer>().enabled = true;
        GetComponent<NetworkServer>().enabled = true;
        IsServer = true;
    }

    public void StartClient() {
        if (IsServer != null) return;
        GetComponent<NetworkDiscoveryClient>().enabled = true;
        GetComponent<NetworkClient>().enabled = true;
        IsServer = false;
    }

    public void StopServer() {
        GetComponent<NetworkDiscoveryServer>().enabled = false;
        GetComponent<NetworkServer>().enabled = false;
        IsServer = null;
    }

    public void StopClient() {
        GetComponent<NetworkDiscoveryClient>().enabled = false;
        GetComponent<NetworkClient>().enabled = false;
        IsServer = null;
    }
}
