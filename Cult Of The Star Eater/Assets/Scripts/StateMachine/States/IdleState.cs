using UnityEngine;
using UnityEngine.TextCore.Text;

public class IdleState : State
{
    public override void EnterState(Player player)
    {
        player.animator.SetBool("IsWalking", false);
        player.moveDirection.x = 0;
    }

    public override void HandleInput(Player player)
    {
        Vector2 input = player.playerInput.actions["Move"].ReadValue<Vector2>();

        if (Mathf.Abs(input.x) > 0.1f)
            player.SwitchState(player.walkState);

        if (player.playerInput.actions["Jump"].WasPressedThisFrame())
            player.SwitchState(player.jumpState);

        if (player.playerInput.actions["Crouch"].IsPressed())
            player.SwitchState(player.crouchState);
    }

    public override void LogicUpdate(Player player)
    {
        player.ApplyGravity();
        player.controller.Move(player.moveDirection * Time.deltaTime);

        if (!player.controller.isGrounded)
            player.SwitchState(player.fallState);
    }



    public override void ExitState(Player player)
    {
       
    }



    public override void OnCollisionEnter(Player player)
    {
       
    }

    public override void OnCollisionExit(Player player)
    {
        
    }

    public override void OnCollisionStay(Player player)
    {
        
    }

    public override void OnTriggerEnter(Player player)
    {
        
    }

    public override void OnTriggerExit(Player player)
    {
       
    }

    public override void OnTriggerStay(Player player)
    {
        
    }

    public override void PhysicsUpdate(Player player)
    {
       
    }
}
