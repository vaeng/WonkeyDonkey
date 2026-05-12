using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Spawns road segments ahead and despawns them behind the stationary player.
/// </summary>
public class RoadSpawner : MonoBehaviour
{
    [Header("Road Prefab")]
    [SerializeField] private GameObject roadSegmentPrefab;

    [Header("World Movement")]
    [SerializeField] private float worldMoveSpeed = 6f;

    [Header("Road Settings")]
    [SerializeField] private float segmentLength = 20f;
    [SerializeField] private int initialSegments = 5;

    [Header("Spawn / Despawn")]
    [SerializeField] private float spawnAheadDistance = 80f;
    [SerializeField] private float despawnBehindDistance = 30f;

    private readonly List<GameObject> activeSegments = new List<GameObject>();

    private void Start()
    {
        SpawnInitialSegments();
    }

    private void Update()
    {
        DespawnSegmentsBehind();
        SpawnSegmentsAhead();
    }

    private void SpawnInitialSegments()
    {
        activeSegments.Clear();

        for (int i = 0; i < initialSegments; i++)
        {
            float spawnZ = i * segmentLength;
            SpawnSegment(spawnZ);
        }
    }

    private void SpawnSegmentsAhead()
    {
        while (GetFarthestSegmentZ() < spawnAheadDistance)
        {
            float spawnZ = GetFarthestSegmentZ() + segmentLength;
            SpawnSegment(spawnZ);
        }
    }

    private void SpawnSegment(float spawnZ)
    {
        Vector3 spawnPosition = new Vector3(0f, 0f, spawnZ);

        GameObject newSegment = Instantiate(
            roadSegmentPrefab,
            spawnPosition,
            Quaternion.identity
        );

        WorldMover worldMover = newSegment.GetComponent<WorldMover>();

        if (worldMover == null)
            worldMover = newSegment.AddComponent<WorldMover>();

        worldMover.SetMoveSpeed(worldMoveSpeed);

        activeSegments.Add(newSegment);
    }

    private void DespawnSegmentsBehind()
    {
        for (int i = activeSegments.Count - 1; i >= 0; i--)
        {
            GameObject segment = activeSegments[i];

            if (segment == null)
            {
                activeSegments.RemoveAt(i);
                continue;
            }

            float segmentEndZ = segment.transform.position.z + segmentLength;

            bool isBehindPlayer =
                segmentEndZ < -despawnBehindDistance;

            if (!isBehindPlayer)
                continue;

            activeSegments.RemoveAt(i);
            Destroy(segment);
        }
    }

    private float GetFarthestSegmentZ()
    {
        if (activeSegments.Count == 0)
            return 0f;

        float farthestZ = float.MinValue;

        foreach (GameObject segment in activeSegments)
        {
            if (segment == null)
                continue;

            if (segment.transform.position.z > farthestZ)
                farthestZ = segment.transform.position.z;
        }

        if (farthestZ == float.MinValue)
            return 0f;

        return farthestZ;
    }
}