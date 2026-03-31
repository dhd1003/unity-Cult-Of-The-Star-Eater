using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HabilityTrigger : MonoBehaviour
{

    public string hability;

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            switch (hability)
            {
                case "doublejump":
                    other.GetComponent<PlayerController>().canDoubleJump = true;
                    Destroy(gameObject);
                break;

                case "dash":
                    other.GetComponent<PlayerController>().canDash = true;
                    Destroy(gameObject);
                break;

                case "walljump":
                    other.GetComponent<PlayerController>().canWallJump = true;
                    Destroy(gameObject);
                break;

                default:
                    Debug.Log("Habilidad no asignada al trigger");
                break;

            }
            
        }
    }
}
