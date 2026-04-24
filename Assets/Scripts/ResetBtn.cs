using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class UIManager : MonoBehaviour
{
    private GameObject spawnedObject;

    // 🔗 References
    public ARSession arSession;
    public GameObject scanFrame;

    public QRModelManager modelManager;
    public QRScanner scanner;
    public ARObjectInteraction interaction;

    // 🔗 Called when object is placed
    public void SetSpawnedObject(GameObject obj)
    {
        spawnedObject = obj;

        if (scanFrame != null)
        {
            scanFrame.SetActive(false);
        }
    }

    // 🔄 FINAL RESET FUNCTION
    public void ResetScene()
    {
        Debug.Log("🔄 Reset Started");

        // 🔥 1. Clear AR object (safe way)
        if (interaction != null)
        {
            interaction.ClearObject();
        }

        // 🔥 2. Reset model selection
        if (modelManager != null)
        {
            modelManager.ResetTracking();
        }

        // 🔥 3. Reset scanner
        if (scanner != null)
        {
            scanner.ResetScanner();
        }

        // 🔥 4. Clear local reference (optional safety)
        spawnedObject = null;

        // 🔥 5. Show scan UI
        if (scanFrame != null)
        {
            scanFrame.SetActive(true);
        }

        Debug.Log("✅ Reset Complete");
    }
}