using UnityEngine;
using UnityEngine.Rendering.Universal;
using Unity.VisualScripting;
using UnityEngine.Events;
using System.Threading.Tasks;
using System;

public class PortalController_v2 : MonoBehaviour
{
    public UnityEvent onEnterPortal,
        onExitPortal;

    [SerializeField] MeshRenderer _portalRenderer;
    
    [SerializeField]
    private Renderer _renderer;
    private BoxCollider _collider;
    private bool _inChange = false;
    UniversalAdditionalCameraData _cameraData;

    void Awake()
    {
        AddColliderToCameara();
        
        if ( !_renderer ) _renderer = GetComponent<Renderer>();
        _portalRenderer = GetComponent<MeshRenderer>();
        _collider = GetComponent<BoxCollider>();
        _cameraData = Camera.main.GetUniversalAdditionalCameraData();
    }

    private void AddColliderToCameara()
    {
        SphereCollider collider = Camera.main.AddComponent<SphereCollider>();
        collider.isTrigger = true;
        collider.radius = 0.05f;
        collider.center = new Vector3(0, 0, 0f);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            _portalRenderer.enabled = false;
            _cameraData.SetRenderer(1);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("MainCamera"))
        {
            _portalRenderer.enabled = true;
            Vector3 localPos = transform.InverseTransformPoint(other.transform.position);
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
            }
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