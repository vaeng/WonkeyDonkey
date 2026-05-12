using UnityEngine;

/// <summary>
/// Spawns collectable items ahead of the stationary player.
/// </summary>
public class ItemSpawner : MonoBehaviour
{
    /// <summary>Used for lane position calculations.</summary>
    [Header("References")]
    [SerializeField] private LaneSystem laneSystem;

    /// <summary>Prefabs of items to spawn.</summary>
    [Header("Item Prefabs")]
    [SerializeField] private GameObject[] itemPrefabs;

    /// <summary>Time in seconds between spawns.</summary>
    [Header("Spawn Timing")]
    [SerializeField] private float spawnInterval = 1.5f;

    /// <summary>Distance in front of the player where items spawn.</summary>
    [Header("Spawn Position")]
    [SerializeField] private float spawnZPosition = 25f;

    /// <summary>Y position for spawned items.</summary>
    [SerializeField] private float itemYPosition = 0.5f;

    /// <summary>Movement speed of spawned world items.</summary>
    [SerializeField] private float worldMoveSpeed = 6f;

    /// <summary>Min lane index to spawn items in.</summary>
    [Header("Lane Range")]
    [SerializeField] private int minSpawnLane = 0;

    /// <summary>Max lane index to spawn items in.</summary>
    [SerializeField] private int maxSpawnLane = 6;

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer < spawnInterval)
            return;

        SpawnRandomItem();
        timer = 0f;
    }

    private void SpawnRandomItem()
    {
        if (itemPrefabs.Length == 0)
        {
            Debug.LogWarning("No item prefabs assigned to ItemSpawner.");
            return;
        }

        int minLane = Mathf.Clamp(
            minSpawnLane,
            Mathf.RoundToInt(laneSystem.MinRoadLane),
            Mathf.RoundToInt(laneSystem.MaxRoadLane)
        );

        int maxLane = Mathf.Clamp(
            maxSpawnLane,
            Mathf.RoundToInt(laneSystem.MinRoadLane),
            Mathf.RoundToInt(laneSystem.MaxRoadLane)
        );

        if (minLane > maxLane)
        {
            Debug.LogWarning("ItemSpawner has invalid lane range.");
            return;
        }

        int randomLane = Random.Range(minLane, maxLane + 1);

        GameObject randomPrefab =
            itemPrefabs[Random.Range(0, itemPrefabs.Length)];

        Vector3 spawnPosition = new Vector3(
            laneSystem.GetWorldXForLanePosition(randomLane),
            itemYPosition,
            spawnZPosition
        );

        GameObject spawnedItem = Instantiate(
            randomPrefab,
            spawnPosition,
            Quaternion.identity
        );

        WorldMover worldMover = spawnedItem.GetComponent<WorldMover>();

        if (worldMover == null)
            worldMover = spawnedItem.AddComponent<WorldMover>();

        worldMover.SetMoveSpeed(worldMoveSpeed);
    }
}