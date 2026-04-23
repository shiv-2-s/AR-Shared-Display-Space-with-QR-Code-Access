using TMPro;
using Mirror;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.Collections;
using ZXing;
using System.Collections;

public class QRScanner : MonoBehaviour
{
    public TMP_Text statusText;
    public GameObject scanFrame;
    public ARCameraManager cameraManager;

    private BarcodeReader reader;
    private Texture2D cameraTexture;

    private bool hasScanned = false;

    void Start()
    {
        reader = new BarcodeReader();
    }

    void OnEnable()
    {
        cameraManager.frameReceived += OnCameraFrameReceived;
    }

    void OnDisable()
    {
        cameraManager.frameReceived -= OnCameraFrameReceived;
    }

    void OnCameraFrameReceived(ARCameraFrameEventArgs args)
    {
        if (hasScanned) return;

        if (!cameraManager.TryAcquireLatestCpuImage(out var cpuImage))
            return;

        var conversionParams = new UnityEngine.XR.ARSubsystems.XRCpuImage.ConversionParams
        {
            inputRect = new RectInt(0, 0, cpuImage.width, cpuImage.height),
            outputDimensions = new Vector2Int(cpuImage.width, cpuImage.height),
            outputFormat = TextureFormat.RGBA32,
            transformation = UnityEngine.XR.ARSubsystems.XRCpuImage.Transformation.MirrorY
        };

        int size = cpuImage.GetConvertedDataSize(conversionParams);
        var buffer = new NativeArray<byte>(size, Allocator.Temp);

        cpuImage.Convert(conversionParams, buffer);
        cpuImage.Dispose();

        if (cameraTexture == null)
        {
            cameraTexture = new Texture2D(cpuImage.width, cpuImage.height, TextureFormat.RGBA32, false);
        }

        cameraTexture.LoadRawTextureData(buffer);
        cameraTexture.Apply();

        buffer.Dispose();

        Color32[] pixels = cameraTexture.GetPixels32();

        var result = reader.Decode(pixels, cameraTexture.width, cameraTexture.height);

        if (result != null)
        {
            hasScanned = true;
            Debug.Log("Is Server: " + NetworkServer.active);
            Debug.Log("QR Found: " + result.Text);
            statusText.text = "Loaded: " + result.Text;
            scanFrame.SetActive(false);

            // 🔥 FIX: wait until NetworkPlayer (QRModelManager) exists
            StartCoroutine(SetModelWhenReady(result.Text));
        }
    }

    // =========================
    // 🔥 WAIT FOR NETWORK PLAYER
    // =========================
    IEnumerator SetModelWhenReady(string qrText)
    {
        QRModelManager modelManager = null;

        while (modelManager == null)
        {
            modelManager = FindObjectOfType<QRModelManager>();
            Debug.Log("Scanner using: " + modelManager);
Debug.Log("Scanner ID: " + (modelManager != null ? modelManager.GetInstanceID().ToString() : "NULL"));
            yield return null; // wait next frame
        }

        Debug.Log("✅ QRModelManager found");

        if (NetworkServer.active)
        {
            Debug.Log("📡 Setting model on HOST: " + qrText);
            modelManager.SelectModel(qrText);
        }
        else
        {
            Debug.Log("⚠️ Not host, skipping model selection");
        }
    }

    // =========================
    // 🔄 RESET
    // =========================
    public void ResetScanner()
    {
        hasScanned = false;
        scanFrame.SetActive(true);
        statusText.text = "Scan QR Code";
    }
}