using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class lwOVRInputHelper : MonoBehaviour
{
    int DownMask = 0;
    int UpMask = 0;

    void Start()
    {
        // foreach (OVRInput.RawButton button in Enum.GetValues(typeof(OVRInput.RawButton))) {
        //     if (OVRInput.GetDown(button)) {
        //         DownMask |= (int) button;
        //     }
        // }
        // DownMask |= (int)OVRInput.RawButton.A;
        // DownMask |= (int)OVRInput.RawButton.B;
        // DownMask |= (int)OVRInput.RawButton.RIndexTrigger;
        // Debug.Log(DownMask);

        // foreach (OVRInput.RawButton button in Enum.GetValues(typeof(OVRInput.RawButton))) {
        //     if ((DownMask & (int) button) != 0) {
        //         Debug.Log(button);
        //     }
        // }

        // DownMask |= (int)OVRInput.RawButton.RIndexTrigger;
        
        Debug.Log((DownMask & (int) OVRInput.RawButton.RIndexTrigger) != 0);
        Debug.Log((DownMask & (int) OVRInput.RawButton.Any) != 0);
    }

    void Update()
    {
        
    }
}
