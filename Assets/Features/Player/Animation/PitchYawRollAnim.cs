using UnityEngine;

public class PitchYawRollAnim : Animateable
{
    [SerializeField] private Vector3 axesMultiplyer;

    public override void Animate(float time)
    {
        base.Animate(time);
        
        Vector3 rotation = transform.localEulerAngles;
        rotation.x = current * axesMultiplyer.x;
        rotation.y = current * axesMultiplyer.y;
        rotation.z = current * axesMultiplyer.z;

        foreach(Transform t in AnimateableTransforms)
        {
            t.localEulerAngles = rotation;
        }
    }
}
