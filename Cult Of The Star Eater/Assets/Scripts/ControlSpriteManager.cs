using UnityEngine;
using UnityEngine.InputSystem;

public class ControlSpriteManager : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private SpriteRenderer spriteRenderer; // O usa Image si es UI de Canvas

    [Header("Sprites de los Controles")]
    [SerializeField] private Sprite keyboardMouseSprite;
    [SerializeField] private Sprite gamepadSprite;

    private void Update()
    {
        //Verifica cuál fue el último dispositivo que envió datos al juego
        InputDevice lastDevice = GetLastUsedDevice();

        if (lastDevice != null)
        {
            // Comprueba si es un mando o teclado/ratón y cambiamos el sprite
            if (lastDevice is Gamepad)
            {
                UpdateSprite(gamepadSprite);
            }
            else if (lastDevice is Keyboard || lastDevice is Mouse)
            {
                UpdateSprite(keyboardMouseSprite);
            }
        }
    }

    private InputDevice GetLastUsedDevice()
    {
        // El Input System guarda en 'InputSystem.devices' todos los dispositivos,
        // busca cuál ha sido el que ha tenido actividad más recientemente.
        InputDevice activeDevice = null;
        double latestUpdateTime = 0;

        foreach (var device in InputSystem.devices)
        {
            // Solo interesan los que el jugador usa (Mando, Teclado, Ratón)
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

    private void UpdateSprite(Sprite newSprite)
    {
        if (spriteRenderer != null && newSprite != null)
        {
            if (spriteRenderer.sprite != newSprite)
            {
                spriteRenderer.sprite = newSprite;
            }
        }
    }
}
