using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    //Input System
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

    private bool isClimbing = false; // Para evitar que la corrutina se dispare mil veces



    void Awake()
    {
        // Inicializamos las referencias del Input System
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
        // 1. Salida rápida si estamos en medio de una acción bloqueante
        if (isDashing || isClimbing) return;

        // 2. Actualizar sensores
        wallCollision = collisionChecker.wallCollision;
        canClimb = collisionChecker.CanClimb;
        Vector2 inputVector = moveAction.ReadValue<Vector2>();

        // 3. CONDICIÓN CRÍTICA: 
        // Solo puede colgarse si: Toca pared + Puede escalar + NO está en el suelo
        bool beingHanging = wallCollision && canClimb && !controller.isGrounded;

        // 4. Interrupción manual: Si pulsa abajo, soltamos la pared
        if (beingHanging && inputVector.y < -0.1f)
        {
            beingHanging = false;
            verticalVelocity = -2f;
        }

        // 5. Aplicar estado al Animator
        animator.SetBool("IsHanging", beingHanging);

        if (beingHanging)
        {
            // ESTADO: COLGADO
            verticalVelocity = 0;
            moveDirection = Vector3.zero;

            if (inputVector.y > 0.1f)
            {
                StartCoroutine(ClimbRoutine());
            }

            // Mantenemos al personaje quieto en la pared
            controller.Move(moveDirection * Time.deltaTime);
        }
        else
        {
            // ESTADO: MOVIMIENTO NORMAL / CAÍDA
            HandleCrouch();
            HandleDashInput();

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

    // He extraído esto a un método para que tu Update sea legible
    void HandleHangingState()
    {
        StopHorizontalMovement();

        // Anulamos velocidad vertical para que no caiga por gravedad
        verticalVelocity = 0;
        moveDirection.y = 0;

        animator.SetBool("IsHanging", true);

        Vector2 inputVector = moveAction.ReadValue<Vector2>();

        // Iniciar escalada hacia arriba
        if (inputVector.y > 0 && !isClimbing)
        {
            StartCoroutine(ClimbRoutine());
        }
        // Soltarse de la pared hacia abajo
        else if (inputVector.y < 0)
        {
            animator.SetBool("IsHanging", false);
            // Le damos un pequeño empujón hacia abajo para que se separe de la pared
            verticalVelocity = -2f;
        }
    }



    IEnumerator ClimbRoutine()
    {
        isClimbing = true;
        animator.SetBool("canClimb", true);
        animator.SetBool("IsHanging", false);
        yield return null;
        // Ya no necesitas esperar segundos aquí, 
        // porque el "Event" de la animación hará el trabajo.
    }

    // Esta función la llamará la animación directamente
    public void FinishClimbMovement()
    {
        controller.enabled = false;
        float direction = transform.localScale.z > 0 ? 1f : -1f;
        transform.position += new Vector3(direction * 1f, 2f, 0);
        controller.enabled = true;

        animator.SetBool("canClimb", false);
        isClimbing = false; // Liberamos el control
    }

    void HandleAnalogMovement()
    {
        // Leer el Vector2 del Stick o D-Pad configurado en Move
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