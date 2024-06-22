using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bluejayvrstudio;
using System;
using Google.Protobuf;
using TransformProto;

public class TestBall : MonoBehaviour, INetworkSaveLoad
{
    private string name;
    public string Name {
        get { return name; }
        set { name = value; }
    }

    void Awake() 
    {
        name = "TestBall";
    }

    public byte[] NetworkSerializeBytes() {
        return new LOCALTRANSFORM() { LocalPosition = new VECTOR3() { X = transform.localPosition.x, Y = transform.localPosition.y, Z = transform.localPosition.z }, LocalRotation = new QUATERNION() { X = transform.localRotation.x, Y = transform.localRotation.y, Z = transform.localRotation.z, W = transform.localRotation.w } }.ToByteArray();
    }

    public void NetworkDeserializeBytes(byte[] bytearray) {
        LOCALTRANSFORM localTransform =  LOCALTRANSFORM.Parser.ParseFrom(bytearray);
        bluejayvrstudio._Vector3 localPos = new bluejayvrstudio._Vector3(localTransform.LocalPosition.X, localTransform.LocalPosition.Y, localTransform.LocalPosition.Z);
        bluejayvrstudio._Quaternion localRot = new bluejayvrstudio._Quaternion(localTransform.LocalRotation.X, localTransform.LocalRotation.Y, localTransform.LocalRotation.Z, localTransform.LocalRotation.W);
        GetComponent<Interpolations>().set_transform(localPos.ToUnityVector3(), localRot.ToUnityQuaternion());
    }
}
