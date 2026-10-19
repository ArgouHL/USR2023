using System.Collections;
using UnityEngine;
using UnityEngine.Splines;

public class BoatControl : MonoBehaviour
{
    [SerializeField]Animator boatManAnimator;
    [SerializeField] SplineAnimate splineAnimate ;
    public float targetMaxSpeed = 5f;
    public float accelerationRate = 2f;

    internal void Move()
    {
        splineAnimate.MaxSpeed = 0.001f;
        boatManAnimator.Play("Move");
        splineAnimate.Restart(true);
        StartCoroutine(AccelerateBoat());
    }

    private IEnumerator AccelerateBoat()
    {
  
        while (splineAnimate.MaxSpeed < targetMaxSpeed)
        {
            float currentProgress = splineAnimate.NormalizedTime;


            splineAnimate.MaxSpeed += accelerationRate * Time.deltaTime;

            splineAnimate.NormalizedTime = currentProgress;
         
            yield return null;
        }
        splineAnimate.MaxSpeed = targetMaxSpeed;
    }

private void Start()
    {
        splineAnimate.MaxSpeed = 0.001f;
        boatManAnimator.Play("Idle",0,Random.value);
        splineAnimate.Restart(false);
    }
}
