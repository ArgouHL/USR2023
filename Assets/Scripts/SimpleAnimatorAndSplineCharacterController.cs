using UnityEngine;
using UnityEngine.Splines;

public class SimpleAnimatorAndSplineCharacterController : MonoBehaviour
{
    [SerializeField]
    private SplineAnimate _splineAnimator;

    [SerializeField]
    private Animator _animator;
    [SerializeField]
    private string _defaultAnimationClipName;

    private void Start()
    {
        if ( _animator && !string.IsNullOrEmpty(_defaultAnimationClipName) )
        {
            _animator.Play(_defaultAnimationClipName);
        }
    }
}
