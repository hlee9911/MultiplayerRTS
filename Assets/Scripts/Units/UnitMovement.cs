using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Mirror;

public class UnitMovement : NetworkBehaviour
{
    [SerializeField] private NavMeshAgent agent = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private float chaseRange = 10f;

    // private Camera mainCamera;

    #region Server

    public override void OnStartServer()
    {
        // subscription
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        // unsubscription
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    // prevents this from being called on clients
    // and doesn't log the warning to console

    // usually for callback, we use it for the
    // methods that we don't control such as
    // start, update, awake, and etc
    [ServerCallback]
    void Update()
    {
        RefinedMove();
    }

    void Targeting()
    {
        Targetable target = targeter.Target;

        if (target != null) 
        { 
            // squared both side, so basically saying if the distance is greater
            // than the range (check how far is it, if it's great than our chase range,
            // then start chasing)
            if ((target.transform.position - transform.position).sqrMagnitude > chaseRange * chaseRange)
            {
                // chase
                agent.SetDestination(target.transform.position);
            }
            else if (agent.hasPath)
            {
                // stop chasing
                agent.ResetPath();
            }

            return;
        }
    }

    void RefinedMove()
    {
        Targeting();
        // if we have no target, then just move

        // will prevent trying to clear the agent's path
        // in the same frame as it calculate it
        if (!agent.hasPath) return;

        if (agent.remainingDistance > agent.stoppingDistance) return;

        agent.ResetPath();
    }

    // end point at which the client can call
    [Command]
    public void CmdMove(Vector3 _position)
    {
        ServerMove(_position);
    }

    [Server]
    public void ServerMove(Vector3 _position)
    {
        targeter.ClearTarget();

        // check validation
        if (!NavMesh.SamplePosition(_position,
                                   out NavMeshHit hit,
                                   1f,
                                   NavMesh.AllAreas)) return;

        agent.SetDestination(hit.position);
    }

    [Server]
    void ServerHandleGameOver()
    {
        // clearing path if the player base dies
        agent.ResetPath();
    }

    #endregion

    //#region Client

    //// only need to know about our player

    //// simply start method for the person for the client that owns this object
    //public override void OnStartAuthority()
    //{
    //    mainCamera = Camera.main;
    //}

    //[ClientCallback]
    //private void Update()
    //{
    //    // client only method 
    //    // but even though its client only, all the clients are going to run it

    //    if (!isOwned) return;

    //    if (!Mouse.current.rightButton.wasPressedThisFrame) return;

    //    Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

    //    if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity)) return;

    //    CmdMove(hit.point);
    //}


    //#endregion
}
