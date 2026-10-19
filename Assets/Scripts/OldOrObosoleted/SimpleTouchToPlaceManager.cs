using UnityEngine;
using UnityEngine.XR.ARFoundation;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using UnityEngine.UI;

public class SimpleTouchToPlaceManager : MonoBehaviour
{
    [SerializeField]
    private ARRaycastManager _arRaycastManager;
    [SerializeField]
    private ARPlaneManager _arPlaneManager;
    [SerializeField]
    private ARAnchorManager _arAnchorManager;

    [SerializeField] 
    private GraphicRaycaster _graphicRaycaster;
    [SerializeField] 
    private EventSystem _eventSystem;

    [SerializeField]
    private InputActionReference _touchActionRef, _touchPositionActionRef;
    [SerializeField]
    private GameObject _placedObjPrefab;

    [SerializeField]
    private float _prefabScaleModifier = 0.1f;

    [SerializeField]
    private List<GameObject> _placedObjectIndexList = new List<GameObject>();
    private List<GameObject> _placedObjects = new List<GameObject>();

    private void OnEnable()
    {
        _touchActionRef.action.performed += OnTouch;
    }

    private void OnDisable()
    {
        _touchActionRef.action.performed -= OnTouch;
    }

    private void OnTouch(InputAction.CallbackContext context)
    {
        var position = _touchPositionActionRef.action.ReadValue<Vector2>();

        if (IsTouchOverUI(position)) {
            return;
        }

        var _hits = new List<ARRaycastHit>();
        if (_arRaycastManager.Raycast(position, _hits, UnityEngine.XR.ARSubsystems.TrackableType.Planes)) {
            var hit = _hits[0];

            // 在 hit 位置建立 Anchor
            // 與平面綁定的 Anchor 使用 AttachAnchor
            ARAnchor anchor = _arAnchorManager.AttachAnchor(
                _arPlaneManager.GetPlane(hit.trackableId),hit.pose);

            if (anchor != null) {
                // 生成物件並掛在 Anchor 下

                if (!_placedObjPrefab) {
                    DebugMessageManager.Instance.AddMessage("Prefab is none.");
                    return;
                }

                var placedObj = Instantiate(_placedObjPrefab, anchor.transform);
                placedObj.transform.localPosition = Vector3.zero;
                placedObj.transform.localRotation = Quaternion.identity;

                // 讓物件只以y軸轉180度
                placedObj.transform.Rotate(0, 180, 0);

                placedObj.transform.localScale *= _prefabScaleModifier;
                _placedObjects.Add(placedObj);
            }
        }
    }

    private bool IsTouchOverUI(Vector2 position)
    {
        var eventData = new PointerEventData(_eventSystem)
        {
            position = position
        };

        var results = new List<RaycastResult>();
        _graphicRaycaster.Raycast(eventData, results);

        return results.Count > 0;
    }

    public void SetScaleModifier(string modifier)
    {
        _prefabScaleModifier = float.Parse(modifier);
        DebugMessageManager.Instance.AddMessage("Set Scale Modifier: " + _prefabScaleModifier);
    }

    public void SetPlaceObjPrefabByIndex(int index)
    {
        _placedObjPrefab = _placedObjectIndexList[index];
        DebugMessageManager.Instance.AddMessage("Set Place Obj Prefab: " + _placedObjPrefab.name);
    }

    public void ClearAllPlacedObjects()
    {
        foreach (var obj in _placedObjects) {
            Destroy(obj);
        }
        _placedObjects.Clear();
        DebugMessageManager.Instance.AddMessage("Clear All Placed Objects");
    }
}

