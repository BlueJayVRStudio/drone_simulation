using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerConsts
{
    public const string CONNECTION_REPLY = "AnyRecS_C";
    public const string COLLECTION_INFO = "AnyRecS_A";
    public const string NETWORK_OBJECT = "AnyRecS_O";
    public const string ACK = "AnyRecS_K";
}

public class ClientConsts
{
    public const string CONNECTION_REQUEST = "AnyRecC_C";
    public const string CONTROLLER_POSITIONS = "AnyRecC_P";
    public const string AXES = "AnyRecC_A";
    public const string INPUTS = "AnyRecC_I";
}