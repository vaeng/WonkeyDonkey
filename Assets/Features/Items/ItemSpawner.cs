using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private PlayerMovement playerMovement;

    [Header("Item Prefabs")]
    [SerializeField] private GameObject[] itemPrefabs;

    [Header("Spawn Timing")]
    [SerializeField] private float spawnInterval = 1.5f;

    [Header("Spawn Position")]
    [SerializeField] private float spawnDistanceAhead = 25f;
    [SerializeField] private float itemYPosition = 0.5f;

    [Header("Lane Positions")]
    [SerializeField] private float[] spawnLanePositions = { 2f, 3f, 4f };

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnInterval)
        {
            SpawnRandomItem();
            timer = 0f;
        }
    }

    private void SpawnRandomItem()
    {
        if (itemPrefabs.Length == 0)
            return;

        float randomLanePosition = spawnLanePositions[
            Random.Range(0, spawnLanePositions.Length)
        ];

        GameObject randomPrefab = itemPrefabs[
            Random.Range(0, itemPrefabs.Length)
        ];

        Vector3 spawnPosition = new Vector3(
            playerMovement.GetWorldXForLanePosition(randomLanePosition),
            itemYPosition,
            player.position.z + spawnDistanceAhead
        );

        Instantiate(randomPrefab, spawnPosition, Quaternion.identity);
    }
}