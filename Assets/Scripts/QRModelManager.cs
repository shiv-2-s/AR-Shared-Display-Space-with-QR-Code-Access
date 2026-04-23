using UnityEngine;
using Mirror;

public class QRModelManager : NetworkBehaviour
{
    public GameObject model1;
    public GameObject model2;

    public GameObject selectedModel;

    public bool hasSpawned = false;

    [SyncVar] public string selectedModelName; // 🔥 sync model name

    // =========================
    // 🔹 QR SELECT MODEL (HOST)
    // =========================
    public void SelectModel(string qrText)
    {
       Debug.Log("Selecting model for: " + qrText);

        if (qrText == "Student_Model_1")
        {
            selectedModel = model1;
            selectedModelName = "Student_Model_1";
        }
        else if (qrText == "Student_Model_2")
        {
            selectedModel = model2;
            selectedModelName = "Student_Model_2";
        }
        Debug.Log("SelectedModel is now: " + selectedModel);

        // 🔥 Send to clients
        if (isServer)
        {
            RpcSyncModel(selectedModelName);
        }
    }

    // =========================
    // 🔹 CLIENT RECEIVES MODEL
    // =========================
    [ClientRpc]
    void RpcSyncModel(string modelName)
    {
        selectedModelName = modelName;

        if (modelName == "Student_Model_1")
        {
            selectedModel = model1;
        }
        else if (modelName == "Student_Model_2")
        {
            selectedModel = model2;
        }

        Debug.Log("📡 Model synced: " + modelName);
    }

    // =========================
    // 🔄 RESET
    // =========================
    public void ResetTracking()
    {
        selectedModel = null;
        selectedModelName = "";
        hasSpawned = false;
    }
}