using System;
using UnityEngine;
using UnityEngine.InputSystem.EnhancedTouch; // 必須引用
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch; // 指定使用 EnhancedTouch 的 Touch 類別

public class SimplePinchDetector : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("靈敏度：數值越大，變數變化越快")]
    [SerializeField] private float _sensitivity = 0.01f;
    [Tooltip("防手震門檻：兩指距離變化超過多少像素才視為有效操作 (建議 1.0 ~ 5.0)")]
    [SerializeField] private float _deadZone = 2.0f;

    private bool _isDetecting = false;

    private float _zoomDelta;
    public float ZoomDelta => _zoomDelta;

    public event Action<float> OnZoomDeltaChanged;

    public void ToggleDetecting(bool isDetecting)
    {
        _isDetecting = isDetecting;

        if (_isDetecting) {
            // 啟用 Enhanced Touch 支援
            EnhancedTouchSupport.Enable();
        }
        else {
            // 停用 Enhanced Touch 支援
            EnhancedTouchSupport.Disable();
        }
    }

    void Update()
    {
        if (!_isDetecting) return;

        // 每一幀重置，確保沒操作時歸零
        _zoomDelta = 0f;

        // 只有當螢幕上正好有兩隻手指時才運算
        if (Touch.activeTouches.Count == 2) {
            // 取得兩隻手指的觸控資訊
            var touch0 = Touch.activeTouches[0];
            var touch1 = Touch.activeTouches[1];

            // 計算兩指「上一幀」的位置
            // 當前位置 - 移動量 = 上一幀位置
            Vector2 touch0Pos = touch0.screenPosition;
            Vector2 touch1Pos = touch1.screenPosition;

            // 計算「上一幀」兩指的距離長度
            Vector2 touch0PrevPos = touch0Pos - touch0.delta;
            Vector2 touch1PrevPos = touch1Pos - touch1.delta;

            // 計算「當前」兩指的距離長度
            float prevDist = (touch0PrevPos - touch1PrevPos).magnitude;
            float currDist = (touch0Pos - touch1Pos).magnitude;

            // 核心公式：當前距離 - 上一幀距離
            // 如果結果 > 0，代表手指在張開 (距離變大)
            // 如果結果 < 0，代表手指在捏合 (距離變小)
            float rawDelta = currDist - prevDist;

            // 只有當變化的絕對值大於門檻時，才輸出數值
            if (Mathf.Abs(rawDelta) > _deadZone) {
                // 為了讓手感更平滑，可以選擇扣除掉 deadZone 的部分，
                // 或者是直接輸出。這邊為了簡單直接輸出即可，
                // 因為手指一動通常就會超過這個值。

                _zoomDelta = rawDelta * _sensitivity;
                OnZoomDeltaChanged?.Invoke(_zoomDelta);

                DebugMessageManager.Instance.AddMessage($"Pinch Zoom Delta: {_zoomDelta:F4}");
            }
            else {
                // 未超過門檻，視為雜訊，歸零
                _zoomDelta = 0f;
            }
        }
    }
}
