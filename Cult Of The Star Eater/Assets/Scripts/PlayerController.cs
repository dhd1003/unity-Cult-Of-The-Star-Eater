using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    CharacterController playerCharacterController;

    Animator animator;

    public float speed = 6f;
    
    public float jumpSpeed = 8.0f;
    
    public float gravity = 9.8f;
    public float fallVelocity;

    private float horizontalMove;
    private float verticalMove;

    private Vector3 playerMoveDirection;
    


    void Start()
    {
        playerCharacterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();


    }

    // Update is called once per frame
    void Update()
    {

        horizontalMove = Input.GetAxis("Horizontal");
        playerMoveDirection.x=horizontalMove * speed * Time.deltaTime;

        if (Input.GetButtonDown("Jump"))
        {
            Debug.Log("Saltando");
        }

        

        //Rota al personaje 
        //playerCharacterController.transform.LookAt(playerCharacterController.transform.position + playerMoveDirection);

        if(playerMoveDirection.x > 0)
        {
            playerCharacterController.transform.localScale = Vector3.one;
        }
        else if (playerMoveDirection.x < 0)
        {
            playerCharacterController.transform.localScale = new Vector3(1,1,-1);
        }

        SetGravity();

        //Mueve al personaje
        playerCharacterController.Move(playerMoveDirection);




        if (playerMoveDirection.x != 0)
        {
            animator.SetBool("IsWalking", true);
        }
        else
        {
            animator.SetBool("IsWalking", false);
        }


        Debug.Log("Toca el suelo:"+ playerCharacterController.isGrounded);


    }

    void SetGravity()
    {
        if (playerCharacterController.isGrounded)
        {
            fallVelocity = -gravity * Time.deltaTime;
            playerMoveDirection.y = fallVelocity;
        }
        else
        {
            fallVelocity -= gravity * Time.deltaTime;
            playerMoveDirection.y = fallVelocity;
        }
    }

}
