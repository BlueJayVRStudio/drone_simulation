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

    float Kp = .3f;
    float Ki = 0f;
    float Kd = .3f;

    public bool printAngles;

    void Start()
    {
        rb = transform.GetComponent<Rigidbody>();
        rollPID = new PIDController(Kp, Ki, Kd);
        pitchPID = new PIDController(Kp, Ki, Kd);
        yawPID = new PIDController(Kp, Ki, Kd);
        thrustPID = new PIDController(Kp, Ki, Kd);
        xPositionPID = new PIDController(Kp, Ki, Kd);
        yPositionPID = new PIDController(Kp, Ki, Kd);
    }

    void Update()
    {
        deltaTime = Time.deltaTime;

        UpdateSensorValues();

        // Compute control signals using PID controllers
        float rollControlSignal = rollPID.Update(rollSetpoint, measuredRoll, deltaTime);
        float pitchControlSignal = pitchPID.Update(pitchSetpoint, measuredPitch, deltaTime);
        float yawControlSignal = yawPID.Update(yawSetpoint, measuredYaw, deltaTime);
        float thrustControlSignal = thrustPID.Update(altitudeSetpoint, measuredAltitude, deltaTime);

        float xControlSignal = xPositionPID.Update(xPositionSetpoint, measuredXPosition, deltaTime);
        float yControlSignal = xPositionPID.Update(yPositionSetpoint, measuredYPosition, deltaTime);

        float xAdjustment = ConvertXPositionToRoll(xControlSignal, yControlSignal, measuredYaw);
        float yAdjustment = ConvertYPositionToPitch(xControlSignal, yControlSignal, measuredYaw);

        // rollControlSignal += xAdjustment;
        // pitchControlSignal += yAdjustment;

        float _FR = thrustControlSignal + rollControlSignal + pitchControlSignal + yawControlSignal;
        float _FL = thrustControlSignal - rollControlSignal + pitchControlSignal - yawControlSignal;
        float _BR = thrustControlSignal + rollControlSignal - pitchControlSignal - yawControlSignal;
        float _BL = thrustControlSignal - rollControlSignal - pitchControlSignal + yawControlSignal;
        
        SetMotorSpeeds(_FR, _FL, _BR, _BL);
        rb.AddTorque(((_FR+_BL) - (_FL+_BR)) * Vector3.up);

        if (printAngles) Debug.Log($"measured roll: {measuredRoll}, measured pitch: {measuredPitch}, measured yaw: {measuredYaw}");
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


    // float ConvertXPositionToRoll()
    // {
    //     var go = new GameObject();
    //     go.transform.position = new Vector3(xPositionSetpoint, 0.0f, yPositionSetpoint);
    //     Vector3 relativePos = CustomM.GetRelativePosition(go, gameObject);
    //     Destroy(go);
    //     return relativePos.x;
    // }

    // float ConvertYPositionToPitch()
    // {
    //     var go = new GameObject();
    //     go.transform.position = new Vector3(xPositionSetpoint, 0.0f, yPositionSetpoint);
    //     Vector3 relativePos = CustomM.GetRelativePosition(go, gameObject);
    //     Destroy(go);
    //     return relativePos.z;
    // }

    void SetMotorSpeeds(float _FR, float _FL, float _BR, float _BL)
    {
        FR.magnitude = _FR;
        FL.magnitude = _FL;
        BR.magnitude = _BR;
        BL.magnitude = _BL;
    }

    float ConvertXPositionToRoll(float xControlSignal, float yControlSignal, float yaw)
    {
        // Convert x position control signal to roll adjustment using the yaw angle
        float radYaw = yaw * Mathf.Deg2Rad;
        return xControlSignal * Mathf.Cos(90-radYaw) - yControlSignal * Mathf.Sin(90-radYaw);
    }

    float ConvertYPositionToPitch(float xControlSignal, float yControlSignal, float yaw)
    {
        // Convert y position control signal to pitch adjustment using the yaw angle
        float radYaw = yaw * Mathf.Deg2Rad;
        return yControlSignal * Mathf.Cos(90-radYaw) + xControlSignal * Mathf.Sin(90-radYaw);
    }
}