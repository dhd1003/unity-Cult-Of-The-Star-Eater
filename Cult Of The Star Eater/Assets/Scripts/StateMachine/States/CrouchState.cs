using UnityEngine;

public class CrouchState : State
{
    public override void EnterState(Player player)
    {
        player.animator.SetBool("IsCrouched", true);
        player.SetColliderHeight(player.originalHeight / 2.5f, 0.4f);
        if (player.crouchSound) player.audioSource.PlayOneShot(player.crouchSound);
        player.moveDirection.x = 0;
    }

    public override void HandleInput(Player player)
    {
        bool wantsToCrouch = player.playerInput.actions["Crouch"].IsPressed();

        if (!wantsToCrouch)
        {
            player.SwitchState(player.idleState);
        }
    }

    public override void LogicUpdate(Player player)
    {
        player.ApplyGravity();
        player.controller.Move(player.moveDirection * Time.deltaTime);
    }

    public override void ExitState(Player player)
    {
        player.animator.SetBool("IsCrouched", false);
        player.SetColliderHeight(player.originalHeight, 1f);
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
