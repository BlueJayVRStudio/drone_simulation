using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Unity.Collections;
using bluejayvrstudio;

/// <summary>
/// Low-level implementation of character controller
/// </summary>
public class BlueJayXRController : MonoBehaviour
{
    [SerializeField] OVRCameraRig CameraRig;
    [SerializeField] GameObject Head;
    [SerializeField] NormalsHandler Contacts;

    public float MoveVelocity = 4.0f;
    public float JumpVelocity = 1.0f;
    public float VerticalVelocity = 0.0f;
    public float Gravity = 9.81f;
    private bool Grounded = true;

    public LayerMask layerMask;


    float RotateCoolDown;
    [SerializeField] float SnapAngle = 45.0f;

    private Vector2 CurrAxis;
    
    public void Start()
    {
        Head = transform.Find("OVRCameraRig/TrackingSpace/CenterEyeAnchor").gameObject;
    }

    public void Update()
    {
        // Player Movement
        // Left Thumbstick Oculus input
        CurrAxis = OVRInput.Get(OVRInput.RawAxis2D.LThumbstick);
        Vector2 _MoveBy = CurrAxis;

#if UNITY_EDITOR
        _MoveBy = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
#endif

        _MoveBy = _MoveBy.normalized;
        Vector3 MoveBy = new Vector3(_MoveBy.x, 0, _MoveBy.y);
        MoveBy = Quaternion.AngleAxis(Head.transform.eulerAngles.y, Vector3.up) * MoveBy;

        RaycastHit hit;
        float distance = Mathf.Infinity;
        if (Physics.Raycast(Contacts.transform.position, transform.TransformDirection(-Vector3.up), out hit, layerMask))
        {
            // Debug.DrawRay(Contacts.transform.position, transform.TransformDirection(-Vector3.up) * hit.distance, Color.yellow);
            distance = hit.distance;
        }

        //Filter player move by colliding objects
        if (MoveBy.magnitude > 0.0f)
        {
            Vector3 A = Vector3.zero;
            Vector3 B = Vector3.zero;
            bool next = true;
            // Detect if player is attempting to move inside an intersection between two walls. Zero out move vector if so.
            foreach (var kvp in Contacts.ContactPointLists)
            {
                if (!next) break;
                foreach (var contactpoint in kvp.Value.Items)
                {
                    // Ignore flat surfaces and low slopes
                    if (contactpoint.slope <= 50.0f) continue;
                    B = A;
                    A = -CustomM.XZPlane(contactpoint.normal).normalized;

                    if (A != Vector3.zero && B != Vector3.zero)
                    {
                        float Total = Vector3.Angle(A, B);

                        if (Total < 180.0f)
                        {
                            float AngleSum = Vector3.Angle(A, MoveBy) + Vector3.Angle(B, MoveBy);
                            if (AngleSum <= Total + 0.001f)
                            {
                                MoveBy = new Vector3(0, 0, 0);
                                next = false;
                                break;
                            }

                        }
                    }
                }
            }
            // If the test above is passed, filter out component vectors of the movement vector going in towards walls
            if (next)
            {
                foreach (var kvp in Contacts.ContactPointLists)
                {
                    foreach (var contactpoint in kvp.Value.Items)
                    {
                        // Ignore flat surfaces and low slopes
                        if (contactpoint.slope <= 50.0f) continue;
                        Vector3 Away = -CustomM.XZPlane(contactpoint.normal).normalized;

                        // handles for perfectly vertical normals (completely flat/horizontal surfaces)
                        if (Away == Vector3.zero) break;

                        if (Vector3.Angle(MoveBy, Away) < 90.0f)
                        {
                            MoveBy = Mathf.Sin(Mathf.PI * Vector3.Angle(MoveBy, Away) / 180.0f) * -(Vector3.Cross(Away, Vector3.Cross(Away, MoveBy))).normalized;
                        }
                    }
                }
            }
        }

        if (Grounded)
        {
            MoveBy *= MoveVelocity * Time.deltaTime;

            if (distance >= 1.1f)
            {
                Grounded = false;
                VerticalVelocity = 0;
            }
            else if (OVRInput.Get(OVRInput.Button.One) || Input.GetKeyDown("space"))
            {
                Grounded = false;
                VerticalVelocity = JumpVelocity;
            }
            else
            {
                Contacts.transform.position = new Vector3(Contacts.transform.position.x, hit.point.y + 1.0f, Contacts.transform.position.z);
                Contacts.transform.position += MoveBy;
                transform.position = new Vector3(Contacts.transform.position.x, Contacts.transform.position.y - 1.0f, Contacts.transform.position.z);
            }
        }
        else
        {
            if (distance <= 1.0f && VerticalVelocity < 0.0f) Grounded = true;

            MoveBy *= MoveVelocity * Time.deltaTime;
            MoveBy += new Vector3(0, VerticalVelocity * Time.deltaTime, 0);
            Contacts.transform.position += MoveBy;
            transform.position = new Vector3(Contacts.transform.position.x, Contacts.transform.position.y - 1.0f, Contacts.transform.position.z);

            VerticalVelocity -= Gravity * Time.deltaTime;
        }

        //Player rotation
        Vector2 _Rotate = OVRInput.Get(OVRInput.RawAxis2D.RThumbstick);
        float Rotate = _Rotate[0];
        if (Rotate > 0.6f)
            Rotate = 1;
        else if (Rotate < -0.6f)
            Rotate = -1;
        else
            Rotate = 0;

        if (RotateCoolDown <= 0.0f && Rotate != 0)
        {
            transform.RotateAround(CameraRig.centerEyeAnchor.position, Vector3.up, SnapAngle * Rotate);
            RotateCoolDown = 0.2f;
        }
        if (RotateCoolDown > 0.0f) RotateCoolDown -= Time.deltaTime;
    }
    
}
