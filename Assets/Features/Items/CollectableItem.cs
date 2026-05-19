using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private ItemType itemType;
    [SerializeField] private float widthInLanes = 1f;
    [SerializeField] private float heightInLanes = 1f;
    [SerializeField] private float mass = 2f;
    [SerializeField] private GameObject stackedPrefab;

    public ItemType ItemType => itemType;
    public float WidthInLanes => widthInLanes;
    public float HeightInLanes => heightInLanes;
    public float Mass => mass;
    public GameObject StackedPrefab => stackedPrefab;
    public bool WasCollected { get; private set; }

    public void MarkAsCollected()
    {
        WasCollected = true;
    }

    public void DestroyItem()
    {
        Destroy(gameObject);
    }
}