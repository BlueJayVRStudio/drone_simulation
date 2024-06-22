using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface INetworkSaveLoad
{
    public string Name { get; set; }
    public byte[] NetworkSerializeBytes();
    public void NetworkDeserializeBytes(byte[] bytearray);
}
