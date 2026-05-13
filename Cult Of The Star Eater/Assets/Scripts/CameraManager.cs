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
    public float yOffsetDown = -2f;
    public float yOffsetUp = 2f;
    public float offsetSpeed = 3f;
    private float normalYOffset;

    [Header("Configuración de Desplazamiento Horizontal")]
    [SerializeField] private GameObject player;
    public float zOffset = 2f;
    public float xOffsetSpeed = 3f;
    private float normalZOffset;

    void Awake()
    {
        vcam = GetComponent<CinemachineVirtualCamera>();
        framingTransposer = vcam.GetCinemachineComponent<CinemachineFramingTransposer>();

        if (framingTransposer != null)
        {
            // Guardamos los valores iniciales del inspector como "centro"
            normalYOffset = framingTransposer.m_TrackedObjectOffset.y;
            normalZOffset = framingTransposer.m_TrackedObjectOffset.z;
        }

        if (playerInput != null)
        {
            prayAction = playerInput.actions.FindAction("Pray");
            moveCameraAction = playerInput.actions.FindAction("MoveCamera");
        }
    }

    void Update()
    {
        if (vcam == null || framingTransposer == null) return;

        bool isPraying = prayAction != null && prayAction.IsPressed();

        HandleZoom(isPraying);
        HandleOffsets(isPraying);
    }

    private void HandleZoom(bool isPraying)
    {
        float targetFOV = isPraying ? prayFOV : normalFOV;
        vcam.m_Lens.FieldOfView = Mathf.Lerp(vcam.m_Lens.FieldOfView, targetFOV, zoomSpeed * Time.deltaTime);
    }

    private void HandleOffsets(bool isPraying)
    {
        float targetY = normalYOffset;
        float targetZ = normalZOffset;

        // SI NO ESTÁ REZANDO: Calculamos los desplazamientos normales
        if (!isPraying)
        {
            // Lógica Horizontal (basada en escala del jugador)
            float lookDirection = player.transform.localScale.z;
            if (lookDirection > 0.1f) targetZ = normalZOffset + zOffset;
            else if (lookDirection < -0.1f) targetZ = normalZOffset - zOffset;

            // Lógica Vertical (basada en input de mirar arriba/abajo)
            if (moveCameraAction != null)
            {
                Vector2 moveValue = moveCameraAction.ReadValue<Vector2>();
                if (moveValue.y > 0.5f) targetY = normalYOffset + yOffsetUp;
                else if (moveValue.y < -0.5f) targetY = normalYOffset + yOffsetDown;
            }
        }
        // SI ESTÁ REZANDO: targetY y targetZ se quedan en normalYOffset/normalZOffset (el centro)

        // Aplicamos ambos movimientos con Lerp
        Vector3 currentOffset = framingTransposer.m_TrackedObjectOffset;
        currentOffset.y = Mathf.Lerp(currentOffset.y, targetY, offsetSpeed * Time.deltaTime);
        currentOffset.z = Mathf.Lerp(currentOffset.z, targetZ, offsetSpeed * Time.deltaTime);

        framingTransposer.m_TrackedObjectOffset = currentOffset;
    }
}