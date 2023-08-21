using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Mirror;

public class ResourcesDisplay : MonoBehaviour
{
    [SerializeField] private TMP_Text resourcesText = null;

    private RTSPlayer player;

    private void Start()
    {
        // get connection, get the player object for our connection, and the player object
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        // before the subscription, we immediately grab the right value for the reesource
        ClientHandleResourcesUpdated(player.Resources);

        // subscription
        // when we have the RTSplayer, we subscribe to resource updated event
        player.ClientOnResourcesUpdated += ClientHandleResourcesUpdated;
    }

    void OnDestroy()
    {
        // unsubscription
        player.ClientOnResourcesUpdated -= ClientHandleResourcesUpdated;
    }

    //IEnumerator CallPlayerScript()
    //{
    //    yield return new WaitForSeconds(0.1f);
    //    player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
    //}

    void ClientHandleResourcesUpdated(int resources)
    {
        resourcesText.text = $"Resources: {resources}";
    }
}
