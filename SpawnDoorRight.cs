using Fusion;
using UnityEngine;

public class SpawnDoorRight : NetworkBehaviour
{
    [SerializeField] private NetworkPrefabRef doorPrefab;

    [Header("Spawn Settings")]
    [SerializeField] private Transform[] spawnPoints;

    public override void Spawned()
    {
        if (!Object.HasStateAuthority)
            return;

        SpawnAllDoors();
    }

    public void SpawnAllDoors()
    {
        foreach (var point in spawnPoints)
        {
            Runner.Spawn(
                doorPrefab,
                point.position,
                Quaternion.identity
            );
        }

        Debug.Log("Spawned " + spawnPoints.Length + " doors");
    }
}