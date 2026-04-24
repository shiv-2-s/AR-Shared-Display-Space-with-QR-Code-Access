using UnityEngine;
using Mirror;

public class QRNetworkSync : NetworkBehaviour
{
    [SyncVar] private Vector3 syncedPosition;
    [SyncVar] private Quaternion syncedRotation;
    [SyncVar] private Vector3 syncedScale;

    private GameObject syncedObject;

[SyncVar(hook = nameof(OnModelChanged))]
private string syncedModelName;

    void Update()
    {
        // 🔥 HOST updates values continuously
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

    // 🔥 Called when host places object
    public void SendPlacement(GameObject obj)
    {
        if (!isServer) return;

        syncedObject = obj;

        string modelName = obj.transform.GetChild(0).name.Replace("(Clone)", "").Trim();

        CmdSpawnObject(obj.transform.position, obj.transform.rotation, modelName);
    }

    [Command]
    void CmdSpawnObject(Vector3 pos, Quaternion rot, string modelName)
    {
        RpcSpawnObject(pos, rot, modelName);
    }

    [ClientRpc]
    void RpcSpawnObject(Vector3 pos, Quaternion rot, string modelName)
    {
        Debug.Log("🔥 RPC SPAWN RECEIVED");

        if (isServer) return;

        GameObject prefab = Resources.Load<GameObject>(modelName);

        if (prefab == null)
        {
            Debug.LogError("❌ Model NOT found: " + modelName);
            return;
        }

        // 🔥 Spawn in front (visibility safe)
        Camera cam = Camera.main;
        Vector3 spawnPos = cam.transform.position + cam.transform.forward * 2f;

        syncedObject = Instantiate(prefab, spawnPos, rot);

        Debug.Log("✅ Client object created");
    }

    void OnModelChanged(string oldModel, string newModel)
{
    Debug.Log("📡 Model received on client: " + newModel);

    // 🔥 Get model manager on THIS player
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