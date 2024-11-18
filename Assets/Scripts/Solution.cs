using System;
using System.Collections;
using UnityEngine;

public class Solution : MonoBehaviour
{
    private CarController carController;

    private void Awake()
    {
        carController = FindFirstObjectByType<CarController>();
    }
    private IEnumerator Start()
    {
        carController.SetSteeringAngle(-1);
        carController.SetSteeringAngle(1);

        carController.SetSteeringAngle(-1);
        carController.SetSteeringAngle(1);

        carController.SetSteeringAngle(-1);
        carController.SetSteeringAngle(1);

        carController.SetSteeringAngle(-1);
        carController.SetSteeringAngle(1);

        carController.SetSteeringAngle(-1);
        carController.SetSteeringAngle(1);
        yield return null;
    }
}