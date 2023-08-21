using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;
using Mirror;

public class LobbyMenu : MonoBehaviour
{
    [SerializeField] private GameObject lobbyUI = null;
    [SerializeField] private Button startGamebutton = null;
    [SerializeField] private TMP_Text[] playerNameTexts = new TMP_Text[4];

    void Start()
    {
        // subscriptions
        RTSNetworkManager.ClientOnConnected += HandleClientConnected;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated += AuthorityHandlePartyOwnerStateupdated;
        RTSPlayer.ClientOnInfoUpdated += ClientHandleInfoUpdated;
    }

    void OnDestroy()
    {
        // unsubscriptions
        RTSNetworkManager.ClientOnConnected -= HandleClientConnected;
        RTSPlayer.AuthorityOnPartyOwnerStateUpdated -= AuthorityHandlePartyOwnerStateupdated;
        RTSPlayer.ClientOnInfoUpdated -= ClientHandleInfoUpdated;
    }

    void HandleClientConnected()
    {
        lobbyUI.SetActive(true);
    }

    void ClientHandleInfoUpdated()
    {
        List<RTSPlayer> players = ((RTSNetworkManager)NetworkManager.singleton).Players;
    
        for (int i = 0; i < players.Count; i++)
        {
            playerNameTexts[i].text = players[i].DisplayName;
        }

        // reupdate if thte player leaves or not
        for (int i = players.Count; i < playerNameTexts.Length; i++)
        {
            playerNameTexts[i].text = "Waiting For Player...";
        }

        // if there are more at least two players in the lobby, we can start the game
        startGamebutton.interactable = (players.Count >= 2);
    }

    void AuthorityHandlePartyOwnerStateupdated(bool state)
    {
        startGamebutton.gameObject.SetActive(state);
    }

    public void StartGame()
    {
        // identity means it's the player or the game object with RTSplayer Script
        NetworkClient.connection.identity.GetComponent<RTSPlayer>().CmdStartGame();
    }

    public void LeaveLobby()
    {
        // if you are running as a server, and you are also a client,
        // meaning you are a host
        if (NetworkServer.active && NetworkClient.isConnected)
        {
            NetworkManager.singleton.StopHost();
        }
        else
        {
            // otherwise you are just a client
            NetworkManager.singleton.StopClient();

            // just reload the main menu
            SceneManager.LoadScene(0);
        }
    }
}
