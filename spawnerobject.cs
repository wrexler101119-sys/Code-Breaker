using Fusion;
using System.Collections.Generic;
using UnityEngine;

public class Spawnerobject : NetworkBehaviour
{
    [System.Serializable]
    public class SpawnEntry
    {
        [Header("Task Prefab")]
        public NetworkPrefabRef prefab;

        [Header("Possible Spawn Points")]
        public Transform[] spawnPoints;
    }

    [Header("Task Spawn Entries")]
    [SerializeField] private SpawnEntry[] spawnEntries;

    // Keeps track of the task objects created by this spawner.
    private readonly List<NetworkObject> spawnedTasks =
        new List<NetworkObject>();

    public override void Spawned()
    {
        if (!Object.HasStateAuthority)
            return;

        // Initial game spawn.
        SpawnAll();
    }

    // =====================================================
    // INITIAL / NEW ROUND SPAWN
    // =====================================================

    public void SpawnAll()
    {
        if (!HasStateAuthority)
            return;

        int spawnedCount = 0;

        foreach (SpawnEntry entry in spawnEntries)
        {
            if (!entry.prefab.IsValid)
            {
                Debug.LogWarning(
                    "Spawnerobject: Invalid task prefab."
                );

                continue;
            }

            Transform[] validPoints =
                GetValidSpawnPoints(entry.spawnPoints);

            if (validPoints.Length == 0)
            {
                Debug.LogWarning(
                    $"Spawnerobject: No valid spawn points for {entry.prefab}."
                );

                continue;
            }

            // Pick ONE random location for this task.
            int randomIndex =
                Random.Range(0, validPoints.Length);

            Transform selectedPoint =
                validPoints[randomIndex];

            NetworkObject spawnedObject =
                Runner.Spawn(
                    entry.prefab,
                    selectedPoint.position,
                    selectedPoint.rotation
                );

            if (spawnedObject != null)
            {
                spawnedTasks.Add(spawnedObject);

                spawnedCount++;

                Debug.Log(
                    $"TASK SPAWNED | " +
                    $"Prefab={entry.prefab} | " +
                    $"Location={selectedPoint.name} | " +
                    $"RandomIndex={randomIndex}"
                );
            }
        }

        Debug.Log(
            $"SPAWNER COMPLETE | " +
            $"Tasks Spawned={spawnedCount}"
        );
    }

    // =====================================================
    // PLAY AGAIN
    // =====================================================

    public void RespawnAllRandomly()
    {
        if (!HasStateAuthority)
        {
            Debug.LogWarning(
                "RESPAWN TASKS FAILED | No State Authority."
            );

            return;
        }

        Debug.Log("RESPAWNING ALL TASKS RANDOMLY");

        // Remove the old task objects first.
        DespawnCurrentTasks();

        // Spawn a fresh set at new random locations.
        SpawnAll();

        Debug.Log(
            "RANDOM TASK RESPAWN COMPLETE"
        );
    }

    // =====================================================
    // DESPAWN CURRENT TASKS
    // =====================================================

    private void DespawnCurrentTasks()
    {
        int removedCount = 0;

        // Work backwards because entries may be removed.
        for (int i = spawnedTasks.Count - 1; i >= 0; i--)
        {
            NetworkObject taskObject =
                spawnedTasks[i];

            if (taskObject != null &&
                taskObject.IsValid)
            {
                Runner.Despawn(taskObject);
                removedCount++;
            }

            spawnedTasks.RemoveAt(i);
        }

        Debug.Log(
            $"OLD TASKS DESPAWNED | Count={removedCount}"
        );
    }

    // =====================================================
    // VALID SPAWN POINTS
    // =====================================================

    private Transform[] GetValidSpawnPoints(
        Transform[] points)
    {
        if (points == null ||
            points.Length == 0)
        {
            return new Transform[0];
        }

        List<Transform> validPoints =
            new List<Transform>();

        foreach (Transform point in points)
        {
            if (point != null)
            {
                validPoints.Add(point);
            }
        }

        return validPoints.ToArray();
    }
}