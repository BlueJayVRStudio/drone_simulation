using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bluejayvrstudio;
using System.Threading.Tasks;
using ClientMessagesProto;
using System.Linq;

public class NetworkFreeMove : MonoBehaviour, INetworkGrabbable
{
    HashSet<(string, bool)> PotentialGrabbers;
    // ("SELF", isLeftHand)
    (string, bool)? CurrentGrabber = null;
    public Axis UpAxis;

    [SerializeField] GameObject root;

    void Awake()
    {
        PotentialGrabbers = new();
    }

    void Update()
    {
        // Handle disconnected players
        foreach ((string, bool) potentialGrabber in PotentialGrabbers.ToList()) {
            if (!NetworkServer.CurrInst.ClientExists(potentialGrabber.Item1) && potentialGrabber.Item1 != "SELF") {
                PotentialGrabbers.Remove(potentialGrabber);
            }
        }

        // Handle Delete
        foreach ((string, bool) potentialGrabber in PotentialGrabbers) {
            string address = potentialGrabber.Item1;
            bool isLeft = potentialGrabber.Item2;
            LWINPUT input = UniversalInputHandler.CurrInst.input_lookup[address];
            if (input == null) continue;

            GameObject controller = UniversalInputHandler.CurrInst.GetController(address, isLeft);
            if (isLeft && ((int) OVRInput.RawButton.Y & input.DownMask) != 0)
                NetworkObjectsManager.CurrInst.RemoveNetworkObject(root);

            else if (!isLeft && ((int) OVRInput.RawButton.B & input.DownMask) != 0)
                NetworkObjectsManager.CurrInst.RemoveNetworkObject(root);

        }

        // Straighten object
        foreach ((string, bool) potentialGrabber in PotentialGrabbers) {
            string address = potentialGrabber.Item1;
            bool isLeft = potentialGrabber.Item2;
            LWINPUT input = UniversalInputHandler.CurrInst.input_lookup[address];
            if (input == null) continue;

            if (isLeft && ((int)OVRInput.RawButton.LHandTrigger & input.DownMask) != 0)
                Straighten();

            else if (!isLeft && ((int)OVRInput.RawButton.RHandTrigger & input.DownMask) != 0)
                Straighten();
        }

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

    public void Straighten()
    {
        if (NetworkInit.CurrInst.IsServer != true) return;
        if (UpAxis == Axis.Y) transform.rotation = Quaternion.Euler(Mathf.Round(transform.rotation.eulerAngles.x/90.0f)*90.0f, transform.rotation.eulerAngles.y, Mathf.Round(transform.rotation.eulerAngles.z/90.0f)*90.0f);
        else if (UpAxis == Axis.X) transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, Mathf.Round(transform.rotation.eulerAngles.y/90.0f)*90.0f, Mathf.Round(transform.rotation.eulerAngles.z/90.0f)*90.0f);
        else if (UpAxis == Axis.Z) transform.rotation = Quaternion.Euler(Mathf.Round(transform.rotation.eulerAngles.x/90.0f)*90.0f, Mathf.Round(transform.rotation.eulerAngles.y/90.0f)*90.0f, transform.rotation.eulerAngles.z);
    }

    public void OnTriggerEnter(Collider other) {
        var controllerInfo = other.transform.GetComponent<IControllerCollider>();
        if (controllerInfo == null || NetworkInit.CurrInst.IsServer != true) return;
        PotentialGrabbers.Add((controllerInfo.address, controllerInfo.Left));
        // Debug.Log("Adding: " + (controllerInfo.address, controllerInfo.Left).ToString());
    }
    
    public void OnTriggerExit(Collider other) {
        var controllerInfo = other.transform.GetComponent<IControllerCollider>();
        if (controllerInfo == null || NetworkInit.CurrInst.IsServer != true) return;
        PotentialGrabbers.Remove((controllerInfo.address, controllerInfo.Left));
        // Debug.Log("Removing: " + (controllerInfo.address, controllerInfo.Left).ToString());
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
