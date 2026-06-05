using UnityEngine;
using System.Collections.Generic;

public class Animateable : MonoBehaviour, IAnimateable
{
    [SerializeField] bool animateOnStart = false, isLooping = false, ignoreDirection = false;
    [SerializeField] public List<Transform> AnimateableTransforms;

    [SerializeField] float goal, start;
    public float current, currentStart, speed;
    float animationTimer;
    [SerializeField] AnimationCurve animCurve = AnimationCurve.Linear(0, 0, 1, 1);

    void Start()
    {
        if (AnimateableTransforms == null || AnimateableTransforms.Count == 0)
        {
            AnimateableTransforms = new List<Transform> { transform };
        }
    }

    public virtual void Animate(float time)
    {
        animationTimer += time * speed;
        float curveValue = animCurve.Evaluate(animationTimer);
        current = Mathf.Lerp(currentStart, goal, curveValue);
        
        if (animationTimer >= 1f)
        {
            currentStart = start;

            if (isLooping)
            {
                animationTimer = 0f;
            }
        }
    }

    public virtual void SetGoal(float goalValue)
    {   
        currentStart = current;

        goal = ignoreDirection ? Mathf.Abs(goalValue) : goalValue;

        animationTimer = 0f;
    }
}
