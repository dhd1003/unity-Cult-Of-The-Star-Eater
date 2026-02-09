using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody))]

public class PlayerController : MonoBehaviour
{
    [Header("Movimiento")]
    public float moveSpeed = 8f;

    [Header("Salto")]
    public float jumpForce = 7f;
    public int maxJumps = 2;

    [Header("Dash")]
    public float dashSpeed = 20f;
    public float dashDuration = 0.2f;
    public float dashCooldown = 0.5f;

    [Header("Ground Check")]
    public Transform groundCheck;
    public float groundDistance = 0.3f;
    public LayerMask groundLayer;

    private Rigidbody rb;
    private int jumpsRemaining;
    private bool isGrounded;
    private bool isDashing;
    private bool canDash = true;

    private float moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody>();

        // Congelar rotaciones para estilo 2D
        rb.constraints = RigidbodyConstraints.FreezeRotation;
    }

    void Update()
    {
        CheckGround();

        if (!isDashing)
        {
            moveInput = Input.GetAxisRaw("Horizontal");
        }

        HandleJump();

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            TryDash();
        }
    }

    void FixedUpdate()
    {
        if (!isDashing)
        {
            rb.velocity = new Vector3(moveInput * moveSpeed, rb.velocity.y, 0f);
        }
    }

    void HandleJump()
    {
        if (isGrounded)
        {
            jumpsRemaining = maxJumps;
        }

        if (Input.GetButtonDown("Jump") && jumpsRemaining > 0)
        {
            rb.velocity = new Vector3(rb.velocity.x, 0f, 0f);
            rb.AddForce(Vector3.up * jumpForce, ForceMode.VelocityChange);
            jumpsRemaining--;
        }
    }

    void CheckGround()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundLayer);
    }

    void TryDash()
    {
        if (canDash)
        {
            StartCoroutine(Dash());
        }
    }

    IEnumerator Dash()
    {
        canDash = false;
        isDashing = true;

        float originalGravity = rb.useGravity ? Physics.gravity.y : 0f;

        rb.velocity = new Vector3(moveInput == 0 ? transform.localScale.x : moveInput, 0f, 0f).normalized * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    void OnDrawGizmosSelected()
    {
        if (groundCheck != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(groundCheck.position, groundDistance);
        }
    }
}
