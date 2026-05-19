using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private LaneSystem laneSystem;
    [SerializeField] private LaneMovement laneMovement;

    [Header("Items")]
    [SerializeField] private GameObject[] itemPrefabs;

    [Header("Spawn Timing")]
    [SerializeField] private float spawnInterval = 1.5f;

    [Header("Spawn Position")]
    [SerializeField] private float spawnZPosition = 25f;
    [SerializeField] private float itemYPosition = 0.5f;

    [Header("Movement")]
    [SerializeField] private float worldMoveSpeed = 6f;

    [Header("Lane Range")]
    [SerializeField] private int minSpawnLane = 0;
    [SerializeField] private int maxSpawnLane = 6;

    [Header("Lane Placement")]
    [SerializeField] private bool allowSpawnBetweenLanes = false;

    private float spawnTimer;

    private void Update()
    {
        spawnTimer += Time.deltaTime;

        if (spawnTimer < spawnInterval)
            return;

        SpawnItem();
        spawnTimer = 0f;
    }

    private void SpawnItem()
    {
        if (!CanSpawn())
            return;

        float lanePosition = GetRandomLanePosition();
        GameObject prefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];

        float roadX = laneSystem.GetWorldXForLanePosition(lanePosition);
        float visualX = laneMovement.GetVisualWorldXForRoadX(roadX);

        GameObject item = Instantiate(
            prefab,
            new Vector3(visualX, itemYPosition, spawnZPosition),
            Quaternion.identity
        );

        WorldMover mover = item.GetComponent<WorldMover>();

        if (mover == null)
            mover = item.AddComponent<WorldMover>();

        mover.Initialize(laneMovement, worldMoveSpeed, roadX);
    }

    private bool CanSpawn()
    {
        if (laneSystem == null || laneMovement == null)
        {
            Debug.LogWarning("ItemSpawner: Missing lane system or lane movement.");
            return false;
        }

        if (itemPrefabs == null || itemPrefabs.Length == 0)
        {
            Debug.LogWarning("ItemSpawner: No item prefabs assigned.");
            return false;
        }

        return true;
    }

    private float GetRandomLanePosition()
    {
        int minLane = Mathf.Clamp(minSpawnLane, 0, laneSystem.RoadLaneCount - 1);
        int maxLane = Mathf.Clamp(maxSpawnLane, 0, laneSystem.RoadLaneCount - 1);

        if (minLane > maxLane)
        {
            Debug.LogWarning("ItemSpawner: Invalid lane range. Using center lane.");
            return laneSystem.RoadLaneCount / 2f;
        }

        if (!allowSpawnBetweenLanes)
            return Random.Range(minLane, maxLane + 1);

        int minStep = minLane * 2;
        int maxStep = maxLane * 2;

        int randomStep = Random.Range(minStep, maxStep + 1);

        return randomStep * 0.5f;
    }
}