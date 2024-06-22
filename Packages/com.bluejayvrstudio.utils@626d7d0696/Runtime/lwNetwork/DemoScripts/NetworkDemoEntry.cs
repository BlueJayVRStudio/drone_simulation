using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bluejayvrstudio;
using System.Threading.Tasks;

public class NetworkDemoEntry : TempSingleton<NetworkDemoEntry>
{
    [SerializeField] bool WindowStartClient = true;
    async void Start()
    {
# if UNITY_ANDROID && !UNITY_EDITOR_WIN
        while (NetworkInit.CurrInst.IsServer == null) {
            if (OVRInput.GetDown(OVRInput.RawButton.Y)) {
                NetworkInit.CurrInst.StartServer();
                var go = Instantiate(NetworkItemsManager.CurrInst.ItemLookUp["Basketball"]);
                NetworkObjectsManager.CurrInst.toAdd.Add(go);
            }
            else if (OVRInput.GetDown(OVRInput.RawButton.X)) NetworkInit.CurrInst.StartClient();
            await Task.Yield();
        }
# else
        if (WindowStartClient) NetworkInit.CurrInst.StartClient();
        else {
            NetworkInit.CurrInst.StartServer();
            var go = Instantiate(NetworkItemsManager.CurrInst.ItemLookUp["Basketball"]);
            NetworkObjectsManager.CurrInst.toAdd.Add(go);
        }
# endif 
    }
}
