using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem;
using System.Collections.Generic;

public class PlayerController : MonoBehaviour
{
    // === Sistema de Input (Nuevo Input System) ===
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dashAction;
    private InputAction crouchAction;
    private InputAction teleportAction;
    private InputAction prayAction;
    private InputAction northAction;
    private InputAction southAction;
    private InputAction eastAction;
    private InputAction westAction;

    [Header("Movimiento")]
    public float moveSpeed = 6f;
    public float gravity = 25f;
    public float jumpForce = 10f;
    [Range(0, 1)] public float cutJumpHeight = 0.75f; // Para controlar la altura del salto soltando el botón

    [Header("Dash")]
    public float dashSpeed = 8f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 0.7f;
    private bool isDashing = false;
    private float dashCooldownTimer = 0f;

    [Header("Coyote Time & Input Buffer")]
    public float coyoteTime = 0.1f;          // Margen para saltar justo al caer de una plataforma
    private float coyoteTimer;
    public float inputBuffer = 0.15f;        // Guarda el input de salto un instante antes de tocar el suelo
    private float inputTimer;

    // === Componentes y físicas básicas ===
    private bool hasDoubleJumped = false;
    private CharacterController controller;
    private Animator animator;
    private Vector3 moveDirection;
    private float verticalVelocity;

    // === Efecto de Fundido (Fade) ===
    private GameObject PanelFade;
    private Animator animatorPanelFade;

    // === Agacharse ===
    private float originalHeight;
    private Vector3 originalCenter;
    private bool isCrouched = false;

    [Header("Sonidos")]
    public AudioSource audioSource;
    public AudioSource audioSourceNoChanges; // Para sonidos estables sin variaciones de pitch
    public AudioClip jumpSound;
    public AudioClip doubleJumpSound;
    public AudioClip dashSound;
    public AudioClip crouchSound;
    public AudioClip landSound;
    public AudioClip power1Sound;
    public AudioClip jumpBuffSound;
    public AudioClip northPray;
    public AudioClip southPray;
    public AudioClip westPray;
    public AudioClip eastPray;

    [Header("Configuración de Sonido Aleatorio")]
    [Range(0.8f, 1.2f)] public float minPitch = 0.85f;
    [Range(0.8f, 1.2f)] public float maxPitch = 1.15f;

    [Header("Colliders")]
    private CollisionChecker collisionChecker;
    private bool wallCollision;
    private bool canClimb;
    private bool isGroundNear;

    // === Mecánica de Colgarse y Escalar ===
    private bool isClimbing = false;
    private bool isAlreadyHanging = false; // Bloqueo para alinearse solo una vez al tocar el borde
    [SerializeField] private Vector3 climbVector = new Vector3(1f, 1.995f, 0); // Desplazamiento final al subir
    public float offsetHanging = 0.3f;     // Separación con la pared al colgarse

    [Header("Pray")]
    private bool isPraying = false;
    private GameObject currentDoor;

    // Lista para almacenar la secuencia de rezos que voy pulsando
    private List<string> currentSequence = new List<string>();

    // Secuencia para abrir la puerta (Oeste, Este, Sur, Oeste, Este, Sur)
    private readonly List<string> correctSequence1 = new List<string> { "West", "East", "South", "West", "East", "South" };

    [Header("Jump Buff Pray")]
    public float jumpBuffMultiplier = 1.5f;
    public float buffDuration = 20f;
    private float originalJumpForce;
    private bool isJumpBuffActive = false;

    // Secuencia para super salto (Norte, Sur, Norte, Sur, Norte, Sur)
    private Renderer characterRenderer;
    private readonly List<string> correctSequenceJump = new List<string> { "North", "South", "North", "South", "North", "South" };

    [Header("Pray Visuals (Notes)")]
    public GameObject praySpritesContainer;
    public GameObject l2Sprite;
    public SpriteRenderer[] noteRenderers;

