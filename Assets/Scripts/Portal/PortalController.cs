using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Rendering.Universal;
using Unity.VisualScripting;
using UnityEngine.Events;
using System.Threading.Tasks;
using System;

public class PortalController : MonoBehaviour
{
    [SerializeField]
    private Transform _outsidePortalTransformRef, // 傳送門外面的參考位置
        _insidePortalTransformRef; // 裡面的參考

    public UnityEvent onEnterPortal,
        onExitPortal;
    [SerializeField]
    private Renderer _renderer;
    private BoxCollider _collider;
    private bool _inChange = false;
    UniversalAdditionalCameraData _cameraData;

    void Awake()
    {
        AddColliderToCameara();
        
        if ( !_renderer ) _renderer = GetComponent<Renderer>();
        _collider = GetComponent<BoxCollider>();
        _renderer.material.SetFloat("_DissolveAmount", 1);
        _cameraData = Camera.main.GetUniversalAdditionalCameraData();
    }

    private void AddColliderToCameara()
    {
        SphereCollider collider = Camera.main.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 0.01f;
        collider.center = new Vector3(0, 0, 0f);
    }

    /*private void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            _cameraData.SetRenderer(1);
        }
    }*/

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {

            var camPos = other.transform.position;
            DebugMessageManager.Instance.AddMessage("玩家接觸 Portal 判定區");

            float distSqFromOutside = (_outsidePortalTransformRef.position - camPos).sqrMagnitude;
            float distSqFromInside = (_insidePortalTransformRef.position - camPos).sqrMagnitude;
            if ( distSqFromOutside < distSqFromInside) // 距離外面比較近 代表當時在外面 -> 要進去
            {
                Debug.Log("玩家進入 Portal");
                DebugMessageManager.Instance.AddMessage("玩家進入 Portal");
                _cameraData.SetRenderer(1);
                onEnterPortal?.Invoke();
            }
            else
            {
                Debug.Log("玩家離開 Portal");
                DebugMessageManager.Instance.AddMessage("玩家離開 Portal");
                _cameraData.SetRenderer(0);
                onExitPortal?.Invoke();
            }

            /*Vector3 localPos = transform.InverseTransformPoint(other.transform.position);
            if (localPos.z > 0)
            {
                Debug.Log("玩家進入 Portal");
                DebugMessageManager.Instance.AddMessage("玩家進入 Portal");
                _cameraData.SetRenderer(1);
                onEnterPortal?.Invoke();
            }
            else
            {
                Debug.Log("玩家離開 Portal");
                DebugMessageManager.Instance.AddMessage("玩家離開 Portal");
                _cameraData.SetRenderer(0);
                onExitPortal?.Invoke();
            }*/
        }
    }

    [ContextMenu("Show")]
    public async Task Show()
    {
        if(_inChange) return;
        _inChange = true;
        float value = 1;
        while (value > 0)
        {
            _renderer.material.SetFloat("_DissolveAmount", value);
            await Task.Delay(TimeSpan.FromSeconds(0.05f));
            value -= 0.02f;
        }
        _collider.enabled = true;
        _inChange = false;
    }

    [ContextMenu("Hide")]
    public async Task Hide()
    {
        if(_inChange) return;
        _inChange = true;
        float value = 0;
        while (value < 1)
        {
            _renderer.material.SetFloat("_DissolveAmount", value);
            await Task.Delay(TimeSpan.FromSeconds(0.05f));
            value += 0.02f;
        }
        _collider.enabled = false;
        _inChange = false;
    }

    public async Task ShowAsync()
    {
        if (_inChange) return;
        _inChange = true;
        float value = 1;
        while (value > 0) {
            _renderer.material.SetFloat("_DissolveAmount", value);
            await Task.Delay(TimeSpan.FromSeconds(0.05f));
            value -= 0.02f;
        }
        _inChange = false;
    }

    public async Task HideAsync()
    {
        if (_inChange) return;
        _inChange = true;
        float value = 0;
        while (value < 1) {
            _renderer.material.SetFloat("_DissolveAmount", value);
            await Task.Delay(TimeSpan.FromSeconds(0.05f));
            value += 0.02f;
        }
        _inChange = false;
    }

}