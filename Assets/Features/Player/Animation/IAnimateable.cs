using UnityEngine;

public interface IAnimateable
{
    /// <summary>
    /// Called by the AnimationSystem to trigger the animation.
    /// </summary>

    public void Animate(float time);
    public void SetGoal(float goal);
}
