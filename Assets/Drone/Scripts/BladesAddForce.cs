using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bluejayvrstudio;

public class BladesAddForce : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    public float magnitude;

    public bool isNormal;

    void Update()
    {
        rb.AddForceAtPosition(Mathf.Clamp(magnitude, 0, 1f) * transform.up, transform.position, ForceMode.Force);
        // Debug.Log($"{transform.name}, {transform.position}");
    }
}

