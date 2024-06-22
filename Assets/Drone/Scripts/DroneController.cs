using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using bluejayvrstudio;

// WIP
public class DroneController : MonoBehaviour
{
    Rigidbody rb;

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

    public float xPositionOffset;
    public float yPositionOffset;
    public float altitudeOffset;

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
    public BladesAddForce FR;
    public BladesAddForce FL;
    public BladesAddForce BR;
    public BladesAddForce BL;

    float Kp = 0.3f;
    float Ki = 0.0f;
    float Kd = 0.005f;

    public bool printAngles;
    public bool printControlSignals;
    public bool printOutputs;

    void Start()
    {
        rb = transform.GetComponent<Rigidbody>();
        rollPID = new PIDController(Kp, Ki, Kd);
        pitchPID = new PIDController(Kp, Ki, Kd);
        yawPID = new PIDController(Kp, Ki, Kd);
        thrustPID = new PIDController(1000f, 0.0f, 10f);
        xPositionPID = new PIDController(Kp, Ki, Kd);
        yPositionPID = new PIDController(Kp, Ki, Kd);

        altitudeSetpoint = transform.position.y;
        xPositionSetpoint = transform.position.x;
        yPositionSetpoint = transform.position.z;
    }
    
    void Update() {
        if (Input.GetKey(KeyCode.W)) {
            yPositionSetpoint += 5.0f * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.S)) {
            yPositionSetpoint -= 5.0f * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.A)) {
            xPositionSetpoint -= 5.0f * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.D)) {
            xPositionSetpoint += 5.0f * Time.deltaTime;
        }
        if (Input.GetKey(KeyCode.Space)) {
            altitudeSetpoint += Time.deltaTime;
        }
        
    }

    void FixedUpdate()
    {
        deltaTime = Time.fixedDeltaTime;

        UpdateSensorValues();

        // Compute control signals using PID controllers
        float rollControlSignal;
        float pitchControlSignal;
        float yawControlSignal = yawPID.Update(yawSetpoint, measuredYaw, deltaTime);
        float thrustControlSignal = thrustPID.Update(altitudeSetpoint, measuredAltitude, deltaTime);

        var go = new GameObject();
        go.transform.position = new Vector3(xPositionSetpoint, 0.0f, yPositionSetpoint);
        Vector3 setPoint = CustomM.GetRelativePosition(go, gameObject);
        go.transform.position = new Vector3(measuredXPosition, 0.0f, measuredYPosition);
        Vector3 measuredPoint = CustomM.GetRelativePosition(go, gameObject);
        float xControlSignal = xPositionPID.Update(0, measuredPoint.x, deltaTime);
        float yControlSignal = yPositionPID.Update(0, measuredPoint.z, deltaTime);
        Destroy(go);

        float xAdjustment = ConvertXPositionToRoll();
        float yAdjustment = ConvertYPositionToPitch();

        rollControlSignal = rollPID.Update(rollSetpoint+xAdjustment, measuredRoll, deltaTime);
        pitchControlSignal = pitchPID.Update(pitchSetpoint+yAdjustment, measuredPitch, deltaTime);

        // don't change this :O
        float _FR = thrustControlSignal - rollControlSignal + pitchControlSignal + yawControlSignal;
        float _FL = thrustControlSignal + rollControlSignal + pitchControlSignal - yawControlSignal;
        float _BR = thrustControlSignal - rollControlSignal - pitchControlSignal - yawControlSignal;
        float _BL = thrustControlSignal + rollControlSignal - pitchControlSignal + yawControlSignal;
        float avg = (Mathf.Abs(_FR) + Mathf.Abs(_FL) + Mathf.Abs(_BR) + Mathf.Abs(_BL)) / 4;

        SetMotorSpeeds(_FR, _FL, _BR, _BL);
        rb.AddTorque(((_FR+_BL) - (_FL+_BR)) * Vector3.up);

        if (printAngles) Debug.Log($"measured roll: {measuredRoll}, measured pitch: {measuredPitch}, measured yaw: {measuredYaw}");
        if (printControlSignals) Debug.Log($"thrustControlSignal: {thrustControlSignal}, rollControlSignal: {rollControlSignal}, pitchControlSignal: {pitchControlSignal}, yawControlSignal: {yawControlSignal}, xControlSignal: {xControlSignal}, yControlSignal: {yControlSignal}");
        if (printOutputs) Debug.Log($"FR: {_FR}, FL: {_FL}, BR: {_BR}, BL: {_BL}, Avg: {avg}");
        // Debug.Log($"altitudeSetpoint: {altitudeSetpoint}, rollSetpoint: {rollSetpoint}, pitchSetpoint: {pitchSetpoint}, yawSetpoint: {yawSetpoint}");
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

    void SetMotorSpeeds(float _FR, float _FL, float _BR, float _BL)
    {
        FR.magnitude = _FR;
        FL.magnitude = _FL;
        BR.magnitude = _BR;
        BL.magnitude = _BL;
    }

    float ConvertXPositionToRoll()
    {
        var go = new GameObject();
        go.transform.position = new Vector3(xPositionSetpoint, 0.0f, yPositionSetpoint);
        Vector3 relativePos = CustomM.GetRelativePosition(go, gameObject);
        Destroy(go);
        return Mathf.Clamp(relativePos.x, -160f, 160f);
    }

    float ConvertYPositionToPitch()
    {
        var go = new GameObject();
        go.transform.position = new Vector3(xPositionSetpoint, 0.0f, yPositionSetpoint);
        Vector3 relativePos = CustomM.GetRelativePosition(go, gameObject);
        Destroy(go);
        return Mathf.Clamp(-relativePos.z, -160f, 160f);
    }
}