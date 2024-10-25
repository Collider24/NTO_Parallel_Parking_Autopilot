using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CarController : MonoBehaviour
{
    private float horizontalInput;
    private float currentSteerAngle;

    public bool Manual = true;

    [SerializeField] private float motorForce, breakForce, maxSteerAngle;

    [SerializeField] private WheelCollider frontLeftWheelCollider, frontRightWheelCollider;
    [SerializeField] private WheelCollider rearLeftWheelCollider, rearRightWheelCollider;

    [SerializeField] private Transform frontLeftWheelTransform, frontRightWheelTransform;
    [SerializeField] private Transform rearLeftWheelTransform, rearRightWheelTransform;

    private float throttle = 0;
    private int direction = 1;

    private float currentSettedSpeed = 0;
    private float currentAngle = 0;

    private Rigidbody rb;

    private List<float>queue = new List<float>();

    private float angleStep = 0.05f;

    [SerializeField]
    private List<ParkingSensorController>parkingSensors = new List<ParkingSensorController>();

    [SerializeField]
    private List<ParkingSensorController>lineSensors = new List<ParkingSensorController>();

    private float time = 0;
    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void Start()
    {
        time = Time.time;
    }

    private void FixedUpdate()
    {
        GetInput();
        HandleMotor();
        HandleSteering();
        UpdateWheels();
        CheckLine();
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
                if (Vector3.Dot(transform.forward, rb.velocity) * 3.6f < currentSettedSpeed)
                {
                    throttle = 1;
                }
                else
                {
                    throttle = -1;
                }
            }
        }
        else if (Vector3.Dot(transform.forward, rb.velocity) * 3.6f < currentSettedSpeed)
        {
            throttle = 1;
        }
        else
        {
            throttle = -1;
        }
    }

    private void HandleMotor()
    {
        rearLeftWheelCollider.motorTorque = direction * throttle * motorForce;
        rearRightWheelCollider.motorTorque = direction * throttle * motorForce;
    }

    private void HandleSteering()
    {
        currentSteerAngle = maxSteerAngle * horizontalInput;
        if (horizontalInput > 0)
        {
            frontLeftWheelCollider.steerAngle = currentSteerAngle;
            frontRightWheelCollider.steerAngle = currentSteerAngle + horizontalInput*10;
        }
        else if (horizontalInput < 0)
        {
            frontLeftWheelCollider.steerAngle = currentSteerAngle + horizontalInput * 10;
            frontRightWheelCollider.steerAngle = currentSteerAngle;
        }
        else
        {
            frontLeftWheelCollider.steerAngle = 0;
            frontRightWheelCollider.steerAngle = 0;
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
            float value = queue[queue.Count - 1];
            queue.RemoveAt(queue.Count - 1);
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

    private void CheckLine()
    {
        if (GetDistanceToLine(0) > 9 || GetDistanceToLine(1) > 9)
        {
            print("Crossing central line!");
        }
    }

    public void EndOfParking()
    {
        print($"Time: {Time.time - time}");
        print($"Car angle: {transform.eulerAngles.y}");
        float minDistToLine = GetDistanceToLine(0) < GetDistanceToLine(1) ? GetDistanceToLine(0) : GetDistanceToLine(1);
        print($"Distance to line: {minDistToLine}");
        print($"Distance to back obstacle: {GetParkingSensorDistance(4)}");
    }
}
