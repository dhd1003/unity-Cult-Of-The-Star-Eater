using UnityEngine;
using System.Collections;

public class OneWayPlatform : MonoBehaviour
{
    [SerializeField] private GameObject player;
    private Collider playerCollider;
    private Collider platformCollider;
    private Animator playerAnimator;

    private bool isWaitngToPass = false; // Controla el tiempo de bajada

    void Start()
    {
        playerCollider = player.GetComponent<Collider>();
        platformCollider = GetComponent<Collider>();
        playerAnimator = player.GetComponent<Animator>();
    }

    void Update()
    {
        if (playerCollider == null || playerAnimator == null) return;

        float playerFeetY = playerCollider.bounds.min.y + 0.25f ;
        float platformTopY = platformCollider.bounds.max.y;

        // 1. Detectamos el estado del animator
        bool isPlayerCrouched = playerAnimator.GetBool("IsCrouched");

        // 2. Si el jugador se agacha y NO estamos ya en proceso de bajar, iniciamos el contador
        if (isPlayerCrouched && !isWaitngToPass && playerFeetY > platformTopY - 0.5f)
        {
            StartCoroutine(TemporaryDisableCollision());
        }

        // 3. LÓGICA DE COLISIÓN
        // Se vuelve trigger si:
        // - Viene desde abajo (playerFeetY < platformTopY)
        // - O si activamos el modo "espera para pasar" al agacharnos
        if (playerFeetY < platformTopY - 0.1f || isWaitngToPass)
        {
            platformCollider.isTrigger = true;
        }
        else
        {
            platformCollider.isTrigger = false;
        }
    }

    // Corrutina para mantener la plataforma abierta aunque el Animator cambie rápido
    IEnumerator TemporaryDisableCollision()
    {
        isWaitngToPass = true;

        // Mantenemos la plataforma como Trigger durante 0.5 segundos
        // Tiempo suficiente para que la gravedad haga caer al jugador
        yield return new WaitForSeconds(0.1f);

        isWaitngToPass = false;
    }
}