using UnityEngine;

public class WheelDustEffect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ParticleSystem dustPrefab;
    [SerializeField] private Transform[] dustPoints;

    [Header("Settings")]
    [SerializeField] private float effectCooldown = 0.12f;

    private float lastEffectTime;

    public void PlayDust()
    {
        if (Time.time < lastEffectTime + effectCooldown)
            return;

        lastEffectTime = Time.time;

        for (int i = 0; i < dustPoints.Length; i++)
        {
            if (dustPoints[i] == null)
                continue;

            ParticleSystem dust = Instantiate(
                dustPrefab,
                dustPoints[i].position,
                dustPoints[i].rotation
            );

            dust.Play();

            Destroy(dust.gameObject, dust.main.duration + dust.main.startLifetime.constantMax);
        }
    }
}