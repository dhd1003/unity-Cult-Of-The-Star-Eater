using UnityEngine;

public class GravityPlatform : MonoBehaviour
{
    private Rigidbody rb;
    private bool estaTocandoSuelo = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            estaTocandoSuelo = true;

            // 1. Primero detenemos el movimiento físico
            if (!rb.isKinematic)
            {
                rb.velocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;

                // 2. Luego lo hacemos cinemático
                rb.isKinematic = true;
            }
        }
    }

    void FixedUpdate()
    {
        if (!estaTocandoSuelo)
        {
            // Solo lo cambiamos si no es cinemático para evitar llamadas innecesarias
            if (rb.isKinematic)
            {
                rb.isKinematic = false;
            }
        }

        estaTocandoSuelo = false;
    }
}