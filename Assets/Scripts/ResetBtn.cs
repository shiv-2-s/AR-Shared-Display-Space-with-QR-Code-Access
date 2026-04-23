using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class UIManager : MonoBehaviour
{
    private GameObject spawnedObject;

    // 🔗 AR Session (optional if you are using it)
    public ARSession arSession;

    // 🔗 UI Elements
    public GameObject scanFrame;   // blurred square frame
    
    // 🔗 Set spawned object from AR script
    public void SetSpawnedObject(GameObject obj)
    {
        spawnedObject = obj;

        // Hide scan frame when object is placed
        if (scanFrame != null)
        {
            scanFrame.SetActive(false);
        }
    }

    // 🔄 Reset button function
    public void ResetScene()
    {
    if (spawnedObject != null)
    {
        Destroy(spawnedObject);
        spawnedObject = null;
    }

    // 👇 SHOW the frame again
    if (scanFrame != null)
    {
        scanFrame.SetActive(true);
    }
    }
}