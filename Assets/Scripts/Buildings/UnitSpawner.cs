using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using Mirror;

// using interface event
public class UnitSpawner : NetworkBehaviour, IPointerClickHandler
{
    [SerializeField] private HealthManager healthManager = null;
    [SerializeField] private UnitManager unitPrefab = null;
    [SerializeField] private Transform unitSpawnPoint = null;

    [Header("UI related field")]
    [SerializeField] private TMP_Text remainingUnitsText = null;
    [SerializeField] private Image unitProgressImage = null;

    [Header("Stats related field")]
    [SerializeField] private int maxUnitQueue = 5;
    [SerializeField] private float spawnMoveRange = 7f;
    [SerializeField] private float unitSpawnDuration = 10f;

    // server needs to know these two variables
    [SyncVar(hook = nameof(ClientHandleQueuedUnitUpdated))]
    private int queuedUnits;
    [SyncVar]
    private float unitTimer;

    private float progressImageVelocity;

    void Update()
    {
        // part of it is server side and

        if (isServer)
        {
            ProduceUnits();
        }
        
        // client side
        if (isClient)
        {
            UpdateTimerDisplay();
        }

    }

    #region Server

    // subscribe to events
    public override void OnStartServer()
    {
        healthManager.ServerOnDie += ServerHandleDie;
    }

    // unsubscribe events
    public override void OnStopServer()
    {
        healthManager.ServerOnDie -= ServerHandleDie;
    }

    [Server]
    void ProduceUnits()
    {
        // if there are no queued units, just return
        if (queuedUnits == 0) return;

        unitTimer += Time.deltaTime;

        if (unitTimer < unitSpawnDuration) return;

        GameObject unitInstance = Instantiate(unitPrefab.gameObject,
                                              unitSpawnPoint.position,
                                              unitSpawnPoint.rotation);
        // pass in the Owner Connection, if we don't pass this in, then it's not
        // belonging to any of the clients, only a server object
        // in our case, we want the owener of this unit to belong to the player the spawner belongs
        // connectionToClient exists in the NetworkBehavior

        // b/c the spawner belongs to me, the unit that spawns will also
        // belongs to me
        // give the authority to the connectionToClient
        NetworkServer.Spawn(unitInstance, connectionToClient);

        // move to the rally point after getting spawned 
        // to avoid being stacked on top of each other after spawning
        Vector3 spawnOffset = Random.insideUnitSphere * spawnMoveRange;
        spawnOffset.y = unitSpawnPoint.position.y;

        UnitMovement unitMovement =  unitInstance.GetComponent<UnitMovement>();
        unitMovement.ServerMove(unitSpawnPoint.position + spawnOffset);

        // reduce the queued unit and reset the timer
        queuedUnits--;
        unitTimer = 0f;
    }

    [Server]
    private void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    [Command]
    private void CmdSpawnUnit()
    {
        // max queue reached, can't spawn anymore
        if (queuedUnits == maxUnitQueue) return;

        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();

        // not enough resources to spawn an unit
        if (player?.Resources < unitPrefab.ResourceCost) return;

        queuedUnits++;

        player.Resources = player.Resources - unitPrefab.ResourceCost;
    }

    #endregion

    #region Client

    void UpdateTimerDisplay()
    {
        float newProgress = unitTimer / unitSpawnDuration;

        // if we've reset, we've looped all the way around and an unit
        // has been just spawned, then we want to set it
        if (newProgress < unitProgressImage.fillAmount)
        {
            unitProgressImage.fillAmount = newProgress;
        }
        else
        {
            // smoothly update the spawning timer UI
            unitProgressImage.fillAmount = Mathf.SmoothDamp(
                unitProgressImage.fillAmount,
                newProgress,
                ref progressImageVelocity,
                0.1f
            );
        }
    }

    // Unity will call this function for us whever I click on this object
    public void OnPointerClick(PointerEventData eventData)
    {
        // checking the validations

        // let left mouse be the one to spwan the units
        if (eventData.button != PointerEventData.InputButton.Left) return;
    
        // if this object is not owned by the client, then don't do anything
        if (!isOwned) return;

        CmdSpawnUnit();
    }

    void ClientHandleQueuedUnitUpdated(int oldQueuedUnits, int newQueuedUnits)
    {
        remainingUnitsText.text = $"{newQueuedUnits}";
    }

    #endregion

}