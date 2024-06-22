using System.Collections;
using System.Collections.Generic;
using ClientMessagesProto;
using UnityEngine;
using System.Linq;

public class NetworkPhysicsGrabbable : MonoBehaviour, INetworkGrabbable
{
    Vector3 InitPos;
    Vector3 CurrVel = new Vector3(0,0,0);
    int count = 0;
    float totalTime = 0.0f;

    Quaternion InitRotation;
    Vector3 CurrAngularVel = new Vector3(0,0,0);
    [SerializeField] bool LockRotation = false;

    Vector3 ObjectVector;
    Vector3 RightHandVector;

    public bool isRigidbody = true;

    // (ipaddress, isLeftHand)
    HashSet<(string, bool)> PotentialGrabbers;
    (string, bool)? CurrentGrabber = null;

    void Start()
    {
        InitPos = transform.parent.transform.position;
        InitRotation = transform.parent.transform.rotation;
        PotentialGrabbers = new();
    }

    void Update()
    {
        // Debug.Log($"count: {count}, total time: {totalTime}");
        // Physics calculations
        if (count == 5)
        {
            CurrVel = (transform.parent.transform.position - InitPos)/totalTime;

            Quaternion deltaQuaternion = transform.parent.transform.rotation * Quaternion.Inverse(InitRotation);
            // must clamp Acos input between -1 and 1 to avoid NaN's
            float angle = 2.0f * Mathf.Acos(Mathf.Clamp(deltaQuaternion.w, -1, 1));
            Vector3 axis = new Vector3(deltaQuaternion.x, deltaQuaternion.y, deltaQuaternion.z).normalized;
            float angularVelocity = angle / totalTime;
            CurrAngularVel = axis * angularVelocity;

            count = 0;
            totalTime = 0.0f;
            InitPos = transform.parent.transform.position;
            InitRotation = transform.parent.transform.rotation;
        }
        else
        {
            count++;
            totalTime += Time.deltaTime;
        }

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
                NetworkObjectsManager.CurrInst.RemoveNetworkObject(transform.parent.gameObject);

            else if (!isLeft && ((int) OVRInput.RawButton.B & input.DownMask) != 0)
                NetworkObjectsManager.CurrInst.RemoveNetworkObject(transform.parent.gameObject);

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

    void OnTriggerEnter(Collider other)
    {
        var controllerInfo = other.transform.GetComponent<IControllerCollider>();
        if (controllerInfo == null) return;
        PotentialGrabbers.Add((controllerInfo.address, controllerInfo.Left));
        // Debug.Log("Adding: " + (controllerInfo.address, controllerInfo.Left).ToString());
    }
    
    void OnTriggerExit(Collider other)
    {
        var controllerInfo = other.transform.GetComponent<IControllerCollider>();
        if (controllerInfo == null) return;
        PotentialGrabbers.Remove((controllerInfo.address, controllerInfo.Left));
        // Debug.Log("Removing: " + (controllerInfo.address, controllerInfo.Left).ToString());
    }

    public void HandleGrab(GameObject grabber, (string, bool) grabberInfo) {
        // Debug.Log(grabberInfo.Item1 + " grabbing");
        HandleSteal();
        CurrentGrabber = grabberInfo;

        transform.parent.transform.SetParent(grabber.transform);
        if (isRigidbody) {
            transform.parent.transform.GetComponent<Rigidbody>().isKinematic = true;
            transform.parent.transform.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.None;
        }
    }

    public void HandleSteal() {
        CurrentGrabber = null;
        transform.parent.transform.SetParent(null);

        if (isRigidbody)
        {
            transform.parent.transform.GetComponent<Rigidbody>().isKinematic = false;
            transform.parent.transform.GetComponent<Rigidbody>().interpolation = RigidbodyInterpolation.Interpolate;
            transform.parent.transform.GetComponent<Rigidbody>().velocity = CurrVel;
            if (!LockRotation) transform.parent.transform.GetComponent<Rigidbody>().angularVelocity = CurrAngularVel;
        }
    }
}
