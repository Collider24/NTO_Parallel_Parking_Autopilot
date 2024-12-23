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
        carController.SetSpeed(10);
        yield return new WaitForSeconds(5);
        carController.SetSpeed(-5);
        yield return new WaitForSeconds(5);
        carController.SetSpeed(0);
        carController.EndOfParking();
    }
}