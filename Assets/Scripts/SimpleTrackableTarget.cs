using UnityEngine;

public class SimpleTrackableTarget : MonoBehaviour
{
    // 若有需要，可以做成 Singleton 獲取
    private SimpleTrackableTargetIndicatorManager _trackableTargetManager;

    public void InitializeAndSetReference(SimpleTrackableTargetIndicatorManager trackableTargetManager)
    {
        _trackableTargetManager = trackableTargetManager;
        
        ToggleTargetTrackable(false);
    }

    private void OnDestroy()
    {
        if (_trackableTargetManager != null) {
            _trackableTargetManager.UnregisterTarget(this.transform);
        }
    }

    public void ToggleTargetTrackable(bool isTrackable)
    {
        this.gameObject.SetActive(isTrackable);

        if ( _trackableTargetManager == null ) return;
        if (isTrackable) {
            _trackableTargetManager.RegisterTarget(this.transform);
        } else {
            _trackableTargetManager.UnregisterTarget(this.transform);
        }
    }
    // 與以下functions二選一使用即可

    private void OnEnable()
    {
        if (_trackableTargetManager != null) _trackableTargetManager.RegisterTarget(this.transform);
    }

    private void OnDisable()
    {
        if (_trackableTargetManager != null) _trackableTargetManager.UnregisterTarget(this.transform);
    }
}