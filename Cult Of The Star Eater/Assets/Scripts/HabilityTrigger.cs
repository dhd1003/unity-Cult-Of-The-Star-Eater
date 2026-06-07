using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HabilityTrigger : MonoBehaviour
{

    [Tooltip("0=Dash, 1=DoubleJump")]
    [Range(0, 1)] public int nHability;
    [Header("Sonidos")]
    public AudioSource audioSource;
    public AudioClip soundGetHability;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            switch (nHability)
            {
                case 1:
                    GameManager.Instance.canDoubleJump = true;
                    audioSource.PlayOneShot(soundGetHability);
                    Destroy(gameObject);
                break;

                case 0:
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
