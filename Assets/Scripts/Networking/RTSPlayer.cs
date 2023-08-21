using System.Collections;
using System.Collections.Generic;
using System;
using Mirror;
using UnityEngine;

public class RTSPlayer : NetworkBehaviour
{
    [Header("Building related fields")]
    [SerializeField] private LayerMask buildingBlockLayer = new LayerMask();
    [SerializeField] private BuildingManager[] buildings = new BuildingManager[0];
    [SerializeField] private float buildingRangeLimit = 5f;

    [Header("Camera related fields")]
    [SerializeField] private Transform cameraTransform = null;

    // server only variable?
    [Header("Starting Resources")]
    [SyncVar(hook = nameof(ClientHandleResourcesUpdated)), SerializeField]
        private int resources = 300;
    // check whether they are party owner or not
    [SyncVar(hook = nameof(AuthorityHandlePartyOwnerStateUpdated))]
        private bool isPartyOnwer = false;
    [SyncVar(hook = nameof(ClientHandleDisplayNameUpdated))]
    private string displayName;
    
    // events
    public event Action<int> ClientOnResourcesUpdated;

    public static event Action ClientOnInfoUpdated;
    public static event Action<bool> AuthorityOnPartyOwnerStateUpdated;

    private Color teamColor = new Color();
    private List<UnitManager> myUnits = new List<UnitManager>();
    private List<BuildingManager> myBuildings = new List<BuildingManager>();

    // getters && setters
    public Color TeamColor { get { return teamColor; } [Server] set { teamColor = value; } }
    public List<UnitManager> MyUnits { get { return myUnits; } }
    public List<BuildingManager> MyBuildings { get { return myBuildings; } }
    public int Resources { get { return resources; } [Server] set { resources = value; } }
    public Transform CameraTransform { get { return cameraTransform; } }
    public bool IsPartyOnwer { get { return isPartyOnwer; } [Server] set { isPartyOnwer = value; } }
    public string DisplayName { get { return displayName; } [Server] set { displayName = value; } }

    // can be use for both client and server
    public bool CanPlaceBuilding(BoxCollider buildingCollider,
                                 Vector3 positionToSpawn)
    {
        // Debug.Log("Can Place Building method");

        // collide with something, so we can't spawn it on here
        if (Physics.CheckBox(positionToSpawn + buildingCollider.center,
                             buildingCollider.size / 2,
                             Quaternion.identity,
                             buildingBlockLayer)) return false;

        // we want to place building within a certain range of other buildings
        foreach (BuildingManager building in myBuildings)
        {
            // check the radius around the center of the building
            if ((positionToSpawn - building.transform.position).sqrMagnitude
                <= buildingRangeLimit * buildingRangeLimit)
            {
                return true;
            }
        }

        return false;
    }

    #region Server

    public override void OnStartServer()
    {
        // subscrition events from eventbus thing

        // whenever we invoke this server on unit spawned,
        // handle the serverhandleUnitSpawned
        UnitManager.ServerOnUnitSpawned += ServerHandleUnitSpawned;
        UnitManager.ServerOnUnitDespawned += ServerHandleUnitDespawned;

        BuildingManager.ServerOnBuildinigSpawned += ServerHandleBuildingSpawned;
        BuildingManager.ServerOnBuildinigDespawned += ServerHandleBuildingDespawned;
        
        // don't destroy game object when we change the scene
        DontDestroyOnLoad(gameObject);
    }

    public override void OnStopServer()
    {
        // unsubscription events onDestory thing
        UnitManager.ServerOnUnitSpawned -= ServerHandleUnitSpawned;
        UnitManager.ServerOnUnitDespawned -= ServerHandleUnitDespawned;

        BuildingManager.ServerOnBuildinigSpawned -= ServerHandleBuildingSpawned;
        BuildingManager.ServerOnBuildinigDespawned -= ServerHandleBuildingDespawned;
    }

    // need a command to tell the server to start the game
    [Command]
    public void CmdStartGame()
    {
        // if you are not the party owner, don't do anything
        if (!IsPartyOnwer) return;

        // otherwise tell the networkmanager to try start the game
        ((RTSNetworkManager)NetworkManager.singleton).StartGame();
    }

