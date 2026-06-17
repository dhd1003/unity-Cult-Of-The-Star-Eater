using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class VideoManager : MonoBehaviour

{
    [SerializeField] private VideoPlayer videoPlayer;
    [SerializeField] bool isFinalCinematic;
    private PlayerInput playerInput;
    private InputAction jumpVideoAction;
    // Start is called before the first frame update


    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        jumpVideoAction = playerInput.actions["JumpVideo"];
    }
    void Start()
    {
        videoPlayer.loopPointReached += VideoPlayer_loopPointReached;
    }

    private void Update()
    {
        if (jumpVideoAction.WasPressedThisFrame())
        {
            VideoPlayer_loopPointReached(videoPlayer);
        }
    }

    // Update is called once per frame

    private void VideoPlayer_loopPointReached(VideoPlayer source)
    {
        if (isFinalCinematic)
        {
        SceneManager.LoadScene(0);
        }
        else
        {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);

        }
    }
}
