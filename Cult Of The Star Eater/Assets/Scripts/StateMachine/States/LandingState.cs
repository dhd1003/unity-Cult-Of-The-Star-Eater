using UnityEngine;

public class LandingState : State
{
    public override void EnterState(Player player)
    {
        player.animator.SetBool("IsJumping", false);
        player.hasDoubleJumped = false;
        if (player.landSound) player.audioSource.PlayOneShot(player.landSound);
    }

    public override void LogicUpdate(Player player)
    {
        Vector2 input = player.playerInput.actions["Move"].ReadValue<Vector2>();

        if (Mathf.Abs(input.x) > 0.1f)
            player.SwitchState(player.walkState);
        else
            player.SwitchState(player.idleState);
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