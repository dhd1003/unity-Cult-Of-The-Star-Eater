using UnityEngine;
using UnityEngine.InputSystem;

public class Tutorial_MaterialDetector : MonoBehaviour
{
    [Header("Referencias")]
    private Renderer objectRenderer; // El MeshRenderer o SpriteRenderer del objeto

    [Header("Materiales de los Controles")]
    [SerializeField] private Material keyboardMouseMaterial;
    [SerializeField] private Material gamepadMaterial;

    private void Awake()
    {
    
            objectRenderer = GetComponent<Renderer>();
      
    }

    private void Update()
    {
        // Verifica cuál fue el último dispositivo usado
        InputDevice lastDevice = GetLastUsedDevice();

        if (lastDevice != null)
        {
            // Cambia el material según corresponda
            if (lastDevice is Gamepad)
            {
                UpdateMaterial(gamepadMaterial);
            }
            else if (lastDevice is Keyboard || lastDevice is Mouse)
            {
                UpdateMaterial(keyboardMouseMaterial);
            }
        }
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

    private void UpdateMaterial(Material newMaterial)
    {
        if (objectRenderer != null && newMaterial != null)
        {
            // Solo cambia el material si es diferente al actual para evitar sobreescribir en cada frame
            if (objectRenderer.sharedMaterial != newMaterial)
            {
                objectRenderer.sharedMaterial = newMaterial;
            }
        }
    }
}