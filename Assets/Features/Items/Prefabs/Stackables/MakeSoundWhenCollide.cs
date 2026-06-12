using UnityEngine;
using FMODUnity;

public class MakeSoundWhenCollide : MonoBehaviour
{
    [SerializeField] private EventReference soundEvent;
    [SerializeField] private float minCollisionForce = 1f, coolDown = 0.2f;
    private bool hasCollided = false;

    private float lastSoundTime;

    private void OnCollisionEnter(Collision collision)
    {
        if (collision == null) return;
        
        float force = collision.relativeVelocity.magnitude;
        float timeSinceLastSound = Time.time - lastSoundTime;
        
        if (hasCollided = false || force >= minCollisionForce && timeSinceLastSound >= coolDown)
        {
            RuntimeManager.PlayOneShot(soundEvent, transform.position);
            lastSoundTime = Time.time;
            hasCollided = true;
        }
    }
}
