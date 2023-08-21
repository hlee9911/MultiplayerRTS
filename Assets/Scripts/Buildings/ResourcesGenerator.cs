using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

public class ResourcesGenerator : NetworkBehaviour
{
    [SerializeField] private HealthManager healthManager = null;
    [SerializeField] private int resourcesPerInterval = 10;
    [SerializeField] private float interval = 2f;

    private float timer;
    private RTSPlayer player;

    #region Server

    public override void OnStartServer()
    {
        timer = interval;
        player = connectionToClient.identity.GetComponent<RTSPlayer>();

        // subscription
        healthManager.ServerOnDie += ServerHandleDie;
        GameOverHandler.ServerOnGameOver += ServerHandleGameOver;
    }

    public override void OnStopServer()
    {
        // unsubscription
        healthManager.ServerOnDie -= ServerHandleDie;
        GameOverHandler.ServerOnGameOver -= ServerHandleGameOver;
    }

    // mark so that the client won't run this method
    [ServerCallback]
    void Update()
    {
        timer -= Time.deltaTime;

        if (timer <= 0)
        {
            player.Resources = player.Resources + resourcesPerInterval;

            timer += interval;
        }
    }

    void ServerHandleDie()
    {
        NetworkServer.Destroy(gameObject);
    }

    void ServerHandleGameOver()
    {
        // disable this component so that it stops generating the resources
        enabled = false;
    }

    #endregion

}
