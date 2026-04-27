using UnityEngine;
using System.Collections;
using UnityEngine.InputSystem; // Necesario para el New Input System

public class OneWayPlatform : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private Collider playerCollider;
    private Collider platformCollider;
    private Animator playerAnimator;

    // Variables para el Input System
    private PlayerInput playerInput;
    private InputAction moveAction;
    private InputAction jumpAction;

    private bool isWaitngToPass = false;

    void Start()
    {
        playerCollider = player.GetComponent<Collider>();
        platformCollider = GetComponent<Collider>();
        playerAnimator = player.GetComponent<Animator>();

        // Configuración de inputs
        playerInput = player.GetComponent<PlayerInput>();
        if (playerInput != null)
        {
            moveAction = playerInput.actions["Move"];
            jumpAction = playerInput.actions["Jump"];
        }
    }

    void Update()
    {
        if (playerCollider == null || playerAnimator == null || playerInput == null) return;

        float playerFeetY = playerCollider.bounds.min.y + 0.25f;
        float platformTopY = platformCollider.bounds.max.y;

        // 1. Detectar Inputs: ¿Pulsando abajo (-y) y presionó salto?
        Vector2 moveInput = moveAction.ReadValue<Vector2>();
        bool isPressingDown = moveInput.y < -0.5f;
        bool pressedJump = jumpAction.WasPressedThisFrame();

        // 2. Condición para bajar: Abajo + Salto + Estar encima de la plataforma
        if (isPressingDown && pressedJump && !isWaitngToPass && playerFeetY > platformTopY - 0.5f)
        {
            StartCoroutine(TemporaryDisableCollision());
        }

        // 3. LÓGICA DE COLISIÓN (Se mantiene igual para permitir subir desde abajo)
        if (playerFeetY < platformTopY - 0.1f || isWaitngToPass)
        {
            platformCollider.isTrigger = true;
        }
        else
        {
            platformCollider.isTrigger = false;
        }
    }

    IEnumerator TemporaryDisableCollision()
    {
        isWaitngToPass = true;

        // Aumentamos ligeramente el tiempo a 0.4s o 0.5s para asegurar que 
        // el cuerpo del jugador atraviese el colisionador completamente
        yield return new WaitForSeconds(0.2f);

        isWaitngToPass = false;
    }
}