    [Header("Sprites Teclado / Ratón")]
    public Sprite keyboardNorth, keyboardSouth, keyboardEast, keyboardWest;

    [Header("Sprites Mando (Gamepad)")]
    public Sprite gamepadNorth, gamepadSouth, gamepadEast, gamepadWest;

    private GameObject currentAltar; // Altar en el que estoy metido actualmente

    void Awake()
    {
        // Enlazo el Input System y los inputs específicos
        playerInput = GetComponent<PlayerInput>();
        PanelFade = GameObject.Find("FadePanel");
        animatorPanelFade = PanelFade.GetComponent<Animator>();

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        dashAction = playerInput.actions["Dash"];
        crouchAction = playerInput.actions["Crouch"];
        teleportAction = playerInput.actions["Teleport"];
        prayAction = playerInput.actions["Pray"];
        northAction = playerInput.actions["North"];
        southAction = playerInput.actions["South"];
        eastAction = playerInput.actions["East"];
        westAction = playerInput.actions["West"];
    }

    void Start()
    {
        // Registro de componentes iniciales y guardado de valores originales
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        collisionChecker = GetComponent<CollisionChecker>();

        GameManager.Instance.CheckPoint = transform.position; // Checkpoint inicial al nacer
        originalHeight = controller.height;
        originalCenter = controller.center;
        characterRenderer = GetComponentInChildren<Renderer>();
        originalJumpForce = jumpForce;
    }

    #region Update

    private void FixedUpdate()
    {
        // Gestión limpia de transiciones de suelo en el animador para evitar tirones
        if (controller.isGrounded)
        {
            animator.SetBool("IsJumping", false);
            animator.SetBool("IsGrounded", true);
        }
        else
        {
            animator.SetBool("IsGrounded", false);
        }
    }

    void Update()
    {
        // Botón de emergencia para volver al último checkpoint manualmente
        if (teleportAction.WasPressedThisFrame()) BackToCheckPoint();

        // Actualizo datos del detector de colisiones externo
        wallCollision = collisionChecker.wallCollision;
        canClimb = collisionChecker.canClimb;
        isGroundNear = collisionChecker.isGroundNear;

        Vector2 inputVector = moveAction.ReadValue<Vector2>();

        // Si caigo al suelo rezando, aseguro que la animación de salto termine
        if (controller.isGrounded && isPraying)
        {
            animator.SetBool("IsJumping", false);
        }

        // Si estoy en mitad de un Dash o escalando, congelo por completo el resto de inputs del Update
        if (isDashing || isClimbing)
        {
            return;
        }

        // --- MODO REZO (PRAY MODE) ---
        // Exijo estar en el suelo y quieto para poder rezar
        if (prayAction.IsPressed() && controller.isGrounded && !isDashing && !isClimbing)
        {
            if (!isPraying)
                EnterPrayMode();

            HandlePrayInputs();
            return; // Bloqueo el movimiento normal mientras rezo
        }
        else if (isPraying && !prayAction.IsPressed())
        {
            ExitPrayMode(); // Si suelto el botón de rezo, salgo inmediatamente
        }

        // --- SISTEMA DE AGARRE EN PARED (HANGING) ---
        // Condición para estar colgado: Tocar pared (o estar ya colgado), que se pueda escalar y que el suelo no esté cerca
        bool beingHanging = (wallCollision || isAlreadyHanging) && canClimb && !isGroundNear;

        // Si pulso hacia abajo o toco el suelo, me descolgo automáticamente
        if (inputVector.y < -0.1f || controller.isGrounded || inputVector.x < 0 && transform.localScale.z == 1 || inputVector.x > 0 && transform.localScale.z == -1)
        {
            beingHanging = false;
            isAlreadyHanging = false;
        }

        animator.SetBool("IsHanging", beingHanging);

        if (beingHanging)
        {
            // Si es el primer frame en el que me cuelgo, ajusto mi posición visual con la pared
            if (!isAlreadyHanging)
            {
                CorrectHangingPosition();
                isAlreadyHanging = true;
            }

            verticalVelocity = 0;
            moveDirection = Vector3.zero; // Cancelo inercias previas

            // Si pulso arriba, inicio la animación/rutina de subir el bordillo
            if (inputVector.y > 0.1f || inputVector.x > 0 && transform.localScale.z == 1 || inputVector.x < 0 && transform.localScale.z == -1)
            {
                isAlreadyHanging = false;
                ClimbRoutine();
                return;
            }

            // Clavo al personaje en el sitio para que no le afecte la gravedad
            controller.Move(Vector3.zero);
        }
        else
        {
            // --- MOVIMIENTO NORMAL Y TIERRA ---
            isAlreadyHanging = false;

            HandleCrouch();
            HandleDashInput();

            // Solo me muevo de lado si no estoy agachado
            if (!isCrouched)
                HandleAnalogMovement();
            else
                StopHorizontalMovement();

            HandleVariableJump();
            ApplyGravity();

            // Ejecuto el movimiento final calculado en este frame
            controller.Move(moveDirection * Time.deltaTime);
        }
    }
    #endregion

