using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : MonoBehaviour
{
    
    // Input System
    public PlayerInput playerInput;
    public InputAction moveAction;
    public InputAction jumpAction;
    public InputAction dashAction;
    public InputAction crouchAction;
    public InputAction teleportAction;
    public InputAction prayAction;

    [Header("Movimiento")]
    public float moveSpeed = 6f;
    public float gravity = 25f;
    public float jumpForce = 12f;
    [Range(0, 1)] public float cutJumpHeight = 0.5f;

    [Header("Dash")]
    public float dashSpeed = 15f;
    public float dashDuration = 0.3f;
    public float dashCooldown = 0.7f;

    public bool isDashing = false;
    public float dashCooldownTimer = 0f;

    [Header("Desbloqueables")]
    public bool canDash = false;
    public bool canDoubleJump = false;

    [Header("Coyote Time & Input Buffer")]
    public float coyoteTime = 0.2f;
    public float coyoteTimer;
    public float inputBuffer = 0.15f;
    public float inputTimer;

    public bool hasDoubleJumped = false;
    public CharacterController controller;
    public Animator animator;
    public Vector3 moveDirection;
    public float verticalVelocity;
    public Vector3 startingPoint;

    public float originalHeight;
    public Vector3 originalCenter;
    public bool isCrouched = false;

    [Header("Sonidos")]
    public AudioSource audioSource;
    public AudioClip jumpSound;
    public AudioClip doubleJumpSound;
    public AudioClip dashSound;
    public AudioClip crouchSound;
    public AudioClip landSound;

    [Header("Colliders")]
    public CollisionChecker collisionChecker;
    public bool wallCollision;
    public bool canClimb;
    public bool isGroundNear;

    public bool isClimbing = false;
    public bool isAlreadyHanging = false; // Control para la alineación única
    [SerializeField] private Vector3 climbVector = new Vector3(1f, 1.9f, 0);
    public float offsetHanging = 0.3f;

    [Header("StateMachine")]
    State currentState;

   public AttackCrouchedState attackCrouchedState = new AttackCrouchedState(); 
   public AttackInAirState attackInAirState = new AttackInAirState();
   public AttackOnGroundState attackOnGroundState = new AttackOnGroundState();
   public ClimbEdgeState climbEdgeState = new ClimbEdgeState();
   public CrouchState crouchState = new CrouchState();
   public DashState dashState = new DashState();
   public DoubleJumpState doubleJumpState = new DoubleJumpState();
   public FallState fallState = new FallState();
   public HangOnLedgeState hangOnLedgeState = new HangOnLedgeState();
   public HurtInAirState hurtairState = new HurtInAirState();
   public HurtOnGroundState hurtOnGroundState = new HurtOnGroundState();
   public IdleState idleState = new IdleState();
   public JumpState jumpState = new JumpState();
   public LandingState landingState = new LandingState();
   public PrayState prayState = new PrayState();
   public WalkState walkState = new WalkState();
   


    void Awake()
    {
        playerInput = GetComponent<PlayerInput>();
        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        dashAction = playerInput.actions["Dash"];
        crouchAction = playerInput.actions["Crouch"];
        teleportAction = playerInput.actions["Teleport"];
    }

    void Start()
    {
        controller = GetComponent<CharacterController>();
        animator = GetComponent<Animator>();
        collisionChecker = GetComponent<CollisionChecker>();
        startingPoint = transform.position;
        originalHeight = controller.height;
        originalCenter = controller.center;

        currentState = idleState;

        currentState.EnterState(this);
    }

  

    // Update is called once per frame
    void Update()
    {
        
        currentState.HandleInput(this);
        currentState.LogicUpdate(this);
    }

    void FixedUpdate()
    {
        currentState.PhysicsUpdate(this);
    }

    public void SwitchState(State newState)
    {
        if (currentState != null)
            currentState.ExitState(this);

        currentState = newState;
        currentState.EnterState(this);

    }

    public void ApplyGravity()
    {
        if (controller.isGrounded && verticalVelocity < 0) verticalVelocity = -2f;
        else verticalVelocity -= gravity * Time.deltaTime;
        moveDirection.y = verticalVelocity;
    }

    public void SetFacingDirection(float horizontal)
    {
        if (horizontal > 0.1f) transform.localScale = new Vector3(1, 1, 1);
        else if (horizontal < -0.1f) transform.localScale = new Vector3(1, 1, -1);
    }

    public void SetColliderHeight(float newHeight, float centerMultiplier)
    {
        controller.height = newHeight;
        controller.center = new Vector3(originalCenter.x, originalCenter.y * centerMultiplier, originalCenter.z);
    }
}
