using UnityEngine;

public class DoubleJumpState : State
{
    public override void EnterState(Player player)
    {
        player.verticalVelocity = player.jumpForce;
        player.hasDoubleJumped = true;
        player.animator.Play("DoubleJump", -1, 0f); // Opcional: reiniciar animación
        if (player.doubleJumpSound) player.audioSource.PlayOneShot(player.doubleJumpSound);
    }

    public override void LogicUpdate(Player player)
    {
        player.ApplyGravity();
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