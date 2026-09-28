using UnityEngine;

public class Highlighter : MonoBehaviour
{
    [Header("Highlight Settings")]
    [SerializeField] private float maxDistance = 10f;   // How far the highlight can reach
    [SerializeField] private Color highlightColor = Color.yellow;
    [SerializeField] private LayerMask highlightableLayer;

    private GameObject currentHighlighted;
    private Material originalMaterial;

    /// <summary>
    /// Call this every frame from CameraFollow.LateUpdate
    /// </summary>
    public void UpdateHighlightable(Vector3 origin, Vector3 direction, Player player)
    {
        Ray ray = new Ray(origin, direction);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, maxDistance, highlightableLayer))
        {
            GameObject hitObject = hit.collider.gameObject;

            // If we are already highlighting this object, do nothing
            if (currentHighlighted != hitObject)
            {
                ClearHighlight();

                // Store original material
                Renderer renderer = hitObject.GetComponent<Renderer>();
                if (renderer != null)
                {
                    originalMaterial = renderer.material;
                    Material highlightMat = new Material(originalMaterial);
                    highlightMat.color = highlightColor;
                    renderer.material = highlightMat;
                    currentHighlighted = hitObject;
                }
            }
        }
        else
        {
            ClearHighlight();
        }
    }

    private void ClearHighlight()
    {
        if (currentHighlighted != null)
        {
            Renderer renderer = currentHighlighted.GetComponent<Renderer>();
            if (renderer != null && originalMaterial != null)
            {
                renderer.material = originalMaterial;
            }
            currentHighlighted = null;
            originalMaterial = null;
        }
    }
}