    private void CorrectHangingPosition()
    {
        // Apago el controller un momento para poder teletransportar al personaje sin conflictos de colisión
        controller.enabled = false;

        // Detecto hacia dónde miro usando la escala en Z
        float faceDir = transform.localScale.z > 0 ? 1f : -1f;

        // Ajusto la X basándome en el punto exacto de impacto que detectó el CollisionChecker
        Vector3 alignedPos = collisionChecker.wallHitPoint;
        alignedPos.x -= (offsetHanging * faceDir);
        alignedPos.y = transform.position.y;
        alignedPos.z = transform.position.z;

        transform.position = alignedPos;
        controller.enabled = true;
    }

    private void ClimbRoutine()
    {
        isClimbing = true;
        animator.SetBool("canClimb", true);
        animator.SetBool("IsHanging", true);
    }

    // Este método lo llamo desde un Evento de Animación justo cuando termina de subir visualmente
    public void FinishClimbMovement()
    {
        controller.enabled = false;
        float direction = transform.localScale.z > 0 ? 1f : -1f;
        // Desplazo al personaje arriba y adelante para dejarlo sobre la plataforma
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

        // Volteo el personaje (Flip) cambiando la escala en Z según la dirección
        if (horizontal > 0.1f) transform.localScale = new Vector3(1, 1, 1);
        else if (horizontal < -0.1f) transform.localScale = new Vector3(1, 1, -1);

        bool isWalking = inputIntensity > 0.1f;
        animator.SetBool("IsWalking", isWalking);

        // Ajusto la velocidad de la animación de caminar según cuánto incline el joystick
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
            SetColliderHeight(originalHeight / 2.5f, 0.4f); // Encojo el collider a menos de la mitad
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
        SetColliderHeight(originalHeight, 1f); // Devuelvo el collider a su tamaño original
    }

    void SetColliderHeight(float newHeight, float centerMultiplier)
    {
        controller.height = newHeight;
        controller.center = new Vector3(originalCenter.x, originalCenter.y * centerMultiplier, originalCenter.z);
    }

