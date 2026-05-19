using System.Collections.Generic;
using UnityEngine;

public class RoadSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LaneMovement laneMovement;

    [Header("Road")]
    [SerializeField] private GameObject roadSegmentPrefab;
    [SerializeField] private float segmentLength = 20f;
    [SerializeField] private int startSegmentCount = 5;

    [Header("Movement")]
    [SerializeField] private float worldMoveSpeed = 6f;

    [Header("Spawn / Despawn")]
    [SerializeField] private float spawnAheadDistance = 80f;
    [SerializeField] private float despawnBehindDistance = 30f;

    private readonly List<GameObject> spawnedSegments = new();

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
        if (laneMovement == null || roadSegmentPrefab == null)
        {
            return;
        }

        float roadX = 0f;
        float visualX = laneMovement.GetVisualWorldXForRoadX(roadX);

        GameObject segment = Instantiate(
            roadSegmentPrefab,
            new Vector3(visualX, 0f, zPosition),
            Quaternion.identity
        );

        WorldMover mover = segment.GetComponent<WorldMover>();

        if (mover == null)
            mover = segment.AddComponent<WorldMover>();

        mover.Initialize(laneMovement, worldMoveSpeed, roadX);

        spawnedSegments.Add(segment);
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