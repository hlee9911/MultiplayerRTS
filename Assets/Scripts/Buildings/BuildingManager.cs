using System.Collections;
using System.Collections.Generic;
using System;
using Mirror;
using UnityEngine;
using UnityEngine.UI;

public class BuildingManager : NetworkBehaviour
{
    [SerializeField] private GameObject buildingPreview = null;
    [SerializeField] private Sprite icon = null;
    [SerializeField] private int buildingId = -1;
    [SerializeField] private int cost = 100;

    [Header("Selected Boolean")]
    public bool IsSelected = false;

    // server side events
    public static event Action<BuildingManager> ServerOnBuildinigSpawned;
    public static event Action<BuildingManager> ServerOnBuildinigDespawned;

    // client side events
    public static event Action<BuildingManager> AuthorityOnBuildingSpawned;
    public static event Action<BuildingManager> AuthorityOnBuildingDespawned;

    // getters
    public Sprite Icon { get { return icon; } }
    public int BuildingId { get { return buildingId; } }
    public int Cost { get { return cost; } }
    public GameObject BuildingPreview { get { return buildingPreview; } }

    #region Server

    public override void OnStartServer()
    {
        ServerOnBuildinigSpawned?.Invoke(this);
    }

    public override void OnStopServer()
    {
        ServerOnBuildinigDespawned?.Invoke(this);
    }

    #endregion

    #region Client

    public override void OnStartAuthority()
    {
        AuthorityOnBuildingSpawned?.Invoke(this);
    }

    public override void OnStopClient()
    {
        // if we are the server or we don't have authority
        // don't do anything
        if (!isOwned) return;

        AuthorityOnBuildingDespawned?.Invoke(this);
    }

    #endregion
}
