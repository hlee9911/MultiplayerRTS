using System.Collections;
using System.Collections.Generic;
using Mirror;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Manages the control of the Camera using keyboard WASD, arrow keys and a mouse
/// </summary>
public class CameraController : NetworkBehaviour
{
    [SerializeField] private Transform playerCameraTransform = null;
    [SerializeField] private float speed = 20f;
    // how close you have to be to the edge for it to actually start moving
    [SerializeField] private float screenBorderThickness = 10f;
    [SerializeField] private Vector2 screenXLimits = Vector2.zero;
    [SerializeField] private Vector2 screenZLimits = Vector2.zero;

    // save the previous input
    private Vector2 previousInput;

    private Controls controls;

    #region Client

    public override void OnStartAuthority()
    {
        playerCameraTransform.gameObject.SetActive(true);

        // playerCameraTransform.position += new Vector3(0, 0, -10);

        controls = new Controls();

        // listen to when I perform the action and when I cancel it
        // two events to listen for
        // for input systems, you don't need to unsubscribe it?
        // handled by itself
        controls.Player.MoveCamera.performed += SetPreviousInput;
        controls.Player.MoveCamera.canceled += SetPreviousInput;


        controls.Enable();   
    }

    public override void OnStartClient()
    {
        UnitBase.ClientOnBaseSpawned += ClientHandleBaseSpawned;
    }

    public override void OnStopClient()
    {
        UnitBase.ClientOnBaseSpawned -= ClientHandleBaseSpawned;
    }

    void ClientHandleBaseSpawned(UnitBase unitBase)
    {
        // setting the initial camera position
        Vector3 basePos = unitBase.transform.position;

        if (playerCameraTransform.gameObject.activeInHierarchy)
        {
            Debug.Log("Setting initial camera position");
            playerCameraTransform.position += new Vector3(basePos.x, 3.5f, basePos.z - 18);
        }
    }

    [ClientCallback]
    void Update()
    {
        // prevent from moving other player's camera
        if (!isOwned || !Application.isFocused) return;

        UpdateCameraPosition();
    }

    void UpdateCameraPosition()
    {
        Vector3 pos = playerCameraTransform.position;

        // if we didn't put anything on the keyboard,
        // we want to check if the mouse is in one of the edges
        if (previousInput == Vector2.zero)
        {
            Vector3 cursorMovement = Vector3.zero;

            Vector2 cursorPosition = Mouse.current.position.ReadValue();

            // check where the mouse is on the screen
            if (cursorPosition.y >= Screen.height - screenBorderThickness)
            {
                // we want to move up a bit
                cursorMovement.z += 1;
            }
            else if (cursorPosition.y < screenBorderThickness)
            {
                cursorMovement.z -= 1;
            }

            if (cursorPosition.x >= Screen.width - screenBorderThickness)
            {
                // we want to move right a bit
                cursorMovement.x += 1;
            }
            else if (cursorPosition.x < screenBorderThickness)
            {
                cursorMovement.x -= 1;
            }

            // prevent from moving faster, same magnitude
            pos += cursorMovement.normalized * speed * Time.deltaTime;
        }
        else
        {
            // otherwise, move the camera with keyboard inputs
            pos += new Vector3(previousInput.x, 0, previousInput.y) * speed * Time.deltaTime;
        }

        // prevent from going out of the bounds
        // x and y for screenXLimits are min and max respectively
        pos.x = Mathf.Clamp(pos.x, screenXLimits.x, screenZLimits.y);
        pos.z = Mathf.Clamp(pos.z, screenXLimits.x, screenZLimits.y);

        // finally move the camera
        playerCameraTransform.position = pos;
    }

    void SetPreviousInput(InputAction.CallbackContext ctx)
    {
        previousInput = ctx.ReadValue<Vector2>();
    }

    #endregion

}
