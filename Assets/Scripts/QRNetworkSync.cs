using UnityEngine;
using Mirror;

public class QRNetworkSync : NetworkBehaviour
{
    [SyncVar] private Vector3 syncedPosition;
    [SyncVar] private Quaternion syncedRotation;
    [SyncVar] private Vector3 syncedScale;

    private GameObject syncedObject;   // Pivot on both sides

    [SyncVar(hook = nameof(OnModelChanged))]
    private string syncedModelName;

    // =========================
    // 🔄 SYNC LOOP
    // =========================
    void Update()
    {
        // 🔥 HOST sends live updates
        if (isServer && syncedObject != null)
        {
            syncedPosition = syncedObject.transform.position;
            syncedRotation = syncedObject.transform.rotation;
            syncedScale = syncedObject.transform.localScale;
        }

        // 🔥 CLIENT applies updates
        if (!isServer && syncedObject != null)
        {
            syncedObject.transform.position = syncedPosition;
            syncedObject.transform.rotation = syncedRotation;
            syncedObject.transform.localScale = syncedScale;
        }
    }

    // =========================
    // 🔥 HOST SEND PLACEMENT
    // =========================
    public void SendPlacement(GameObject obj)
    {
        if (!isServer) return;

        syncedObject = obj;

        string modelName = obj.transform.GetChild(0).name.Replace("(Clone)", "").Trim();

        CmdSpawnObject(
            obj.transform.position,
            obj.transform.rotation,
            obj.transform.localScale,
            modelName
        );
    }

    // =========================
    // 🔥 COMMAND
    // =========================
    [Command]
    void CmdSpawnObject(Vector3 pos, Quaternion rot, Vector3 scale, string modelName)
    {
        RpcSpawnObject(pos, rot, scale, modelName);
    }

    // =========================
    // 🔥 CLIENT SPAWN (FIXED)
    // =========================
    [ClientRpc]
    void RpcSpawnObject(Vector3 pos, Quaternion rot, Vector3 scale, string modelName)
    {
        if (isServer) return;

        Debug.Log("🔥 CLIENT SPAWN RECEIVED");

        GameObject prefab = Resources.Load<GameObject>(modelName);

        if (prefab == null)
        {
            Debug.LogError("❌ Model NOT found: " + modelName);
            return;
        }

        // 🔥 CREATE SAME PIVOT
        GameObject pivot = new GameObject("Pivot");
        pivot.transform.position = pos;
        pivot.transform.rotation = rot;

        // 🔥 SPAWN MODEL AS CHILD
        GameObject model = Instantiate(prefab, pivot.transform);
        model.transform.localPosition = Vector3.zero;
        model.transform.localRotation = Quaternion.identity;

        // 🔥 CENTER MODEL (VERY IMPORTANT)
        Renderer[] renderers = model.GetComponentsInChildren<Renderer>();

        if (renderers.Length > 0)
        {
            Bounds bounds = renderers[0].bounds;
            foreach (Renderer r in renderers)
                bounds.Encapsulate(r.bounds);

            Vector3 localCenter = pivot.transform.InverseTransformPoint(bounds.center);
            model.transform.localPosition -= localCenter;
        }

        // 🔥 APPLY SCALE
        pivot.transform.localScale = scale;

        syncedObject = pivot;

        Debug.Log("✅ Client object perfectly synced");
    }

    // =========================
    // 🔥 MODEL SYNC
    // =========================
    void OnModelChanged(string oldModel, string newModel)
    {
        QRModelManager modelManager = GetComponent<QRModelManager>();

        if (modelManager != null)
        {
            modelManager.SelectModel(newModel);
        }
    }

    public void SendModelSelection(string modelName)
    {
        if (!isServer) return;

        syncedModelName = modelName;
    }

    public override void OnStartClient()
    {
        Debug.Log("✅ Client player initialized");
    }
}