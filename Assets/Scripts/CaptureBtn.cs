using UnityEngine;

public class CaptureBtn : MonoBehaviour
{
    public GameObject helpPanel;

    public void Capture() // keep same name (no need to change button)
    {
        // Toggle Help Panel
        helpPanel.SetActive(!helpPanel.activeSelf);
    }
}