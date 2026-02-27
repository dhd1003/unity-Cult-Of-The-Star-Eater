using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class PlayerController : MonoBehaviour
{
    CharacterController playerCharacterController;

    Animator animator;

    public float moveSpeed = 6f;
    
    
    public float gravity = 20f;
    public float fallVelocity;
    public float jumpForce =20f;

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
        playerMoveDirection.x=horizontalMove * moveSpeed;

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

        PlayerSkills();

        playerMoveDirection = playerMoveDirection*Time.deltaTime;

        //Mueve al personaje
        playerCharacterController.Move(playerMoveDirection);




        if (playerMoveDirection.x != 0 && playerCharacterController.isGrounded)
        {
            animator.SetBool("IsWalking", true);
        }
        else if(playerMoveDirection.y < 0)
        {
            animator.SetBool("IsWalking", false);
        }


        Debug.Log("Toca el suelo:"+ playerCharacterController.isGrounded);
        Debug.Log(playerMoveDirection);

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

    //Funcion para las habilidades del jugador
    public void PlayerSkills()
    {
        if (playerCharacterController.isGrounded && Input.GetButtonDown("Jump"))
            {
            animator.SetBool("IsJumping",true);
            fallVelocity = jumpForce;
            playerMoveDirection.y = fallVelocity;
            
        }
        else
        {
            animator.SetBool("IsJumping", false);
        }
       
    }
}
