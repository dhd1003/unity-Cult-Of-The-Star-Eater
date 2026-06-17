using UnityEngine;

public class WalkState : State
{
    public override void EnterState(Player player)
    {
        player.animator.SetBool("IsWalking", true);
    }

    public override void LogicUpdate(Player player)
    {
        Vector2 input = player.playerInput.actions["Move"].ReadValue<Vector2>();
        player.moveDirection.x = input.x * player.moveSpeed;
        player.SetFacingDirection(input.x);

        player.ApplyGravity();
        player.controller.Move(player.moveDirection * Time.deltaTime);

        if (Mathf.Abs(input.x) < 0.1f)
            player.SwitchState(player.idleState);

        if (!player.controller.isGrounded)
            player.SwitchState(player.fallState);
    }

    public override void HandleInput(Player player)
    {
        if (player.playerInput.actions["Jump"].WasPressedThisFrame())
            player.SwitchState(player.jumpState);
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

