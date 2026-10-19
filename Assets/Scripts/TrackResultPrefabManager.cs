using System;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine.XR.ARSubsystems;

public class TrackResultPrefabManager : MonoBehaviour
{
    [SerializeField]
    private ARSession _arSession;
    [SerializeField]
    private ARTrackedImageManager _arTrackedImageManager;
    [SerializeField]
    private ARAnchorManager _arAnchorManager;

    [Serializable]
    public struct TrackablePrefab
    {
        public string trackableName;
        public GameObject prefab;
    }

    [SerializeField]
    private List<TrackablePrefab> _trackablePrefabList = 
        new List<TrackablePrefab>();

    private Dictionary<string, ARAnchor> _anchorByTrackingImageName = 
        new Dictionary<string, ARAnchor>();

    private Dictionary<ARTrackedImage, GameObject> _spawnedPrefabByTrackedImage = 
        new Dictionary<ARTrackedImage, GameObject>();

    public event Action<string, GameObject> OnSpawnedObjectByImageNameEnable, OnSpawnedObjectByImageNameDisable;

    private void Awake()
    {
        if (!_arSession) {

            _arSession = FindFirstObjectByType<ARSession>();
            return;
        }
        _arSession.Reset();
    }

    private void OnEnable()
    {
        _arTrackedImageManager.trackablesChanged.AddListener(OnTrackedImagesChanged);
    }

    private void OnDisable()
    {
        _arTrackedImageManager.trackablesChanged.RemoveListener(OnTrackedImagesChanged);
    }

    private async void OnTrackedImagesChanged(ARTrackablesChangedEventArgs<ARTrackedImage> args)
    {
        foreach (var addedImage in args.added) {

            DebugMessageManager.Instance.AddMessage("Detected Image: " + addedImage.referenceImage.name);

            var prefab = _trackablePrefabList.Find(x => x.trackableName == addedImage.referenceImage.name).prefab;
            if (!prefab) {
                continue;
            }

            // trackingStste �b added �ɶ��I�q�`�O None�A�S��k�����إ� anchor
            var spawnedObject = Instantiate(prefab);
            spawnedObject.transform.SetParent(addedImage.transform, false);
            spawnedObject.transform.localPosition = Vector3.zero;
            spawnedObject.transform.localRotation = Quaternion.identity;
            spawnedObject.SetActive(false);

            _spawnedPrefabByTrackedImage.Add(addedImage, spawnedObject);
        }

        foreach (var updated in args.updated) {
            
            // �ݭn���� Tracking���A�A�~��إ� anchor�A�ӥB�u�ݫإߤ@��
            if ( updated.trackingState == TrackingState.Tracking
                && !_anchorByTrackingImageName.ContainsKey(updated.referenceImage.name)) {

                DebugMessageManager.Instance.AddMessage("Tracking Image: " + updated.referenceImage.name);

                ARAnchor anchor = null;
                anchor = await CreateAnchorAtTrackedImage(updated);

                if (anchor != null) {
                    _anchorByTrackingImageName.Add(updated.referenceImage.name, anchor);
                    var spawnedObject = _spawnedPrefabByTrackedImage[updated];
                    spawnedObject.transform.SetParent(anchor.transform, false);
                    spawnedObject.transform.localPosition = Vector3.zero;
                    spawnedObject.transform.localRotation = Quaternion.identity;
                    spawnedObject.SetActive(true);

                    OnSpawnedObjectByImageNameEnable?.Invoke(updated.referenceImage.name, spawnedObject);

                    DebugMessageManager.Instance.AddMessage("Placed Object on Anchor for " + updated.referenceImage.name);
                }
            }
        }

        foreach (var removed in args.removed) {
            DebugMessageManager.Instance.AddMessage("Removed Image: " + removed.Value.referenceImage.name);
        }
    }

    private async Task<ARAnchor> CreateAnchorAtTrackedImage(ARTrackedImage trackedImage)
    {
        DebugMessageManager.Instance.AddMessage("Creating Anchor for " + trackedImage.referenceImage.name);

        var result = await _arAnchorManager.TryAddAnchorAsync(trackedImage.pose);

        if (result.status.IsSuccess()) {
            var anchor = result.value;
            DebugMessageManager.Instance.AddMessage("Created Anchor for " + trackedImage.referenceImage.name);
            return anchor;
        }
        return null;
    }

    public void ResetAllSpawnedObject() // 由UI按鈕呼叫，清除所有已生成、放置的物件
    {
        foreach (var kvp in _spawnedPrefabByTrackedImage) {

            kvp.Value.transform.SetParent(null);
            kvp.Value.SetActive(false);

            if ( _anchorByTrackingImageName.ContainsKey(kvp.Key.referenceImage.name)) {
                _arAnchorManager.TryRemoveAnchor(_anchorByTrackingImageName[kvp.Key.referenceImage.name]);
            }

            OnSpawnedObjectByImageNameDisable?.Invoke(kvp.Key.referenceImage.name, kvp.Value);
        }
        _anchorByTrackingImageName.Clear();

        DebugMessageManager.Instance.AddMessage("Reset All Spawned Object");
    }
}