    // we can't send entire gameObject over the network
    // so we send the buildingId instead
    [Command]
    public void CmdTryPlaceBuilding(int buildingId, Vector3 positionToSpawn)
    {
        // go thru the buildings list
        BuildingManager buildingToPlace = null;

        foreach (BuildingManager building in buildings)
        {
            if (building.BuildingId == buildingId)
            {
                buildingToPlace = building;
                break;
            }
        }

        // if the buildingId is invalid, we do not spawn the building
        if (buildingToPlace == null) return;

        if (resources < buildingToPlace.Cost) return;

        BoxCollider buildingCollider = buildingToPlace.GetComponent<BoxCollider>();

        if (!CanPlaceBuilding(buildingCollider, positionToSpawn)) return;

        // we finally instantiate the building if buildingId is valid
        GameObject buildingInstance =  Instantiate(buildingToPlace.gameObject,
                                                   positionToSpawn,
                                                   buildingToPlace.transform.rotation);
        
        // spawn on the server and give the ownership to the client that is connected
        NetworkServer.Spawn(buildingInstance, connectionToClient);

        resources = resources - buildingToPlace.Cost;
    }

    /// units handlers ///
    void ServerHandleUnitSpawned(UnitManager unit)
    {
        // checking if the connection to Client Id
        // to make sure if this client who own this unit 
        // is same as player
        if (unit.connectionToClient.connectionId !=
            connectionToClient.connectionId) return;

        myUnits.Add(unit);   
    }

    void ServerHandleUnitDespawned(UnitManager unit)
    {
        if (unit.connectionToClient.connectionId !=
            connectionToClient.connectionId) return;

        myUnits.Remove(unit);
    }
    /// ///

    /// buidlings handlers ///
    void ServerHandleBuildingSpawned(BuildingManager building)
    {
        // checking if the connection to Client Id
        // to make sure if this client who own this unit 
        // is same as player
        if (building.connectionToClient.connectionId !=
            connectionToClient.connectionId) return;

        myBuildings.Add(building);

        //foreach (BuildingManager buildingManger in myBuildings)
        //{
        //    Debug.Log($"Id building: {buildingManger.BuildingId}");
        //}
    }

    void ServerHandleBuildingDespawned(BuildingManager building)
    {
        if (building.connectionToClient.connectionId !=
            connectionToClient.connectionId) return;

        myBuildings.Remove(building);
    }
    /// ///

    #endregion



    #region Client

    public override void OnStartAuthority()
    {
        // make sure to subscribe once if we are the host

        // if we're the server we return
        if (NetworkServer.active) return;

        UnitManager.AuthorityOnUnitSpawned += AuthorityHandleUnitSpawned;
        UnitManager.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;

        BuildingManager.AuthorityOnBuildingSpawned += AuthorityHandleBuildingSpawned;
        BuildingManager.AuthorityOnBuildingDespawned += AuthorityHandleBuildingDespawned;
    }

    public override void OnStartClient()
    {
        // if we're the server we return
        if (NetworkServer.active) return;

        DontDestroyOnLoad(gameObject);
        
        ((RTSNetworkManager)NetworkManager.singleton).Players.Add(this);
    }

    public override void OnStopClient()
    {
        // when client disconnects
        ClientOnInfoUpdated?.Invoke();

        // check if we are the server
        if (!isClientOnly) return;

        ((RTSNetworkManager)NetworkManager.singleton).Players.Remove(this);

        // check if we have the authority
        if (!isOwned) return;

        UnitManager.AuthorityOnUnitSpawned -= AuthorityHandleUnitSpawned;
        UnitManager.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;

        BuildingManager.AuthorityOnBuildingSpawned -= AuthorityHandleBuildingSpawned;
        BuildingManager.AuthorityOnBuildingDespawned -= AuthorityHandleBuildingDespawned;
    }

    void ClientHandleResourcesUpdated(int oldResources, int newResources)
    {
        ClientOnResourcesUpdated?.Invoke(newResources);
    }

    void ClientHandleDisplayNameUpdated(string oldDisplayName, string newDisplayName)
    {
        // publish the event when the player either leaves or enter the lobby
        ClientOnInfoUpdated?.Invoke();
    }

    void AuthorityHandlePartyOwnerStateUpdated(bool oldState, bool newState)
    {
        // allows you to actually change who the party owner during the lobby
        if (!isOwned) return;

        // but if you do, public an event that the UI can listen to turn on and off the button
        AuthorityOnPartyOwnerStateUpdated?.Invoke(newState);
    }

    void AuthorityHandleUnitSpawned(UnitManager unit)
    {
        // check if the authority is over particular player
        // if (!isOwned) return;

        myUnits.Add(unit);
    }

    void AuthorityHandleUnitDespawned(UnitManager unit)
    {
        // if (!isOwned) return;

        myUnits.Remove(unit);
    }

    void AuthorityHandleBuildingSpawned(BuildingManager building)
    {
        // check if the authority is over particular player
        // if (!isOwned) return;

        myBuildings.Add(building);
    }

    void AuthorityHandleBuildingDespawned(BuildingManager building)
    {
        // if (!isOwned) return;

        myBuildings.Remove(building);
    }

    #endregion
}
