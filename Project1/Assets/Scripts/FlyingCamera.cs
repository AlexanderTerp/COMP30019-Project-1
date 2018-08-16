﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlyingCamera : MonoBehaviour {

    public float movementSpeed;
    public float sprintMultiplier;
    public float xLookSensitivity;
    public float yLookSensitivity;

    void Start() {
        // Lock the cursor to the application and hide the cursor.
        OnUiToggle(false);

        // Register with the GameController's UiToggle observer list.
        GameController.uiToggleEvent += OnUiToggle;
    }

    // Update is called once per frame
    void Update()
    {
        PerformCameraRotation();
        PerformCameraTranslation();
    }

    /**
     * Takes care of the camera rotation, based on mouse movement.
     */
    private void PerformCameraRotation() {
        // If the UI is enabled, we do not want mouse movements rotating the camera.
        if (GameController.uiEnabled)
        {
            return;
        }

        // Get values for how much the axes have moved this frame.
        float xRotationAmount = Input.GetAxis("Mouse X") * xLookSensitivity;
        float yRotationAmount = Input.GetAxis("Mouse Y") * yLookSensitivity;

        // Translate those values and rotate the camera accordingly.
        transform.localEulerAngles += new Vector3(-yRotationAmount, xRotationAmount, 0) * Time.deltaTime;
    }

    /**
     * Takes care of the camera translation/movement, using WASD.
     */
    private void PerformCameraTranslation()
    {
        // Prepare a vector to hold the movement direction.
        Vector3 movement = Vector3.zero;

        // Add vectors if the appropriate keys are pressed.
        movement += Input.GetKey(KeyCode.W) ? transform.forward : Vector3.zero;
        movement += Input.GetKey(KeyCode.A) ? -transform.right : Vector3.zero;
        movement += Input.GetKey(KeyCode.S) ? -transform.forward : Vector3.zero;
        movement += Input.GetKey(KeyCode.D) ? transform.right : Vector3.zero;

        // Ensure that the base movement speed is == movementSpeed.
        movement = movement.normalized * movementSpeed;

        // Take into account sprinting, if necessary.
        movement *= Input.GetKey(KeyCode.LeftShift) ? sprintMultiplier : 1;

        // Translate the camera.
        transform.position += movement * Time.deltaTime;
    }

    /**
     * Called when UI is toggled in GameController. Ensures that the cursor becomes
     * visible and unlocked from the center of the screen.
     */
    private void OnUiToggle(bool uiEnabled)
    {
        Cursor.lockState = uiEnabled ? CursorLockMode.Confined : CursorLockMode.Locked;
        Cursor.visible = uiEnabled;
    }
}
