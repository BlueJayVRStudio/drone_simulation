using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bluejayvrstudio;

// WIP
public class DroneController : MonoBehaviour
{
    // PID Controllers
    public PIDController rollPID;
    public PIDController pitchPID;
    public PIDController yawPID;
    public PIDController thrustPID;
    public PIDController xPositionPID;
    public PIDController yPositionPID;

    public GameObject vis;

    // Desired setpoints
    public float rollSetpoint;
    public float pitchSetpoint;
    public float yawSetpoint;
    public float altitudeSetpoint;
    public float xPositionSetpoint;
    public float yPositionSetpoint;

    // Measured values
    private float measuredRoll;
    private float measuredPitch;
    private float measuredYaw;
    private float measuredAltitude;
    private float measuredXPosition;
    private float measuredYPosition;

    // Time frame for PID updates
    private float deltaTime;

    // Motors
    public BladesAddForce Blades1;
    public BladesAddForce Blades2;
    public BladesAddForce Blades3;
    public BladesAddForce Blades4;


    float Kp = 100f;
    float Ki = 0f;
    float Kd = 1f;


    void Start()
    {
        rollPID = new PIDController(Kp, Ki, Kd);
        pitchPID = new PIDController(Kp, Ki, Kd);
        yawPID = new PIDController(Kp, Ki, Kd);
        thrustPID = new PIDController(10f, 0f, 10f);
        xPositionPID = new PIDController(Kp, Ki, Kd);
        yPositionPID = new PIDController(1000, 0, 10); 
    }

    void Update()
    {
        deltaTime = Time.deltaTime;

        UpdateSensorValues();

        // Compute control signals using PID controllers
        float rollControlSignal ;
        float pitchControlSignal;
        rollControlSignal = rollPID.Update(rollSetpoint, measuredRoll, deltaTime);
        pitchControlSignal = pitchPID.Update(pitchSetpoint, measuredPitch, deltaTime);
        float yawControlSignal = yawPID.Update(yawSetpoint, measuredYaw, deltaTime);
        float thrustControlSignal = thrustPID.Update(altitudeSetpoint, measuredAltitude, deltaTime);
        
        var go = new GameObject();
        go.transform.position = new Vector3(xPositionSetpoint, 0.0f, yPositionSetpoint);
        Vector3 relativePos = CustomM.GetRelativePosition(go, gameObject);

        rollSetpoint = Mathf.Clamp(xPositionPID.Update(0, ConvertXPositionToRoll(), deltaTime), -60, 60);
        pitchSetpoint = yPositionPID.Update(0, ConvertYPositionToPitch(), deltaTime);
        print(ConvertXPositionToRoll());
        Destroy(go);

        transform.GetComponent<Rigidbody>().AddTorque(yawControlSignal * Vector3.up);
        transform.GetComponent<Rigidbody>().AddRelativeTorque(-pitchControlSignal * Vector3.right);
        transform.GetComponent<Rigidbody>().AddRelativeTorque(-rollControlSignal * Vector3.forward);
        transform.GetComponent<Rigidbody>().AddForceAtPosition(Mathf.Clamp(thrustControlSignal, 0, 50f) * transform.up, transform.position, ForceMode.Force);

        // Debug.Log($"measured roll: {measuredRoll}, measured pitch: {measuredPitch}, measured yaw: {measuredYaw}");
    }

    void UpdateSensorValues()
    {
        measuredRoll = GetRollFromSensor();
        measuredPitch = GetPitchFromSensor();
        measuredYaw = GetYawFromSensor();
        measuredAltitude = GetAltitudeFromSensor();
        measuredXPosition = GetXPositionFromSensor();
        measuredYPosition = GetYPositionFromSensor();
    }

    float GetYawFromSensor() {
        if (transform.eulerAngles.y <= 180.0f) 
            return transform.eulerAngles.y;
        else 
            return transform.eulerAngles.y - 360.0f;
    }

    float GetRollFromSensor() {
        float angle = Vector3.Angle(transform.right, Vector3.up);
        if (angle >= 90.0f) {
            if (transform.up.y > 0)
                return angle - 90.0f;
            else
                return 180.0f - angle + 90.0f;
        } else {
            if (transform.up.y > 0)
                return -(90.0f - angle);
            else
                return -angle - 90.0f;
        }
    }

    float GetPitchFromSensor() {
        float angle = Vector3.Angle(transform.forward, Vector3.up);
        if (angle >= 90.0f) {
            if (transform.up.y > 0)
                return -(angle - 90.0f);
            else
                return -(180.0f - angle + 90.0f);
        } else {
            if (transform.up.y > 0)
                return (90.0f - angle);
            else
                return (angle + 90.0f);
        }
    }

    float GetAltitudeFromSensor() {  return transform.position.y;}
    float GetXPositionFromSensor() => transform.position.x;

    float GetYPositionFromSensor() => transform.position.z;


    float ConvertXPositionToRoll()
    {
        var go = new GameObject();
        go.transform.position = new Vector3(xPositionSetpoint, 0.0f, yPositionSetpoint);
        Vector3 relativePos = CustomM.GetRelativePosition(go, gameObject);
        Destroy(go);
        // return Mathf.Clamp(relativePos.x + Mathf.Cos(xPositionSignal), -160f, 160f);
        return Mathf.Clamp(relativePos.x, -160f, 160f);
    }

    float ConvertYPositionToPitch()
    {
        var go = new GameObject();
        go.transform.position = new Vector3(xPositionSetpoint, 0.0f, yPositionSetpoint);
        Vector3 relativePos = CustomM.GetRelativePosition(go, gameObject);
        Destroy(go);
        // Debug.Log(Mathf.Clamp(-relativePos.z, -30.0f, 30.0f));
        // return Mathf.Clamp(-relativePos.z + Mathf.Sin(yPositionSignal), -160f, 160f);
        return Mathf.Clamp(-relativePos.z, -160f, 160f);
    }
}