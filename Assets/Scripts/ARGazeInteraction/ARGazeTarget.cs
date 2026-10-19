using UnityEngine;
using UnityEngine.Events;

public class ARGazeTarget : MonoBehaviour
{
    [SerializeField] private string _targetID;
    public string TargetID => _targetID;

    [Header("設定")]
    [SerializeField]
    private bool _usingCustomActivationDuration = false;
    public bool UsingCustomActivationDuration => _usingCustomActivationDuration;
    [Tooltip("需要注視多久才能觸發 (秒)")]
    [SerializeField] private float _activationDuration = 1.0f;
    public float ActivationDuration => _activationDuration;
    [Tooltip("是否可被互動")]
    [SerializeField] private bool _isInteractable = true;
    public bool IsInteractable => _isInteractable;

    [Header("事件")]
    [Tooltip("當注視開始時觸發")]
    public UnityEvent OnGazeEnter;
    [Tooltip("當注視離開時觸發")]
    public UnityEvent OnGazeExit;

    [Tooltip("當注視時間達成，觸發的主要事件")]
    public UnityEvent OnGazeTriggered;

    public void Initialize()
    {
        _isInteractable = false;
    }

    public void ToggleInteractable(bool isInteractable)
    {
        _isInteractable = isInteractable;
    }
}
