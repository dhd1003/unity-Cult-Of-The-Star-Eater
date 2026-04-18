using UnityEngine;

public class JumpState : State
{
    public override void EnterState(Player player)
    {
        player.verticalVelocity = player.jumpForce;
        player.animator.SetBool("IsJumping", true);
        player.audioSource.PlayOneShot(player.jumpSound);
    }

    public override void LogicUpdate(Player player)
    {
        player.ApplyGravity();

        // Control aéreo básico
        Vector2 input = player.playerInput.actions["Move"].ReadValue<Vector2>();
        player.moveDirection.x = input.x * player.moveSpeed;

        player.controller.Move(player.moveDirection * Time.deltaTime);

        if (player.verticalVelocity <= 0)
            player.SwitchState(player.fallState);
    }
    

    public override void ExitState(Player player)
    {

    }

    public override void HandleInput(Player player)
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