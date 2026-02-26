using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    CharacterController characterController;

    Animator animator;

    public float speed = 6f;
    public float jumpSpeed = 8.0f;
    public float gravity = 1f;

    private float horizontalMove;
    private float verticalMove;

    //private Vector3 moveDirection = Vector3.zero;//Preguntar JONE
    


    void Start()
    {
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();


    }

    // Update is called once per frame
    void Update()
    {

        horizontalMove = Input.GetAxis("Horizontal");
        
        if (Input.GetButtonDown("Jump"))
        {
            Debug.Log("Saltando");
        }
        
        
        //if (characterController.isGrounded) 
        //{
        //    animator.SetBool("IsJumping", false);


        //}


    }

    private void FixedUpdate()
    {
        // Apply gravity
        characterController.Move(new Vector3(0, -gravity, 0) * Time.deltaTime);


        if (characterController.isGrounded)
        {
            if (Input.GetButton("Jump"))
            {
                characterController.Move(new Vector3(0, jumpSpeed, 0) * Time.deltaTime);

            }
        }

        characterController.Move(new Vector3(horizontalMove,0, 0) * speed * Time.deltaTime);
    }
}
