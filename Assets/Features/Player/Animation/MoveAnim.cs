using UnityEngine;

public class MoveAnim : Animateable
{
    [SerializeField] private Vector3 axesMultiplyer;
    public override void Animate(float time)
    {
        base.Animate(time);
        
        Vector3 position = transform.localPosition;
        position.x = current * axesMultiplyer.x;
        position.y = current * axesMultiplyer.y;
        position.z = current * axesMultiplyer.z;
        transform.localPosition = position;
    }
}
