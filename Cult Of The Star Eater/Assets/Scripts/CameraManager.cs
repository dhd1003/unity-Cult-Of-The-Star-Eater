using Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraManager : MonoBehaviour
{
    private CinemachineVirtualCamera vcam;
    private CinemachineFramingTransposer framingTransposer;

    [Header("Input")]
    [SerializeField] private PlayerInput playerInput;
    private InputAction prayAction;
    private InputAction moveCameraAction;

    [Header("Configuración de Zoom")]
    public float normalFOV = 11f;
    public float prayFOV = 8f;
    public float zoomSpeed = 5f;

    [Header("Configuración de Desplazamiento Vertical")]
    public float yOffsetDown = -2f; // Cuánto baja la cámara
    public float yOffsetUp = 2f;   // Cuánto sube la cámara
    public float offsetSpeed = 3f;  // Velocidad de la transición
    private float normalYOffset;

    [Header("Configuración de Desplazamiento Horizontal")]
    [SerializeField]private GameObject player;
    public float zOffset = 2f;   // Cuánto se mueve la cámara
    public float xOffsetSpeed = 3f;  // Velocidad de la transición
    private float normalZOffset;

    void Awake()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        framingTransposer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();

        if (framingTransposer != null)
        {
            normalYOffset = framingTransposer.m_TrackedObjectOffset.y;
            normalZOffset = framingTransposer.m_TrackedObjectOffset.x;
        }

        if (playerInput != null)
        {
            prayAction = playerInput.actions.FindAction("Pray");
            moveCameraAction = playerInput.actions.FindAction("MoveCamera");
        }

    }

    void Update()
    {
        if (vcam == null) return;

        HandleZoom();
        HandleVerticalPan();
        HandleHorizontalPan();
    }

    private void HandleZoom()
    {
        if (prayAction == null) return;

        bool isPraying = prayAction.IsPressed();
        float targetFOV = isPraying ? prayFOV : normalFOV;
        vcam.m_Lens.FieldOfView = Mathf.Lerp(vcam.m_Lens.FieldOfView, targetFOV, zoomSpeed * Time.deltaTime);
    }

    private void HandleVerticalPan()
    {
        if (moveCameraAction == null || framingTransposer == null) return;

        Vector2 moveValue = moveCameraAction.ReadValue<Vector2>();
        float targetY = normalYOffset;

        // Lógica de detección: Arriba (> 0.5) o Abajo (< -0.5)
        if (moveValue.y > 0.5f)
        {
            targetY = normalYOffset + yOffsetUp;
        }
        else if (moveValue.y < -0.5f)
        {
            targetY = normalYOffset + yOffsetDown;
        }
        // Si está entre -0.5 y 0.5, targetY se mantiene en normalYOffset (vuelve al centro)

        // Aplicamos el movimiento suave
        Vector3 currentOffset = framingTransposer.m_TrackedObjectOffset;
        currentOffset.y = Mathf.Lerp(currentOffset.y, targetY, offsetSpeed * Time.deltaTime);
        framingTransposer.m_TrackedObjectOffset = currentOffset;
    }
    private void HandleHorizontalPan()
    {
        if (moveCameraAction == null || framingTransposer == null) return;

        float moveValue = player.transform.localScale.z;
        float targetZ = normalZOffset;

        // Lógica de detección: Arriba (> 0.5) o Abajo (< -0.5)
        if (moveValue > 0f)
        {
            targetZ= normalZOffset + zOffset;
        }
        else if (moveValue < -0f)
        {
            targetZ = normalZOffset - zOffset;
        }
        // Si está entre -0.5 y 0.5, targetY se mantiene en normalYOffset (vuelve al centro)

        // Aplicamos el movimiento suave
        Vector3 currentOffset = framingTransposer.m_TrackedObjectOffset;
        currentOffset.z = Mathf.Lerp(currentOffset.z, targetZ, offsetSpeed * Time.deltaTime);
        framingTransposer.m_TrackedObjectOffset = currentOffset;
    }
}