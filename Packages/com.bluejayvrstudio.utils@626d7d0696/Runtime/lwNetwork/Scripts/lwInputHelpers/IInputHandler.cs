using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bluejayvrstudio;

public interface IInputHandler
{
    public GameObject LeftHand { get; set; }
    public GameObject RightHand { get; set; }
    public void AddRTransform(Vector3 position, Quaternion rotation);
    public void AddLTransform(Vector3 position, Quaternion rotation);
    public void AddRAxes(Vector2 _vector2);
    public void AddLAxes(Vector2 _vector2);
    void OnDestroy() { }
}
