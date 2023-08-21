using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class Targeter : NetworkBehaviour
{
    private Targetable target;

    public Targetable Target { get { return target; } }

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

    [Command]
    public void CmdSetTarget(GameObject targetGameObject)
    {
        // client tells a server that this is what I want
        // the target to be

        if (!targetGameObject.TryGetComponent<Targetable>(out Targetable _target)) return;

        target = _target;
    }

    [Server]
    public void ClearTarget()
    {
        target = null;
    }

    [Server]
    void ServerHandleGameOver()
    {
        // clearing target if the player's base dies
        ClearTarget();
    }

    #endregion

}
