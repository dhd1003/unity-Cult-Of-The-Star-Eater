using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Configuración de Movimiento")]
    public float moveSpeed = 6f;
    public float gravity = 25f;
    public float jumpForce = 12f;
    [Range(0, 1)] public float cutJumpHeight = 0.5f;

    [Header("Configuración de Dash")]
    public float dashSpeed = 8f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 0.7f;
    private bool isDashing = false;
    private float dashCooldownTimer = 0f;

    [Header("Referencias")]
    private CharacterController controller;
    private Animator animator;
    private Vector3 moveDirection;
    private Vector3 startingPoint;
    private float verticalVelocity;

    // Variables para el Collider y Estado
    private float originalHeight;
    private Vector3 originalCenter;
    private bool isCrouched = false;

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
        // 1. Si está haciendo Dash, ignoramos el resto del Input
        if (isDashing) return;

        // 2. Lógica de Agacharse (Bloquea movimiento)
        HandleCrouch();

        // 3. Movimiento Horizontal (Solo si NO está agachado)
        if (!isCrouched)
        {
            HandleAnalogMovement();
        }
        else
        {
            moveDirection.x = 0; // Forzamos que no se mueva horizontalmente
            animator.SetBool("IsWalking", false);
        }

        // 4. Salto y Gravedad
        HandleVariableJump();
        ApplyGravityLogic();

        // 5. DASH: Solo en el suelo, si no está agachado y cooldown disponible
        bool dashInput = Input.GetKeyDown(KeyCode.LeftShift) || Input.GetButtonDown("R2");
        if (controller.isGrounded && !isCrouched && dashInput && dashCooldownTimer <= 0)
        {
            StartCoroutine(DashRoutine());
        }

        // Timer de Cooldown
        if (dashCooldownTimer > 0) dashCooldownTimer -= Time.deltaTime;

        // Movimiento Final
        controller.Move(moveDirection * Time.deltaTime);

        if (Input.GetButtonDown("Fire2")) TeleportToStart();
    }

    void HandleAnalogMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        moveDirection.x = horizontal * moveSpeed;

        // Giro de personaje (Lógica eje Z)
        if (horizontal > 0.1f) transform.localScale = new Vector3(1, 1, 1);
        else if (horizontal < -0.1f) transform.localScale = new Vector3(1, 1, -1);

        animator.SetBool("IsWalking", Mathf.Abs(horizontal) > 0.1f);
    }

    void HandleCrouch()
    {
        float verticalInput = Input.GetAxis("Vertical");

        // Detectar si pulsa abajo (S, Flecha Abajo o Joystick)
        if (verticalInput < -0.5f && controller.isGrounded)
        {
            if (!isCrouched)
            {
                isCrouched = true;
                animator.SetBool("IsCrouched", true);
                SetColliderHeight(originalHeight / 2.5f);
                Debug.Log("Darcy está agachada (Movimiento bloqueado)");
            }
        }
        else
        {
            if (isCrouched)
            {
                isCrouched = false;
                animator.SetBool("IsCrouched", false);
                SetColliderHeight(originalHeight);
                Debug.Log("Darcy se ha levantado");
            }
        }
    }

    void SetColliderHeight(float newHeight)
    {
        controller.height = newHeight;
        controller.center = new Vector3(originalCenter.x, newHeight / 2f, originalCenter.z);
    }

    void HandleVariableJump()
    {
        if (controller.isGrounded)
        {
            animator.SetBool("IsJumping", false);
            // Solo saltar si no está agachado
            if (Input.GetButtonDown("Jump") && !isCrouched)
            {
                verticalVelocity = jumpForce;
                animator.SetBool("IsJumping", true);
            }
        }
        else
        {
            if (Input.GetButtonUp("Jump") && verticalVelocity > 0) verticalVelocity *= cutJumpHeight;
            if (verticalVelocity < -1f) animator.SetBool("IsJumping", true);
        }
    }

    void ApplyGravityLogic()
    {
        if (controller.isGrounded && verticalVelocity < 0) verticalVelocity = -2f;
        else verticalVelocity -= gravity * Time.deltaTime;

        moveDirection.y = verticalVelocity;
    }

    IEnumerator DashRoutine()
    {
        isDashing = true;
        dashCooldownTimer = dashCooldown;
        animator.SetBool("IsDashing", true);

        // Reducimos el collider durante el dash para pasar por huecos
        SetColliderHeight(originalHeight / 2.5f);

        float dashDirection = transform.localScale.z > 0 ? 1f : -1f;
        float startTime = Time.time;

        while (Time.time < startTime + dashDuration)
        {
            controller.Move(new Vector3(dashDirection * dashSpeed, 0, 0) * Time.deltaTime);
            yield return null;
        }

        SetColliderHeight(originalHeight);
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