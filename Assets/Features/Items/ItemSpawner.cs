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
    [SerializeField] private float spawnIntervalDecreaseRate = 0.05f;
    [SerializeField] private float spawnIntervalMin = 0.5f;


    [Header("Spawn Position")]
    [SerializeField] private float spawnZPosition = 25f;
    [SerializeField] private float itemYPosition = 0.5f;

    [Header("Movement")]
    [SerializeField] private float worldMoveSpeed = 6f;

    [Header("Lane Range")]
    [SerializeField] private int minSpawnLane = 0;
    [SerializeField] private int maxSpawnLane = 6;

    [Header("Lane Placement")]
    [SerializeField] private bool allowSpawnBetweenLanes;

    private float timer;
    private bool canSpawnItems = true;

    public float SpawnZPosition => spawnZPosition;

    private void Update()
    {
        if (!canSpawnItems)
            return;

        timer += Time.deltaTime;

        if (spawnInterval > spawnIntervalMin)
        {
            spawnInterval -= spawnIntervalDecreaseRate * Time.deltaTime;
            spawnInterval = Mathf.Max(spawnInterval, spawnIntervalMin);
        }

        if (timer < spawnInterval)
            return;

        SpawnItem();
        timer = 0f;
    }

    private void SpawnItem()
    {
        if (laneSystem == null || laneMovement == null)
            return;

        if (itemPrefabs == null || itemPrefabs.Length == 0)
            return;

        float lane = GetRandomLane();
        float roadX = laneSystem.GetWorldXForLanePosition(lane);
        float visualX = laneMovement.GetVisualWorldXForRoadX(roadX);

        GameObject prefab = itemPrefabs[Random.Range(0, itemPrefabs.Length)];
        GameObject item = Instantiate(prefab, new Vector3(visualX, itemYPosition, spawnZPosition), Quaternion.identity);

        WorldMover mover = item.GetComponent<WorldMover>();

        if (mover == null)
            mover = item.AddComponent<WorldMover>();

        mover.Initialize(laneMovement, worldMoveSpeed, roadX);
    }

    private float GetRandomLane()
    {
        int minLane = Mathf.Clamp(minSpawnLane, 0, laneSystem.RoadLaneCount - 1);
        int maxLane = Mathf.Clamp(maxSpawnLane, 0, laneSystem.RoadLaneCount - 1);

        if (minLane > maxLane)
            return laneSystem.RoadLaneCount * 0.5f;

        if (!allowSpawnBetweenLanes)
            return Random.Range(minLane, maxLane + 1);

        int minStep = minLane * 2;
        int maxStep = maxLane * 2;

        return Random.Range(minStep, maxStep + 1) * 0.5f;
    }

    public void StopSpawning()
    {
        canSpawnItems = false;
    }
}