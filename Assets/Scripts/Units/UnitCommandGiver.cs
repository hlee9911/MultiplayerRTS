using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class UnitCommandGiver : MonoBehaviour
{
    [SerializeField] private UnitSelectionHandler unitSelectionHandler = null;
    [SerializeField] private LayerMask layerMask = new LayerMask();
    // layer mask is a struct not a class

    private Camera mainCameera;

    void Start()
    {
        mainCameera = Camera.main;

        // subscription
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    void OnDestroy()
    {
        // unsubscription
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    void Update()
    {
        MoveTheUnit();
    }

    void MoveTheUnit()
    {
        // right click, do a raycast
        if (!Mouse.current.rightButton.wasPressedThisFrame) return;

        Ray ray = mainCameera.ScreenPointToRay(Mouse.current.position.ReadValue());

        // if hit something,
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) return;

        if (hit.collider.TryGetComponent<Targetable>(out Targetable target))
        {
            // prevent from attacking our own units
            if (target.isOwned)
            {
                TryMove(hit.point);
                return;
            }

            // try target the enemies
            TryTarget(target);
            return;
        }

        // if we don't click on a targetable thing, we want to try moving
        // try moving all of our selected units
        TryMove(hit.point);
    }

    void TryMove(Vector3 point)
    {
        foreach (UnitManager unit in unitSelectionHandler.SelectedUnits) 
        {
            unit.GetUnitMovement.CmdMove(point);
        }
    }

    void TryTarget(Targetable target)
    {
        foreach (UnitManager unit in unitSelectionHandler.SelectedUnits)
        {
            unit.GetTargeter.CmdSetTarget(target.gameObject);
        }
    }

    void ClientHandleGameOver(string winnerName)
    {
        // stop doing raycasting and commands
        enabled = false;
    }

}
