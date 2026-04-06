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
    public float dashSpeed = 15f; // Aumentado para que se note el impulso
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
    // ESTADOS Y REFERENCIAS
    // ================================
    private bool hasDoubleJumped = false;
    private CharacterController controller;
    private Animator animator;
    private Vector3 moveDirection;
    private float verticalVelocity;
    private Vector3 startingPoint;

    private float originalHeight;
    private Vector3 originalCenter;
    private bool isCrouched = false;

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
        if (isDashing) return;

        HandleCrouch();
        HandleDashInput(); // Dash puede interrumpir el agachado

     
        if (!isCrouched)
         HandleAnalogMovement();
        else
         StopHorizontalMovement();
        
        HandleVariableJump();
        ApplyGravity();

        controller.Move(moveDirection * Time.deltaTime);

        if (Input.GetButtonDown("Fire2"))
            TeleportToStart();
    }

    void HandleAnalogMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float inputIntensity = Mathf.Abs(horizontal);

        moveDirection.x = horizontal * moveSpeed;

        if (horizontal > 0.1f) transform.localScale = new Vector3(1, 1, 1);
        else if (horizontal < -0.1f) transform.localScale = new Vector3(1, 1, -1);

        bool isWalking = inputIntensity > 0.1f;
        animator.SetBool("IsWalking", isWalking);

        if (isWalking)
        {
            float animSpeed = Mathf.Lerp(0.5f, 1.0f, inputIntensity);
            animator.SetFloat("WalkSpeedMultiplier", animSpeed);
        }
    }

    void StopHorizontalMovement()
    {
        moveDirection.x = 0;
        animator.SetBool("IsWalking", false);
        animator.SetFloat("WalkSpeedMultiplier", 1.0f);
    }

    void HandleCrouch()
    {
        float verticalInput = Input.GetAxis("Vertical");
        // Solo permite agacharse si está en el suelo y no haciendo dash
        bool wantsToCrouch = verticalInput < -0.5f && controller.isGrounded;

        if (wantsToCrouch && !isCrouched)
        {
            isCrouched = true;
            animator.SetBool("IsCrouched", true);
            SetColliderHeight(originalHeight / 2.5f, 0.4f);
            if (crouchSound) audioSource.PlayOneShot(crouchSound);
        }
        else if (!wantsToCrouch && isCrouched)
        {
            ExitCrouch();
        }
    }

    void ExitCrouch()
    {
        isCrouched = false;
        animator.SetBool("IsCrouched", false);
        SetColliderHeight(originalHeight, 1f);
    }

    void SetColliderHeight(float newHeight, float centerMultiplier)
    {
        controller.height = newHeight;
        controller.center = new Vector3(originalCenter.x, originalCenter.y * centerMultiplier, originalCenter.z);
    }

    void HandleVariableJump()
    {
        if (controller.isGrounded)
        {
            coyoteTimer = coyoteTime;
            hasDoubleJumped = false;
            animator.SetBool("IsJumping", false);
            if (verticalVelocity < -5f && landSound) audioSource.PlayOneShot(landSound);
        }
        else
        {
            coyoteTimer -= Time.deltaTime;
            if (Input.GetButtonUp("Jump") && verticalVelocity > 0) verticalVelocity *= cutJumpHeight;
            if (verticalVelocity < -1f) animator.SetBool("IsJumping", true);
        }

        if (Input.GetButtonDown("Jump")) inputTimer = inputBuffer;
        else inputTimer -= Time.deltaTime;

        // SALTO NORMAL O EN PARED
        if (inputTimer > 0)
        {
            // Prioridad 1: Salto NORMAL
            if (coyoteTimer > 0 && !isCrouched)
            {
                DoJump(jumpForce, jumpSound);
                coyoteTimer = 0;
            }
            // Prioridad 2: Doble Salto
            else if (!controller.isGrounded && !hasDoubleJumped && canDoubleJump)
            {
                hasDoubleJumped = true;
                DoJump(jumpForce, doubleJumpSound);
            }
        }
    }

    void DoJump(float force, AudioClip sound)
    {
        verticalVelocity = force;
        moveDirection.y = verticalVelocity;
        animator.SetBool("IsJumping", true);
        if (sound) audioSource.PlayOneShot(sound);
        inputTimer = 0;
    }

    void ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0) verticalVelocity = -2f;
        else verticalVelocity -= gravity * Time.deltaTime;
        moveDirection.y = verticalVelocity;
    }

    void HandleDashInput()
    {
        bool dashInput = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetButtonDown("R2");

        if (dashInput && dashCooldownTimer <= 0 && canDash && controller.isGrounded)
        {
            // Si estamos agachados, cancelamos el estado para poder movernos
            //if (isCrouched) ExitCrouch();

            StartCoroutine(DashRoutine());
        }

        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;
    }

    IEnumerator DashRoutine()
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;
        animator.SetBool("IsDashing", true);
        if (dashSound) audioSource.PlayOneShot(dashSound);

        // El Dash siempre usa collider bajo (como un deslizamiento)
        SetColliderHeight(originalHeight / 2.5f, 0.4f);

        float direction = transform.localScale.z > 0 ? 1f : -1f;
        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            // Movimiento puramente horizontal durante el Dash
            controller.Move(new Vector3(direction * dashSpeed, 0, 0) * Time.deltaTime);
            yield return null;
        }

        // Recuperar altura si no hay techo (simplificado)
        SetColliderHeight(originalHeight, 1f);
        animator.SetBool("IsDashing", false);
        isDashing = false;
    }

    public void TeleportToStart()
    {
        controller.enabled = false;
        transform.position = startingPoint;
        verticalVelocity = 0;
        controller.enabled = true;
    }
}