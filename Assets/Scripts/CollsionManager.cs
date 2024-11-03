using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollsionManager : MonoBehaviour
{
    private void OnCollisionEnter(Collision collision)
    {
        print("Collision!");
        Debug.Break();
    }
}
