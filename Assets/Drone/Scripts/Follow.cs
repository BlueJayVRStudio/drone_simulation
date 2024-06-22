using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Follow : MonoBehaviour
{
    public GameObject ToFollow;

    void Update() {
        transform.position = ToFollow.transform.position;
        transform.rotation = ToFollow.transform.rotation;
    }
}
