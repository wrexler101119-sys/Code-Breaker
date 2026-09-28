using UnityEngine;

[RequireComponent(typeof(Camera))]
public class MobileCameraAspectFix : MonoBehaviour
{
    private Camera cam;

    private void Start()
    {
        cam = GetComponent<Camera>();

        float targetAspect = 16f / 9f;
        float currentAspect = (float)Screen.width / Screen.height;

        if (currentAspect > targetAspect)
        {
            cam.fieldOfView = 55f;
        }
        else
        {
            cam.fieldOfView = 65f;
        }
    }
}