using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;
using Steamworks;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private GameObject landingPagePanel = null;

    // Steam Transport
    // when we try and host a lobby, if we're using steam, then just do it normally
    [SerializeField] private bool useSteam = false;

    private const string pchKey = "HostAddress";

    // need to use callbacks to recieve responses
    // similar to events
    protected Callback<LobbyCreated_t> lobbyCreated;
    protected Callback<GameLobbyJoinRequested_t> gameLobbyJoinRequested_t;
    protected Callback<LobbyEnter_t> lobbyEntered_t;

    private void Start()
    {
        // if we don't want to use stam, don't do anything
        if (!useSteam) return;

        // when Steam tells us a lobby is created, we then want to run this method
        lobbyCreated = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        gameLobbyJoinRequested_t = Callback<GameLobbyJoinRequested_t>.Create(OnGameLobbyJoinRequested);
        lobbyEntered_t = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void HostLobby()
    {
        landingPagePanel.SetActive(false);

        // wait for the Steam to actually create a lobby
        if (useSteam)
        {
            // create lobby for friends list on the Steam server, with max number of 4
            SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypeFriendsOnly, 4);
            return;
        }

        // and then start a host
        NetworkManager.singleton.StartHost();
    }


    void OnLobbyCreated(LobbyCreated_t callback)
    {
        // steam has failed to create a lobby for some reason
        if (callback.m_eResult != EResult.k_EResultOK)
        {
            landingPagePanel.SetActive(true);
            return;
        }

        // if succeeded, then start the host
        NetworkManager.singleton.StartHost();

        // when Steam create a lobby, when someone joins our lobby, we need to send
        // that person our ID, and they will try to connect to that ID

        // just simply some data that is tied into a particular lobby
        SteamMatchmaking.SetLobbyData(new CSteamID(callback.m_ulSteamIDLobby),
                                      pchKey, // key
                                      SteamUser.GetSteamID().ToString() // value
                                      );
    }

    void OnGameLobbyJoinRequested(GameLobbyJoinRequested_t callback)
    {
        SteamMatchmaking.JoinLobby(callback.m_steamIDLobby);
    }

    // gets called if even you as a host join your own lobby
    void OnLobbyEntered(LobbyEnter_t callback)
    {
        // if we are the host, don't do anything
        if (NetworkServer.active) return;

        // grab the host address value, which is the SteamID,
        string hostAddress = SteamMatchmaking.GetLobbyData(new CSteamID(callback.m_ulSteamIDLobby),
                                                           pchKey);

        NetworkManager.singleton.networkAddress = hostAddress;

        // then start as a client
        NetworkManager.singleton.StartClient();

        landingPagePanel.SetActive(false);
    }
}
