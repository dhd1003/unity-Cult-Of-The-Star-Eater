using UnityEngine;

public class DashState : State
{
    private float dashTimer;

    public override void EnterState(Player player)
    {
        dashTimer = player.dashDuration;
        player.animator.SetBool("IsDashing", true);
        player.SetColliderHeight(player.originalHeight / 2.5f, 0.4f);
        if (player.dashSound) player.audioSource.PlayOneShot(player.dashSound);
    }

    public override void LogicUpdate(Player player)
    {
        dashTimer -= Time.deltaTime;

        float direction = player.transform.localScale.z > 0 ? 1f : -1f;
        player.controller.Move(new Vector3(direction * player.dashSpeed, 0, 0) * Time.deltaTime);

        if (dashTimer <= 0)
        {
            player.SwitchState(player.idleState);
        }
    }

    public override void ExitState(Player player)
    {
        player.animator.SetBool("IsDashing", false);
        player.SetColliderHeight(player.originalHeight, 1f);
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
