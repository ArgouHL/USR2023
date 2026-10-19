using System;
using UnityEngine;

public class ARGazeInteractionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private CameraGazeToTargetDetector _cameraGazeDetector;
    [SerializeField] private ARGazeUIController _gazeUIController;

    [Header("Settings")]
    [SerializeField]
    private float _defaultActivationDuration = 3f;

    public event Action<string, ARGazeTarget> OnTargetTriggered;

    // 內部狀態
    private ARGazeTarget _currentTarget;
    private float _timer;
    private bool _isCounting;

    public void Initialize()
    {
        // 初始化
        _gazeUIController.Initialize();

        // 訂閱偵測器事件
        _cameraGazeDetector.OnTargetChanged += OnTargetChanged;
    }

    private void OnDestroy()
    {
        if (_cameraGazeDetector != null) {
            _cameraGazeDetector.OnTargetChanged -= OnTargetChanged;
        }
    }

    public void ToggleGazeInteraction(bool isEnable)
    {
        _cameraGazeDetector.ToggleActive(isEnable);
        _gazeUIController.ToggleDisplay(isEnable);

        if (!isEnable) {
            // 停用時，重置狀態
            OnTargetChanged(null);
        }
    }

    private void OnTargetChanged(ARGazeTarget newTarget)
    {
        // 1. 處理舊目標的離開邏輯
        if (_currentTarget != null) {
            _currentTarget.OnGazeExit?.Invoke();
        }

        // 2. 更新當前目標
        _currentTarget = newTarget;
        _timer = 0f;

        _gazeUIController.ResetProgress();

        // 3. 處理新目標的進入邏輯
        if (_currentTarget != null) {
            _isCounting = true;
            _currentTarget.OnGazeEnter?.Invoke();
        }
        else {
            // 沒看著東西 -> 停止計時
            _isCounting = false;
        }
    }

    private void Update()
    {
        if (!_isCounting || _currentTarget == null) return;

        float activationDuration = _defaultActivationDuration;
        if ( _currentTarget.UsingCustomActivationDuration ) {
            activationDuration = _currentTarget.ActivationDuration;
        }

        // 累加時間
        _timer += Time.deltaTime;
        // 計算進度 (0 ~ 1)
        float progress = Mathf.Clamp01(_timer / activationDuration);
        // 更新 UI
        _gazeUIController.SetProgress(progress);

        // 檢查是否達成觸發條件
        if (_timer >= activationDuration) {
            PerformTrigger();
        }
    }

    private void PerformTrigger()
    {
        _isCounting = false;

        DebugMessageManager.Instance.AddMessage($"Gaze Triggered: {_currentTarget.name}");

        if (_currentTarget != null) {
            DebugMessageManager.Instance.AddMessage($"[GazeSystem] Trigger ID: {_currentTarget.TargetID}");
            OnTargetTriggered?.Invoke(_currentTarget.TargetID, _currentTarget);
        }

        _gazeUIController.ResetProgress();
    }
}
