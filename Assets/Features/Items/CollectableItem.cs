using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    [Header("Item Data")]
    [SerializeField] private ItemType itemType;
    [SerializeField] private int widthInLanes = 1;
    [SerializeField] private int heightInLanes = 1;
    [SerializeField] private float mass = 2f;
    [SerializeField] private GameObject stackedPrefab;

    public ItemType ItemType => itemType;
    public int WidthInLanes => widthInLanes;
    public int HeightInLanes => heightInLanes;
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