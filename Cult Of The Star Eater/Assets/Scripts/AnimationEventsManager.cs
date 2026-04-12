using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.XR;

public class AnimationEventsManager : MonoBehaviour
{

    private CharacterController controller;
    // Start is called before the first frame update
    void Start()
    {
        controller = GetComponent<CharacterController>();
    }

    public void CLimb()
    {
        controller.transform.position = controller.transform.position + new Vector3(1, 2, 0);
    }
}
