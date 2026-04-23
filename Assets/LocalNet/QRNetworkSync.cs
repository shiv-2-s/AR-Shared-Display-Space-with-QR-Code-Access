using UnityEngine;
using Mirror;

public class QRNetworkSync : NetworkBehaviour
{
    // 🔥 Called by HOST after placing object
    public void SendPlacement(GameObject obj)
    {
        Debug.Log("🔥 SendPlacement CALLED");

        // ❗ Only server (host) should send
        if (!isServer)
        {
            Debug.Log("❌ Not server, cannot send");
            return;
        }

        // 🔥 Get model name (clean)
        string modelName = obj.transform.GetChild(0).name.Replace("(Clone)", "").Trim();

        Debug.Log("📦 Sending Model: " + modelName);

        // 🔥 Send to server → then to all clients
        CmdSendPlacement(obj.transform.position, obj.transform.rotation, modelName);
    }

    // 🔥 Runs on SERVER
    [Command]
    void CmdSendPlacement(Vector3 pos, Quaternion rot, string modelName)
    {
        Debug.Log("🔥 COMMAND RECEIVED ON SERVER");

        // 🔥 Send to ALL clients
        RpcPlaceObject(pos, rot, modelName);
    }

    // 🔥 Runs on ALL clients (including host)
    [ClientRpc]
void RpcPlaceObject(Vector3 pos, Quaternion rot, string modelName)
{
    Debug.Log("🔥 RPC RECEIVED");

    // 🔥 Skip host (host already spawned it locally)
    if (isServer) return;

    GameObject prefab = Resources.Load<GameObject>(modelName);

    if (prefab == null)
    {
        Debug.LogError("❌ Model NOT found in Resources: " + modelName);
        return;
    }

    Camera cam = Camera.main;

Vector3 spawnPos = cam.transform.position + cam.transform.forward * 1.0f;

Instantiate(prefab, spawnPos, rot);

    Debug.Log("✅ Object spawned on client");
    GameObject obj = Instantiate(prefab, spawnPos, rot);
Debug.Log("✅ Spawned object: " + obj.name);
}

    // 🔥 Debug: confirms client player exists
    public override void OnStartClient()
    {
        Debug.Log("✅ Client player initialized");
    }
}