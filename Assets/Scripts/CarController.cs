using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    [SerializeField]
    private Rigidbody rb;
    private float horizontalInput;
    private float currentSteerAngle;

    [SerializeField] private float motorForce, breakForce, maxSteerAngle;

    [SerializeField] private WheelCollider frontLeftWheelCollider, frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;

    [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;

    private float throttle = 0;

    private float currentSettedSpeed = 0;
    private float currentAngle = 0;

    private List<float>queue = new List<float>();

    private float angleStep = 0.05f;

    [SerializeField]
    private List<ParkingSensorController>parkingSensors = new List<ParkingSensorController>();

    [SerializeField]
    private List<ParkingSensorController>lineSensors = new List<ParkingSensorController>();

    private float time = 0;
    private float speed;

    private void Start()
    {
        time = 0;
    }

    private void FixedUpdate()
    {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
        time += Time.deltaTime;
        if (Time.timeScale != 1)
        {
            Time.timeScale = 1;
        }
    }

    private void GetInput()
    {
        if (horizontalInput > 1)
        {
            horizontalInput = 1;
        }
        else if (horizontalInput < -1)
        {
            horizontalInput = -1;
        }
        if (currentSettedSpeed == 0)
        {
            if (rb.velocity.magnitude < 0.1f)
            {
                throttle = 0;
            }
            else
            {
                if (Vector3.Dot(rb.transform.forward, rb.velocity) * 3.6f < currentSettedSpeed)
                {
                    throttle = 1;
                }
                else
                {
                    throttle = -1;
                }
            }
        }
        else if (Vector3.Dot(rb.transform.forward, rb.velocity) * 3.6f < currentSettedSpeed)
        {
            throttle = 1;
        }
        else
        {
            throttle = -1;
        }
        speed = rb.velocity.magnitude * 3.6f;
    }

    private void HandleMotor()
    {
        rearLeftWheelCollider.motorTorque = throttle * motorForce;
        rearRightWheelCollider.motorTorque = throttle * motorForce;
    }

    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        if (horizontalInput >= 0)
        {
            frontLeftWheelCollider.steerAngle = currentSteerAngle;
            frontRightWheelCollider.steerAngle = currentSteerAngle + horizontalInput*10;
        }
        else if (horizontalInput < 0)
        {
            frontLeftWheelCollider.steerAngle = currentSteerAngle + horizontalInput * 10;
            frontRightWheelCollider.steerAngle = currentSteerAngle;
        }
    }

    private void UpdateWheels()
    {
        UpdateSingleWheel(frontLeftWheelCollider, frontLeftWheelTransform);
        UpdateSingleWheel(frontRightWheelCollider, frontRightWheelTransform);
        UpdateSingleWheel(rearRightWheelCollider, rearRightWheelTransform);
        UpdateSingleWheel(rearLeftWheelCollider, rearLeftWheelTransform);
    }

    private void UpdateSingleWheel(WheelCollider wheelCollider, Transform wheelTransform)
    {
        Vector3 pos;
        Quaternion rot;
        wheelCollider.GetWorldPose(out pos, out rot);
        wheelTransform.rotation = rot;
        wheelTransform.position = pos;
    }

    public void SetSpeed(float value)
    {
        if (value > 60)
        {
            value = 60;
        }
        else if (value < -60)
        {
            value = -60;
        }
        currentSettedSpeed = value;
    }

    private Coroutine currentCoroutine = null;
    public void SetStearingAngle(float value)
    {
        if (value > 1)
        {
            value = 1;
        }
        else if (value < -1)
        {
            value = -1;
        }
        if (currentCoroutine == null)
        {
            currentCoroutine = StartCoroutine(ChangeAngle(value));
        }
        else
        {
            queue.Add(value);
        }
    }

    private IEnumerator ChangeAngle(float newCurrentAngle)
    {
        if (newCurrentAngle < currentAngle)
        {
            while (newCurrentAngle < horizontalInput)
            {
                horizontalInput -= angleStep;
                yield return new WaitForSeconds(0.02f);
            }
        }
        else if (newCurrentAngle > currentAngle)
        {
            while (newCurrentAngle > horizontalInput)
            {
                horizontalInput += angleStep;
                yield return new WaitForSeconds(0.02f);
            }
        }
        horizontalInput = newCurrentAngle;
        currentAngle = newCurrentAngle;

        currentCoroutine = null;
        if (queue.Count > 0)
        {
            float value = queue[0];
            queue.RemoveAt(0);
            SetStearingAngle(value);
        }
    }

    public float GetParkingSensorDistance(int index)
    {
        if (index >= parkingSensors.Count || index < 0)
        {
            return -1;
        }
        else
        {
            return parkingSensors[index].GetValue();
        }
    }

    public float GetDistanceToLine(int index)
    {
        if (index > 1 || index < 0)
        {
            return -1;
        }
        else
        {
            return lineSensors[index].GetValue();
        }
    }

    public void EndOfParking()
    {
        StartCoroutine(EndOfParkingAsync());
    }

    private IEnumerator EndOfParkingAsync()
    {
        if (currentSettedSpeed != 0)
        {
            print("Car in move!");
            print("Score: 0");
        }
        else if (speed > 0.01f)
        {
            while (speed > 0.01f)
            {
                yield return null;
            }
        }
        print($"Time: {time}");
        if (time > 50f)
        {
            print("Time Limit!");
        }
        print($"Car angle: {rb.transform.eulerAngles.y}");
        float angleScore = 0;
        if (45 <= rb.transform.eulerAngles.y && rb.transform.eulerAngles.y < 89)
        {
            angleScore = (rb.transform.eulerAngles.y - 45) / (89 - 45);
        }
        else if (89 <= rb.transform.eulerAngles.y && rb.transform.eulerAngles.y <= 91)
        {
            angleScore = 1;
        }
        else if (91 <= rb.transform.eulerAngles.y && rb.transform.eulerAngles.y < 135)
        {
            angleScore = (rb.transform.eulerAngles.y - 135) / (91 - 135);
        }
        float alpha = rb.transform.eulerAngles.y > 90 ? rb.transform.eulerAngles.y - 90 : 90 - rb.transform.eulerAngles.y;
        float l = GetDistanceToLine(0) < GetDistanceToLine(1) ? GetDistanceToLine(0) : GetDistanceToLine(1); ;
        float minDistToLine = l * Mathf.Cos(alpha * Mathf.Deg2Rad);
        print($"Distance to line: {minDistToLine}");
        float lineScore = 0;
        if (0.7f < minDistToLine && minDistToLine < 3)
        {
            lineScore = (minDistToLine - 0.7f) / (3 - 0.7f);
        }
        else if (minDistToLine >= 3)
        {
            lineScore = 1;
        }

        print($"Distance to back obstacle: {GetParkingSensorDistance(4)}");

        float distanceScore = 0;

        if (GetParkingSensorDistance(4) < 0.2f)
        {
            distanceScore = GetParkingSensorDistance(4) / 0.2f;
        }
        else if (0.2f <= GetParkingSensorDistance(4) && GetParkingSensorDistance(4) <= 0.4f)
        {
            distanceScore = 1;
        }
        else if (0.4f < GetParkingSensorDistance(4) && GetParkingSensorDistance(4) < 0.6f)
        {
            distanceScore = (GetParkingSensorDistance(4) - 0.6f) / (0.4f - 0.6f);
        }
        if (time <= 50f)
        {
            print($"Score: {(int)(100 * Mathf.Min(angleScore, distanceScore, lineScore))}");
        }
        else
        {
            print("Score: 0");
        }
    }
}