    public void HandleVariableJump()
    {
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        bool isPressingDown = moveInput.y < -0.5f;

        if (controller.isGrounded)
        {
            coyoteTimer = coyoteTime; // Reseteo el tiempo de Coyote al pisar el suelo
            hasDoubleJumped = false;
            animator.SetBool("IsJumping", false);
            // Si venía cayendo rápido, reproduzco sonido de aterrizaje con pitch variado
            if (verticalVelocity < -5f && landSound) PlayRandomPitch(landSound);
        }
        else
        {
            coyoteTimer -= Time.deltaTime;

            // SALTO VARIABLE: Si suelto el botón de salto subiendo, corto el impulso a la mitad
            if (jumpAction.WasReleasedThisFrame() && verticalVelocity > 0)
                verticalVelocity *= cutJumpHeight;

            if (verticalVelocity < -1f) animator.SetBool("IsJumping", true);
        }

        // Buffer de entrada para el salto
        if (jumpAction.WasPressedThisFrame()) inputTimer = inputBuffer;
        else inputTimer -= Time.deltaTime;

        if (inputTimer > 0)
        {
            // Bloqueo el salto si estoy pulsando hacia abajo (evita saltos raros al querer bajar/agacharse)
            if (coyoteTimer > 0 && !isCrouched && !isPressingDown)
            {
                DoJump(jumpForce, jumpSound);
                coyoteTimer = 0; // Consumo el Coyote Time
            }
            // Lógica para el doble salto si está desbloqueado en el juego
            else if (!controller.isGrounded && !hasDoubleJumped && GameManager.Instance.canDoubleJump && !isPressingDown)
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

        // Activo los triggers necesarios en el Animator
        if (animator.GetBool("IsJumping"))
        {
            animator.SetTrigger("DoubleJump");
        }
        else
        {
            animator.SetTrigger("DoubleJump");
            animator.SetBool("IsJumping", true);
        }

        if (sound) PlayRandomPitch(sound);
        inputTimer = 0; // Consumo el buffer de salto
    }

    void ApplyGravity()
    {
        // Fuerza constante hacia abajo al estar en el suelo para evitar que flote en pendientes descendientes
        if (controller.isGrounded && verticalVelocity < 0) verticalVelocity = -2f;
        else verticalVelocity -= gravity * Time.deltaTime;

        moveDirection.y = verticalVelocity;
    }

    public void HandleDashInput()
    {
        // Solo permito el dash si está listo, desbloqueado en el GameManager y estoy tocando el suelo
        if (dashAction.WasPressedThisFrame() && dashCooldownTimer <= 0 && GameManager.Instance.canDash && controller.isGrounded)
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
        if (dashSound) PlayRandomPitch(dashSound);

        SetColliderHeight(originalHeight / 2.5f, 0.4f); // Encojo el collider para pasar por sitios estrechos durante el dash

        float direction = transform.localScale.z > 0 ? 1f : -1f;
        float startTime = Time.time;

        // Bucle temporal para desplazar al personaje a velocidad constante lo que dure el dash
        while (Time.time < startTime + dashDuration)
        {
            controller.Move(new Vector3(direction * dashSpeed, 0, 0) * Time.deltaTime);
            yield return null;
        }

        SetColliderHeight(originalHeight, 1f); // Devuelvo el collider a la normalidad
        animator.SetBool("IsDashing", false);
        isDashing = false;
    }

    public void BackToCheckPoint()
    {
        animatorPanelFade.SetTrigger("Fade"); // Lanzo la animación de fundido a negro
        controller.enabled = false;
        transform.position = GameManager.Instance.CheckPoint; // Lo muevo al checkpoint guardado
        verticalVelocity = 0;
        controller.enabled = true;
    }

    private void HandlePrayInputs()
    {
        // Escucho las 4 direcciones mientras rezo para guardar el código e iniciar sus respectivos sonidos estables
        if (northAction.WasPressedThisFrame())
        {
            RegisterPrayInput("North");
            audioSourceNoChanges.PlayOneShot(northPray);
        }
        if (southAction.WasPressedThisFrame())
        {
            RegisterPrayInput("South");
            audioSourceNoChanges.PlayOneShot(southPray);
        }
        if (eastAction.WasPressedThisFrame())
        {
            RegisterPrayInput("East");
            audioSourceNoChanges.PlayOneShot(eastPray);
        }
        if (westAction.WasPressedThisFrame())
        {
            RegisterPrayInput("West");
            audioSourceNoChanges.PlayOneShot(westPray);
        }
    }

    private void EnterPrayMode()
    {
        if (l2Sprite != null) l2Sprite.SetActive(false); // Oculto el texto flotante de "Rezar"
        isPraying = true;
        currentSequence.Clear(); // Limpio basura de intentos anteriores
        animator.SetBool("IsPraying", true);

        if (praySpritesContainer != null) praySpritesContainer.SetActive(true); // Muestro el HUD de las notas musicales
        ClearAllNotes();

        // Freno en seco al personaje para que rece quieto
        moveDirection = Vector3.zero;
        verticalVelocity = 0;
    }

    private void ExitPrayMode()
    {
        isPraying = false;
        animator.SetBool("IsPraying", false);
        currentSequence.Clear();
        if (praySpritesContainer != null) praySpritesContainer.SetActive(false); // Oculto el HUD de notas
    }

    private void RegisterPrayInput(string dir)
    {
        // Si ya metí 6 notas (el límite de la contraseña), limpio el HUD para volver a empezar desde la primera
        if (currentSequence.Count >= noteRenderers.Length)
        {
            currentSequence.Clear();
            ClearAllNotes();
        }

        // Pinto el sprite de la dirección correspondiente en el hueco que toca del HUD
        if (currentSequence.Count < noteRenderers.Length)
        {
            DrawNote(currentSequence.Count, dir);
        }

        // --- AJUSTE VISUAL DEL ESPEJO ---
        // Si el personaje mira a la izquierda (Z < 0), invierto Este y Oeste solo para la animación visual,
        // garantizando que los movimientos del cuerpo coincidan correctamente hacia donde mira.
        bool isFacingLeft = transform.localScale.z < 0;
        string finalDir = dir;

        if (isFacingLeft)
        {
            if (dir == "East") finalDir = "West";
            else if (dir == "West") finalDir = "East";
        }

        animator.SetTrigger(finalDir);

        // Agrego la dirección ORIGINAL a la lista técnica, para que la contraseña no falle por el "Espejo visual"
        currentSequence.Add(dir);

        CheckSequence(); // Compruebo si ya completó alguna combinación correcta
    }

    private InputDevice GetLastUsedDevice()
    {
        InputDevice activeDevice = null;
        double latestUpdateTime = 0;

        foreach (var device in InputSystem.devices)
        {
            if (device is Gamepad || device is Keyboard || device is Mouse)
            {
                if (device.lastUpdateTime > latestUpdateTime)
                {
                    latestUpdateTime = device.lastUpdateTime;
                    activeDevice = device;
                }
            }
        }
        return activeDevice;
    }

    private void DrawNote(int index, string dir)
    {
        if (noteRenderers[index] == null) return;

        // Averiguamos el dispositivo en el instante en que pulsas el botón
        InputDevice lastDevice = GetLastUsedDevice();
        bool isGamepad = lastDevice is Gamepad;

        // Asignamos el sprite correspondiente según el dispositivo y la dirección
        switch (dir)
        {
            case "North":
                noteRenderers[index].sprite = isGamepad ? gamepadNorth : keyboardNorth;
                break;
            case "South":
                noteRenderers[index].sprite = isGamepad ? gamepadSouth : keyboardSouth;
                break;
            case "East":
                noteRenderers[index].sprite = isGamepad ? gamepadEast : keyboardEast;
                break;
            case "West":
                noteRenderers[index].sprite = isGamepad ? gamepadWest : keyboardWest;
                break;
        }
    }

    private void ClearAllNotes()
    {
        // Limpio por completo las imágenes de los slots del HUD de rezo
        foreach (var renderer in noteRenderers)
        {
            if (renderer != null) renderer.sprite = null;
        }
    }

    private void CheckSequence()
    {
        // Verifico si es la secuencia para romper la puerta
        CheckSpecificSequence(currentSequence, correctSequence1, ActivatePray1);

        // Verifico si es la secuencia para el buff de salto
        CheckSpecificSequence(currentSequence, correctSequenceJump, ActivateJumpBuff);
    }

    // Método automatizado para comparar listas de secuencias sin duplicar código
    private void CheckSpecificSequence(List<string> current, List<string> target, System.Action onSuccess)
    {
        if (current.Count != target.Count) return;

        for (int i = 0; i < current.Count; i++)
        {
            if (current[i] != target[i]) return; // Si un solo botón no coincide, aborto
        }

        onSuccess?.Invoke(); // Ejecuto la función que pasé por parámetro (ActivatePray1 o ActivateJumpBuff)
        currentSequence.Clear();
    }

    private void ActivatePray1()
    {
        l2Sprite.SetActive(false);
        animator.SetTrigger("Power1");

        // Si tengo una puerta guardada en rango, la destruyo y limpio la referencia
        if (currentDoor != null)
        {
            Destroy(currentDoor);
            audioSource.PlayOneShot(power1Sound);
            currentDoor = null;
        }
    }

    private void ActivateJumpBuff()
    {
        // Solo lo activo si no está ya puesto y si estoy físicamente dentro de un Altar válido
        if (!isJumpBuffActive && currentAltar != null)
        {
            StartCoroutine(JumpBuffRoutine());
        }
        else if (currentAltar == null)
        {
            currentSequence.Clear(); // Si intento el truco fuera del altar, limpio el código metido
        }
    }

    IEnumerator JumpBuffRoutine()
    {
        isJumpBuffActive = true;
        jumpForce = originalJumpForce * jumpBuffMultiplier; // Aplico la multiplicación de salto
        audioSourceNoChanges.PlayOneShot(jumpBuffSound);

        animator.SetTrigger("Power1");
        if (power1Sound) audioSourceNoChanges.PlayOneShot(power1Sound);

        // Feedback visual: Instancio el material de forma única y pinto al personaje de rojo
        if (characterRenderer != null)
        {
            characterRenderer.material.color = Color.red;
        }

        yield return new WaitForSeconds(buffDuration); // Mantengo el estado el tiempo configurado

        // Restauración de valores iniciales (fuerza y color blanco original)
        jumpForce = originalJumpForce;

        if (characterRenderer != null)
        {
            characterRenderer.material.color = Color.white;
        }

        isJumpBuffActive = false;
    }

    private void PlayRandomPitch(AudioClip clip)
    {
        if (clip == null || audioSource == null) return;

        // Modifico levemente el tono (pitch) para que los sonidos repetitivos como saltar no cansen al jugador
        audioSource.pitch = Random.Range(minPitch, maxPitch);
        audioSource.PlayOneShot(clip);
    }

    // === DETECCIÓN DE ZONAS E INTERACTUABLES (TRIGGERS) ===
    private void OnTriggerEnter(Collider other)
    {
        // Almaceno la puerta si entro en su área
        if (other.CompareTag("Door1"))
        {
            currentDoor = other.gameObject;
        }
        // Almaceno el altar si entro en su área
        if (other.CompareTag("Altar"))
        {
            currentAltar = other.gameObject;
        }
        // Actualizo checkpoint del GameManager automáticamente al pisar la zona
        if (other.CompareTag("CheckPoint"))
        {
            GameManager.Instance.CheckPoint = transform.position;
        }
        // Si caigo al vacío, me devuelvo instantáneamente al checkpoint
        if (other.CompareTag("DeadZone"))
        {
            BackToCheckPoint();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        // Mientras esté cerca de altares o puertas, muestro el aviso de "Rezar" en pantalla (L2) a menos que ya esté rezando
        if (other.CompareTag("Door1") || other.CompareTag("Altar"))
        {
            if (!isPraying)
            {
                l2Sprite.SetActive(true);
            }
            else
            {
                l2Sprite.SetActive(false);
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Al alejarme de los interactuables, apago avisos y limpio referencias por seguridad (así evito activar cosas desde lejos)
        if (other.CompareTag("Door1"))
        {
            l2Sprite.SetActive(false);
            currentDoor = null;
        }
        if (other.CompareTag("Altar"))
        {
            currentAltar = null;
            l2Sprite.SetActive(false);
        }
    }
}