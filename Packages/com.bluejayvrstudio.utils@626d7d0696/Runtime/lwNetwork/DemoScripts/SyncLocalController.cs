using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SyncLocalController : MonoBehaviour
{
    [SerializeField] GameObject LHand;
    [SerializeField] GameObject RHand;
    [SerializeField] bool SyncLeft = true;
    void Update()
    {
        if (SyncLeft)
        {
            LHand.transform.position = transform.position;
            LHand.transform.rotation = transform.rotation;
        }
        else
        {
            RHand.transform.position = transform.position;
            RHand.transform.rotation = transform.rotation;
        }
    }
}
