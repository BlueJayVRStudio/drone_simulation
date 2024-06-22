using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bluejayvrstudio;

public class NetworkItemsManager : TempSingleton<NetworkItemsManager>
{
    public List<GameObject> Items;
    public Dictionary<string, GameObject> ItemLookUp;

    void Awake()
    {
        ItemLookUp = new();
        foreach (GameObject i in Items) ItemLookUp[i.transform.name] = i;
    }
}
