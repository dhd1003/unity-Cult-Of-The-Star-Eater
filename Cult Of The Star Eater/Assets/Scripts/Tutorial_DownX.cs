using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Tutorial_DownX : MonoBehaviour
{
    public GameObject downXSprite;

    [SerializeField] private PlayerInput playerInput;
    [Range(0, 1)] public int nTutorial;
    private InputAction moveAction;
    private InputAction jumpAction;
    private InputAction dashAction;
    private Vector2 inputVector;

    // Variable para saber si el jugador está dentro del trigger
    private bool isPlayerInside = false;

    void Awake()
    {
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        dashAction = playerInput.actions["Dash"];
    }

    private void Update()
    {
        inputVector = moveAction.ReadValue<Vector2>();

        if (isPlayerInside)
        {
            bool downPressed = inputVector.y < -0.5f;
            bool southPressed = jumpAction.WasPressedThisFrame();
            bool dashPressed = dashAction.WasPressedThisFrame();

            if (downPressed && southPressed && nTutorial == 0)
            {
                if (downXSprite != null)
                    downXSprite.SetActive(false);

                Destroy(gameObject);
            }
            if (dashPressed && nTutorial == 1)
            {
                if (downXSprite != null)
                    downXSprite.SetActive(false);

                Destroy(gameObject);
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = true;
            if (downXSprite != null) downXSprite.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isPlayerInside = false;
            if (downXSprite != null) downXSprite.SetActive(false);
        }
    }
}