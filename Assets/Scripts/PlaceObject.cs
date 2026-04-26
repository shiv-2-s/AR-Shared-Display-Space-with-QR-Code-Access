using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using Mirror;
using UnityEngine.UI;

public class ARObjectInteraction : MonoBehaviour
{
    [Header("References")]
    public UIManager uiManager;

    public enum RotationMode
    {
        YAxis,
        XAxis
    }

    public Image rotationIcon;
    public Sprite yAxisSprite;
    public Sprite xAxisSprite;

    public RotationMode rotationMode = RotationMode.YAxis;

    private GameObject spawnedObject;
    private GameObject pivotObject;

    private ARRaycastManager raycastManager;
    private Camera arCamera;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private float initialDistance;
    private Vector3 initialScale;

    // 🔄 ROTATION MODE TOGGLE
    public void ToggleRotationMode()
    {
        if (rotationMode == RotationMode.YAxis)
        {
            rotationMode = RotationMode.XAxis;
            if (rotationIcon != null && xAxisSprite != null)
                rotationIcon.sprite = xAxisSprite;
        }
        else
        {
            rotationMode = RotationMode.YAxis;
            if (rotationIcon != null && yAxisSprite != null)
                rotationIcon.sprite = yAxisSprite;
        }
    }

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        arCamera = Camera.main;
    }

    void Update()
    {
        if (Input.touchCount == 0)
            return;

        if (NetworkClient.localPlayer == null)
            return;

        QRModelManager modelManager = NetworkClient.localPlayer.GetComponent<QRModelManager>();
        if (modelManager == null)
            return;

        // ---------- SINGLE TOUCH ----------
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            // 🔹 PLACE OBJECT
            if (touch.phase == TouchPhase.Began && pivotObject == null)
            {
                if (modelManager.selectedModel == null)
                    return;

                if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = hits[0].pose;

                    // 🔥 CREATE PIVOT AT PLANE
                    pivotObject = new GameObject("Pivot");
                    pivotObject.transform.position = hitPose.position;
                    pivotObject.transform.rotation = hitPose.rotation;

                    // 🔥 SPAWN MODEL AS CHILD
                    spawnedObject = Instantiate(modelManager.selectedModel, pivotObject.transform);

                    // 🔥 RESET LOCAL TRANSFORM
                    spawnedObject.transform.localPosition = Vector3.zero;
                    spawnedObject.transform.localRotation = Quaternion.identity;

                    // 🔥 CALCULATE BOUNDS
                    Renderer[] renderers = spawnedObject.GetComponentsInChildren<Renderer>();
                    if (renderers.Length == 0) return;

                    Bounds bounds = renderers[0].bounds;
                    foreach (Renderer r in renderers)
                    {
                        bounds.Encapsulate(r.bounds);
                    }

                    // 🔥 ALIGN CENTER TO PIVOT
                    Vector3 localCenter = pivotObject.transform.InverseTransformPoint(bounds.center);
                    spawnedObject.transform.localPosition -= localCenter;

                    // 🔥 FIX HEIGHT (place on plane)
                    float bottomY = bounds.min.y;
                    float pivotY = pivotObject.transform.position.y;
                    float heightOffset = pivotY - bottomY;

                    spawnedObject.transform.position += new Vector3(0, heightOffset, 0);

                    // 🔥 NETWORK SYNC (HOST ONLY)
                    QRNetworkSync net = NetworkClient.localPlayer.GetComponent<QRNetworkSync>();
                    if (net != null && NetworkServer.active)
                    {
                        net.SendPlacement(pivotObject);
                    }

                    // 🔥 UI LINK
                    if (uiManager != null)
                    {
                        uiManager.SetSpawnedObject(pivotObject);
                    }
                }
            }

            // 🔹 DRAG
            if (touch.phase == TouchPhase.Moved && pivotObject != null)
            {
                if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                {
                    pivotObject.transform.position = hits[0].pose.position;
                }
            }
        }

        // ---------- TWO TOUCH ----------
        if (Input.touchCount == 2 && pivotObject != null)
        {
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            // 🔹 SCALE
            if (touch1.phase == TouchPhase.Began || touch2.phase == TouchPhase.Began)
            {
                initialDistance = Vector2.Distance(touch1.position, touch2.position);
                initialScale = pivotObject.transform.localScale;
            }
            else
            {
                float currentDistance = Vector2.Distance(touch1.position, touch2.position);

                if (initialDistance > 0)
                {
                    float scaleFactor = currentDistance / initialDistance;
                    pivotObject.transform.localScale = initialScale * scaleFactor;
                }
            }

            // 🔹 ROTATE (STRICT AXIS)
            Vector2 delta = touch1.deltaPosition + touch2.deltaPosition;
            float rotationSpeed = 0.2f;

            if (rotationMode == RotationMode.YAxis)
            {
                float rotY = -delta.x * rotationSpeed;
                pivotObject.transform.Rotate(0f, rotY, 0f, Space.World);
            }
            else
            {
                float rotX = delta.y * rotationSpeed;
                pivotObject.transform.Rotate(rotX, 0f, 0f, Space.World);
            }
        }
    }

    // 🔄 RESET
    public void ClearObject()
    {
        if (pivotObject != null)
        {
            Destroy(pivotObject);
            pivotObject = null;
            spawnedObject = null;
        }
    }
}