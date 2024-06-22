using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LocalInputCollider : MonoBehaviour, IControllerCollider
{
    [SerializeField] private string Address;
    public string address {
        get { return Address; }
        set { Address = value; }
    }

    [SerializeField] private bool left;
    public bool Left {
        get { return left; }
        set { left = value; }
    }
}
