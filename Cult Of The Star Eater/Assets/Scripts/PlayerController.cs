using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    // Input System
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dashAction;
    private InputAction crouchAction;
    private InputAction teleportAction;

    [Header("Movimiento")]
    public float moveSpeed = 6f;
    public float gravity = 25f;
    public float jumpForce = 12f;
    [Range(0, 1)] public float cutJumpHeight = 0.5f;

    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 0.7f;

    private bool isDashing = false;
    private float dashCooldownTimer = 0f;

    [Header("Desbloqueables")]
    public bool canDash = false;
    public bool canDoubleJump = false;

    [Header("Coyote Time & Input Buffer")]
    public float coyoteTime = 0.2f;
    private float coyoteTimer;
    public float inputBuffer = 0.15f;
    private float inputTimer;

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

    [Header("Colliders")]
    private CollisionChecker collisionChecker;
    private bool wallCollision;
    private bool canClimb;
    private bool isGroundNear;

    private bool isClimbing = false;
    private bool isAlreadyHanging = false; // Control para la alineación única
    [SerializeField] private Vector3 climbVector = new Vector3(1f, 1.9f, 0);
    public float offsetHanging = 0.3f;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        dashAction = playerInput.actions["Dash"];
        crouchAction = playerInput.actions["Crouch"];
        teleportAction = playerInput.actions["Teleport"];
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        collisionChecker = GetComponent<CollisionChecker>();
        startingPoint = transform.position;
        originalHeight = controller.height;
        originalCenter = controller.center;
    }

    void Update()
    {
        if (isDashing || isClimbing) return;

        wallCollision = collisionChecker.wallCollision;
        canClimb = collisionChecker.CanClimb;
        isGroundNear = collisionChecker.isGroundNear;
        Vector2 inputVector = moveAction.ReadValue<Vector2>();

        // Lógica de detección: se mantiene si ya estábamos colgados o si el rayo detecta pared
        bool beingHanging = (wallCollision || isAlreadyHanging) && canClimb && !isGroundNear;

        // Salida por suelo o input hacia abajo
        if (inputVector.y < -0.1f || controller.isGrounded)
        {
            beingHanging = false;
            isAlreadyHanging = false;
        }

        animator.SetBool("IsHanging", beingHanging);

        if (beingHanging)
        {
            // --- ESTADO COLGADO ---

            // Alineamos al personaje en el frame que toca la pared
            if (!isAlreadyHanging)
            {
                CorrectHangingPosition();
                isAlreadyHanging = true;
            }

            verticalVelocity = 0;
            moveDirection = Vector3.zero; // Bloquea cualquier movimiento previo

            // Iniciar escalada
            if (inputVector.y > 0.1f)
            {
                isAlreadyHanging = false;
                StartCoroutine(ClimbRoutine());
                return;
            }

            // Mantiene al personaje estático en el aire
            controller.Move(Vector3.zero);
        }
        else
        {
            // --- ESTADO MOVIMIENTO NORMAL ---
            isAlreadyHanging = false;

            HandleCrouch();
            HandleDashInput();

            // Solo procesamos movimiento horizontal si NO estamos colgados
            if (!isCrouched)
                HandleAnalogMovement();
            else
                StopHorizontalMovement();

            HandleVariableJump();
            ApplyGravity();

            controller.Move(moveDirection * Time.deltaTime);
        }

        if (teleportAction.WasPressedThisFrame()) TeleportToStart();
    }

    private void CorrectHangingPosition()
    {
        controller.enabled = false;

        // Dirección basada en escala (Z es tu eje de flip según el CollisionChecker)
        float faceDir = transform.localScale.z > 0 ? 1f : -1f;

        // Usamos el punto de impacto exacto del Raycast para el ajuste
        Vector3 alignedPos = collisionChecker.wallHitPoint;
        alignedPos.x -= (offsetHanging * faceDir);
        alignedPos.y = transform.position.y;
        alignedPos.z = transform.position.z;

        transform.position = alignedPos;
        controller.enabled = true;
    }

    IEnumerator ClimbRoutine()
    {
        isClimbing = true;
        // La posición ya es correcta por el ajuste al colgarse
        animator.SetBool("canClimb", true);
        animator.SetBool("IsHanging", false);
        yield return null;
    }

    public void FinishClimbMovement()
    {
        controller.enabled = false;
        float direction = transform.localScale.z > 0 ? 1f : -1f;
        transform.position += new Vector3(direction * climbVector.x, climbVector.y, 0);
        controller.enabled = true;

        animator.SetBool("canClimb", false);
        isClimbing = false;
    }

    void HandleAnalogMovement()
    {
        Vector2 inputVector = moveAction.ReadValue<Vector2>();
        float horizontal = inputVector.x;
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
        Debug.Log("Dejó de moverse");
        moveDirection.x = 0;
        animator.SetBool("IsWalking", false);
        animator.SetFloat("WalkSpeedMultiplier", 1.0f);
    }

    void HandleCrouch()
    {

        Vector2 inputVector = moveAction.ReadValue<Vector2>();
        bool wantsToCrouch = crouchAction.IsPressed() && controller.isGrounded;

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

    public void HandleVariableJump()
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
            // Salto variable al soltar el botón
            if (jumpAction.WasReleasedThisFrame() && verticalVelocity > 0)
                verticalVelocity *= cutJumpHeight;

            if (verticalVelocity < -1f) animator.SetBool("IsJumping", true);
        }

        if (jumpAction.WasPressedThisFrame()) inputTimer = inputBuffer;
        else inputTimer -= Time.deltaTime;

        if (inputTimer > 0)
        {
            if (coyoteTimer > 0 && !isCrouched)
            {
                DoJump(jumpForce, jumpSound);
                coyoteTimer = 0;
            }
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

    public void HandleDashInput()
    {
        // Dash es "Right Trigger"
        if (dashAction.WasPressedThisFrame() && dashCooldownTimer <= 0 && canDash && controller.isGrounded)
        {
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

    public void TeleportToStart()
    {
        controller.enabled = false;
        transform.position = startingPoint;
        verticalVelocity = 0;
        controller.enabled = true;
    }
}