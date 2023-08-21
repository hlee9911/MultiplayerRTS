using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using Mirror;

public class UnitSelectionHandler : MonoBehaviour
{
    [SerializeField] private RectTransform unitSelectionArea = null;
    [SerializeField] LayerMask layerMask = new LayerMask();

    private Vector2 startPosition;

    private RTSPlayer player;
    private Camera mainCamera;

    private bool isBuilding = false;
    private bool isMinimap = false;
    
    public List<UnitManager> SelectedUnits { get; } = new List<UnitManager>();
    public bool IsBuilding { get { return isBuilding; } set { isBuilding = value; } }
    public bool IsMinimap { get { return isMinimap; } set { isMinimap = value; } }

    void Start()
    {
        mainCamera = Camera.main;

        // get connection, get the player object for our connection, and the player object
        // player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
        // StartCoroutine(CallPlayerScript());
        player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();

        UnitManager.AuthorityOnUnitDespawned += AuthorityHandleUnitDespawned;
        GameOverHandler.ClientOnGameOver += ClientHandleGameOver;
    }

    void OnDestroy()
    {
        UnitManager.AuthorityOnUnitDespawned -= AuthorityHandleUnitDespawned;
        GameOverHandler.ClientOnGameOver -= ClientHandleGameOver;
    }

    void Update()
    {
        if (IsOverUI()) return;

        //if (IsOverUI())
        if (Mouse.current.leftButton.wasPressedThisFrame)
        {
            // Start the selection area
            StartSelectionArea();
        }
        else if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            ClearSelectionArea();
        }
        else if (Mouse.current.leftButton.isPressed)
        {
            // dragging the screen
            UpdateSelectionArea();
        }
    }

    // prevent annoying drag box to be drawn when clicking or dragging the UI
    private bool IsOverUI()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                pointerId = -1,
            };
            pointerData.position = Input.mousePosition;
            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            if (results.Count > 0)
            {
                for (int i = 0; i < results.Count; ++i)
                {
                    if (results[i].gameObject.CompareTag("UI"))
                        return true;
                }
            }
            return false;
        }
        return false;
    }

    //IEnumerator CallPlayerScript()
    //{
    //    yield return new WaitForSeconds(0.5f);
    //    player = NetworkClient.connection.identity.GetComponent<RTSPlayer>();
    //}

    void StartSelectionArea()
    {
        // if (isBuilding || isMinimap) return;

        if (!Keyboard.current.leftShiftKey.isPressed)
        {
            foreach (UnitManager selectedUnit in SelectedUnits)
            {
                selectedUnit.Deselect();
            }
            SelectedUnits.Clear();
        }

        unitSelectionArea.gameObject.SetActive(true);
        startPosition = Mouse.current.position.ReadValue();
        UpdateSelectionArea();
    }

    void ClearSelectionArea()
    {
        unitSelectionArea.gameObject.SetActive(false);

        // selected but didn't drag and create a drag box
        if (unitSelectionArea.sizeDelta.magnitude == 0)
        {
            Ray ray = mainCamera.ScreenPointToRay(Mouse.current.position.ReadValue());
            if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, layerMask)) return;
            if (!hit.collider.TryGetComponent<UnitManager>(out UnitManager unit)) return;
            if (!unit.isOwned) return;
            SelectedUnits.Add(unit);

            foreach (UnitManager selectedUnit in SelectedUnits)
            {
                selectedUnit.Select();
            }
            return;
        }

        Vector2 min = unitSelectionArea.anchoredPosition - (unitSelectionArea.sizeDelta / 2);
        Vector2 max = unitSelectionArea.anchoredPosition + (unitSelectionArea.sizeDelta / 2);

        foreach (UnitManager unit in player.MyUnits)
        {
            if (SelectedUnits.Contains(unit)) continue;

            Vector3 screenPosition = mainCamera.WorldToScreenPoint(unit.transform.position);          
            if (screenPosition.x > min.x && screenPosition.x < max.x &&
                screenPosition.y > min.y && screenPosition.y < max.y)
            {
                SelectedUnits.Add(unit);
                unit.Select();
            }
        }
    }

    void UpdateSelectionArea()
    {
        // drawing the dragging box
        Vector2 mousePosition = Mouse.current.position.ReadValue();

        float areaWidth = mousePosition.x - startPosition.x;
        float areaHeight = mousePosition.y - startPosition.y;

        unitSelectionArea.sizeDelta = new Vector2(Mathf.Abs(areaWidth), Mathf.Abs(areaHeight));
        // center of the drag box, so half of the width and the height
        unitSelectionArea.anchoredPosition = startPosition + 
            new Vector2(areaWidth / 2, areaHeight / 2);
    }

    void AuthorityHandleUnitDespawned(UnitManager unitManager)
    {
        // remove this unit from the selected units
        // if the unit is dead
        SelectedUnits.Remove(unitManager);
    }

    void ClientHandleGameOver(string winnerName)
    {
        enabled = false;
    }
}
