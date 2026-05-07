using System.Collections.Generic;
using UnityEngine;

public class RoadSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject roadSegmentPrefab;

    [Header("Road Settings")]
    [SerializeField] private float segmentLength = 20f;
    [SerializeField] private int initialSegments = 5;

    [Header("Spawn / Despawn")]
    [SerializeField] private float spawnAheadDistance = 80f;
    [SerializeField] private float despawnBehindDistance = 30f;

    private readonly Queue<GameObject> activeSegments = new Queue<GameObject>();
    private float nextSpawnZ;

    private void Start()
    {
        SpawnInitialSegments();
    }

    private void Update()
    {
        SpawnSegmentsAhead();
        DespawnSegmentsBehind();
    }

    private void SpawnInitialSegments()
    {
        nextSpawnZ = 0f;

        for (int i = 0; i < initialSegments; i++)
        {
            SpawnSegment();
        }
    }

    private void SpawnSegmentsAhead()
    {
        while (nextSpawnZ < player.position.z + spawnAheadDistance)
        {
            SpawnSegment();
        }
    }

    private void SpawnSegment()
    {
        Vector3 spawnPosition = new Vector3(0f, 0f, nextSpawnZ);

        GameObject newSegment = Instantiate(
            roadSegmentPrefab,
            spawnPosition,
            Quaternion.identity
        );

        activeSegments.Enqueue(newSegment);

        nextSpawnZ += segmentLength;
    }

    private void DespawnSegmentsBehind()
    {
        while (activeSegments.Count > 0)
        {
            GameObject oldestSegment = activeSegments.Peek();

            float segmentEndZ = oldestSegment.transform.position.z + segmentLength;

            bool isBehindPlayer = segmentEndZ < player.position.z - despawnBehindDistance;

            if (!isBehindPlayer)
                break;

            activeSegments.Dequeue();
            Destroy(oldestSegment);
        }
    }
}