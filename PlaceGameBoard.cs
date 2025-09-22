
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems; // Required for TrackableType

public class PlaceGameBoard : MonoBehaviour
{
    public GameObject gameBoard;

    private ARRaycastManager raycastManager;
    private ARPlaneManager planeManager;
    private bool placed = false;
    public delegate void SetBoardPosition(Vector3 newPosition);
    public event SetBoardPosition SetBoardPositionEvent;
    public delegate void SetBoardUp(Vector3 newPosition);
    public event SetBoardUp SetBoardUpEvent;

    void Start()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        planeManager = GetComponent<ARPlaneManager>();

        // Set detection mode to Horizontal
        planeManager.requestedDetectionMode = PlaneDetectionMode.Vertical;
    }

    void Update()
    {
        if (!placed)
        {
            // Use touch input if available, otherwise use mouse input (for XR simulation on laptop)
            if (IsTouchOrMousePressed(out Vector2 touchPosition))
            {
                // Perform the AR Raycast
                List<ARRaycastHit> hits = new List<ARRaycastHit>();
                if (raycastManager.Raycast(touchPosition, hits, TrackableType.PlaneWithinPolygon))
                {
                    placed = true;

                    Pose hitPose = hits[0].pose;

                    // Place and activate the game board
                    gameBoard.SetActive(true);
                    gameBoard.transform.position = hitPose.position;
                    gameBoard.transform.rotation = hitPose.rotation;

                    // send the board center coordinates and up vector to gameController.cs
                    if (SetBoardPositionEvent != null)
                    {
                        Vector3 adjustedPosition = hitPose.position + gameBoard.transform.up * 0.25f;
                        SetBoardPositionEvent(adjustedPosition);
                    }
                    else
                        Debug.LogError("WHY IS IT NULL");
                    if (SetBoardUpEvent != null)
                        SetBoardUpEvent(gameBoard.transform.right);
                    else
                        Debug.LogError("WHY IS IT NULL");


                    // Disable further plane detection
                    planeManager.requestedDetectionMode = PlaneDetectionMode.None;
                    DisableAllPlanes();
                }
            }
        }
    }


    // Check if touch or mouse is pressed and return the position
    private bool IsTouchOrMousePressed(out Vector2 touchPosition)
    {
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
        {
            // Use touch input
            touchPosition = Input.GetTouch(0).position;
            return true;
        }
        else if (Input.GetMouseButtonDown(0))
        {
            // Use mouse input
            touchPosition = Input.mousePosition;
            return true;
        }

        touchPosition = default;
        return false;
    }

    private void DisableAllPlanes()
    {
        foreach (var plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(false);
        }
    }

    public void AllowMoveGameBoard()
    {
        placed = false;
        gameBoard.SetActive(false);
        planeManager.requestedDetectionMode = PlaneDetectionMode.Horizontal;
        EnableAllPlanes();
    }

    private void EnableAllPlanes()
    {
        foreach (var plane in planeManager.trackables)
        {
            plane.gameObject.SetActive(true);
        }
    }

    public bool Placed()
    {
        return placed;
    }
}