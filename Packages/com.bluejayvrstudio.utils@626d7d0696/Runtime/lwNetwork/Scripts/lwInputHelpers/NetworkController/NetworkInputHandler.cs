using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkInputHandler : MonoBehaviour, IInputHandler
{
    [SerializeField] private GameObject leftHand;
    public GameObject LeftHand {
        get { return leftHand; }
        set { leftHand = value; }
    }
    
    [SerializeField] private GameObject rightHand;
    public GameObject RightHand {
        get { return rightHand; }
        set { rightHand = value; }
    }

    public void AddLTransform(Vector3 position, Quaternion rotation) {
        leftHand.GetComponent<lwNetworkRelativeTransform>().set_relative_transform(position, rotation);
    }
    public void AddRTransform(Vector3 position, Quaternion rotation) {
        rightHand.GetComponent<lwNetworkRelativeTransform>().set_relative_transform(position, rotation);
    }

    public void AddLAxes(Vector2 axis) {}
    public void AddRAxes(Vector2 axis) {}

}
