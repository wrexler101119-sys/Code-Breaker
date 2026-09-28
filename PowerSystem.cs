using UnityEngine;

public class PowerSystem : MonoBehaviour
{
    public static PowerSystem Instance;

    [SerializeField] private Light[] lights;

    private void Awake()
    {
        Instance = this;
    }

    public void RestorePower()
    {
        foreach (var light in lights)
        {
            light.enabled = true;
        }

        Debug.Log("Power Restored!");
    }
}