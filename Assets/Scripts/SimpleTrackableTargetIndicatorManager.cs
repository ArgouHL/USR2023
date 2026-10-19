using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SimpleTrackableTargetIndicatorManager : MonoBehaviour
{
    [Header("UI 設定")]
    [SerializeField] private GameObject _indicatorPrefab; 
    [SerializeField] private RectTransform _indicatorsContainer; 
    [SerializeField] private float _screenMargin = 50f; // 邊緣留白
    [SerializeField] private GameObject _UIRoot;

    [Header("相機")]
    [SerializeField] private Camera _trackingCamera;

    [Serializable]
    private class CameraIDPair
    {
        public string ID;
        public Camera Camera;
    }
    [SerializeField] List<CameraIDPair> TrackingCameraList; // 如有需要多部攝影機之間切換

    // 目標物(Transform) 與 UI指標(RectTransform) 的對照表
    private Dictionary<Transform, RectTransform> _targetIndicators = new Dictionary<Transform, RectTransform>();

    public void Initialize()
    {
        if (_trackingCamera == null) _trackingCamera = Camera.main;

        ToggleActive(false);
    }

    public void ToggleActive(bool isActive)
    {
        this.enabled = isActive;
        _UIRoot.SetActive(isActive);
    }

    public void RegisterTarget(Transform target)
    {
        if (!_targetIndicators.ContainsKey(target)) {
            GameObject uiObj = Instantiate(_indicatorPrefab, _indicatorsContainer);
            uiObj.SetActive(true);
            _targetIndicators.Add(target, uiObj.GetComponent<RectTransform>());
        }
    }

    public void UnregisterTarget(Transform target)
    {
        if (_targetIndicators.ContainsKey(target)) {
            Destroy(_targetIndicators[target].gameObject);
            _targetIndicators.Remove(target);
        }
    }

    private void LateUpdate()
    {
        // 畫布中心點
        Vector3 screenCenter = new Vector3(Screen.width / 2f, Screen.height / 2f, 0f);
        // 邊界範圍 (從中心到邊緣的距離)
        Vector3 bounds = screenCenter - new Vector3(_screenMargin, _screenMargin, 0f);

        foreach (var pair in _targetIndicators) {
            Transform target = pair.Key;
            RectTransform indicatorUI = pair.Value;

            if (target == null) continue;

            // 1. 世界座標 -> 螢幕座標
            Vector3 screenPos = _trackingCamera.WorldToScreenPoint(target.position);

            // 2. 處理「相機背後」的情況
            bool isBehind = screenPos.z < 0;
            if (isBehind) {
                screenPos -= screenCenter;
                screenPos *= -1;
                screenPos += screenCenter;
            }

            // UI 是 2D 的，計算方向時不能讓 Z 軸參與，否則 dir 會指向螢幕深處
            screenPos.z = 0f;

            // 3. 判斷是否在螢幕範圍內
            bool onScreen = !isBehind &&
                            screenPos.x > _screenMargin &&
                            screenPos.x < Screen.width - _screenMargin &&
                            screenPos.y > _screenMargin &&
                            screenPos.y < Screen.height - _screenMargin;

            if (onScreen) {
                // A. 在範圍內
                indicatorUI.position = screenPos;
                indicatorUI.rotation = Quaternion.identity;

                // DebugMessageManager.Instance.AddMessage($"Indicator for {target.name}, Pos: {indicatorUI.position}, On Screen");
            }
            else {
                // B. 在範圍外：限制在邊框

                // 計算從中心指向目標的向量
                Vector3 dir = screenPos - screenCenter;

                // 縮放因子法
                // "如果要碰到 X 邊界，需要把向量縮放多少倍？" (tX)
                // "如果要碰到 Y 邊界，需要把向量縮放多少倍？" (tY)
                // 取最小的倍數，就是先撞到的牆。

                float tX = (dir.x != 0) ? Mathf.Abs(bounds.x / dir.x) : float.MaxValue;
                float tY = (dir.y != 0) ? Mathf.Abs(bounds.y / dir.y) : float.MaxValue;

                // 取最小縮放比例
                float t = Mathf.Min(tX, tY);

                // 應用縮放，算出最終座標
                screenPos = screenCenter + dir * t;

                // 更新位置 (這裡的 screenPos.z 已經保證是 0 了)
                indicatorUI.position = screenPos;

                // 更新旋轉
                float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
                indicatorUI.rotation = Quaternion.Euler(0, 0, angle - 90);

                // DebugMessageManager.Instance.AddMessage($"Indicator for {target.name}, Pos: {indicatorUI.position}, Angle: {angle - 90}, OffScreen");
            }
        }
    }

    public void SwitchTrackingCameraWithID(string ID)
    {
        var cameraPair = TrackingCameraList.Find(x => x.ID == ID);
        if ( cameraPair == null ){
            DebugMessageManager.Instance.AddMessage($"Failed to switch tracking camera with id {ID}", true);
            Debug.LogWarning($"Failed to switch tracking camera with id {ID}");
            return;
        }

        _trackingCamera = cameraPair.Camera;
    }
}