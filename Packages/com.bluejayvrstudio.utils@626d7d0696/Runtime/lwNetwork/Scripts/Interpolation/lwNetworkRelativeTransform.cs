using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bluejayvrstudio;

public class lwNetworkRelativeTransform : MonoBehaviour
{
    Vector3 last_position;
    Vector3 current_position;
    Quaternion last_rotation;
    Quaternion current_rotation;

    float timer = 0;

    void Update()
    {
        if (NetworkInit.CurrInst.IsServer == true) {
            transform.localPosition = Vector3.Lerp(last_position, current_position, timer / (1.0f / NetworkInit.CurrInst.tickrate));
            transform.localRotation = Quaternion.Slerp(last_rotation, current_rotation, timer / (1.0f / NetworkInit.CurrInst.tickrate));
            timer += Time.deltaTime;
        }
    }

    public void set_relative_transform(Vector3 position, Quaternion rotation) {
        if (NetworkInit.CurrInst.IsServer == true) {
            last_position = current_position;
            current_position = CustomM.GetWorldPosition(position, UniversalInputHandler.CurrInst.RelativeAnchor);

            last_rotation = current_rotation;
            current_rotation = CustomM.GetWorldRotation(rotation, UniversalInputHandler.CurrInst.RelativeAnchor);
            timer = 0;
        }
    }
}
