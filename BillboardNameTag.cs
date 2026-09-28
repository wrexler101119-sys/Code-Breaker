using UnityEngine;

public class BillboardNameTag : MonoBehaviour
{
    private Camera cam;

    private void LateUpdate()
    {
        if (cam == null)
            cam = Camera.main;

        if (cam == null)
            return;

        // Face the local player's camera
        transform.LookAt(cam.transform);

        // Fix backwards text
        transform.Rotate(0f, 180f, 0f);
    }
}