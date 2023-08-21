using System.Collections;
using System.Collections.Generic;
using System;
using Mirror;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RTSNetworkManager : NetworkManager
{
    [Header("RTS Related fields")]
    [SerializeField] private GameObject unitBasePrefab = null;
    [SerializeField] private GameOverHandler gameOverHandlerPrefab = null;

    // events
    public static event Action ClientOnConnected;
    public static event Action ClientOnDisconnected;

    // readonly list of players who joined?
    public List<RTSPlayer> Players { get; } = new List<RTSPlayer>();

    // player shouldn't be able to ocnnect during the game
    private bool isGameInProgress = false;

    #region Server

    // whenever a client connects, we can tell it to kick the player
    public override void OnServerConnect(NetworkConnectionToClient conn)
    {
        // only kick them if the game is in progress
        if (!isGameInProgress) return;

        conn.Disconnect();
    }

    // when server disconnects, someone, let's grab the player
    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        // if that playaer disconnects, we remove from the player's list
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

        // then remove that player
        Players.Remove(player);

        base.OnServerDisconnect(conn);
    }

    public override void OnStopServer()
    {
        Players.Clear();

        isGameInProgress = false;
    }

    public void StartGame()
    {
        // can't start the game with less than two players
        if (Players.Count < 2) return;

        isGameInProgress = true;

        ServerChangeScene("MainScene");
    }

    // server adds the player
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        base.OnServerAddPlayer(conn);

        // set the team color for the player upon entering the game
        RTSPlayer player = conn.identity.GetComponent<RTSPlayer>();

        Players.Add(player);

        player.DisplayName = $"Player {Players.Count}";

        player.TeamColor = new Color(
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f),
            UnityEngine.Random.Range(0f, 1f)
        );


        //// spawn the base for where the spawn point is
        //GameObject unitSpawnerInstance = Instantiate(
        //    unitSpawnerPrefab, 
        //    conn.identity.transform.position,
        //    conn.identity.transform.rotation);

        //// spawn all the base on the client as well, and give the ownership
        //// to that client
        //NetworkServer.Spawn(unitSpawnerInstance, conn);

        // if there's only one player in the lobby, that person
        // owns the lobby
        // player.SetPartyOwner(Players.Count == 1);
        player.IsPartyOnwer = (Players.Count == 1);
    }

    // called just after the scene is changed
    public override void OnServerSceneChanged(string sceneName)
    {
        // whenever the changes the scene, is this new scene, main scene?
        // if so, spawn in the network gameOverHandlerInstance
        if (SceneManager.GetActiveScene().name.StartsWith("MainScene"))
        {
            GameOverHandler gameOverHandlerInstance = Instantiate(
                gameOverHandlerPrefab);

            NetworkServer.Spawn(gameOverHandlerInstance.gameObject);

            // spawn all the players' bases
            foreach (var player in Players)
            {
                GameObject baseInstance = Instantiate(unitBasePrefab,
                                                      GetStartPosition().position,
                                                      Quaternion.identity);

                // this player' who owns this, owns the ownership
                NetworkServer.Spawn(baseInstance, player.connectionToClient);
            }
        }
    }

    #endregion

    #region Client
    public override void OnClientConnect()
    {
        base.OnClientConnect();

        ClientOnConnected?.Invoke();
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        ClientOnDisconnected?.Invoke();
    }

    public override void OnStopClient()
    {
        Players.Clear();
    }

    #endregion

}
