using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.InputSystem;
public abstract class State 
{

    public abstract void EnterState(Player player);


    public abstract void HandleInput(Player player);

    public abstract void LogicUpdate(Player player);

    public abstract void PhysicsUpdate(Player player);

    public abstract void OnTriggerEnter(Player player);

    public abstract void OnTriggerExit(Player player);

    public abstract void OnTriggerStay(Player player);

    public abstract void OnCollisionEnter(Player player);
    public abstract void OnCollisionExit(Player player);

    public abstract void OnCollisionStay(Player player);

    public abstract void ExitState(Player player);

}
