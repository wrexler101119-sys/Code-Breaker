using UnityEngine;

public class CameraFOVManager : MonoBehaviour
{
    public static CameraFOVManager Instance;

    [Header("FOV Settings")]
    [SerializeField] private float defaultFOV = 75f;
    [SerializeField] private float minFOV = 60f;
    [SerializeField] private float maxFOV = 90f;

    private const string FOVKey = "Camera_FieldOfView";

    private float currentFOV;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        DontDestroyOnLoad(gameObject);

        LoadFOV();
    }

    // ==========================================
    // SET FOV
    // ==========================================

    public void SetFOV(float value)
    {
        currentFOV = Mathf.Clamp(
            value,
            minFOV,
            maxFOV
        );

        PlayerPrefs.SetFloat(
            FOVKey,
            currentFOV
        );

        PlayerPrefs.Save();

        ApplyFOV();

        Debug.Log(
            $"FOV CHANGED | {currentFOV:F0}"
        );
    }

    // ==========================================
    // GET FOV
    // ==========================================

    public float GetFOV()
    {
        return currentFOV;
    }

    // ==========================================
    // RESET
    // ==========================================

    public void ResetFOV()
    {
        currentFOV = defaultFOV;

        PlayerPrefs.SetFloat(
            FOVKey,
            currentFOV
        );

        PlayerPrefs.Save();

        ApplyFOV();

        Debug.Log(
            $"FOV RESET | {currentFOV:F0}"
        );
    }

    // ==========================================
    // LOAD
    // ==========================================

    private void LoadFOV()
    {
        currentFOV = Mathf.Clamp(
            PlayerPrefs.GetFloat(
                FOVKey,
                defaultFOV
            ),
            minFOV,
            maxFOV
        );

        Debug.Log(
            $"FOV LOADED | {currentFOV:F0}"
        );

        ApplyFOV();
    }

    // ==========================================
    // APPLY TO ALL CURRENT CAMERAS
    // ==========================================

    private void ApplyFOV()
    {
        Camera[] cameras =
            FindObjectsByType<Camera>(
                FindObjectsSortMode.None
            );

        foreach (Camera cam in cameras)
        {
            ApplyToCamera(cam);
        }
    }

    // ==========================================
    // APPLY TO ONE CAMERA
    // ==========================================

    public void ApplyToCamera(Camera targetCamera)
    {
        if (targetCamera == null)
            return;

        if (targetCamera.orthographic)
            return;

        targetCamera.fieldOfView = currentFOV;

        Debug.Log(
            $"PLAYER CAMERA FOV APPLIED | " +
            $"Camera={targetCamera.name} | " +
            $"FOV={currentFOV:F0}"
        );
    }

    // ==========================================
    // APPLY WHEN A NEW CAMERA BECOMES ACTIVE
    // ==========================================

    public void RefreshAllCameras()
    {
        ApplyFOV();
    }
    private void LateUpdate()
    {
        if (currentFOV <= 0f)
            return;

        Camera mainCamera = Camera.main;

        if (mainCamera != null &&
            !mainCamera.orthographic &&
            Mathf.Abs(
                mainCamera.fieldOfView -
                currentFOV
            ) > 0.01f)
        {
            mainCamera.fieldOfView =
                currentFOV;
        }
    }
}