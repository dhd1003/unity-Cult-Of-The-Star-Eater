using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionChecker : MonoBehaviour
{

    public LayerMask Ground;
    public float rayCastDistance = 1f;
    private Vector3 originPointWall;
    private Vector3 originPointClimb;
    public Vector3 offset;
    public Vector3 originalDirection = new Vector3(1, 0, 0);
    private Vector3 direction;
    public float aditionRayCastClimb = 0.5f;

    private CharacterController controller;

    public bool wallCollision;
    public bool CanClimb;


    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
        direction = originalDirection;
        wallCollision = false;
        CanClimb = true; ;
    }

    // Update is called once per frame
    void FixedUpdate()
    {

        ////RayCast Wall

        if (controller.transform.localScale.z == -1)
        {
            direction = originalDirection*-1;
        }
        else
        {
            direction = originalDirection;

        }

        originPointWall = transform.position + offset;
        RaycastHit hitr;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(originPointWall, direction, out hitr, rayCastDistance, Ground))
        {
            Debug.DrawRay(originPointWall, direction * rayCastDistance, Color.red);
            wallCollision = true;
            
        }
        else
        {
            Debug.DrawRay(originPointWall, direction * rayCastDistance, Color.blue);
            wallCollision = false;
            
            
        }
        
    

        ////RayCast Climb
        originPointClimb = transform.position + offset + new Vector3(0,aditionRayCastClimb,0);

        RaycastHit climbHit;
        // Does the ray intersect any objects excluding the player layer
        if (Physics.Raycast(originPointClimb, direction, out climbHit, rayCastDistance, Ground))
        {
            Debug.DrawRay(originPointClimb, direction * rayCastDistance, Color.green);
            CanClimb = false;
            
        }
        else
        {
            Debug.DrawRay(originPointClimb, direction * rayCastDistance, Color.magenta);
            CanClimb = true;
            
        }

   


    }
}
