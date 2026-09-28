using UnityEngine;

public class SabotageTarget : MonoBehaviour
{
    [SerializeField] private NetworkTask networkTask;

    public NetworkTask Task
    {
        get
        {
            if (networkTask == null)
            {
                networkTask =
                    GetComponentInParent<NetworkTask>();
            }

            return networkTask;
        }
    }

    private void Awake()
    {
        if (Task == null)
        {
            Debug.LogError(
                $"SABOTAGE TARGET ERROR: " +
                $"No NetworkTask found for {name}.");
        }
    }
}