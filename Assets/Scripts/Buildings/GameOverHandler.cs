using System.Collections;
using System.Collections.Generic;
using System;
using Mirror;
using UnityEngine;

public class GameOverHandler : NetworkBehaviour
{
    // game over event, server version
    public static event Action ServerOnGameOver;

    // pass in the player name, the client version
    public static event Action<string> ClientOnGameOver;

    // list of bases
    private List<UnitBase> unitBases = new List<UnitBase>();

    #region Server

    public override void OnStartServer()
    {
        // subscription
        UnitBase.ServerOnBaseSpawned += ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawned += ServerHandleBaseDespawned;
    }

    //public override void OnStopServer()
    //{
    //    // unsubscription
    //    UnitBase.ServerOnBaseSpawned -= ServerHandleBaseSpawned;
    //    UnitBase.ServerOnBaseDespawned -= ServerHandleBaseDespawned;
    //}

    void OnDestroy()
    {
        // unsubscription
        UnitBase.ServerOnBaseSpawned -= ServerHandleBaseSpawned;
        UnitBase.ServerOnBaseDespawned -= ServerHandleBaseDespawned;
    }

    [Server]
    void ServerHandleBaseSpawned(UnitBase unitBase)
    {
        unitBases.Add(unitBase);
    }

    [Server]
    void ServerHandleBaseDespawned(UnitBase unitBase)
    {
        unitBases.Remove(unitBase);

        // return if we don't have a single base in the list
        if (unitBases.Count != 1) return;

        // game is over if only 1 base is left
        // Debug.Log("Game Over!");
        int playerID = unitBases[0].connectionToClient.connectionId;

        RpcGameOver($"Player {playerID + 1}");

        ServerOnGameOver?.Invoke();
    }

    #endregion

    #region Client

    // server calling client functions to publish the game over events
    [ClientRpc]
    void RpcGameOver(string winner)
    {
        // stop doing selecting units once game is over
        ClientOnGameOver?.Invoke(winner);
    }

    #endregion
}
