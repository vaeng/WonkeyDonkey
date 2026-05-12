using UnityEngine;

public class AnimationSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Animateable donkey, carrotStick, rider, riderHat, waggon, wheel;

    public void StartAnimation(float goal)
    {
        donkey?.SetGoal(goal);
        carrotStick?.SetGoal(goal);
        rider?.SetGoal(goal);
        riderHat?.SetGoal(goal);
        waggon?.SetGoal(goal);
        wheel?.SetGoal(goal);
    }

    public void Animate(float time)
    {
        donkey?.Animate(time);
        carrotStick?.Animate(time);
        rider?.Animate(time);
        riderHat?.Animate(time);
        waggon?.Animate(time);
        wheel?.Animate(time);
    }

    void Update()
    {
        Animate(Time.deltaTime);
    }
}
