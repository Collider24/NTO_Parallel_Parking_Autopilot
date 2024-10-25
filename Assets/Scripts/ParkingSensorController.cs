using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParkingSensorController : MonoBehaviour
{
    private GameObject sphere;
    private float value;
    public bool LineControl = false;
    [SerializeField]
    private LayerMask mask;
    [SerializeField]
    private Material sphereMaterial;
    private void Awake()
    {
        sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        sphere.transform.localScale = Vector3.one * 0.1f;
        Destroy(sphere.GetComponent<Collider>());
        sphere.GetComponent<MeshRenderer>().material = sphereMaterial;
    }
    private void Update()
    {
        RaycastHit hit;
        Physics.Raycast(transform.position, transform.forward, out hit, 10f, mask);
        
        if (hit.collider != null)
        {
            sphere.transform.position = hit.point;
        }
        else
        {
            sphere.transform.position = transform.position + transform.forward * 10f;
        }
        Debug.DrawLine(transform.position, sphere.transform.position, LineControl ? Color.blue : Color.red);
        value = (sphere.transform.position - transform.position).magnitude;
    }

    public float GetValue()
    {
        return value;
    }
}
