using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bluejayvrstudio;

public class BladesAddForce : MonoBehaviour
{
    [SerializeField] Rigidbody rb;
    public float magnitude;


    void Update()
    {
        rb.AddForceAtPosition(magnitude * transform.up, transform.position, ForceMode.Impulse);
    }
}
