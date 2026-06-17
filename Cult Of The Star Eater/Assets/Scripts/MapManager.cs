using UnityEngine;
using UnityEngine.InputSystem;

public class MapManager : MonoBehaviour
{
    private PlayerInput playerInput;
    private InputAction mapAction;

    [SerializeField] private GameObject panelMapa;
    private bool isMapOpen = false;

    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        mapAction = playerInput.actions["Map"];
    }

    void OnEnable()
    {
        // Nos suscribimos al evento de presionar el botón
        mapAction.performed += ToggleMap;
    }

    void OnDisable()
    {
        // Nos desuscribimos al destruir o desactivar el objeto para evitar errores
        mapAction.performed -= ToggleMap;
    }

    private void ToggleMap(InputAction.CallbackContext context)
    {
        // Invertimos el estado actual del mapa
        isMapOpen = !isMapOpen;

        if (isMapOpen)
        {
            panelMapa.SetActive(true);
            Time.timeScale = 0f; // Pausa el juego (físicas, animaciones, etc.)
        }
        else
        {
            panelMapa.SetActive(false);
            Time.timeScale = 1f; // Reanuda el juego
        }
    }
}