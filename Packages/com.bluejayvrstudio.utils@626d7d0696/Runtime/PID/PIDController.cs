using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PIDController
{
    public float Kp;
    public float Ki;
    public float Kd;

    private float previousError;
    private float integral;

    public PIDController(float kp, float ki, float kd)
    {
        Kp = kp;
        Ki = ki;
        Kd = kd;
    }

    public float Update(float setpoint, float measuredValue, float timeFrame)
    {
        float error = setpoint - measuredValue;
        integral += error * timeFrame;

        float derivative = (error - previousError) / timeFrame;
        previousError = error;
        
        return (Kp * error) + (Ki * integral) + (Kd * derivative);
    }

    public void SetParams(float kp, float ki, float kd)
    {
        Kp = kp;
        Ki = ki;
        Kd = kd;
    }
}
