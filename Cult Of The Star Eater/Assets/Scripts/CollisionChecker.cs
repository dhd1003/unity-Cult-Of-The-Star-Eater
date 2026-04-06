using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionChecker : MonoBehaviour
{

    public LayerMask Ground;
    public float rayCastDistance = 5f;
    private Vector3 originPoint;
    public Vector3 offset;
    public Vector3 direction;
    // Start is called before the first frame update
    void Start()
    {
        originPoint = transform.position + offset;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        RaycastHit hit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(originPoint, direction, out hit, rayCastDistance, Ground))

        {
            Debug.DrawRay(originPoint, originPoint * hit.distance, Color.red);
            Debug.Log("Did Hit");
        }
        else
        {
            Debug.DrawRay(originPoint, direction * rayCastDistance, Color.blue);
            Debug.Log("Did not Hit");
        }

    }
}
