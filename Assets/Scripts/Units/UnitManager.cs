using System.Collections;
using System.Collections.Generic;
using System;
using Mirror;
using UnityEngine;
using UnityEngine.Events;

public class UnitManager : NetworkBehaviour
{
    [SerializeField] private int resourceCost = 20;
    [SerializeField] private HealthManager healthManager = null;
    [SerializeField] private UnitMovement unitMovement = null;
    [SerializeField] private Targeter targeter = null;
    [SerializeField] private UnityEvent onSelected = null;
    [SerializeField] private UnityEvent onDeselected = null;

    [Header("Selected Boolean")]
    public bool IsSelected = false;

    // C#'s default event
    // only being called on the server and called only when a unit is spawned
    public static event Action<UnitManager> ServerOnUnitSpawned;
    public static event Action<UnitManager> ServerOnUnitDespawned;

    // client events
    public static event Action<UnitManager> AuthorityOnUnitSpawned;
    public static event Action<UnitManager> AuthorityOnUnitDespawned;

    // getters
    public UnitMovement GetUnitMovement { get { return unitMovement; } }
    public Targeter GetTargeter { get { return targeter; } }
    public int ResourceCost { get { return resourceCost; } }

    #region Server

    // where we trigger events
    // basically publishing events?
    public override void OnStartServer()
    {
        ServerOnUnitSpawned?.Invoke(this);
        healthManager.ServerOnDie += ServerHandleDie;
    }

    public override void OnStopServer()
    {
        healthManager.ServerOnDie -= ServerHandleDie;
        ServerOnUnitDespawned?.Invoke(this);
    }

    [Server]
    void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        // if we are the server or we don't have authority
        // don't do anything

        AuthorityOnUnitSpawned?.Invoke(this);
    }

    // we can't directly remove authority from an
    // object on the server
    public override void OnStopClient()
    {
        // if we are the server or we don't have authority
        // don't do anything
        if (!isOwned) return;

        AuthorityOnUnitDespawned?.Invoke(this);
    }


    [Client]
    public void Select()
    {
        // check authority
        if (!isOwned) return;

        onSelected?.Invoke();
        IsSelected = true;
    }

    [Client]
    public void Deselect()
    {
        if (!isOwned) return;

        onDeselected?.Invoke();
        IsSelected = false;
    }

    #endregion
}
