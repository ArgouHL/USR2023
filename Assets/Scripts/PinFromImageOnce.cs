using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class TrackResultPrefabAnchor : MonoBehaviour
{
    [SerializeField] ARTrackedImageManager trackedImageManager;
    [SerializeField] ARAnchorManager anchorManager;
    [SerializeField] GameObject contentPrefab;
    private readonly Dictionary<string, ARAnchor> _anchors = new();

    bool _pinned;
    ARAnchor _anchor;
    GameObject _content;

    private void OnEnable()
    {
        trackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    private void OnDisable()
    {
        trackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
    }
    async void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        // 只要第一次偵測到一張圖就「釘住」，之後就不再處理
        if (_pinned) return;

        foreach (var img in args.added)
            await TryPin(img);

        // 有些裝置先進 added、後續才變成 Tracking；保守起見也處理 updated
        foreach (var img in args.updated)
            await TryPin(img);
    }

    async Task TryPin(ARTrackedImage img)
    {
        // 以 ReferenceImageName 當 key（也可改用 trackableId.ToString()）
        string key = img.referenceImage.name;
        if (_anchors.ContainsKey(key))
            return; // 已生成過就不再生成

        // 確保這張影像正在穩定追蹤
        if (img.trackingState != TrackingState.Tracking)
            return;

        // 建立世界錨點
        Pose pose = new Pose(img.transform.position, img.transform.rotation);
        var res = await anchorManager.TryAddAnchorAsync(pose);

        if (!res.status.IsSuccess())
        {
            Debug.LogWarning($"建立錨點失敗: {key}");
            return;
        }

        ARAnchor anchor = res.value;
        _anchors[key] = anchor; // 記錄起來

        // 生成內容並掛到錨點底下
        var go = Instantiate(contentPrefab, pose.position, pose.rotation);
        go.transform.SetParent(anchor.transform, worldPositionStays: true);

        //（可選）稍等一下再微調一次錨點
        await Task.Delay(250);
        if (img.trackingState == TrackingState.Tracking)
            anchor.transform.SetPositionAndRotation(img.transform.position, img.transform.rotation);
    }

    public void ClearPinned()
    {
        if (_content) Destroy(_content);
        if (_anchor) Destroy(_anchor.gameObject);
        _content = null;
        _anchor = null;
        _pinned = false;
    }
}