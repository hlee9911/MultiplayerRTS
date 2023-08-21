using System.Collections;
using System.Collections.Generic;
using System;
using Mirror;
using UnityEngine;

public class UnitBase : NetworkBehaviour
{
    [SerializeField] private HealthManager healthManager = null;

    // passing the id of player that died
    public static event Action<int> ServerOnPlayerDie;

    public static event Action<UnitBase> ServerOnBaseSpawned;
    public static event Action<UnitBase> ServerOnBaseDespawned;

    public static event Action<UnitBase> ClientOnBaseSpawned;

    #region Server

    public override void OnStartServer()
    {
        // subscribe
        healthManager.ServerOnDie += ServerHandleDie;
        
        ServerOnBaseSpawned?.Invoke(this);
    }

    public override void OnStopServer()
    {
        ServerOnBaseDespawned?.Invoke(this);
        
        // unsubscribe
        healthManager.ServerOnDie -= ServerHandleDie;
    }

    [Server]
    private void ServerHandleDie()
    {
        ServerOnPlayerDie?.Invoke(connectionToClient.connectionId);

        NetworkServer.Destroy(gameObject);
    }

    #endregion


    #region Client

    public override void OnStartClient()
    {
        if (!isOwned) return;

        ClientOnBaseSpawned?.Invoke(this);
    }

    #endregion 
}
