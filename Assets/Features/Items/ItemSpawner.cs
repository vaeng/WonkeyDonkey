using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    /// <summary>Used to determine where to spawn items ahead of the player.</summary>
    [Header("References")]
    [SerializeField] private Transform player;
    /// <summary>Used for lane position calculations.</summary>
    [SerializeField] private LaneSystem laneSystem;

    /// <summary>Prefabs of items to spawn.</summary>
    [Header("Item Prefabs")]
    [SerializeField] private GameObject[] itemPrefabs;

    /// <summary>Time in seconds between spawns.</summary>
    [Header("Spawn Timing")]
    [SerializeField] private float spawnInterval = 1.5f;

    /// <summary>Distance ahead of the player to spawn items.</summary>
    [Header("Spawn Position")]
    [SerializeField] private float spawnDistanceAhead = 25f;
    /// <summary>Y position (height) for spawned items. Adjust based on item prefab heights.</summary>
    [SerializeField] private float itemYPosition = 0.5f;

    /// <summary>Min lane index to spawn items in. Should be greater than or equal to the min lane defined in LaneSystem.</summary>
    [Header("Lane Range")]
    [SerializeField] private int minSpawnLane = 0;
    /// <summary>Max lane index to spawn items in. Should be less than or equal to the max lane defined in LaneSystem.</summary>
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

        GameObject randomPrefab = itemPrefabs[
            Random.Range(0, itemPrefabs.Length)
        ];

        Vector3 spawnPosition = new Vector3(
            laneSystem.GetWorldXForLanePosition(randomLane),
            itemYPosition,
            player.position.z + spawnDistanceAhead
        );

        Instantiate(randomPrefab, spawnPosition, Quaternion.identity);
    }
}