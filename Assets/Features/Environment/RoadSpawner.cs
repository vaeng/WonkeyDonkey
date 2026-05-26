using System.Collections.Generic;
using UnityEngine;

public class RoadSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LaneMovement laneMovement;

    [Header("Road")]
    [SerializeField] private GameObject[] roadSegmentPrefabs;
    [SerializeField] private float segmentLength = 20f;
    [SerializeField] private int startSegmentCount = 5;

    [Header("Variation")]
    [SerializeField] private int avoidRepeatForSpawns = 3;

    [Header("Movement")]
    [SerializeField] private float worldMoveSpeed = 6f;

    [Header("Spawn / Despawn")]
    [SerializeField] private float spawnAheadDistance = 80f;
    [SerializeField] private float despawnBehindDistance = 30f;

    private readonly List<GameObject> spawnedSegments = new();
    private readonly Queue<GameObject> recentlyUsedPrefabs = new();

    private void Start()
    {
        SpawnStartSegments();
    }

    private void Update()
    {
        RemoveOldSegments();
        SpawnMissingSegments();
    }

    private void SpawnStartSegments()
    {
        spawnedSegments.Clear();
        recentlyUsedPrefabs.Clear();

        for (int i = 0; i < startSegmentCount; i++)
            SpawnSegment(i * segmentLength);
    }

    private void SpawnMissingSegments()
    {
        while (GetLastSegmentZ() < spawnAheadDistance)
        {
            float nextZ = GetLastSegmentZ() + segmentLength;
            SpawnSegment(nextZ);
        }
    }

    private void SpawnSegment(float zPosition)
    {
        if (!CanSpawn())
            return;

        float roadX = 0f;
        float visualX = laneMovement.GetVisualWorldXForRoadX(roadX);
        GameObject prefab = GetRandomRoadSegment();

        GameObject segment = Instantiate(
            prefab,
            new Vector3(visualX, 0f, zPosition),
            Quaternion.identity
        );

        WorldMover mover = segment.GetComponent<WorldMover>();

        if (mover == null)
            mover = segment.AddComponent<WorldMover>();

        mover.Initialize(laneMovement, worldMoveSpeed, roadX);

        spawnedSegments.Add(segment);
        RememberPrefab(prefab);
    }

    private bool CanSpawn()
    {
        return laneMovement != null
            && roadSegmentPrefabs != null
            && roadSegmentPrefabs.Length > 0;
    }

    private GameObject GetRandomRoadSegment()
    {
        List<GameObject> possiblePrefabs = new();

        foreach (GameObject prefab in roadSegmentPrefabs)
        {
            if (prefab == null)
                continue;

            if (recentlyUsedPrefabs.Contains(prefab))
                continue;

            possiblePrefabs.Add(prefab);
        }

        if (possiblePrefabs.Count == 0)
            return roadSegmentPrefabs[Random.Range(0, roadSegmentPrefabs.Length)];

        return possiblePrefabs[Random.Range(0, possiblePrefabs.Count)];
    }

    private void RememberPrefab(GameObject prefab)
    {
        recentlyUsedPrefabs.Enqueue(prefab);

        while (recentlyUsedPrefabs.Count > avoidRepeatForSpawns)
            recentlyUsedPrefabs.Dequeue();
    }

    private void RemoveOldSegments()
    {
        for (int i = spawnedSegments.Count - 1; i >= 0; i--)
        {
            GameObject segment = spawnedSegments[i];

            if (segment == null)
            {
                spawnedSegments.RemoveAt(i);
                continue;
            }

            bool isBehindPlayer = segment.transform.position.z + segmentLength < -despawnBehindDistance;

            if (!isBehindPlayer)
                continue;

            spawnedSegments.RemoveAt(i);
            Destroy(segment);
        }
    }

    private float GetLastSegmentZ()
    {
        float lastZ = 0f;

        foreach (GameObject segment in spawnedSegments)
        {
            if (segment == null)
                continue;

            if (segment.transform.position.z > lastZ)
                lastZ = segment.transform.position.z;
        }

        return lastZ;
    }
}