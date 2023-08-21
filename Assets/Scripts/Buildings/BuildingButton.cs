using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
using Mirror;

public class BuildingButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] private BuildingManager buildingManager = null;
    [SerializeField] private Image iconImage = null;
    [SerializeField] private TMP_Text costText = null;
    [SerializeField] private LayerMask floorMask = new LayerMask();

    private Camera mainCamera;
    private BoxCollider buildingCollider;
    private RTSPlayer player;
    private GameObject buildingPreviewInstance;
    // red or greeen mark when we try to place the building on the floor
    private Renderer buildingRendererInstance;
    private UnitSelectionHandler unitSelectionHandler;

    void Start()
    {
        mainCamera = Camera.main;

        iconImage.sprite = buildingManager.Icon;
        costText.text = $"{buildingManager.Cost}";

        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        buildingCollider = buildingManager.GetComponent<BoxCollider>();
        unitSelectionHandler = FindObjectOfType<UnitSelectionHandler>();
    }

    void Update()
    {
        //if (player == null)
        //{
        //    // get connection, get the player object for our connection, and the player object
        //    // player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        //    // StartCoroutine(CallPlayerScript());
        //    player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        //}

        if (buildingPreviewInstance == null) return;

        UpdateBuildingPreview();
    }

    //IEnumerator CallPlayerScript()
    //{
    //    yield return new WaitForSeconds(0.1f);
    //    player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
    //}

    // left mouse button thing for dragging
    public void OnPointerDown(PointerEventData eventData)
    {
        if (eventData.button != PointerEventData.InputButton.Left) return;

        // when we try to place down the building, first check the resources
        if (player.Resources < buildingManager.Cost) return;

        unitSelectionHandler.IsBuilding = true;

        buildingPreviewInstance = Instantiate(buildingManager.BuildingPreview);
        buildingRendererInstance = buildingPreviewInstance.GetComponentInChildren<Renderer>();

        // disable as soon as it spwans
        buildingPreviewInstance.SetActive(false);
    }

    // mouse release on the floor, not currently dragging
    public void OnPointerUp(PointerEventData eventData)
    {
        if (buildingPreviewInstance == null) return;

        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask))
        {
            // place the building
            player.CmdTryPlaceBuilding(buildingManager.BuildingId, hit.point);
        }

        unitSelectionHandler.IsBuilding = false;

        // after we place the actual building, destroy the preview of the building
        Destroy(buildingPreviewInstance);
    }

    void UpdateBuildingPreview()
    {
        Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, floorMask)) return;

        buildingPreviewInstance.transform.position = hit.point;

        // only if it's disabled, we set it to be active
        if (!buildingPreviewInstance.activeSelf)
        {
            buildingPreviewInstance.SetActive(true);
        }

        // update the renderer colo based on what we can or can't actually
        // place the building
        Color color = player.CanPlaceBuilding(buildingCollider, hit.point) ? Color.green : Color.red;
        buildingRendererInstance.material.SetColor("_BaseColor", color);
    }
}
