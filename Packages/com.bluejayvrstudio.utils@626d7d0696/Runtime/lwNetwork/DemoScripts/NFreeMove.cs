using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bluejayvrstudio;
using System.Threading.Tasks;
using ClientMessagesProto;

public class NFreeMove : MonoBehaviour
{
    HashSet<(string, bool)> PotentialGrabbers;
    // ("SELF", isLeftHand)
    (string, bool)? CurrentGrabber = null;
    


    void Awake()
    {
        PotentialGrabbers = new();
    }

    void Update()
    {
        // Handle Grab
        foreach ((string, bool) potentialGrabber in PotentialGrabbers) {
            string address = potentialGrabber.Item1;
            bool isLeft = potentialGrabber.Item2;
            LWINPUT input = UniversalInputHandler.CurrInst.input_lookup[address];
            if (input == null) continue;

            GameObject controller = UniversalInputHandler.CurrInst.GetController(address, isLeft);
            if (isLeft && ((int) OVRInput.RawButton.LIndexTrigger & input.DownMask) != 0)
                HandleGrab(controller, potentialGrabber);

            else if (!isLeft && ((int) OVRInput.RawButton.RIndexTrigger & input.DownMask) != 0)
                HandleGrab(controller, potentialGrabber);
        }

        // Handle Release
        if (CurrentGrabber != null)
        {
            string address = CurrentGrabber.Value.Item1;
            bool isLeft = CurrentGrabber.Value.Item2;
            LWINPUT input = UniversalInputHandler.CurrInst.input_lookup[address];
            if (input != null) {
                if (isLeft && ((int)OVRInput.RawButton.LIndexTrigger & input.UpMask) != 0) 
                    HandleSteal();
                
                else if (!isLeft && ((int)OVRInput.RawButton.RIndexTrigger & input.UpMask) != 0) 
                    HandleSteal();
            }
        }
    }

    public void OnTriggerEnter(Collider other) {
        var controllerInfo = other.transform.GetComponent<IControllerCollider>();
        if (controllerInfo == null || controllerInfo.address != "SELF") return;
        PotentialGrabbers.Add((controllerInfo.address, controllerInfo.Left));
        // Debug.Log("local player OnTriggerEnter");
    }
    
    public void OnTriggerExit(Collider other) {
        var controllerInfo = other.transform.GetComponent<IControllerCollider>();
        if (controllerInfo == null || controllerInfo.address != "SELF") return;
        PotentialGrabbers.Remove((controllerInfo.address, controllerInfo.Left));
        // Debug.Log("local player OnTriggerExit");
    }

    public void HandleGrab(GameObject grabber, (string, bool) grabberInfo) {
        HandleSteal();
        CurrentGrabber = grabberInfo;

        transform.SetParent(grabber.transform);
    }

    public void HandleSteal() {
        CurrentGrabber = null;
        transform.SetParent(null);
    }
}
