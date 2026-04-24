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
    private InputAction moveAction;

    [Header("Configuración de Zoom")]
    public float normalFOV = 11f;
    public float prayFOV = 8f;
    public float zoomSpeed = 5f;

    [Header("Configuración de Desplazamiento Vertical")]
    public float yOffsetDown = -2f; // Cuánto baja la cámara
    public float yOffsetUp = 2f;   // Cuánto sube la cámara
    public float offsetSpeed = 3f;  // Velocidad de la transición
    private float normalYOffset;

    void Awake()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        framingTransposer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();

        if (framingTransposer != null)
        {
            normalYOffset = framingTransposer.m_TrackedObjectOffset.y;
        }

        if (playerInput != null)
        {
            prayAction = playerInput.actions.FindAction("Pray");
            moveAction = playerInput.actions.FindAction("Move");
        }
    }

    void Update()
    {
        if (vcam == null) return;

        HandleZoom();
        HandleVerticalPan();
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
        if (moveAction == null || framingTransposer == null) return;

        Vector2 moveValue = moveAction.ReadValue<Vector2>();
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
}