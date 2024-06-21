using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bluejayvrstudio;
using System;

public class BladesRotation : MonoBehaviour
{
    public float RPS = 1000f;
    float AngularVelocity;
    
    void Start()
    {
        AngularVelocity = RPS * 360f;
    }

    void Update()
    {
        AngularVelocity = RPS * 360f;
        Vector3 localAngles = transform.localEulerAngles;
        transform.localRotation = Quaternion.Euler(new Vector3(localAngles.x, AngularVelocity * Time.deltaTime + localAngles.y, localAngles.z));
    }
}
