using System.Collections;
using System.Collections.Generic;
using System;
using Mirror;
using UnityEngine;

public class HealthManager : NetworkBehaviour
{
    [SerializeField] private int maxHealth = 100;

    // we only want the server to change this variable
    // hook this as sync var and if this gets modified,
    // calls the client method
    [SyncVar(hook = nameof(HandleHealthUpdated))]
    private int currentHealth;

    public event Action ServerOnDie;

    // first int for current, and second int for max health
    // only triggered on client
    public event Action<int, int> ClientOnHealthUdpated;

    #region Server

    public override void OnStartServer()
    {
        currentHealth = maxHealth;

        // subscription
        UnitBase.ServerOnPlayerDie += ServerHandlePlayerDie;
    }

    public override void OnStopServer()
    {
        // unsubscription
        UnitBase.ServerOnPlayerDie -= ServerHandlePlayerDie;
    }

    [Server]
    void ServerHandlePlayerDie(int connectionId)
    {
        // if the connectionId doesn't match, don't do anything
        if (connectionToClient.connectionId != connectionId) return;
        
        // otherwise, deal the maximum and to destory
        // (kill all units and building if the player's base dies)
        DealDamage(currentHealth);
    }

    [Server]
    public void DealDamage(int damageAmount)
    {
        if (currentHealth <= 0) return;

        currentHealth = Mathf.Max(currentHealth - damageAmount, 0);

        if (currentHealth != 0) return;

        // publish the deathEvent
        ServerOnDie?.Invoke();

        // Debug.Log("We died :(");
    }

    #endregion

    #region Client

    private void HandleHealthUpdated(int oldHealth, int newHealth)
    {
        // publish the health change event on Client
        ClientOnHealthUdpated?.Invoke(newHealth, maxHealth);
    }

    #endregion
}
