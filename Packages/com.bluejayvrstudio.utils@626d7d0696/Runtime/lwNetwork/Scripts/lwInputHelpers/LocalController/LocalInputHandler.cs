using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalInputHandler : MonoBehaviour, IInputHandler
{
    [SerializeField] private GameObject leftHand;
    public GameObject LeftHand {
        get { return leftHand; }
        set { leftHand = value; }
    }
    
    [SerializeField]  private GameObject rightHand;
    public GameObject RightHand {
        get { return rightHand; }
        set { rightHand = value; }
    }

    public void AddLTransform(Vector3 position, Quaternion rotation) {}
    public void AddRTransform(Vector3 position, Quaternion rotation) {}
    public void AddLAxes(Vector2 axis) {}
    public void AddRAxes(Vector2 axis) {}

}
