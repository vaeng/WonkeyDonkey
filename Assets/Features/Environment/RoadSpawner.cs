using System.Collections.Generic;
using UnityEngine;

public class RoadSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LaneMovement laneMovement;

    [Header("Road")]
    [SerializeField] private GameObject[] roadSegmentPrefabs;
    [SerializeField] private GameObject finishSegmentPrefab;
    [SerializeField] private float segmentLength = 20f;
    [SerializeField] private int startSegmentCount = 5;

    [Header("Level")]
    [SerializeField] private float levelDuration = 30f;
    [SerializeField] private float finishLineZ = 0f;

    [Header("Variation")]
    [SerializeField] private int avoidRepeatForSpawns = 3;

    [Header("Movement")]
    [SerializeField] private float worldMoveSpeed = 6f;

    [Header("Spawn / Despawn")]
    [SerializeField] private float spawnAheadDistance = 80f;
    [SerializeField] private float despawnBehindDistance = 30f;

    private readonly List<GameObject> segments = new();
    private readonly Queue<GameObject> recentlyUsed = new();

    private GameObject finishSegment;
    private float finishSegmentStartZ;
    private bool finishSegmentSpawned;

    public float LevelDuration => levelDuration;

    public bool HasReachedFinish
    {
        get
        {
            if (finishSegment == null)
                return false;

            return finishSegment.transform.position.z <= finishLineZ;
        }
    }

    private void Start()
    {
        finishSegmentStartZ = CalculateFinishSegmentZ();

        for (int i = 0; i < startSegmentCount; i++)
            SpawnRoadSegment(i * segmentLength);
    }

    private void Update()
    {
        RemoveOldSegments();

        while (GetLastSegmentZ() < spawnAheadDistance)
        {
            float nextZ = GetLastSegmentZ() + segmentLength;

            if (!finishSegmentSpawned && nextZ >= finishSegmentStartZ)
                SpawnFinishSegment();
            else
                SpawnRoadSegment(nextZ);
        }
    }

    private float CalculateFinishSegmentZ()
    {
        float distance = levelDuration * worldMoveSpeed;
        float roundedDistance = Mathf.Round(distance / segmentLength) * segmentLength;
        float firstPossibleFinish = startSegmentCount * segmentLength;

        return Mathf.Max(roundedDistance, firstPossibleFinish);
    }

    private void SpawnRoadSegment(float z)
    {
        if (laneMovement == null || roadSegmentPrefabs == null || roadSegmentPrefabs.Length == 0)
            return;

        SpawnSegment(PickRoadPrefab(), z);
    }

    private void SpawnFinishSegment()
    {
        if (laneMovement == null || finishSegmentPrefab == null)
            return;

        finishSegment = SpawnSegment(finishSegmentPrefab, finishSegmentStartZ);
        finishSegmentSpawned = true;
    }

    private GameObject SpawnSegment(GameObject prefab, float z)
    {
        float roadX = 0f;
        float visualX = laneMovement.GetVisualWorldXForRoadX(roadX);

        GameObject segment = Instantiate(prefab, new Vector3(visualX, 0f, z), Quaternion.identity);

        WorldMover mover = segment.GetComponent<WorldMover>();

        if (mover == null)
            mover = segment.AddComponent<WorldMover>();

        mover.Initialize(laneMovement, worldMoveSpeed, roadX);

        segments.Add(segment);
        return segment;
    }

    private GameObject PickRoadPrefab()
    {
        List<GameObject> choices = new();

        foreach (GameObject prefab in roadSegmentPrefabs)
        {
            if (prefab != null && !recentlyUsed.Contains(prefab))
                choices.Add(prefab);
        }

        GameObject picked;

        if (choices.Count > 0)
            picked = choices[Random.Range(0, choices.Count)];
        else
            picked = roadSegmentPrefabs[Random.Range(0, roadSegmentPrefabs.Length)];

        recentlyUsed.Enqueue(picked);

        while (recentlyUsed.Count > avoidRepeatForSpawns)
            recentlyUsed.Dequeue();

        return picked;
    }

    private void RemoveOldSegments()
    {
        for (int i = segments.Count - 1; i >= 0; i--)
        {
            GameObject segment = segments[i];

            if (segment == null)
            {
                segments.RemoveAt(i);
                continue;
            }

            if (segment.transform.position.z + segmentLength > -despawnBehindDistance)
                continue;

            segments.RemoveAt(i);
            Destroy(segment);
        }
    }

    private float GetLastSegmentZ()
    {
        float lastZ = 0f;

        foreach (GameObject segment in segments)
        {
            if (segment != null && segment.transform.position.z > lastZ)
                lastZ = segment.transform.position.z;
        }

        return lastZ;
    }

    public float GetFinishSegmentZ()
    {
        if (finishSegment == null)
            return float.MaxValue;

        return finishSegment.transform.position.z;
    }
}