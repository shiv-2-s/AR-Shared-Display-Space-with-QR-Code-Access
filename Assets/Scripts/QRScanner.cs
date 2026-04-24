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

    void Update()
    {
        // 🔥 CLIENT: Hide scan UI when connected
        if (NetworkClient.isConnected && !NetworkServer.active && !hasScanned)
        {
            Debug.Log("📱 Client connected → hiding scan UI");

            scanFrame.SetActive(false);
            statusText.text = "Connected to Host";

            hasScanned = true; // prevent scanning on client
        }
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
        // 🔥 ONLY HOST scans QR
        if (!NetworkServer.active) return;

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

            Debug.Log("📷 QR Found: " + result.Text);

            statusText.text = "Loaded: " + result.Text;
            scanFrame.SetActive(false);

            // 🔥 Wait until NetworkPlayer is ready
            StartCoroutine(SetModelWhenReady(result.Text));
        }
    }

    // =========================
    // 🔥 WAIT + SET + SEND MODEL
    // =========================
    IEnumerator SetModelWhenReady(string qrText)
    {
        QRModelManager modelManager = null;
        QRNetworkSync net = null;

        while (modelManager == null || net == null)
        {
            if (NetworkClient.localPlayer != null)
            {
                modelManager = NetworkClient.localPlayer.GetComponent<QRModelManager>();
                net = NetworkClient.localPlayer.GetComponent<QRNetworkSync>();
            }

            yield return null;
        }

        Debug.Log("✅ NetworkPlayer ready");

        // 🔥 HOST selects model
        modelManager.SelectModel(qrText);

        // 🔥 SEND model to client
        net.SendModelSelection(qrText);

        Debug.Log("📡 Model sent to client: " + qrText);
    }

    // =========================
    // 🔄 RESET
    // =========================
    public void ResetScanner()
    {
        hasScanned = false;

        // 🔥 HOST shows scanner
        if (NetworkServer.active)
        {
            scanFrame.SetActive(true);
            statusText.text = "Scan QR Code";
        }
        else
        {
            // 🔥 CLIENT stays connected UI
            scanFrame.SetActive(false);
            statusText.text = "Connected to Host";
        }
    }
}