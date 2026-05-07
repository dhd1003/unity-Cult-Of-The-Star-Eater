using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HabilityTrigger : MonoBehaviour
{

    public string hability;
    [Header("Sonidos")]
    public AudioSource audioSource;
    public AudioClip soundGetHability;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            switch (hability)
            {
                case "doublejump":
                    GameManager.Instance.canDoubleJump = true;
                    audioSource.PlayOneShot(soundGetHability);
                    Destroy(gameObject);
                break;

                case "dash":
                    GameManager.Instance.canDash = true;
                    audioSource.PlayOneShot(soundGetHability);
                    Destroy(gameObject);
                break;

                default:
                    Debug.Log("Habilidad no asignada al trigger");
                break;

            }
            
        }
    }
}
