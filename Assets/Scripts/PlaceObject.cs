using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using System.Collections.Generic;
using Mirror;

public class ARObjectInteraction : MonoBehaviour
{
    [Header("References")]
    public UIManager uiManager;

    private GameObject spawnedObject;
    private GameObject pivotObject;

    private ARRaycastManager raycastManager;
    private Camera arCamera;

    private List<ARRaycastHit> hits = new List<ARRaycastHit>();

    private float initialDistance;
    private Vector3 initialScale;

    void Awake()
    {
        raycastManager = GetComponent<ARRaycastManager>();
        arCamera = Camera.main;
    }

    void Update()
    {
        if (Input.touchCount == 0)
            return;

        // 🔥 GET QRModelManager FROM NETWORK PLAYER
        if (NetworkClient.localPlayer == null)
        {
            Debug.Log("❌ Local player not ready yet");
            return;
        }

        QRModelManager modelManager = NetworkClient.localPlayer.GetComponent<QRModelManager>();

        if (modelManager == null)
        {
            Debug.Log("❌ QRModelManager missing on NetworkPlayer");
            return;
        }

        // ---------- SINGLE FINGER ----------
        if (Input.touchCount == 1)
        {
            Touch touch = Input.GetTouch(0);

            // 🔹 PLACE OBJECT
            if (touch.phase == TouchPhase.Began && pivotObject == null)
            {
                if (modelManager.selectedModel == null)
                {
                    Debug.Log("❌ No model selected from QR!");
                    return;
                }

                if (raycastManager.Raycast(touch.position, hits, TrackableType.PlaneWithinPolygon))
                {
                    Pose hitPose = hits[0].pose;
                    Vector3 spawnPosition = hitPose.position + new Vector3(0, 0.05f, 0);

                    // 🔥 CREATE PIVOT
                    pivotObject = new GameObject("Pivot");
                    pivotObject.transform.position = spawnPosition;

                    // 🔥 SPAWN MODEL
                    spawnedObject = Instantiate(
                        modelManager.selectedModel,
                        spawnPosition,
                        hitPose.rotation
                    );

                    // 🔥 SET PARENT
                    spawnedObject.transform.SetParent(pivotObject.transform);

                    // 🔥 CENTER MODEL
                    CenterObject(spawnedObject);

                    // 🔥 NETWORK SYNC
                    QRNetworkSync net = NetworkClient.localPlayer.GetComponent<QRNetworkSync>();

                    if (net == null)
                    {
                        Debug.Log("❌ QRNetworkSync missing on player");
                        return;
                    }

                    // 🔥 ONLY HOST SENDS DATA
                    if (NetworkServer.active)
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
                    Pose hitPose = hits[0].pose;
                    pivotObject.transform.position = hitPose.position;
                }
            }
        }

        // ---------- TWO FINGERS ----------
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

            // 🔹 ROTATE
            Vector2 delta = touch1.deltaPosition + touch2.deltaPosition;

            float rotationSpeed = 0.2f;

            float rotX = delta.y * rotationSpeed;
            float rotY = -delta.x * rotationSpeed;

            pivotObject.transform.Rotate(rotX, rotY, 0, Space.Self);
        }
    }

    // 🔥 CENTER MODEL
    void CenterObject(GameObject obj)
    {
        Renderer[] renderers = obj.GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;

        foreach (Renderer r in renderers)
        {
            bounds.Encapsulate(r.bounds);
        }

        Vector3 center = bounds.center;

        obj.transform.position -= center - pivotObject.transform.position;
    }

    // 🔥 RESET
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