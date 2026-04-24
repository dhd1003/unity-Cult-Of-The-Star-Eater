using UnityEngine;

public class GravityPlatform : MonoBehaviour
{
    private Rigidbody rb;
    private bool estaTocandoSuelo = false;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Mientras esté dentro del trigger del suelo...
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Ground"))
        {
            estaTocandoSuelo = true;
            rb.isKinematic = true;
            rb.velocity = Vector3.zero;
        }
    }

    void FixedUpdate()
    {
        // Si en el último frame físico no detectamos suelo, activamos gravedad
        if (!estaTocandoSuelo)
        {
            rb.isKinematic = false;
        }

        // Reseteamos el flag para el siguiente frame
        estaTocandoSuelo = false;
    }
}