using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bluejayvrstudio;

public class BladesAddForce : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    public float magnitude;

    void FixedUpdate()
    {
        rb.AddForceAtPosition(Mathf.Clamp(magnitude, 0, 1f) * transform.up, transform.position, ForceMode.Force);
    }
}

