using System.Collections;
using System.Collections.Generic;
using Google.Protobuf;
using TransformProto;
using UnityEngine;
using bluejayvrstudio;

public class NetworkBasketball : MonoBehaviour, INetworkSaveLoad
{
    private string name;
    public string Name { get { return name; } set { name = value; } }

    [SerializeField] List<Component> ComponentsToDisable;
    [SerializeField] List<GameObject> ToDestroy;

    void Awake()
    {
        if (NetworkInit.CurrInst.IsServer == false) {
            transform.SetParent(UniversalInputHandler.CurrInst.RelativeAnchor.transform);
            for (int i = 0; i < ComponentsToDisable.Count; i++) Destroy(ComponentsToDisable[i]);
            for (int i = 0; i < ToDestroy.Count; i++) Destroy(ToDestroy[i]);
        }

        name = "Basketball";
    }

    public byte[] NetworkSerializeBytes() {
        var go = new GameObject();
        go.transform.position = CustomM.GetRelativePosition(transform.gameObject, UniversalInputHandler.CurrInst.RelativeAnchor);
        go.transform.rotation = CustomM.GetRelativeRotation(transform.gameObject, UniversalInputHandler.CurrInst.RelativeAnchor);

        var toReturn = ProtoTransformHelper.FromUnityLocalTransform(go.transform).ToByteArray();
        Destroy(go);
        
        return toReturn;
    }

    public void NetworkDeserializeBytes(byte[] bytearray) {
        var _transform = LOCALTRANSFORM.Parser.ParseFrom(bytearray);
        GetComponent<lwNetworkTransform>().set_transform(ProtoTransformHelper.ToUnityVector3(_transform.LocalPosition), ProtoTransformHelper.ToUnityQuaternion(_transform.LocalRotation));
    }

}
