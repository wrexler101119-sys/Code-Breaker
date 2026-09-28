using Fusion;
using UnityEngine;

public class SpawnSlidingDoor : NetworkBehaviour
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
      Quaternion.Euler(0f, -90f, 0f),
      onBeforeSpawned: (runner, obj) =>
      {
          obj.transform.localScale = point.lossyScale;
      }
  );
        }

        Debug.Log("Spawned " + spawnPoints.Length + " doors");
    }
}