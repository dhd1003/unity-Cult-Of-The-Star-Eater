using UnityEngine;

public class FallState : State
{
   
        public override void EnterState(Player player)
        {
            player.animator.SetBool("IsJumping", true);
        }

        public override void LogicUpdate(Player player)
        {
            player.ApplyGravity();

            // Movimiento aéreo
            Vector2 input = player.playerInput.actions["Move"].ReadValue<Vector2>();
            player.moveDirection.x = input.x * player.moveSpeed;
            player.SetFacingDirection(input.x);

            player.controller.Move(player.moveDirection * Time.deltaTime);

            if (player.controller.isGrounded)
            {
                player.SwitchState(player.landingState);
            }
        }

        public override void HandleInput(Player player)
        {
            if (player.playerInput.actions["Jump"].WasPressedThisFrame() && !player.hasDoubleJumped && player.canDoubleJump)
            {
                player.SwitchState(player.doubleJumpState);
            }
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
