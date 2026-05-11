using UnityEngine;

public class CollectableItem : MonoBehaviour
{
    private bool wasCollected;

    public bool WasCollected => wasCollected;

    public void MarkAsCollected()
    {
        if (wasCollected)
            return;

        wasCollected = true;
    }

    public void DestroyItem()
    {
        Destroy(gameObject);
    }
}