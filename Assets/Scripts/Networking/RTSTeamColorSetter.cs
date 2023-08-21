using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;

/// <summary>
/// Whenever the new object is spawned,
/// find out which player owns this object and
/// store there color here
/// </summary>
public class RTSTeamColorSetter : NetworkBehaviour
{
    [SerializeField] private Renderer[] colorRenderers = new Renderer[0];

    // chagne the color whenever the color is synced
    [SyncVar(hook = nameof(HandleTeamColorUpdated))]
    private Color teamColor = new Color();

    #region Server

    public override void OnStartServer()
    {
        RTSPlayer player = connectionToClient.identity.GetComponent<RTSPlayer>();

        teamColor = player.TeamColor;
    }

    #endregion


    #region Client

    void HandleTeamColorUpdated(Color oldColor, Color newColor)
    {
        if (colorRenderers.Length == 0) return;

        // change the color of all the team player
        foreach (Renderer renderer in colorRenderers)
        {
            renderer.material.SetColor("_BaseColor", newColor);
        }
    }

    #endregion
}
