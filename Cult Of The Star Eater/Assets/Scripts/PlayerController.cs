using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    // ================================
    // CONFIGURACIÓN DE MOVIMIENTO
    // ================================
    [Header("Movimiento")]
    public float moveSpeed = 6f;
    public float gravity = 25f;
    public float jumpForce = 12f;
    [Range(0, 1)] public float cutJumpHeight = 0.5f;

    // ================================
    // CONFIGURACIÓN DE DASH
    // ================================
    [Header("Dash")]
    public float dashSpeed = 8f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 0.7f;

    private bool isDashing = false;
    private float dashCooldownTimer = 0f;

    // ================================
    // DESBLOQUEABLES
    // ================================
    [Header("Desbloqueables")]
    public bool canDash = false;
    public bool canDoubleJump = false;

    // ================================
    // COYOTE TIME & INPUT BUFFER
    // ================================
    [Header("Coyote Time & Input Buffer")]
    public float coyoteTime = 0.2f;
    private float coyoteTimer;

    public float inputBuffer = 0.15f;
    private float inputTimer;

    // ================================
    // DOBLE SALTO
    // ================================
    private bool hasDoubleJumped = false;


    // ================================
    // REFERENCIAS
    // ================================
    private CharacterController controller;
    private Animator animator;

    private Vector3 moveDirection;
    private float verticalVelocity;

    private Vector3 startingPoint;

    // ================================
    // COLLIDER & ESTADOS
    // ================================
    private float originalHeight;
    private Vector3 originalCenter;
    private bool isCrouched = false;

    // ================================
    // SONIDOS
    // ================================
    [Header("Sonidos")]
    public AudioSource audioSource;
    public AudioClip jumpSound;
    public AudioClip doubleJumpSound;
    public AudioClip dashSound;
    public AudioClip crouchSound;
    public AudioClip landSound;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();

        startingPoint = transform.position;

        originalHeight = controller.height;
        originalCenter = controller.center;
    }

    void Update()
    {
        // Si está en dash, ignoramos todo input
        if (isDashing) return;


        HandleCrouch();

        if (!isCrouched)
            HandleAnalogMovement();
        else
            StopHorizontalMovement();

        HandleVariableJump();
        ApplyGravity();

        HandleDashInput();

        controller.Move(moveDirection * Time.deltaTime);

        // Teletransporte debug
        if (Input.GetButtonDown("Fire2"))
            TeleportToStart();
    }

    // ================================
    // MOVIMIENTO HORIZONTAL
    // ================================
    void HandleAnalogMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        moveDirection.x = horizontal * moveSpeed;

        // Girar sprite según dirección
        if (horizontal > 0.1f) transform.localScale = new Vector3(1, 1, 1);
        else if (horizontal < -0.1f) transform.localScale = new Vector3(1, 1, -1);

        animator.SetBool("IsWalking", Mathf.Abs(horizontal) > 0.1f);
    }

    void StopHorizontalMovement()
    {
        moveDirection.x = 0;
        animator.SetBool("IsWalking", false);
    }

    // ================================
    // AGACHARSE
    // ================================
    void HandleCrouch()
    {
        float verticalInput = Input.GetAxis("Vertical");

        bool wantsToCrouch = verticalInput < -0.5f && controller.isGrounded;

        if (wantsToCrouch && !isCrouched)
        {
            isCrouched = true;
            animator.SetBool("IsCrouched", true);
            SetColliderHeight(originalHeight / 2.5f, 0.4f);
            audioSource.PlayOneShot(crouchSound);
        }
        else if (!wantsToCrouch && isCrouched)
        {
            isCrouched = false;
            animator.SetBool("IsCrouched", false);
            SetColliderHeight(originalHeight, 1f);
        }
    }

    void SetColliderHeight(float newHeight, float centerMultiplier)
    {
        controller.height = newHeight;
        controller.center = new Vector3(
            originalCenter.x,
            originalCenter.y * centerMultiplier,
            originalCenter.z
        );
    }

    // ================================
    // SALTO + COYOTE + DOBLE SALTO + WALL JUMP
    // ================================
    void HandleVariableJump()
    {
        if (controller.isGrounded)
        {
            coyoteTimer = coyoteTime;
            hasDoubleJumped = false;
            animator.SetBool("IsJumping", false);

            if (verticalVelocity < -5f)
                audioSource.PlayOneShot(landSound);
        }
        else
        {
            coyoteTimer -= Time.deltaTime;

            if (Input.GetButtonUp("Jump") && verticalVelocity > 0)
                verticalVelocity *= cutJumpHeight;

            if (verticalVelocity < -1f)
                animator.SetBool("IsJumping", true);
        }

        if (Input.GetButtonDown("Jump"))
            inputTimer = inputBuffer;
        else
            inputTimer -= Time.deltaTime;

        // SALTO NORMAL
        if (inputTimer > 0 && coyoteTimer > 0 && !isCrouched)
        {
            DoJump(jumpForce, jumpSound);
            coyoteTimer = 0;
            return;
        }


        // DOBLE SALTO
        if (inputTimer > 0 && !controller.isGrounded && !hasDoubleJumped && canDoubleJump)
        {
            hasDoubleJumped = true;
            DoJump(jumpForce, doubleJumpSound);
            return;
        }
    }

    void DoJump(float force, AudioClip sound)
    {
        verticalVelocity = force;
        animator.SetBool("IsJumping", true);
        audioSource.PlayOneShot(sound);
        inputTimer = 0;
    }

    // ================================
    // GRAVEDAD
    // ================================
    void ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0)
            verticalVelocity = -2f;
        else
            verticalVelocity -= gravity * Time.deltaTime;

        moveDirection.y = verticalVelocity;
    }

    // ================================
    // DASH
    // ================================
    void HandleDashInput()
    {
        bool dashInput = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetButtonDown("R2");

        if (controller.isGrounded && !isCrouched && dashInput && dashCooldownTimer <= 0 && canDash)
        {
            StartCoroutine(DashRoutine());
        }

        if (dashCooldownTimer > 0)
            dashCooldownTimer -= Time.deltaTime;
    }

    IEnumerator DashRoutine()
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;

        animator.SetBool("IsDashing", true);
        audioSource.PlayOneShot(dashSound);

        SetColliderHeight(originalHeight / 2.5f, 0.4f);

        float direction = transform.localScale.z > 0 ? 1f : -1f;
        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            controller.Move(new Vector3(direction * dashSpeed, 0, 0) * Time.deltaTime);
            yield return null;
        }

        SetColliderHeight(originalHeight, 1f);
        animator.SetBool("IsDashing", false);
        isDashing = false;
    }

    // ================================
    // DEBUG: TELETRANSPORTE
    // ================================
    public void TeleportToStart()
    {
        controller.enabled = false;
        transform.position = startingPoint;
        verticalVelocity = 0;
        controller.enabled = true;
    }
}
