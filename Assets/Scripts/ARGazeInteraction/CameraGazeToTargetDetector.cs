using System;
using UnityEngine;
using UnityEngine.Events;

public class CameraGazeToTargetDetector : MonoBehaviour
{
    [SerializeField]
    private Transform _cameraTrans; // 相機Transform
    [SerializeField]
    private float _maxDistance = 100f;    // 射線最大距離

    private bool _isActive = false; // 是否啟用此檢測器

    public ARGazeTarget CurrentGazeTarget { get; private set; }

    public event Action<ARGazeTarget> OnTargetChanged;

    public void Initialize()
    {
        ToggleActive(false);
    }

    public void ToggleActive(bool active)
    {
        _isActive = active;

        if (!_isActive && CurrentGazeTarget != null) {
            // 如果被強制關閉，要視為離開目標
            HandleTargetChange(null);
        }
    }

    void Update()
    {
        if (!_isActive) return;

        ARGazeTarget foundTarget = null;

        Ray ray = new Ray(_cameraTrans.position, _cameraTrans.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, _maxDistance)) {
            // 嘗試獲取目標組件
            ARGazeTarget target = hit.collider.GetComponent<ARGazeTarget>();

            // 確認目標存在且處於可互動狀態
            if (target != null && target.IsInteractable) {
                foundTarget = target;
            }
        }

        // 如果目標發生變化 (從 A 變 B，或是從 A 變 Null，或是從 Null 變 A)
        if (foundTarget != CurrentGazeTarget) {
            HandleTargetChange(foundTarget);
        }
    }

    private void HandleTargetChange(ARGazeTarget newTarget)
    {
        CurrentGazeTarget = newTarget;
        OnTargetChanged?.Invoke(CurrentGazeTarget);
    }
}
