using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TransformProto;

public class ProtoTransformHelper
{
    public static VECTOR2 FromUnityVector2(Vector2 vector2) {
        return new VECTOR2() {
            X = vector2.x,
            Y = vector2.y
        };
    }
    public static Vector2 ToUnityVector2(VECTOR2 vector2) {
        return new Vector2(vector2.X, vector2.Y);
    }

    public static VECTOR3 FromUnityVector3(Vector3 vector3) {
        return new VECTOR3() {
            X = vector3.x,
            Y = vector3.y,
            Z = vector3.z
        };
    }
    public static Vector3 ToUnityVector3(VECTOR3 vector3) {
        return new Vector3(vector3.X, vector3.Y, vector3.Z);
    }

    public static QUATERNION FromUnityQuaternion(Quaternion quaternion) {
        return new QUATERNION() {
            X = quaternion.x,
            Y = quaternion.y,
            Z = quaternion.z,
            W = quaternion.w
        };
    }
    public static Quaternion ToUnityQuaternion(QUATERNION quaternion) {
        var rotation = new Quaternion();
        rotation.x = quaternion.X;
        rotation.y = quaternion.Y;
        rotation.z = quaternion.Z;
        rotation.w = quaternion.W;
        return rotation;
    }

    public static LOCALTRANSFORM FromUnityLocalTransform(Transform transform) {
        return new LOCALTRANSFORM() {
            LocalPosition = new VECTOR3() {
                X = transform.localPosition.x,
                Y = transform.localPosition.y,
                Z = transform.localPosition.z
            },
            LocalRotation = new QUATERNION() {
                X = transform.localRotation.x,
                Y = transform.localRotation.y,
                Z = transform.localRotation.z,
                W = transform.localRotation.w
            }
        };
    }

    public static LOCALTRANSFORM FromUnityLocalTransform(Vector3 localPosition, Quaternion localRotation) {
        return new LOCALTRANSFORM() {
            LocalPosition = new VECTOR3() {
                X = localPosition.x,
                Y = localPosition.y,
                Z = localPosition.z
            },
            LocalRotation = new QUATERNION() {
                X = localRotation.x,
                Y = localRotation.y,
                Z = localRotation.z,
                W = localRotation.w
            }
        };
    }
}
