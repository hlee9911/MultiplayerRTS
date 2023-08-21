using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Mirror;

public class MinimapManager : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] private RectTransform minimapRect = null;
    [SerializeField] private float mapScale = 20f;
    [SerializeField] private float offset = -6f;
    // get referernce to a camera
    private Transform playerCameraTransform;
    private UnitSelectionHandler unitSelectionHandler;

    void Start()
    {
        unitSelectionHandler = FindObjectOfType<UnitSelectionHandler>();
    }

    void Update()
    {
        if (playerCameraTransform != null) return;

        // player is not ready yet
        if (NetworkClient.connection?.identity == null) return;

        playerCameraTransform = 
            NetworkClient.connection.identity.GetComponent<RTSPlayer>()?.CameraTransform;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        unitSelectionHandler.IsMinimap = true;

        MoveCamera();   
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        unitSelectionHandler.IsMinimap = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        unitSelectionHandler.IsMinimap = true;

        MoveCamera();
    }

    void MoveCamera()
    {
        // this is world space tho
        Vector2 mousePos = Mouse.current.position.ReadValue();

        // we take the spcreen point, the mouse position,
        // and we want it to convert it to the local point in the rectangle
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            minimapRect,
            mousePos,
            null,
            out Vector2 localPoint
        )) return;

        Vector2 lerp = new Vector2((localPoint.x - minimapRect.rect.x) / minimapRect.rect.width, 
                                   (localPoint.y - minimapRect.rect.y) / minimapRect.rect.height);
        // give us a value based on A and B based on T
        Vector3 newCameraPos = new Vector3(
            Mathf.Lerp(-mapScale, mapScale, lerp.x),
            playerCameraTransform.position.y,
            Mathf.Lerp(-mapScale, mapScale, lerp.y)
        );

        playerCameraTransform.position = newCameraPos + new Vector3(0f, 0f, offset);
    }

}
